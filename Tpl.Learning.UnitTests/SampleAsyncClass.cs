using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tpl.Learning.UnitTests
{
    public class SampleAsyncClass
    {
        int m_ClassField = 1;
        public async Task DoJobsAsync()
        {
            Task jobOne = DoJobOneAsync(m_ClassField);
            Task<int> jobTwo = DoJobTwoAsync(m_ClassField);
            await Task.WhenAll(jobOne, jobTwo);
        }

        private async Task DoJobOneAsync(int jobOneIntParameter)
        {
            int localVariable = 5;
            await Task.Delay(1000);
            ++jobOneIntParameter;
            await DoJobTwoAsync(jobOneIntParameter);
        }
        private async Task<int> DoJobTwoAsync(int jobTwoIntParameter)
        {
            int a = 5;
            await Task.Delay(1000);
            ++jobTwoIntParameter;
            return jobTwoIntParameter;
        }
    }
}
