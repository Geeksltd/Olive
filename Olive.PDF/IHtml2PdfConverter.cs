namespace Olive.PDF
{
    public interface IHtml2PdfConverter
    {
        byte[] GetPdfFromUrlBytes(string url);
    }
}
