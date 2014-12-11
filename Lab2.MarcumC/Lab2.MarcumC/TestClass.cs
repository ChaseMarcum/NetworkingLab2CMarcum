using System;
using System.Threading;

namespace Lab2.MarcumC
{
    class TestClass
    {
        public TestClass()
        {
            var oneThread = new Thread(new ThreadStart(StartFirstThread));
            oneThread.Start();
           // Thread twoThread = new Thread(new ThreadStart(startSecondThread));
            //twoThread.Start();
            //Thread threeThread = new Thread(new ThreadStart(startThirdThread));
            //threeThread.Start();

            Console.Read();
        }

        public void StartFirstThread()
        {
            var newLab3 = new ClientGetter();
            Console.Read();
        }

        public void StartSecondThread()
        {
            var newLab2 = new LabTwoPointTwo();
            
            Console.Read();
        }

        public void StartThirdThread()
        {
            var newLab3 = new AsynchronousClient();
            Console.Read();
        }
    }
}