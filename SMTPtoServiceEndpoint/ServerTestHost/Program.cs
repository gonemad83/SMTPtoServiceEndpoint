using Gonemad.SMTPtoServiceEndpoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerTestHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new SMTPListener();

            Console.WriteLine("Server Started");
            server.MailReceived += server_MailReceived;

            server.StartServerAsync("Gonemad.SMTPtoServiceEndpoint", IPAddress.Any, 25, 10);

            Console.WriteLine("Press any key to stop the server...");
            Console.Read();
            Console.WriteLine("Stopping the server...");

            try
            {
                server.StopServer(true);
            }
            catch (Exception)
            {
                
                throw;
            }
            
            Console.WriteLine("Server Stopped");
            Console.WriteLine("Press any key to exit...");
            Console.Read();
            Console.Read();
            Console.Read();

        }

        private static void server_MailReceived(object sender, SMTPClientHandlerNewMailItemEventArgs e)
        {
            Console.WriteLine(string.Format("New Email: {0}", e.Message.Subject));
        }
    }
}
