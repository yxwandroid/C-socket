using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] bytes = new Byte[1024];
            IPAddress ipAddress = IPAddress.Parse("192.168.1.61");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8096);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(100);
            listener.BeginAccept(new AsyncCallback(AcceptCallback),listener);
            Console.WriteLine("\nPress ENTER to continue");
            Console.Read();
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }


        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);
                handler.Send(Encoding.ASCII.GetBytes("2<EOF>"));

                // Not all data received. Get more.
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }

        }

    }

    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }
}
