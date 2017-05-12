using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string msg = "";
            


            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(ipep);
            clientSocket.Send(Encoding.ASCII.GetBytes("test message<EOF>"));
            clientSocket.Shutdown(SocketShutdown.Both);

            clientSocket.Close();
        }
    }
}
