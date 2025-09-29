using Olive.Drawing;
using SkiaSharp;
namespace Olive.Drawing.Test;


[TestClass]
public sealed class ImageOptimizerTest
{

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void OptimizeTest_Webp()
    {
        var stream = this.GetType().Assembly.GetManifestResourceStream("Olive.Drawing.Test.Data.TestImage.webp");
        var data = stream.ReadAllBytes();
        var optimizer = new ImageOptimizer(1200, 675, 75);
        var output = optimizer.Optimize(data,"webp", toJpeg: false);
        File.WriteAllBytes(Path.Combine(TestContext.TestResultsDirectory.Or(AppDomain.CurrentDomain.BaseDirectory), "TestImage1_Optimized.webp"), output);
        var img = SKBitmap.Decode(output);
        Assert.IsTrue(img.Width <= 1200);
        Assert.IsTrue(img.Height <= 675);
    }
}
