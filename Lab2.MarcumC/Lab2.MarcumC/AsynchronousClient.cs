using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Lab2.MarcumC
{
    public class AsynchronousClient
    {
        // ManualResetEvent instances signal completion.
        private static readonly ManualResetEvent ConnectDone =
            new ManualResetEvent(false);
        private static readonly ManualResetEvent SendDone =
            new ManualResetEvent(false);
        private static readonly ManualResetEvent ReceiveDone =
            new ManualResetEvent(false);

        // The response from the remote device.
        private static String _response = String.Empty;

        // MS to wait between requests.
        private const int PauseTime = 10;
        private const int NumRequests = 10000;

        // The port number for the remote device.
        private const int Port = 2605;
        private const string IpStr = "10.1.20.9";

        public AsynchronousClient()
        {
            Start();
        }
        private static void StartClient()
        {
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // The name of the 
                // remote device is "host.contoso.com".
                IPHostEntry ipHostInfo = Dns.Resolve(IpStr);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                // IPAddress ipAddress = AsynchronousSocketListener.myIpAddress; //ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, Port);

                // Create a TCP/IP socket.
                Socket client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);
                ConnectDone.WaitOne();

                var transStopWatch = new Stopwatch();
                var reqStopWatch = new Stopwatch();
                var rspStopWatch = new Stopwatch();

                transStopWatch.Start();
                for (var i = 0; i < NumRequests; ++i)
                {
                    String request = RequestBuilder.GetRequestString(client);
                    Logger.StoreLine(request);

                    reqStopWatch.Start();
                    // Send test data to the remote device.
                    Send(client, request);


                    ++Trailer.RequestsTransmitted;

                    SendDone.WaitOne();

                    // Receive the response from the remote device.
                    Receive(client);
                    rspStopWatch.Start();
                    ++Trailer.ResponsesReceived;
                    ReceiveDone.WaitOne();


                    // Write the response to the console.
                    //Console.WriteLine("Response received : {0}", response);
                    Thread.Sleep(PauseTime);
                }
                transStopWatch.Stop();
                reqStopWatch.Stop();
                rspStopWatch.Stop();

                Trailer.RequestsTransmitted = NumRequests;
                Trailer.TransDurationMs = transStopWatch.Elapsed.Milliseconds + transStopWatch.Elapsed.Seconds * 1000 + transStopWatch.Elapsed.Minutes * 60000;
                Trailer.RspRunDurationMs = rspStopWatch.Elapsed.Milliseconds + rspStopWatch.Elapsed.Seconds * 1000 + rspStopWatch.Elapsed.Minutes * 60000;
                Trailer.ReqRunDurationMs = reqStopWatch.Elapsed.Milliseconds + reqStopWatch.Elapsed.Seconds * 1000 + reqStopWatch.Elapsed.Minutes * 60000;
                Trailer.ActualReqPaceMs = Trailer.ReqRunDurationMs / NumRequests;
                Trailer.ActualRspPaceMs = Trailer.RspRunDurationMs / NumRequests;
                Trailer.TransactionAvgMs = Trailer.TransDurationMs / NumRequests;
                Trailer.ConfiguredPaceMs = PauseTime;

                // Release the socket.
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                Logger.WriteLog();

            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString(e));
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                var client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                ConnectDone.Set();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
            }
        }

        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                var state = new StateObject {WorkSocket = client};

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    ReceiveCallback, state);
            }
            catch (Exception e)
            {
                // Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                var state = (StateObject)ar.AsyncState;
                var client = state.WorkSocket;


                // Read data from the remote device.

                var bytesRead = client.EndReceive(ar);

                if (bytesRead <= 0) return;
                // There might be more data, so store the data received so far.
                state.Sb.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

                // All the data has arrived; put it in response.
                if (state.Sb.Length > 1)
                {
                    _response = state.Sb.ToString();
                    state.Sb.Clear();
                    Logger.StoreLine(_response);
                    //Console.WriteLine("client recieve : " + response + "\r\n");
                }
                // Signal that all bytes have been received.
                ReceiveDone.Set();

                // Get the rest of the data.
                //client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                //  new AsyncCallback(ReceiveCallback), state);
                //else
                //{
                //    // All the data has arrived; put it in response.
                //    if (state.sb.Length > 1)
                //    {
                //        response = state.sb.ToString();
                //    }
                //    // Signal that all bytes have been received.
                //    receiveDone.Set();
                //}
            }
            catch (Exception e)
            {
                //  Console.WriteLine(e.ToString());
            }
        }

        private static void Send(Socket client, String req)
        {
            var asen = new ASCIIEncoding();
            //Console.WriteLine("client send : " + req + "\r\n");
            var ba = asen.GetBytes(req);
            var size = (short)ba.Length;
            var sizePacket = BitConverter.GetBytes(size);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(sizePacket);
            }

            var requestWithHeader = new byte[sizePacket.Length + ba.Length];
            sizePacket.CopyTo(requestWithHeader, 0);
            ba.CopyTo(requestWithHeader, sizePacket.Length);

            // Begin sending the data to the remote device.
            client.BeginSend(requestWithHeader, 0, requestWithHeader.Length, 0,

            SendCallback, client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                var client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                var bytesSent = client.EndSend(ar);

                // Signal that all bytes have been sent.
                SendDone.Set();
            }
            catch (Exception e)
            {
                //  Console.WriteLine(e.ToString());
            }
        }

        public static int Start()
        {
            StartClient();

            return 0;
        }
    }

    public class Trailer
    {
        public static float RequestsTransmitted = 0;
        public static float ResponsesReceived = 0;
        public static float ReqRunDurationMs = 0;
        public static float RspRunDurationMs = 0;
        public static float TransDurationMs = 0;
        public static float ActualReqPaceMs = 0;
        public static float ActualRspPaceMs = 0;
        public static float ConfiguredPaceMs = 0;
        public static float TransactionAvgMs = 0;
        public static string YourName = "Chase Marcum";
        public static string MyName = "";
    }

    // State object for receiving data from remote device.
    public class StateObject
    {
        // Client socket.
        public Socket WorkSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] Buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder Sb = new StringBuilder();
    }
}