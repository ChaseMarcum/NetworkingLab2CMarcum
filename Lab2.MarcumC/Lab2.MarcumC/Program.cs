using System;

namespace Lab2.MarcumC
{
    class Program
    {
        static void Main(string[] args)
        {
            //Client myClient = new Client();
            var newtest = new TestClass();
        }

        public void StartLab3()
        {
            var serverTestOne = new LabThreePointOne();
            Console.Read();
        }
        static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
