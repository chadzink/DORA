using System;
using System.Collections.Generic;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace DORA.Notify
{
    public class EMailSender
    {
        public string Address { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public EMailSender(string fromAddress, IConfiguration configuration)
        {
            this.Address = fromAddress;
            this.SmtpServer = configuration["NotificationSMTP:Server"];
            int port = -1;

            if (int.TryParse(configuration["NotificationSMTP:Port"], out port))
                Port = port;

            this.UserName = configuration["NotificationSMTP:UserName"];
            this.Password = configuration["NotificationSMTP:Password"];

            this.UseSSL = configuration["NotificationSMTP:UseSSL"] != null
                && (new List<string> {"yes", "y", "true", "t"}).Contains(
                    configuration["NotificationSMTP:UseSSL"].ToLower()
                );
        }

        public EMailSender(string fromAddress, string smtpServer, int port, string username, string password, bool useSSL)
        {
            this.Address = fromAddress;
            this.SmtpServer = smtpServer;
            this.Port = port;
            this.UserName = username;
            this.Password = password;
            this.UseSSL = useSSL;
        }

        private class ToEmail
        {
            public string Address { get; set; }
            public string Subject { get; set; }
            public string Content { get; set; }
        }

        public bool MessageTo(string ToAddress, string subject, string content, out string errorMessage)
        {
            bool success = false;
            errorMessage = null;

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
                        smtpClient.Connect(this.SmtpServer, this.Port, this.UseSSL);
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
                errorMessage = e.Message;
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
