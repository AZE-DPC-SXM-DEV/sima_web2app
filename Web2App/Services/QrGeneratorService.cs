using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Web2App.Interfaces;

namespace Web2App.Services
{
    public class QrGeneratorService : IQrGenerator
    {
        public async Task<string> GenerateQr(string body)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(body, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);



            string fileName = Guid.NewGuid() + ".png";
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "qrCodes", fileName);
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                qrCodeImage.Save(stream, ImageFormat.Png);
            }

            return fileName;
        }
    }
}
