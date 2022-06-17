using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Tpl.Learning.UnitTests
{
    public class _03_ExceptionHandlingTest
    {
        private readonly ITestOutputHelper m_OutputHelper;
        private const int NUM_AES_KEYS = 8000000;
        private const int NUM_MD5_HASHES = 100000;
        public _03_ExceptionHandlingTest(ITestOutputHelper output)
        {
            this.m_OutputHelper = output;
        }
        [Fact]
        public void ParallelInvoke()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                Parallel.Invoke(
                () => GenerateAESKeysWithNormalParallelFor(),
                () => GenerateMD5HashesWithNormalParallelFor());
            }
            catch (AggregateException ae)
            {
                int i = 0;
                m_OutputHelper.WriteLine($"There is/are {ae.InnerExceptions.Count} exception(s) occurred while invoking actions in parallel.");
                foreach (var ex in ae.InnerExceptions)
                {
                    m_OutputHelper.WriteLine("----------------------------------------------------------------");
                    m_OutputHelper.WriteLine($"Exception #{++i}");
                    m_OutputHelper.WriteLine(ex.InnerException.ToString());
                    m_OutputHelper.WriteLine("----------------------------------------------------------------");
                }
            }

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
            var sw = Stopwatch.StartNew();
            var loopResult = Parallel.For(1, NUM_AES_KEYS + 1, (i, ls) =>
            {
                if (sw.ElapsedMilliseconds > 3000)
                {
                    throw new TimeoutException("Creating AES Keys is taking longer than expected.");
                }
                var aesM = new AesManaged();
                aesM.GenerateKey();
                byte[] result = aesM.Key;
                string hexString = ConvertToHexString(result);
            });
            PrintLoopResult("AES with Parallel.For", loopResult, sw.Elapsed.ToString());
        }



        private void GenerateMD5HashesWithNormalParallelFor()
        {
            var sw = Stopwatch.StartNew();
            var loopResult = Parallel.For(1, NUM_MD5_HASHES + 1, (i, ls) =>
            {
                if (sw.ElapsedMilliseconds > 3000)
                {
                    throw new ArgumentException("Creating MD5 hashes is taking longer than expected.");
                }
                var md5M = MD5.Create();
                byte[] data =
                    Encoding.Unicode.GetBytes(
                    Environment.UserName + i.ToString());
                byte[] result = md5M.ComputeHash(data);
                string hexString = ConvertToHexString(result);
            });
            PrintLoopResult("MD5 with Parallel.For", loopResult, sw.Elapsed.ToString());
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