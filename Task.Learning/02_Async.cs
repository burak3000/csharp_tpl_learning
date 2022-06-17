using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Xunit.Abstractions;

namespace Task.Learning.UnitTests
{
    public class _02_ParallelInvokeTests
    {
        private readonly ITestOutputHelper m_OutputHelper;
        private const int NUM_AES_KEYS = 8000000;
        private const int NUM_MD5_HASHES = 100000;
        public _02_ParallelInvokeTests(ITestOutputHelper output)
        {
            this.m_OutputHelper = output;
        }
        [Fact]
        public void ParallelInvoke()
        {
            var sw = Stopwatch.StartNew();
            Parallel.Invoke(
                () => GenerateAESKeysWithNormalParallelFor(),
                () => GenerateAESKeysWithNormalParallelForWithBreak(),
                () => GenerateAESKeysWithNormalParallelForWithStop(),
                () => GenerateMD5HashesWithNormalParallelFor(),
                () => GenerateAESKeysWithParallelForEach(),
                () => GenerateMD5HashesWithParallelForEach());
            m_OutputHelper.WriteLine("Execution time is " + sw.Elapsed.ToString());
        }


        private string ConvertToHexString(Byte[] byteArray)
        {
            // Convert the byte array to hexadecimal string
            var sb = new StringBuilder(byteArray.Length);

            for (int i = 0; i < byteArray.Length; i++)
            {
                sb.Append(byteArray[i].ToString("X2"));
            }

            return sb.ToString();
        }



        private void GenerateAESKeysWithNormalParallelFor()
        {
            /*
             We can set the max degree of parallelism also
            var parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = 3;(by default it is -1 which means use all available resources
            It can often be advantageous to set MaxDegreeOfParallelism to Environment.ProcessorCount
            or some value derived from it (Environment.ProcessorCount * 2)
            By default, when no MaxDegreeOfParallelism is specified, 
            TPL allows a heuristic to ramp up and down the number of threads, 
            potentially beyond ProcessorCount. It does this in order to better support
            mixed CPU and I/O-bound workloads.
            */
            var sw = Stopwatch.StartNew();
            var loopResult = Parallel.For(1, NUM_AES_KEYS + 1, (i) =>
            {
                var aesM = new AesManaged();
                aesM.GenerateKey();
                byte[] result = aesM.Key;
                string hexString = ConvertToHexString(result);
            });
            PrintLoopResult("AES with Parallel.For", loopResult, sw.Elapsed.ToString());
        }

        private void GenerateAESKeysWithNormalParallelForWithBreak()
        {
            var sw = Stopwatch.StartNew();
            var loopResult = Parallel.For(1, NUM_AES_KEYS + 1, (i, ls) =>
            {
                if (ls.ShouldExitCurrentIteration)
                {
                    return;
                }
                if (i % 1000 == 0)
                {
                    ls.Break();
                    m_OutputHelper.WriteLine($"AES with Parallel.For Break is called with number {i}: " + sw.Elapsed.ToString());
                }
                var aesM = new AesManaged();
                aesM.GenerateKey();
                byte[] result = aesM.Key;
                string hexString = ConvertToHexString(result);
            });
            PrintLoopResult("AES with Parallel.For with Break", loopResult, sw.Elapsed.ToString());
        }

        private void GenerateAESKeysWithNormalParallelForWithStop()
        {
            var sw = Stopwatch.StartNew();
            var loopResult = Parallel.For(1, NUM_AES_KEYS + 1, (i, ls) =>
            {
                if (ls.IsStopped)
                {
                    return;
                }
                if (i % 1000 == 0)
                {
                    ls.Stop();
                    m_OutputHelper.WriteLine($"AES with Parallel.For Stop is called with number {i}: " + sw.Elapsed.ToString());
                }
                var aesM = new AesManaged();
                aesM.GenerateKey();
                byte[] result = aesM.Key;
                string hexString = ConvertToHexString(result);
            });
            PrintLoopResult("AES with Parallel.For with Stop", loopResult, sw.Elapsed.ToString());
        }

        private void GenerateMD5HashesWithNormalParallelFor()
        {
            var sw = Stopwatch.StartNew();
            var loopResult = Parallel.For(1, NUM_MD5_HASHES + 1, (i) =>
            {
                var md5M = MD5.Create();
                byte[] data =
                    Encoding.Unicode.GetBytes(
                    Environment.UserName + i.ToString());
                byte[] result = md5M.ComputeHash(data);
                string hexString = ConvertToHexString(result);
            });
            PrintLoopResult("MD5 with Parallel.For", loopResult, sw.Elapsed.ToString());
        }


        private void GenerateAESKeysWithParallelForEach()
        {
            var sw = Stopwatch.StartNew();
            var loopResult = Parallel.ForEach(Partitioner.Create(1, NUM_AES_KEYS + 1), range =>
            {
                var aesM = new AesManaged();
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    aesM.GenerateKey();
                    byte[] result = aesM.Key;
                    string hexString = ConvertToHexString(result);
                }
            });
            PrintLoopResult("AES with Parallel.Foreach", loopResult, sw.Elapsed.ToString());
        }

        private void GenerateMD5HashesWithParallelForEach()
        {
            var sw = Stopwatch.StartNew();
            var loopResult = Parallel.ForEach(Partitioner.Create(1, NUM_MD5_HASHES + 1),
            range =>
            {
                var md5M = MD5.Create();
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    byte[] data =
                        Encoding.Unicode.GetBytes(
                        Environment.UserName + i.ToString());
                    byte[] result = md5M.ComputeHash(data);
                    string hexString = ConvertToHexString(result);
                }
            });
            PrintLoopResult("MD5 with Parallel.Foreach", loopResult, sw.Elapsed.ToString());
        }

        private void PrintLoopResult(string scenarioName, ParallelLoopResult result, string executionTime)
        {
            string text;
            if (result.IsCompleted)
            {
                text = "The loop ran to completion.";
            }
            else
            {
                if (result.LowestBreakIteration.HasValue)
                {
                    text = "The loop ended by calling the Break statement";
                }
                else
                {
                    text = "The loop ended prematurely with a Stop statement";
                }
            }

            m_OutputHelper.WriteLine("------------------------------------------------------------------" +
                                     $"{Environment.NewLine}" +
                                     $"Scenario Name: {scenarioName}" +
                                     $"{Environment.NewLine}" +
                                     $"Loop result is: {text}" +
                                     $"{Environment.NewLine}" +
                                     $"Execution time: {executionTime}" +
                                     $"{Environment.NewLine}" +
                                     "------------------------------------------------------------------");
        }
    }
}