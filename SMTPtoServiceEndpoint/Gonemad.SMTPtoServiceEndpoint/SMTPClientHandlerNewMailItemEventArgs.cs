using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gonemad.SMTPtoServiceEndpoint
{
    public class SMTPClientHandlerNewMailItemEventArgs : EventArgs
    {
        public System.Net.Mail.MailMessage Message { get; set; }
    }
}
