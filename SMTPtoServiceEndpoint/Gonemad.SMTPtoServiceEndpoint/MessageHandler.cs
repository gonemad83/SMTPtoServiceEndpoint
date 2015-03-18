using Gonemad.SMTPtoServiceEndpoint.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Gonemad.SMTPtoServiceEndpoint
{
    public class MessageHandler
    {
        private MailMessage _email;

        public static void StartNew(MailMessage email)
        {
            var handler = new MessageHandler();
            handler._email = email;
            handler.HandleAsync();
        }

        protected virtual async void HandleAsync()
        {
            try
            {
                // Parse message body
                var msg = ParseMessage();

                // Check connection strings for message end point
                var factory = new ChannelFactory<IMessageReceiverService>(msg.ServiceName);
                var service = factory.CreateChannel();
                
                // Send
                await service.ProcessAsync(msg);
            }
            catch (Exception ex)
            {
                // $TODO: add logging service support
                Console.WriteLine(ex.Message);
            }
        }

        protected Message ParseMessage()
        {
            // extract xml
            var body = this._email.Body;

            var startTag = "<message";
            var endTag = "</message>";

            if (string.IsNullOrEmpty(body) || !body.Contains(startTag))
            {
                throw new Exception("Message data was not in the correct format.");
            }

            var startIndex = body.IndexOf(startTag);
            var endIndex = body.IndexOf(endTag) + endTag.Length;

            var xml = body.Substring(startIndex, endIndex - startIndex);

            // deserialize
            var serializer = new XmlSerializer(typeof(Message));
            object result;

            using (TextReader reader = new StringReader(xml))
            {
                result = serializer.Deserialize(reader);
            }

            return (Message)result;
            // done
        }
    }
}
