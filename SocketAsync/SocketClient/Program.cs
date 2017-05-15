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
            


            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("192.168.1.61"), 8096);
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(ipep);
            clientSocket.Send(Encoding.ASCII.GetBytes("1<EOF>"));


            while (true) {
                byte[] data = new byte[1024];//一次接收数据存放buf，最大1024个
                int bufLen;//一次接收到的数据的长度
                           //***阻塞执行，等待直到接收buf收到字符，否则一直阻塞到此处
                bufLen = clientSocket.Available;  //能读到的数据个数        
                clientSocket.Receive(data, 0, bufLen, SocketFlags.None);
                string rtbRxStr = System.Text.Encoding.ASCII.GetString(data).Substring(0, bufLen);

                Console.WriteLine("----—-Receive—————" + rtbRxStr);

            }
            Console.ReadLine();

            //  clientSocket.Shutdown(SocketShutdown.Both);
        
            //  clientSocket.Close();
        }
    }
}
