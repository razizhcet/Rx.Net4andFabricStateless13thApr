using Rx.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rx.Concurrency
{
    class RxScheduler
    {
        static void Main(string[] args)
        {
            CanceleProcess();
            Console.ReadKey();
        }

        static void CanceleProcess()
        {
            var list = new List<int>();
            Console.WriteLine("Enter to quit:");
            var token = Scheduler.Schedule(list, Work);
            Console.ReadLine();
            Console.WriteLine("Cancelling...");
            token.Dispose();
            Console.WriteLine("Cancelled");
        }

        public IDisposable Work(IScheduler scheduler, List<int> list)
        {
            var tokenSource = new CancellationTokenSource();
            var cancelToken = tokenSource.Token;
            var task = new Task(() =>
            {
                Console.WriteLine();
                for (int i = 0; i < 1000; i++)
                {
                    var sw = new SpinWait();
                    for (int j = 0; j < 3000; j++) sw.SpinOnce();
                    Console.Write(".");
                    list.Add(i);
                    if (cancelToken.IsCancellationRequested)
                    {
                        Console.WriteLine("Cancelation requested");
                        //cancelToken.ThrowIfCancellationRequested();
                        return;
                    }
                }
            }, cancelToken);
            task.Start();
            return Disposable.Create(tokenSource.Cancel);
        }
    }
}
