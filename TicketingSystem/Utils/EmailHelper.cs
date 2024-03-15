using MimeKit;
using MimeKit.Text;
using MimeKit.Utils;
using System.Net;
using System.Text;
using TicketingSystem.Models;

namespace TicketingSystem.Utils
{
    public class EmailHelper
    {
        private readonly IConfiguration configuration;
        private readonly QRCoderHelper qRCoderHelper;
        private readonly string SenderEmail;
        private readonly string SenderPassword;
        private readonly string Host;
        private readonly int Port;
        public EmailHelper(IConfiguration configuration, QRCoderHelper qRCoderHelper)
        {
            this.configuration = configuration;
            this.qRCoderHelper = qRCoderHelper;
            SenderEmail = configuration.GetValue<string>("Gmail:SenderEmail");
            SenderPassword = configuration.GetValue<string>("Gmail:SenderPassword");
            Host = configuration.GetValue<string>("Gmail:Host");
            Port = configuration.GetValue<int>("Gmail:Port");
        }
        public MailInfo GetEmailBody(IEnumerable<TicketsInfo> ticketsInfo, UserInfo userInfo)
        {
            var now = DateTime.Now;
            //信件 qrcode
            StringBuilder ticketSb = new();

            List<ImageAttr> imageAttrs = new List<ImageAttr>();
            foreach (var eachTicket in ticketsInfo)
            {
                //base 64 沒有辦法在gmail呈現圖片
                //string htmlBase64 = "data:image/png;base64," + qRCoderHelper.GetQRCodePngBase64(eachTicket.UUID);
                imageAttrs.Add(new ImageAttr()
                {
                    bytes = qRCoderHelper.GetQRCodePngBytes(eachTicket.UUID),
                    contentId = eachTicket.UUID,
                    fileName = eachTicket.UUID + ".png"
                });
                ticketSb.AppendLine($@"
                    <div class='box'>
                        <div>{eachTicket.Title}</div>
                        <div>{eachTicket.Description}</div>
                        <div>{eachTicket.date}</div>
                        <div>{eachTicket.UUID}</div>
                        <img src='cid:{eachTicket.UUID}' alt='QR Code2'>
                    </div>
                ");
            }
            string css = @"
                .box {
                    border: 2px solid #000; 
                    padding: 20px; 
                    width: 200px; 
                    margin: 5px;
                }
                .display-flow-wrap{
                    display: flex;
                    flex-wrap: wrap;
                    margin: 20px;
                }
            ";

            string text = $@"

<!DOCTYPE html>
<html>
    <head>
        <title>Page Title</title>
        <style>
            {css}
        </style>
    </head>
    <body>
        <div>親愛的 {userInfo.Name} 您好,</div>
        <div> 您購買的票卷共 {ticketsInfo.Count()}張</div>
        <div>詳細資料如下:</div>
        <div class='display-flow-wrap'>
            {ticketSb.ToString()}
        </div>
    </body>
</html>

            ";

            MailInfo mailInfo = new MailInfo()
            {
                Subject = $"購買票卷資訊 {now.ToString("yyyy/MM/dd HH:mm:ss")}",
                EmailTo = userInfo.Email,
                TextFormat = TextFormat.Html,
                Text = text,
                imageAttrs = imageAttrs,
            };

            return mailInfo;
        }
        public string Send(string subject, string text
            , TextFormat textFormat, string emailTo
            , IEnumerable<ImageAttr> imageAttrs)
        {
            // 建立郵件消息
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("寄件人姓名", SenderEmail));
            message.To.Add(new MailboxAddress("收件人姓名", emailTo));
            message.Subject = subject;
            var multipart = new Multipart("mixed");
            var textPart = new TextPart(textFormat)
            {
                Text = text
            };
            multipart.Add(textPart);
            //圖片附件
            foreach (var imageAttr in imageAttrs)
            {
                byte[] bytes1 = imageAttr.bytes;
                string fileName1 = imageAttr.fileName;
                string contentId = imageAttr.contentId;
                var attachment = new MimePart("application", "octet-stream")
                {
                    Content = new MimeContent(new MemoryStream(bytes1), ContentEncoding.Default),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = fileName1
                };
                attachment.ContentId = contentId;
                multipart.Add(attachment);
            }

            message.Body = multipart;

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
            return Send(mailInfo.Subject, mailInfo.Text, mailInfo.TextFormat, mailInfo.EmailTo, mailInfo.imageAttrs);
        }
    }

    public class MailInfo
    {
        public string Subject { get; set; }
        public string Text { get; set; }
        public TextFormat TextFormat { get; set; }
        public string EmailTo { get; set; }
        public IEnumerable<ImageAttr> imageAttrs { get; set; }

    }
    public class ImageAttr
    {
        public byte[] bytes { get; set; }
        public string fileName { get; set; }
        public string contentId { get; set; }
    }
}
