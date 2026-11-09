using System;
using Shouldly;
using SkiaSharp;
using TransactionProcessor.BusinessLogic.Services;
using Xunit;
using ZXing;
using ZXing.SkiaSharp;

namespace TransactionProcessor.BusinessLogic.Tests.Services;

public class VoucherBarcodeGeneratorTests
{
    [Fact]
    public void GenerateBase64Png_CreatesCode128PngContainingVoucherCode()
    {
        const string voucherCode = "ABCDE1234";

        byte[] png = Convert.FromBase64String(VoucherBarcodeGenerator.GenerateBase64Png(voucherCode));

        png[..8].ShouldBe(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });

        using SKBitmap bitmap = SKBitmap.Decode(png);
        Result result = new BarcodeReader().Decode(bitmap);

        result.ShouldNotBeNull();
        result.BarcodeFormat.ShouldBe(BarcodeFormat.CODE_128);
        result.Text.ShouldBe(voucherCode);
    }
}
