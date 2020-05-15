using Microsoft.Extensions.Configuration;
using Olive.Mvc;

namespace Olive.BlobAws
{
    class S3FileUploadMarkupGenerator : DefaultFileUploadMarkupGenerator
    {
        readonly FileUploadSettings Settings;

        public S3FileUploadMarkupGenerator(FileUploadSettings settings) => Settings = settings;

        protected override string GetHtmlAttributes(object htmlAttributes)
        {
            var s3Url = $" data-s3-url=\"{S3DocumentUrlBuilder.GetFileUploadUrl(Settings.Region, Settings.BucketName)}\"";
            return base.GetHtmlAttributes(htmlAttributes) + s3Url;
        }
    }
}