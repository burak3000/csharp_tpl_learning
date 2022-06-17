using System;
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
        public void TaskStates()
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
            m_OutputHelper.WriteLine(firstFinishedTaskIndex.ToString());
            m_OutputHelper.WriteLine($"AES Task Status: {aesTask.Status.ToString()}");
            m_OutputHelper.WriteLine($"MD5 Task Status: {aesTask.Status.ToString()}");
            m_OutputHelper.WriteLine($"Waiting for 3 secs...");
            m_OutputHelper.WriteLine($"AES Task Status: {aesTask.Status.ToString()}");
            m_OutputHelper.WriteLine($"MD5 Task Status: {aesTask.Status.ToString()}");


            Task.WaitAll(aesTask, md5Task);
            m_OutputHelper.WriteLine($"AES Task Status: {aesTask.Status.ToString()}");
            m_OutputHelper.WriteLine($"MD5 Task Status: {aesTask.Status.ToString()}");
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

    }
}
