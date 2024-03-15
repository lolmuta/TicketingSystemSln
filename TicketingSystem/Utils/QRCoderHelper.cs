using QRCoder;

namespace TicketingSystem.Utils
{
    public class QRCoderHelper
    {
        public byte[] GetQRCodePngBytes(string text)
        {
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode(text, QRCodeGenerator.ECCLevel.L);
            var pngCodeGfx = new PngByteQRCode(data).GetGraphic(5);
            return pngCodeGfx;
        }
        public string GetQRCodePngBase64(string text)
        {
            var bytes = GetQRCodePngBytes(text);
            return Convert.ToBase64String(bytes);
        }

    }
}
