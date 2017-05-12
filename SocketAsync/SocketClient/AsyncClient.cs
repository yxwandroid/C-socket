using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketClient
{
    /// <summary>  
    /// 异步客户端  
    /// </summary>  
    public class AsyncClient : IDisposable
    {
        #region Fields  

        private Socket _client;
        private bool disposed = false;
        private int retries = 0;

        #endregion

        #region Properties  

        /// <summary>  
        /// 是否已与服务器建立连接  
        /// </summary>  
        public bool Connected { get { return _client.Connected; } }
        /// <summary>  
        /// 远端服务器的IP地址列表  
        /// </summary>  
        public IPAddress[] Addresses { get; private set; }
        /// <summary>  
        /// 远端服务器的端口  
        /// </summary>  
        public int Port { get; private set; }
        /// <summary>  
        /// 连接重试次数  
        /// </summary>  
        public int Retries { get; set; }
        /// <summary>  
        /// 连接重试间隔  
        /// </summary>  
        public int RetryInterval { get; set; }
        /// <summary>  
        /// 远端服务器终结点  
        /// </summary>  
        public IPEndPoint RemoteIPEndPoint
        {
            get { return new IPEndPoint(Addresses[0], Port); }
        }
        /// <summary>  
        /// 本地客户端终结点  
        /// </summary>  
        protected IPEndPoint LocalIPEndPoint { get; private set; }
        /// <summary>  
        /// 通信所使用的编码  
        /// </summary>  
        public Encoding Encoding { get; set; }

        #endregion

        #region 构造函数  

        /// <summary>  
        /// 异步TCP客户端  
        /// </summary>  
        /// <param name="remoteEP">远端服务器终结点</param>  
        public AsyncClient(IPEndPoint remoteEP)
            : this(new[] { remoteEP.Address }, remoteEP.Port)
        {
        }
        /// <summary>  
        /// 异步TCP客户端  
        /// </summary>  
        /// <param name="remoteEP">远端服务器终结点</param>  
        /// <param name="localEP">本地客户端终结点</param>  
        public AsyncClient(IPEndPoint remoteEP, IPEndPoint localEP)
            : this(new[] { remoteEP.Address }, remoteEP.Port, localEP)
        {
        }

        /// <summary>  
        /// 异步TCP客户端  
        /// </summary>  
        /// <param name="remoteIPAddress">远端服务器IP地址</param>  
        /// <param name="remotePort">远端服务器端口</param>  
        public AsyncClient(IPAddress remoteIPAddress, int remotePort)
            : this(new[] { remoteIPAddress }, remotePort)
        {
        }

        /// <summary>  
        /// 异步TCP客户端  
        /// </summary>  
        /// <param name="remoteIPAddress">远端服务器IP地址</param>  
        /// <param name="remotePort">远端服务器端口</param>  
        /// <param name="localEP">本地客户端终结点</param>  
        public AsyncClient(
          IPAddress remoteIPAddress, int remotePort, IPEndPoint localEP)
            : this(new[] { remoteIPAddress }, remotePort, localEP)
        {
        }

        /// <summary>  
        /// 异步TCP客户端  
        /// </summary>  
        /// <param name="remoteHostName">远端服务器主机名</param>  
        /// <param name="remotePort">远端服务器端口</param>  
        public AsyncClient(string remoteHostName, int remotePort)
            : this(Dns.GetHostAddresses(remoteHostName), remotePort)
        {
        }

        /// <summary>  
        /// 异步TCP客户端  
        /// </summary>  
        /// <param name="remoteHostName">远端服务器主机名</param>  
        /// <param name="remotePort">远端服务器端口</param>  
        /// <param name="localEP">本地客户端终结点</param>  
        public AsyncClient(
          string remoteHostName, int remotePort, IPEndPoint localEP)
            : this(Dns.GetHostAddresses(remoteHostName), remotePort, localEP)
        {
        }

        /// <summary>  
        /// 异步TCP客户端  
        /// </summary>  
        /// <param name="remoteIPAddresses">远端服务器IP地址列表</param>  
        /// <param name="remotePort">远端服务器端口</param>  
        public AsyncClient(IPAddress[] remoteIPAddresses, int remotePort)
            : this(remoteIPAddresses, remotePort, null)
        {
        }

        /// <summary>  
        /// 异步TCP客户端  
        /// </summary>  
        /// <param name="remoteIPAddresses">远端服务器IP地址列表</param>  
        /// <param name="remotePort">远端服务器端口</param>  
        /// <param name="localEP">本地客户端终结点</param>  
        public AsyncClient(
          IPAddress[] remoteIPAddresses, int remotePort, IPEndPoint localEP)
        {
            this.Addresses = remoteIPAddresses;
            this.Port = remotePort;
            this.LocalIPEndPoint = localEP;
            this.Encoding = Encoding.Default;

            if (this.LocalIPEndPoint != null)
            {
                _client = new Socket(LocalIPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }

            Retries = 3;
            RetryInterval = 5;
        }

        #endregion

        #region Connect  
        /// <summary>  
        /// 连接到服务器  
        /// </summary>  
        /// <returns>异步TCP客户端</returns>  
        public AsyncClient Connect()
        {
            if (!Connected)
            {
                // start the async connect operation  
                _client.BeginConnect(
                   Addresses, Port, HandleTcpServerConnected, _client);
            }

            return this;
        }
        /// <summary>  
        /// 关闭与服务器的连接  
        /// </summary>  
        /// <returns>异步TCP客户端</returns>  
        public AsyncClient Close()
        {
            if (Connected)
            {
                retries = 0;
                _client.Close();
                RaiseServerDisconnected(Addresses, Port);
            }

            return this;
        }

        #endregion

        #region Receive  

        private void HandleTcpServerConnected(IAsyncResult ar)
        {
            try
            {
               
                _client.EndConnect(ar);
                RaiseServerConnected(Addresses, Port);
                retries = 0;
            }
            catch (Exception ex)
            {

            }
            // we are connected successfully and start asyn read operation.  
            //byte[] buffer = new byte[_client.ReceiveBufferSize];
            //_client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, HandleDatagramReceived, buffer);
        }

        private void HandleDatagramReceived(IAsyncResult ar)
        {
            try
            {
                int recv = _client.EndReceive(ar);

                byte[] buffer = (byte[])ar.AsyncState;

                byte[] receivedBytes = new byte[recv];

                Buffer.BlockCopy(buffer, 0, receivedBytes, 0, recv);

                RaiseDatagramReceived(_client, receivedBytes);

                // then start reading from the network again  
                _client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, HandleDatagramReceived, buffer);
            }
            catch (Exception)
            {

            }
        }

        #endregion

        #region Events  

        /// <summary>  
        /// 接收到数据报文事件  
        /// </summary>  
        public event EventHandler<EventArgs> DatagramReceived;

        private void RaiseDatagramReceived(Socket sender, byte[] datagram)
        {
            if (DatagramReceived != null)
            {
                DatagramReceived(this, new EventArgs());
            }
        }

        /// <summary>  
        /// 与服务器的连接已建立事件  
        /// </summary>  
        public event EventHandler<EventArgs> ServerConnected;
        /// <summary>  
        /// 与服务器的连接已断开事件  
        /// </summary>  
        public event EventHandler<EventArgs> ServerDisconnected;
        /// <summary>  
        /// 与服务器的连接发生异常事件  
        /// </summary>  
        public event EventHandler<EventArgs> ServerExceptionOccurred;

        private void RaiseServerConnected(IPAddress[] ipAddresses, int port)
        {
            if (ServerConnected != null)
            {
                ServerConnected(this, new EventArgs());
            }
        }

        private void RaiseServerDisconnected(IPAddress[] ipAddresses, int port)
        {
            if (ServerDisconnected != null)
            {
                ServerDisconnected(this, new EventArgs());
            }
        }

        private void RaiseServerExceptionOccurred(
          IPAddress[] ipAddresses, int port, Exception innerException)
        {
            if (ServerExceptionOccurred != null)
            {
                ServerExceptionOccurred(this, new EventArgs());
            }
        }

        #endregion

        #region Send  

        /// <summary>  
        /// 发送报文  
        /// </summary>  
        /// <param name="datagram">报文</param>  
        public void Send(byte[] datagram)
        {
            if (datagram == null)
                throw new ArgumentNullException("datagram");

            if (!Connected)
            {
                RaiseServerDisconnected(Addresses, Port);
                throw new InvalidProgramException(
                  "This client has not connected to server.");
            }

            _client.BeginSend(datagram, 0, datagram.Length, SocketFlags.None, HandleDataSend, _client);
        }

        private void HandleDataSend(IAsyncResult ar)
        {
            ((Socket)ar.AsyncState).EndSend(ar);
        }

        /// <summary>  
        /// 发送报文  
        /// </summary>  
        /// <param name="datagram">报文</param>  
        public void Send(string datagram)
        {
            Send(this.Encoding.GetBytes(datagram));
        }

        #endregion

        #region IDisposable Members  

        /// <summary>  
        /// Performs application-defined tasks associated with freeing,   
        /// releasing, or resetting unmanaged resources.  
        /// </summary>  
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>  
        /// Releases unmanaged and - optionally - managed resources  
        /// </summary>  
        /// <param name="disposing"><c>true</c> to release both managed   
        /// and unmanaged resources; <c>false</c>   
        /// to release only unmanaged resources.  
        /// </param>  
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    try
                    {
                        Close();

                        if (_client != null)
                        {
                            _client = null;
                        }
                    }
                    catch (SocketException)
                    {
                    }
                }

                disposed = true;
            }
        }

        #endregion

    }
}
