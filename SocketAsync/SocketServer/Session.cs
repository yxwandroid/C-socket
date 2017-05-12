using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer
{
    /// <summary>  
    /// 客户端与服务器之间的会话类  
    /// </summary>  
    public class Session
    {
        #region 字段  
        /// <summary>  
        /// 接收数据缓冲区  
        /// </summary>  
        private byte[] _recvBuffer;

        /// <summary>  
        /// 客户端发送到服务器的报文  
        /// 注意:在有些情况下报文可能只是报文的片断而不完整  
        /// </summary>  
        private string _datagram;

        /// <summary>  
        /// 客户端的Socket  
        /// </summary>  
        private Socket _clientSock;

        #endregion

        #region 属性  

        /// <summary>  
        /// 接收数据缓冲区   
        /// </summary>  
        public byte[] RecvDataBuffer
        {
            get
            {
                return _recvBuffer;
            }
            set
            {
                _recvBuffer = value;
            }
        }

        /// <summary>  
        /// 存取会话的报文  
        /// </summary>  
        public string Datagram
        {
            get
            {
                return _datagram;
            }
            set
            {
                _datagram = value;
            }
        }

        /// <summary>  
        /// 获得与客户端会话关联的Socket对象  
        /// </summary>  
        public Socket ClientSocket
        {
            get
            {
                return _clientSock;

            }
        }


        #endregion

        /// <summary>  
        /// 构造函数  
        /// </summary>  
        /// <param name="cliSock">会话使用的Socket连接</param>  
        public Session(Socket cliSock)
        {

            _clientSock = cliSock;
        }
        /// <summary>  
        /// 关闭会话  
        /// </summary>  
        public void Close()
        {

            //关闭数据的接受和发送  
            _clientSock.Shutdown(SocketShutdown.Both);

            //清理资源  
            _clientSock.Close();
        }
    }
}
