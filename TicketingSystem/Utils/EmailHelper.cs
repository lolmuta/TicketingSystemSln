using MimeKit;
using MimeKit.Text;
using System.Net;
using TicketingSystem.Models;

namespace TicketingSystem.Utils
{
    public class EmailHelper
    {
        private readonly IConfiguration configuration;
        private readonly string SenderEmail;
        private readonly string SenderPassword ;
        private readonly string Host;
        private readonly int Port;
        public EmailHelper(IConfiguration configuration)
        {
            this.configuration = configuration;
            SenderEmail = configuration.GetValue<string>("Gmail:SenderEmail");
            SenderPassword = configuration.GetValue<string>("Gmail:SenderPassword");
            Host = configuration.GetValue<string>("Gmail:Host");
            Port = configuration.GetValue<int>("Gmail:Port");
        }
        public MailInfo GetEmailBody(IEnumerable<TicketsInfo> ticketsInfo, UserInfo userInfo)
        {
            //todo 信件 qrcode
            MailInfo mailInfo = new MailInfo()
            {
                Subject = "購買票卷成功",
                EmailTo = userInfo.Email,
                TextFormat = MimeKit.Text.TextFormat.Html,
                Text = @"<html>
                        <body>
                            <p>這是測試用的啦。</p>
                            <img src='data:image/png;base64,{qrCodeBase64}' alt='QR Code'>
                        </body>
                        </html>
                "
            };
            return mailInfo;
        }
        public string Send2(string subject, string text
            , TextFormat textFormat, string emailTo)
        {
            // 建立郵件消息
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("寄件人姓名", SenderEmail));
            message.To.Add(new MailboxAddress("收件人姓名", emailTo));
            message.Subject = subject;
            message.Body = new TextPart(textFormat)
            {
                Text = text
            };

            // 連接到 SMTP 服務器並發送郵件
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect(Host, Port, false); // 修改為您的 SMTP 服務器地址和端口號
                client.Authenticate(SenderEmail, SenderPassword); // 使用您的帳戶名稱和密碼進行身份驗證
                client.Send(message);
                client.Disconnect(true);
            }
            return "";
        }
        public string Send(MailInfo mailInfo)
        {
            return Send2(mailInfo.Subject, mailInfo.Text, mailInfo.TextFormat, mailInfo.EmailTo);
        }
    }

    public class MailInfo
    {
        public string Subject { get; set; }
        public string Text { get; set; }
        public TextFormat TextFormat { get; set; }
        public string EmailTo { get; set; }
    }
}
