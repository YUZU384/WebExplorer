using QRCoder;
using System.Drawing;

namespace WebExplorer.Utils
{
    /// <summary>
    /// 基于 QRCoder 库的 QR 码生成器
    /// </summary>
    public static class QRCodeGenerator
    {
        /// <summary>
        /// 生成 QR 码 Bitmap 图像（可直接赋值给 PictureBox.Image）
        /// </summary>
        /// <param name="text">要编码的文本（通常是 URL）</param>
        /// <param name="pixelSize">每个模块的像素大小，建议 4-8</param>
        public static Bitmap Generate(string text, int pixelSize = 6)
        {
            using var qrGen = new global::QRCoder.QRCodeGenerator();
            var qrData = qrGen.CreateQrCode(text, global::QRCoder.QRCodeGenerator.ECCLevel.M);
            using var qrCode = new global::QRCoder.QRCode(qrData);
            return qrCode.GetGraphic(pixelSize);
        }
    }
}
