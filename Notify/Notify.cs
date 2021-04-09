using System;
using MailKit.Net.Smtp;
using MimeKit;

namespace DORA.Notify
{
    public class EMailSender
    {
        public string Address { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        private class ToEmail
        {
            public string Address { get; set; }
            public string Subject { get; set; }
            public string Content { get; set; }
        }

        public bool MessageTo(string ToAddress, string subject, string content)
        {
            bool success = false;

            ToEmail toEmail = new ToEmail()
            {
                Address = ToAddress,
                Subject = subject,
                Content = content
            };

            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    if (!smtpClient.IsConnected)
                    {
                        smtpClient.Connect(this.SmtpServer, this.Port, true);
                        smtpClient.Authenticate(this.UserName, this.Password);
                    }

                    smtpClient.Send(this.CreateMimeMessage(toEmail));
                    success = true;

                    if (smtpClient.IsConnected)
                        smtpClient.Disconnect(true);
                }

            }
            catch (Exception e)
            {
                success = false;
            }

            return success;
        }

        private MimeMessage CreateMimeMessage(ToEmail toEmail)
        {
            MimeMessage mimeMessage = new MimeMessage();
            BodyBuilder builder = new BodyBuilder();

            builder.HtmlBody = toEmail.Content;

            mimeMessage.From.Add(new MailboxAddress(this.Address));
            mimeMessage.To.Add(new MailboxAddress(toEmail.Address));
            mimeMessage.Subject = toEmail.Subject;
            mimeMessage.Body = builder.ToMessageBody();

            return mimeMessage;
        }
    }
}
