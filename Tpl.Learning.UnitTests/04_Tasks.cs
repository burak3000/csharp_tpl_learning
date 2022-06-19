using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Tpl.Learning.UnitTests
{
    public class _04_Tasks
    {
        private readonly ITestOutputHelper m_OutputHelper;
        private const int NUM_AES_KEYS = 8000000;
        private const int NUM_MD5_HASHES = 100000;

        public _04_Tasks(ITestOutputHelper output)
        {
            m_OutputHelper = output;
        }
        [Fact]
        public void ShowTaskStates()
        {
            Task aesTask = new Task(() => GenerateAESKeys());
            Task md5Task = new Task(() => GenerateMD5Hashes());
            m_OutputHelper.WriteLine($"AES Task Status: {aesTask.Status.ToString()}");
            m_OutputHelper.WriteLine($"MD5 Task Status: {aesTask.Status.ToString()}");

            aesTask.Start();
            md5Task.Start();

            m_OutputHelper.WriteLine($"AES Task Status: {aesTask.Status.ToString()}");
            m_OutputHelper.WriteLine($"MD5 Task Status: {aesTask.Status.ToString()}");

            int firstFinishedTaskIndex = Task.WaitAny(aesTask, md5Task);
            m_OutputHelper.WriteLine("First finished task index is: " + firstFinishedTaskIndex.ToString());
            m_OutputHelper.WriteLine($"AES Task Status: {aesTask.Status.ToString()}");
            m_OutputHelper.WriteLine($"MD5 Task Status: {md5Task.Status.ToString()}");

            Task.WaitAll(aesTask, md5Task);
            m_OutputHelper.WriteLine($"AES Task Status: {aesTask.Status.ToString()}");
            m_OutputHelper.WriteLine($"MD5 Task Status: {md5Task.Status.ToString()}");
        }

        [Fact]
        public void ShowManagedThreadIds()
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                Task aesTask = new Task(() =>
                {
                    m_OutputHelper.WriteLine($"GenerateAESKeys Task Current Managed Thread Id: {Environment.CurrentManagedThreadId}");
                    GenerateAESKeys();
                });
                Task md5Task = new Task(() =>
                {
                    m_OutputHelper.WriteLine($"GenerateMD5 Task Current Managed Thread Id: {Environment.CurrentManagedThreadId}");
                    GenerateMD5Hashes();
                });

                tasks.Add(aesTask);
                tasks.Add(md5Task);
                aesTask.Start();
                md5Task.Start();
            }


            Task.WaitAll(tasks.ToArray());
        }

        [Fact]
        public async Task CancellationAndContinuation()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken ct = cts.Token;
            Task cancelTask = Task.Run(async () => { await Task.Delay(10000); cts.Cancel(); });

            //aesTaskWithCt will be canceled before it is finished. We will handle this situation with continuation.
            Task aesTaskWithCt = Task.Factory.StartNew(() => GenerateAESKeysWithCancellation(ct), ct);

            Task aesCanceledTask = aesTaskWithCt.ContinueWith((t) =>
            {
                m_OutputHelper.WriteLine("aesCancelTask continuation: aesTask is canceled.");
            }, TaskContinuationOptions.OnlyOnCanceled);

            Task aesCompletedTask = aesTaskWithCt.ContinueWith((t) =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    m_OutputHelper.WriteLine("aesTask continuation: aesTask is completed.");
                }
                else if (t.Status == TaskStatus.Canceled)
                {
                    m_OutputHelper.WriteLine("aesTask continuation: aesTask is canceled.");
                }
                else if (t.Status == TaskStatus.Faulted)
                {
                    m_OutputHelper.WriteLine("aesTask continuation: aesTask threw an unhandled exception.");
                }
            });

            Task md5TaskWithCt = Task.Factory.StartNew(() => GenerateMD5HashesWithCancellation(ct), ct);

            Task waitTask = Task.WhenAll(aesCanceledTask, aesCompletedTask, md5TaskWithCt);
            await waitTask;
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

        private void GenerateAESKeys()
        {
            var sw = Stopwatch.StartNew();
            var aesM = new AesManaged();
            for (int i = 1; i <= NUM_AES_KEYS; i++)
            {
                aesM.GenerateKey();
                byte[] result = aesM.Key;
                string hexString = ConvertToHexString(result);
            }
            m_OutputHelper.WriteLine("AES: " + sw.Elapsed.ToString());
        }

        private void GenerateMD5Hashes()
        {
            var sw = Stopwatch.StartNew();
            var md5M = MD5.Create();
            for (int i = 1; i <= NUM_MD5_HASHES; i++)
            {
                byte[] data =
                    Encoding.Unicode.GetBytes(
                    Environment.UserName + i.ToString());
                byte[] result = md5M.ComputeHash(data);
                string hexString = ConvertToHexString(result);
            }
            m_OutputHelper.WriteLine("MD5: " + sw.Elapsed.ToString());
        }

        private void GenerateAESKeysWithCancellation(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var sw = Stopwatch.StartNew();
            var aesM = new AesManaged();
            for (int i = 1; i <= NUM_AES_KEYS; i++)
            {
                aesM.GenerateKey();
                byte[] result = aesM.Key;
                string hexString = ConvertToHexString(result);
                ct.ThrowIfCancellationRequested();
            }
            m_OutputHelper.WriteLine("AES: " + sw.Elapsed.ToString());
        }

        private void GenerateMD5HashesWithCancellation(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var sw = Stopwatch.StartNew();
            var md5M = MD5.Create();
            for (int i = 1; i <= NUM_MD5_HASHES; i++)
            {
                byte[] data =
                    Encoding.Unicode.GetBytes(
                    Environment.UserName + i.ToString());
                byte[] result = md5M.ComputeHash(data);
                string hexString = ConvertToHexString(result);
                ct.ThrowIfCancellationRequested();
            }
            m_OutputHelper.WriteLine("MD5: " + sw.Elapsed.ToString());
        }

    }
}
