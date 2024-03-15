using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
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

            QRCoderHelper qRCoderHelper = new QRCoderHelper();
            EmailHelper emailHelper = new EmailHelper(configuration, qRCoderHelper);
            MailInfo mailInfo = new MailInfo()
            {
                Subject = "�o�Osubject2118",
                EmailTo = "lolmuta@gmail.com",
                TextFormat = MimeKit.Text.TextFormat.Html,
                Text = @"

<!DOCTYPE html>
<html>
    <head>
        <title>Page Title</title>
        <style>
            .box {
                border: 2px solid #000; 
                padding: 20px; 
                width: 200px; 
                margin: 5px;
            }
            .display-flow{
                display: flex;
                flex-wrap: wrap;
                margin: 20px;
            }
        </style>
    </head>
    <body>
        <div>�˷R�� ${userInfo.Name} �z�n,</div>
        <div> �z�ʶR�������@ {ticketsInfo.Count()}�i</div>
        <div>�ԲӸ�Ʀp�U:</div>
        <div class='display-flow'>
            <div class='box'>
                <div>title</div>
                <div>description</div>
                <div>date</div>
                <img src='cid:aa' alt='QR Code'>
            </div>
            <div class='box'>
                <div>title</div>
                <div>description</div>
                <div>date</div>
                <img src='cid:bb' alt='QR Code'>
            </div>
        </div>
    </body>
</html>
                ",
                imageAttrs = new List<ImageAttr>()
                {
                    new ImageAttr 
                    { 
                        bytes = File.ReadAllBytes(@"D:\career_form\��\qrcode\1.png"),
                        fileName = "1",
                        contentId = "aa"
                    },
                    new ImageAttr 
                    { 
                        bytes = File.ReadAllBytes(@"D:\career_form\��\qrcode\2.png"),
                        fileName = "2",
                        contentId = "bb"
                    },
                }
            };

            var test = emailHelper.Send(mailInfo);
            TestContext.WriteLine(test);
            Assert.IsEmpty(test);

        }
        [Test]
        public void TestQRCoderHelper()
        {
            QRCoderHelper qRCoderHelper = new QRCoderHelper();
            var bytes = qRCoderHelper.GetQRCodePngBytes("asdfsdfsadf");
            File.WriteAllBytes(@"D:\career_form\��\qrcode\2.png", bytes);
        }
    }
}