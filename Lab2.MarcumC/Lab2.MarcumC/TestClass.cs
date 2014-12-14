using System;
using System.Threading;

namespace Lab2.MarcumC
{
    class TestClass
    {
        public TestClass()
        {
            var oneThread = new Thread(StartFirstThread);
            oneThread.Start();
            var twoThread = new Thread(StartSecondThread);
            twoThread.Start();
            var threeThread = new Thread(StartThirdThread);
            threeThread.Start();
            //var fourThread = new Thread(StartForthThread);
            //fourThread.Start();
            //var fiveThread = new Thread(StartFifthThread);
            //fiveThread.Start();
            //var sixThread = new Thread(StartSixthThread);
            //sixThread.Start();
            //var sevenThread = new Thread(StartSeventhThread);
            //sevenThread.Start();

            Console.Read();
        }

        public void StartFirstThread()
        {
            var newLab3 = new MyServerClass();
            Console.Read();
        }

        public void StartSecondThread()
        {
            var newLab2 = new ReceiveClient();
            Console.Read();
        }

        public void StartThirdThread()
        {
            var newLab3 = new AsynchronousClient();
            Console.Read();
        }
        public void StartForthThread()
        {
            var newLab3 = new JohnsClient();
            Console.Read();
        }
        public void StartFifthThread()
        {
            var newLab3 = new Client();
            Console.Read();
        }
        public void StartSixthThread()
        {
            //var newLab3 = new MiddleWare();
            //Console.Read();
        }
        public void StartSeventhThread()
        {
            //var newLab3 = new JohnsServer();
            //Console.Read();
        }


    }
}