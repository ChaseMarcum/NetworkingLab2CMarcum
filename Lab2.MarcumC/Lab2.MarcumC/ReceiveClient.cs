using System;
using System.Net.Sockets;
using System.Threading;

namespace Lab2.MarcumC
{
    class ReceiveClient
    {
        private const int Port = 2605;
        public TcpListener MyListener;
        TcpClient[] _myClients = new TcpClient[15];
        public ReceiveClient()
        {
            MyListener = new TcpListener(Port);
            MyListener.Start(3);

            var clientIndex = 0;
            while (true)
            {
                if (MyListener.Pending())
                {
                    var newClient = MyListener.AcceptTcpClient();
                    var newThread = new Thread(() => StartNewServerClass(newClient));
                    newThread.Start();
                    clientIndex++;
                }
                else
                {
                    Thread.Sleep(15);
                }
            }
        }

        public void StartNewServerClass(TcpClient passedClient)
        {
            var newServer = new MyServerClass(passedClient);
            Console.Read();
        }
    }
}
