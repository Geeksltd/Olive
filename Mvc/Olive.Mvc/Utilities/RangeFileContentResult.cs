namespace Olive.Mvc
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Olive.Entities;

    public class RangeFileContentResult : RangeFileResult
    {
        public byte[] FileContents { get; private set; }

        public RangeFileContentResult(byte[] fileContents, string contentType, string fileName, DateTime modificationDate)
            : base(contentType, fileName, modificationDate, fileContents.Length) =>
            FileContents = fileContents ?? throw new ArgumentNullException(nameof(fileContents));

        public static async Task<RangeFileContentResult> From(Blob blob)
        {
            var data = await blob.GetFileDataAsync();
            var mime = blob.GetMimeType();
            return new RangeFileContentResult(data, mime, blob.FileName, LocalTime.Now);
        }

        protected override void WriteEntireEntity(HttpResponse response) =>
            response.Body.Write(FileContents, 0, FileContents.Length);

        protected override void WriteEntityRange(HttpResponse response, long rangeStartIndex, long rangeEndIndex)
        {
            response.Body.Write(FileContents, Convert.ToInt32(rangeStartIndex), Convert.ToInt32(rangeEndIndex - rangeStartIndex) + 1);
        }
    }
}