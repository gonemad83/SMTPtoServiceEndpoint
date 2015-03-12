using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gonemad.SMTPtoServiceEndpoint
{
    public class SMTPListener : IDisposable
    {

        #region Attributes

        private TcpListener _tcpListener;
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;
        private string _serverName;

        #endregion

        #region Events

        public event EventHandler<SMTPClientHandlerNewMailItemEventArgs> MailReceived;

        #endregion

        #region Constructors

        public SMTPListener()
        {
            
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the SMTP Server
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="maxConnections"></param>
        /// <returns></returns>
        public async Task StartServerAsync(string serverName, IPAddress ip, int port, int maxConnections)
        {
            this._serverName = serverName;

            this._cancellationTokenSource = new CancellationTokenSource();
            this._cancellationToken = this._cancellationTokenSource.Token;

            this._tcpListener = new TcpListener(ip, port);
            this._tcpListener.Start(maxConnections);

            await AcceptSocketsAsync();
        }

        public void StopServer(bool finishProcessingRequests)
        {
            this._cancellationTokenSource.Cancel();
            
            if(finishProcessingRequests)
            {
                while(this._tcpListener.Pending())
                {
                    // do nothing and keep waiting
                }
                
                this._tcpListener.Stop();
            }
            else
            {
                this._tcpListener.Stop();
            }
            
        }

        #endregion

        public void Dispose()
        {
            // TODO: Cancel/Close all connections

            if (this._tcpListener != null)
            {
                this._tcpListener.Stop();
                this._tcpListener = null;
            }
        }

        #region Private Methods

        private async Task AcceptSocketsAsync()
        {
            while (true)
            {
                var tcpClient = await this._tcpListener.AcceptTcpClientAsync();
                var handler = new SMTPClientHandler(tcpClient, this._serverName);
                handler.MailReceived += handler_MailReceived;

                if(this._cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private void handler_MailReceived(object sender, SMTPClientHandlerNewMailItemEventArgs e)
        {
            if (this.MailReceived != null)
                this.MailReceived(this, e);
        }

        #endregion

    }
}
