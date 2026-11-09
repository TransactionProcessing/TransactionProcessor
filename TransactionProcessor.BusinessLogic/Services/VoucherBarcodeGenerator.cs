using System;
using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.SkiaSharp;

namespace TransactionProcessor.BusinessLogic.Services;

public static class VoucherBarcodeGenerator
{
    private const Int32 ImageWidth = 300;
    private const Int32 ImageHeight = 150;
    private const Int32 QuietZone = 10;

    public static String GenerateBase64Png(String voucherCode)
    {
        BarcodeWriter barcodeWriter = new()
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width = ImageWidth,
                Height = ImageHeight,
                Margin = QuietZone,
                PureBarcode = true
            }
        };

        using SKBitmap bitmap = barcodeWriter.Write(voucherCode);
        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData png = image.Encode(SKEncodedImageFormat.Png, 100);

        return Convert.ToBase64String(png.ToArray());
    }
}
