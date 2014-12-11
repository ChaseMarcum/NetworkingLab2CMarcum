﻿using System;
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

            Console.Read();
        }

        public void StartFirstThread()
        {
            var newLab3 = new ReceiveClient();
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
        public void StartForthThread()
        {
            var newLab3 = new JohnsClient();
            Console.Read();
        }

    }
}