using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tpl.Learning.UnitTests
{
    public class SampleSynchronizedClass
    {
        int m_Nr = 1;
        public void DoJobs()
        {
            DoJobOne(m_Nr);
            var result = DoJobTwo(m_Nr);
        }



        private void DoJobOne(int nr)
        {
            int a = 5;
            Thread.Sleep(1000);
            ++nr;
            Thread.Sleep(1000);
        }
        private int DoJobTwo(int nr)
        {
            int a = 5;
            Thread.Sleep(1000);
            ++nr;
            Thread.Sleep(1000);
            return nr;
        }
    }
}
