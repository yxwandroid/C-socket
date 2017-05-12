using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer
{
    class AsyncEventArgs : EventArgs
    {
        /// <summary>  
        /// 提示信息  
        /// </summary>  
        public string _msg;

        public Session _sessions;

        /// <summary>  
        /// 是否已经处理过了  
        /// </summary>  
        public bool IsHandled { get; set; }

        public AsyncEventArgs(string msg)
        {
            this._msg = msg;
            IsHandled = false;
        }
        public AsyncEventArgs(Session session)
        {
            this._sessions = session;
            IsHandled = false;
        }
        public AsyncEventArgs(string msg, Session session)
        {
            this._msg = msg;
            this._sessions = session;
            IsHandled = false;
        }
    }
}
