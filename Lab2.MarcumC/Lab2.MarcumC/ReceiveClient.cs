using System;
using System.Net.Sockets;
using System.Threading;

namespace Lab2.MarcumC
{
    class ReceiveClient
    {
        private const int Port = 2605;
        public TcpListener MyListener;// = new TcpListener()
        TcpClient[] _myClients = new TcpClient[15];
        public ReceiveClient()
        {
            MyListener = new TcpListener(Port);
            // myWatch.Start();
            MyListener.Start(1);
            // while (!myListener.Pending())
            // {
            // Thread.Sleep(1000);
            // Console.WriteLine("Waiting for pending connection requests");
            //Just gonna loop here until something is pending
            // }

            var clientIndex = 0;
            while (true)
            {
                if (MyListener.Pending())
                {
                    //myClients[clientIndex] = myListener.AcceptTcpClient();
                    var newClient = MyListener.AcceptTcpClient();
                    var newThread = new Thread(() => StartNewServerClass(newClient));
                    newThread.Start();
                    clientIndex++;
                }
                else
                {
                    Thread.Sleep(15);
                }

                //serverClient = myListener.AcceptTcpClient();

            }
        }

        public void StartNewServerClass(TcpClient passedClient)
        {
            var newServer = new MyServerClass(passedClient);
            Console.Read();
        }
    }
}
