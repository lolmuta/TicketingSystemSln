using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using TicketingSystem.Utils;

namespace TicketingSystem.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestEmailHelper()
        {
            IConfiguration configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .Build();
            EmailHelper emailHelper = new EmailHelper(configuration);
            MailInfo mailInfo = new MailInfo()
            {
                Subject = "這是subject",
                EmailTo = "lolmuta@gmail.com",
                TextFormat = MimeKit.Text.TextFormat.Html,
                Text = @"<html>
                        <body>
                            <p>這是測試用的啦。</p>
                            <img src='data:image/png;base64,{qrCodeBase64}' alt='QR Code'>
                        </body>
                        </html>
                "
            };
            var test = emailHelper.Send(mailInfo);
            TestContext.WriteLine(test);
            Assert.IsEmpty(test);

        }
    }
}