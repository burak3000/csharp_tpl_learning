using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Xunit.Abstractions;

namespace Task.Learning.UnitTests
{
    public class _01_SyncronizedTests
    {
        private readonly ITestOutputHelper m_OutputHelper;
        private const int NUM_AES_KEYS = 8000000;
        private const int NUM_MD5_HASHES = 100000;
        public _01_SyncronizedTests(ITestOutputHelper output)
        {
            this.m_OutputHelper = output;
        }
        [Fact]
        public void Synchronized()
        {
            var sw = Stopwatch.StartNew();

            GenerateAESKeys();
            GenerateMD5Hashes();
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