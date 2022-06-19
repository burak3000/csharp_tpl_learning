using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Tpl.Learning.UnitTests
{
    public class _05_ConcurrentCollections
    {
        ITestOutputHelper m_OutputHelper;
        public _05_ConcurrentCollections(ITestOutputHelper output)
        {
            m_OutputHelper = output;
        }

        #region Concurrent Queue
        private const int NUM_AES_KEYS = 8000000;
        /*ConcurrentQueue is completely lock free. But when Compare-And-Swap(CAS) operations fail and faced with contention, 
         * they may end up spinning and retrying. Contention is the condition that arises when many tasks or threads attemt
         * to use a single resource at the same time.*/
        ConcurrentQueue<string> _keysQueue = new ConcurrentQueue<string>();

        [Fact]
        public async void ConcurrentQueueExample()
        {
            await Task.Run(() => ParallelPartitionGenerateAESKeys());
            Assert.Equal(NUM_AES_KEYS, _keysQueue.Count);
        }

        private void ParallelPartitionGenerateAESKeys()
        {
            var sw = Stopwatch.StartNew();
            Parallel.ForEach(Partitioner.Create(1, NUM_AES_KEYS + 1), range =>
            {
                var aesM = new AesManaged();
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    aesM.GenerateKey();
                    byte[] result = aesM.Key;
                    string hexString = ConvertToHexString(result);
                    _keysQueue.Enqueue(hexString);
                    // Console.WriteLine(“AES KEY: {0} “, hexString);
                }
            });
        }

        private string ConvertToHexString(byte[] byteArray)
        {
            // Convert the byte array to hexadecimal string
            var sb = new StringBuilder(byteArray.Length);

            for (int i = 0; i < byteArray.Length; i++)
            {
                sb.Append(byteArray[i].ToString("X2"));
            }

            return sb.ToString();
        }
        #endregion

        #region Blocking Collection
        [Fact]
        public void BlockingCollectionExample()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken ct = cts.Token;

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(10000);
                cts.Cancel();
            });
            Parallel.Invoke(
                () => ProduceSentences(ct),
                () => ConsumeSentences());
        }

        private const int NUM_SENTENCES = 50;
        private BlockingCollection<string> _sentencesBC = new BlockingCollection<string>(NUM_SENTENCES / 10);


        private void ProduceSentences(System.Threading.CancellationToken ct)
        {
            string[] possibleSentences =
            {
                 "Simple sentence 1",
                 "Simple sentence 2",
                 "Simple sentence 3",
                 "Simple sentence 4",
                 "Simple sentence 5",
                 "Simple sentence 6",
                 "Simple sentence 7",
                 "Simple sentence 8",
                 "Simple sentence 9",
            };

            int sleepTime = 10;
            var rnd = new Random();
            for (int i = 0; i < NUM_SENTENCES; i++)
            {
                var sb = new StringBuilder();
                int sentenceIndex = rnd.Next(possibleSentences.Length);
                string newSentence = possibleSentences[sentenceIndex];

                try
                {
                    //Adding new element to the colllection with cancellation token and timeout 
                    bool isAdded = _sentencesBC.TryAdd(newSentence, 2000, ct);
                    if (isAdded)
                    {
                        m_OutputHelper.WriteLine($"{newSentence} is produced.");
                    }
                    else
                    {
                        throw new TimeoutException("_sentencesBC took more than 2 seconds to add an item");
                    }
                }
                catch (OperationCanceledException ex)
                {
                    // The operation was cancelled
                    m_OutputHelper.WriteLine("Operation is cancelled by the user.");
                    break;
                    // The next statement after the loop will let
                    // the consumer know the producer’s work is done
                }
                catch (TimeoutException ex)
                {
                    break;
                }
                sleepTime += 5;
                Thread.Sleep(sleepTime);
            }

            // Let the consumer know the producer’s work is done
            _sentencesBC.CompleteAdding();
        }

        private void ConsumeSentences()
        {
            int sleepTime = 100;
            while (!_sentencesBC.IsCompleted)
            {
                string takenSenctence;
                if (_sentencesBC.TryTake(out takenSenctence))
                {
                    m_OutputHelper.WriteLine($"{takenSenctence} is consumed.");
                    sleepTime -= 5;
                    if (sleepTime >= 0)
                        Thread.Sleep(sleepTime);
                }

            }
        }
        #endregion

        #region Concurrent Dictionary
        private const int dictEntryCount = 10000;
        [Fact]
        public void ConcurrentDictionaryExample()
        {
            ConcurrentDictionary<int, string> dictionary = new ConcurrentDictionary<int, string>();
            for (int i = 0; i < dictEntryCount; i++)
            {
                int key = i % 1000;
                string value = key.ToString();
                dictionary.AddOrUpdate(key, value, (existingKey, existingValue) =>
                 {
                     lock (existingValue)
                     {
                         existingValue += value;
                     }
                     return existingValue;
                 });
            }

            m_OutputHelper.WriteLine("Dict key values");
            foreach (var item in dictionary)
            {
                m_OutputHelper.WriteLine($"Key: {item.Key}, Value: {item.Value}");
            }
        }
        #endregion

    }
}
