using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Gonemad.SMTPtoServiceEndpoint
{
    public interface IMessageHandler
    {
        Task HandleMessageAsync(MailMessage message);
    }
}
