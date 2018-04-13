using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rx.TamingSequence
{
    class HotAndColdObservable
    {
        private const string connectionString = @"data source=.\RAZISQL;persist security info=true;user id=sa;password=gb2266";

        static void Main(string[] args)
        {
            //ColdSample();
            //SimpleColdSample();
            //PublishAndConnect();
            //ConnectAfterSubscribe();
            //ConnSubscrDisposal();
            //AutoDisposalConnection();
            //RefCountMethod();
            //PublishLastMethod();
            //ReplayMethod();
            Multicast();
            Console.ReadKey();
        }

        static void ColdSample()
        {
            var products = GetProducts().Take(3);
            products.Subscribe(Console.WriteLine);
        }

        private static IObservable<string> GetProducts()
        {
            return Observable.Create<string>(
            o =>
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("select Name from tblEmployee", conn))
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    while (reader.Read())
                    {
                        o.OnNext(reader.GetString(0));
                    }
                    o.OnCompleted();
                    return Disposable.Create(() => Console.WriteLine("--Disposed--"));
                }
            });
        }

        static void SimpleColdSample()
        {
            var period = TimeSpan.FromSeconds(1);
            var observable = Observable.Interval(period);
            observable.Subscribe(i => Console.WriteLine("first subscription : {0}", i));
            Thread.Sleep(period);
            observable.Subscribe(i => Console.WriteLine("second subscription : {0}", i));
        }

        static void PublishAndConnect()
        {
            var period = TimeSpan.FromSeconds(1);
            var observable = Observable.Interval(period).Publish();
            observable.Connect();
            observable.Subscribe(i => Console.WriteLine("first subscription : {0}", i));
            Thread.Sleep(period);
            observable.Subscribe(i => Console.WriteLine("second subscription : {0}", i));
        }

        static void ConnectAfterSubscribe()
        {
            var period = TimeSpan.FromSeconds(1);
            var observable = Observable.Interval(period).Publish();
            observable.Subscribe(i => Console.WriteLine("first subscription : {0}", i));
            Thread.Sleep(period);
            observable.Subscribe(i => Console.WriteLine("second subscription : {0}", i));
            observable.Connect();
        }

        static void ConnSubscrDisposal()
        {
            var period = TimeSpan.FromSeconds(1);
            var observable = Observable.Interval(period).Publish();
            observable.Subscribe(i => Console.WriteLine("subscription : {0}", i));
            var exit = false;
            while (!exit)
            {
                Console.WriteLine("Press enter to connect, esc to exit.");
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    var connection = observable.Connect(); //--Connects here--
                    Console.WriteLine("Press any key to dispose of connection.");
                    Console.ReadKey();
                    connection.Dispose(); //--Disconnects here--
                }
                if (key.Key == ConsoleKey.Escape)
                {
                    exit = true;
                }
            }
        }

        static void AutoDisposalConnection()
        {
            var period = TimeSpan.FromSeconds(1);
            var observable = Observable.Interval(period)
            .Do(l => Console.WriteLine("Publishing {0}", l)) //Side effect to show it is running
            .Publish();
            observable.Connect();
            Console.WriteLine("Press any key to subscribe");
            Console.ReadKey();
            var subscription = observable.Subscribe(i => Console.WriteLine("subscription : {0}", i));
            Console.WriteLine("Press any key to unsubscribe.");
            Console.ReadKey();
            subscription.Dispose();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        static void RefCountMethod()
        {
            var period = TimeSpan.FromSeconds(1);
            var observable = Observable.Interval(period)
            .Do(l => Console.WriteLine("Publishing {0}", l)) //side effect to show it is running
            .Publish()
            .RefCount();
            //observable.Connect(); Use RefCount instead now 
            Console.WriteLine("Press any key to subscribe");
            Console.ReadKey();
            var subscription = observable.Subscribe(i => Console.WriteLine("subscription : {0}", i));
            Console.WriteLine("Press any key to unsubscribe.");
            Console.ReadKey();
            subscription.Dispose();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        static void PublishLastMethod()
        {
            var period = TimeSpan.FromSeconds(1);
            var observable = Observable.Interval(period)
            .Take(5)
            .Do(l => Console.WriteLine("Publishing {0}", l)) //side effect to show it is running
            .PublishLast();
            observable.Connect();
            Console.WriteLine("Press any key to subscribe");
            Console.ReadKey();
            var subscription = observable.Subscribe(i => Console.WriteLine("subscription : {0}", i));
            Console.WriteLine("Press any key to unsubscribe.");
            Console.ReadKey();
            subscription.Dispose();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        static void ReplayMethod()
        {
            var period = TimeSpan.FromSeconds(1);
            var hot = Observable.Interval(period)
            .Take(3)
            .Publish();
            hot.Connect();
            Thread.Sleep(period); //Run hot and ensure a value is lost.
            var observable = hot.Replay();
            observable.Connect();
            observable.Subscribe(i => Console.WriteLine("first subscription : {0}", i));
            Thread.Sleep(period);
            observable.Subscribe(i => Console.WriteLine("second subscription : {0}", i));
            Console.ReadKey();
            observable.Subscribe(i => Console.WriteLine("third subscription : {0}", i));
            Console.ReadKey();
        }

        static void Multicast()
        {
            var period = TimeSpan.FromSeconds(1);
            //var observable = Observable.Interval(period).Publish();
            var observable = Observable.Interval(period);
            var shared = new Subject<long>();
            shared.Subscribe(i => Console.WriteLine("first subscription : {0}", i));
            observable.Subscribe(shared);   //'Connect' the observable.
            Thread.Sleep(period);
            //Thread.Sleep(period);
            shared.Subscribe(i => Console.WriteLine("second subscription : {0}", i));
        }
    }
}
