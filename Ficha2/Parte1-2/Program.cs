using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parte1_2
{
    class Program
    {
        public static void Main()
        {
            Task t1 = Task.Factory.StartNew(() =>
            {
                Task.Factory.StartNew(() =>
                {
                    throw new Exception("Child exception!");
                }, TaskCreationOptions.AttachedToParent);
                throw new Exception("Parent exception!");
            });
            ((IAsyncResult)t1).AsyncWaitHandle.WaitOne();
            t1 = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.ReadLine();
        }
    }
}
