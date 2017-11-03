namespace Olive.Services
{
    public interface IHtml2PdfConverter
    {
        byte[] GetPdfFromUrlBytes(string url);
    }
}
