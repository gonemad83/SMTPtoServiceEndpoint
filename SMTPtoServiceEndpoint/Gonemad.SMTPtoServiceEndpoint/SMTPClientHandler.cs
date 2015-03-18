using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Gonemad.SMTPtoServiceEndpoint
{

    public class SMTPClientHandler : IDisposable
    {
        #region Attributes

        NetworkStream _stream;
        StreamReader _reader;
        StreamWriter _writer;
        TcpClient _client;
        bool _processing;
        string _serverName;
        System.Net.Mail.MailMessage _message;

        #endregion

        #region Constructors

        public SMTPClientHandler(TcpClient tcpClient, string serverName)
        {
            this._client = tcpClient;
            this._stream = this._client.GetStream();
            this._reader = new StreamReader(this._stream, Encoding.ASCII, false, 8192);
            this._writer = new StreamWriter(this._stream, Encoding.ASCII);
            this._writer.NewLine = "\r\n";
            this._processing = true;
            this._serverName = serverName;

            ProcessMailRequestAsync();
        }

        #endregion

        private async Task WriteAsync(string strMessage)
        {
            //ASCIIEncoding encoder = new ASCIIEncoding();
            //byte[] buffer = encoder.GetBytes(strMessage + "\r\n");

            //await this._stream.WriteAsync(buffer, 0, buffer.Length);
            //await this._stream.FlushAsync();

            await this._writer.WriteLineAsync(strMessage);
            await this._writer.FlushAsync();
        }

        #region Events

        public event EventHandler<SMTPClientHandlerNewMailItemEventArgs> MailReceived;

        #endregion

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._reader != null)
                    this._reader.Dispose();

                if (this._writer != null)
                    this._writer.Dispose();

                if (this._stream != null)
                    this._stream.Dispose();

                if (this._client != null)
                    this._client.Close();
            }
        }

        ~SMTPClientHandler()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Public Methods

        public async void ProcessMailRequestAsync()
        {
            await WriteAsync(string.Format("220 {0} Simple Mail Transfer Service Ready", this._serverName));
            try
            {
                while (true)
                {
                    var msg = string.Empty;
                    try
                    {
                        msg = await this._reader.ReadLineAsync();
                    }
                    catch (Exception e)
                    {
                        this._client.Close();
                        break;
                    }

                    if (msg.Length >= 4)
                    {
                        switch (msg.Substring(0, 4))
                        {
                            case SMTP_HELO:
                            case SMTP_EHLO:
                                await ProcessHELOAsync(msg);
                                break;
                            case SMTP_MAIL:
                                await ProcessMAILAsync(msg);
                                break;
                            case SMTP_RCPT:
                                await ProcessRCPTAsync(msg);
                                break;
                            case SMTP_DATA:
                                await ProcessDATAAsync(msg);
                                break;
                            case SMTP_NOOP:
                                await ProcessNOOPAsync(msg);
                                break;
                            case SMTP_RSET:
                                await ProcessRSETAsync(msg);
                                break;
                            case SMTP_QUIT:
                                await ProcessQUITAsync();
                                return;
                            default:
                                this._client.Close();
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Most likely quit
            }

            if (this._message != null && this.MailReceived != null)
                this.MailReceived(this, new SMTPClientHandlerNewMailItemEventArgs() { Message = this._message });
        }

        #endregion

        protected async Task ProcessQUITAsync()
        {
            await WriteAsync(string.Format("221 {0} Service closing transmission channel", this._serverName));
            this._client.Close();
        }

        protected async Task ProcessDATAAsync(string msg)
        {
            await WriteAsync("354 Start mail input; end with <CRLF>.<CRLF>");

            var eml = new StringBuilder();

            while (true)
            {
                var dataLine = await this._reader.ReadLineAsync();

                if (dataLine == "..")
                    eml.AppendLine(".");
                else if (dataLine == ".")
                    break;
                else
                    eml.AppendLine(dataLine);
            }

            await WriteAsync("250 OK");

            this._message = Amende.Snorre.MailMessageMimeParser.ParseMessage(new StringReader(eml.ToString()));

        }

        protected async Task ProcessMAILAsync(string msg)
        {
            await RespondOkAsync();
        }

        protected async Task ProcessHELOAsync(string msg)
        {
            await RespondOkAsync();
        }

        protected async Task ProcessRCPTAsync(string msg)
        {
            await RespondOkAsync();
        }

        protected async Task ProcessRSETAsync(string msg)
        {
            await RespondOkAsync();
        }

        protected async Task ProcessNOOPAsync(string msg)
        {
            await RespondOkAsync();
        }

        protected async Task RespondOkAsync()
        {
            await WriteAsync("250 OK");
        }

        #region Messages

        private const string SMTP_QUIT = "QUIT";
        private const string SMTP_HELO = "HELO";
        private const string SMTP_EHLO = "EHLO";
        private const string SMTP_RCPT = "RCPT";
        private const string SMTP_MAIL = "MAIL";
        private const string SMTP_DATA = "DATA";

        private const string SMTP_RSET = "RSET";
        private const string SMTP_NOOP = "NOOP";

        #endregion
    }
}
