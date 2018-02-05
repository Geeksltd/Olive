using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Olive.Web;

namespace Olive.Mvc
{
    public abstract class RangeFileResult : ActionResult
    {
        static string[] DateFormats = new string[] { "r", "dddd, dd-MMM-yy HH':'mm':'ss 'GMT'", "ddd MMM d HH':'mm':'ss yyyy" };

        const int HTTP_STATUS_CODE_OK = 200;
        const int HTTP_STATUS_CODE_PARTIAL_CONTENT = 206;
        const int HTTP_STATUS_CODE_PAYLOAD_TO_LONG = 413;
        const int HTTP_STATUS_CODE_BAD_REQUEST = 400;
        const int HTTP_STATUS_CODE_PRECONDITION_FAILED = 412;
        const int HTTP_STATUS_CODE_NOT_MODIFIED = 304;

        const int ADDITIONAL_CONTENT_LENGTH = 49;

        DateTime HttpModificationDate;
        string EntityTag;
        long[] RangesStartIndexes, RangesEndIndexes;
        bool RangeRequest, MultipartRequest;

        protected RangeFileResult(string contentType, string fileName, DateTime modificationDate, long fileLength)
        {
            if (contentType.IsEmpty()) throw new ArgumentNullException(nameof(contentType));

            ContentType = contentType;
            FileName = fileName;
            FileModificationDate = modificationDate;
            HttpModificationDate = modificationDate.ToUniversal();
            HttpModificationDate = new DateTime(HttpModificationDate.Year, HttpModificationDate.Month, HttpModificationDate.Day, HttpModificationDate.Hour, HttpModificationDate.Minute, HttpModificationDate.Second, DateTimeKind.Utc);
            FileLength = fileLength;
        }

        protected virtual string GenerateEntityTag(ActionContext context)
        {
            var entityTagBytes = Encoding.ASCII.GetBytes($"{FileName}|{FileModificationDate}");
            return Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(entityTagBytes));
        }

        public string ContentType { get; private set; }

        public string FileName { get; private set; }

        public DateTime FileModificationDate { get; private set; }

        public long FileLength { get; private set; }

        protected abstract void WriteEntireEntity(HttpResponse response);

        protected abstract void WriteEntityRange(HttpResponse response, long rangeStartIndex, long rangeEndIndex);

        public override void ExecuteResult(ActionContext context)
        {
            EntityTag = GenerateEntityTag(context);
            FillRanges(context.HttpContext.Request);

            if (!(IsRangesValid(context.HttpContext.Response) &&
                IsModificationDateValid(context.HttpContext.Request, context.HttpContext.Response) &&
                IsEntityTagValid(context.HttpContext.Request, context.HttpContext.Response)))
                return;

            context.HttpContext.Response.Headers.Add("Last-Modified", FileModificationDate.ToString("r"));
            context.HttpContext.Response.Headers.Add("ETag", string.Format("\"{0}\"", EntityTag));
            context.HttpContext.Response.Headers.Add("Accept-Ranges", "bytes");

            if (!RangeRequest)
            {
                context.HttpContext.Response.Headers.Add("Content-Length", FileLength.ToString());
                context.HttpContext.Response.ContentType = ContentType;
                context.HttpContext.Response.StatusCode = HTTP_STATUS_CODE_OK;
                if (context.HttpContext.Request.Method != "HEAD")
                    WriteEntireEntity(context.HttpContext.Response);
            }
            else
            {
                var boundary = "---------------------------" + LocalTime.Now.Ticks.ToString("x");

                context.HttpContext.Response.Headers.Add("Content-Length", GetContentLength(boundary).ToString());
                if (!MultipartRequest)
                {
                    context.HttpContext.Response.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", RangesStartIndexes[0], RangesEndIndexes[0], FileLength));
                    context.HttpContext.Response.ContentType = ContentType;
                }
                else
                    context.HttpContext.Response.ContentType = string.Format("multipart/byteranges; boundary={0}", boundary);
                context.HttpContext.Response.StatusCode = HTTP_STATUS_CODE_PARTIAL_CONTENT;
                if (context.HttpContext.Request.Method != "HEAD")
                {
                    for (var i = 0; i < RangesStartIndexes.Length; i++)
                    {
                        if (MultipartRequest)
                        {
                            context.HttpContext.Response.Write(string.Format("--{0}\r\n", boundary));
                            context.HttpContext.Response.Write(string.Format("Content-Type: {0}\r\n", ContentType));
                            context.HttpContext.Response.Write(string.Format("Content-Range: bytes {0}-{1}/{2}\r\n\r\n", RangesStartIndexes[i], RangesEndIndexes[i], FileLength));
                        }

                        if (!context.HttpContext.RequestAborted.IsCancellationRequested)
                        {
                            WriteEntityRange(context.HttpContext.Response, RangesStartIndexes[i], RangesEndIndexes[i]);
                            if (MultipartRequest) context.HttpContext.Response.Write("\r\n");
                        }
                        else
                            return;
                    }

                    if (MultipartRequest) context.HttpContext.Response.Write(string.Format("--{0}--", boundary));
                }
            }
        }

        string GetHeader(HttpRequest request, string header, string defaultValue = "")
        {
            return request.Headers[header].ToString().IsEmpty() ? defaultValue :
                request.Headers[header].ToString().Replace("\"", string.Empty);
        }

        void FillRanges(HttpRequest request)
        {
            var rangesHeader = GetHeader(request, "Range");
            var ifRangeHeader = GetHeader(request, "If-Range", EntityTag);
            var isIfRangeHeaderDate = DateTime.TryParseExact(ifRangeHeader, DateFormats, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var ifRangeHeaderDate);

            if (rangesHeader.IsEmpty() || (!isIfRangeHeaderDate && ifRangeHeader != EntityTag) || (isIfRangeHeaderDate && HttpModificationDate > ifRangeHeaderDate))
            {
                RangesStartIndexes = new long[] { 0 };
                RangesEndIndexes = new long[] { FileLength - 1 };
                RangeRequest = false;
                MultipartRequest = false;
            }
            else
            {
                var ranges = rangesHeader.Remove("bytes=").Split(',');

                RangesStartIndexes = new long[ranges.Length];
                RangesEndIndexes = new long[ranges.Length];
                RangeRequest = true;
                MultipartRequest = ranges.HasMany();

                for (var i = 0; i < ranges.Length; i++)
                {
                    var currentRange = ranges[i].Split('-');

                    if (currentRange[1].IsEmpty())
                        RangesEndIndexes[i] = FileLength - 1;
                    else
                        RangesEndIndexes[i] = currentRange[1].To<long>();

                    if (currentRange[0].IsEmpty())
                    {
                        RangesStartIndexes[i] = FileLength - 1 - RangesEndIndexes[i];
                        RangesEndIndexes[i] = FileLength - 1;
                    }
                    else
                        RangesStartIndexes[i] = currentRange[0].To<long>();
                }
            }
        }

        int GetContentLength(string boundary)
        {
            var contentLength = 0;

            for (var i = 0; i < RangesStartIndexes.Length; i++)
            {
                contentLength += Convert.ToInt32(RangesEndIndexes[i] - RangesStartIndexes[i]) + 1;

                if (MultipartRequest)
                    contentLength += boundary.Length + ContentType.Length + RangesStartIndexes[i].ToString().Length +
                        RangesEndIndexes[i].ToString().Length + FileLength.ToString().Length + ADDITIONAL_CONTENT_LENGTH;
            }

            if (MultipartRequest)
                contentLength += boundary.Length + 4;

            return contentLength;
        }

        bool IsRangesValid(HttpResponse response)
        {
            if (FileLength > int.MaxValue)
            {
                response.StatusCode = HTTP_STATUS_CODE_PAYLOAD_TO_LONG;
                return false;
            }

            for (var i = 0; i < RangesStartIndexes.Length; i++)
            {
                if (RangesStartIndexes[i] > FileLength - 1 || RangesEndIndexes[i] > FileLength - 1 || RangesStartIndexes[i] < 0 || RangesEndIndexes[i] < 0 || RangesEndIndexes[i] < RangesStartIndexes[i])
                {
                    response.StatusCode = HTTP_STATUS_CODE_BAD_REQUEST;
                    return false;
                }
            }

            return true;
        }

        bool IsModificationDateValid(HttpRequest request, HttpResponse response)
        {
            var modifiedSinceHeader = GetHeader(request, "If-Modified-Since");
            if (modifiedSinceHeader.HasValue())
            {
                DateTime.TryParseExact(modifiedSinceHeader, DateFormats, null,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var modifiedSinceDate);

                if (HttpModificationDate <= modifiedSinceDate)
                {
                    response.StatusCode = HTTP_STATUS_CODE_NOT_MODIFIED;
                    return false;
                }
            }

            var unmodifiedSinceHeader = GetHeader(request, "If-Unmodified-Since", GetHeader(request, "Unless-Modified-Since"));
            if (unmodifiedSinceHeader.HasValue())
            {
                var unmodifiedSinceDateParsed = DateTime.TryParseExact(unmodifiedSinceHeader, DateFormats, null,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var unmodifiedSinceDate);

                if (HttpModificationDate > unmodifiedSinceDate)
                {
                    response.StatusCode = HTTP_STATUS_CODE_PRECONDITION_FAILED;
                    return false;
                }
            }

            return true;
        }

        bool IsEntityTagValid(HttpRequest request, HttpResponse response)
        {
            var matchHeader = GetHeader(request, "If-Match");
            if (matchHeader.HasValue() && matchHeader != "*")
            {
                var entitiesTags = matchHeader.Split(',');
                int entitieTagIndex;
                for (entitieTagIndex = 0; entitieTagIndex < entitiesTags.Length; entitieTagIndex++)
                {
                    if (EntityTag == entitiesTags[entitieTagIndex])
                        break;
                }

                if (entitieTagIndex >= entitiesTags.Length)
                {
                    response.StatusCode = HTTP_STATUS_CODE_PRECONDITION_FAILED;
                    return false;
                }
            }

            var noneMatchHeader = GetHeader(request, "If-None-Match");
            if (noneMatchHeader.HasValue())
            {
                if (noneMatchHeader == "*")
                {
                    response.StatusCode = HTTP_STATUS_CODE_PRECONDITION_FAILED;
                    return false;
                }

                var entitiesTags = noneMatchHeader.Split(',');
                foreach (var entityTag in entitiesTags)
                {
                    if (EntityTag != entityTag) continue;

                    response.Headers.Add("ETag", $"\"{entityTag}\"");
                    response.StatusCode = HTTP_STATUS_CODE_NOT_MODIFIED;
                    return false;
                }
            }

            return true;
        }
    }
}