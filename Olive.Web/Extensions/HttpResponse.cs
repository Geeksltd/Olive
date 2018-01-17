﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Olive.Entities;

namespace Olive.Web
{
    partial class OliveExtensions
    {
        const int MOVED_PERMANENTLY_STATUS_CODE = 301;

        /// <summary>
        /// Writes the specified content wrapped in a DIV tag.
        /// </summary>
        public static void WriteLine(this HttpResponse response, string content) =>
            response.WriteAsync($"<div>{content}</div>").RunSynchronously();

        /// <summary>
        /// Redirects the client to the specified URL with a 301 status (permanent).
        /// </summary>
        public static void RedirectPermanent(this HttpResponse response, string permanentUrl)
        {
            response.StatusCode = MOVED_PERMANENTLY_STATUS_CODE;
            response.Headers.Add("Location", permanentUrl);
        }

        /// <summary>
        /// Dispatches a file back to the client.
        /// </summary>
        /// <param name="fileName">If set to null, the same file name of the file will be used.</param>
        public static async Task Dispatch(this HttpResponse response, FileInfo responseFile, string fileName = null, string contentType = "Application/octet-stream")
        {
            if (responseFile == null)
                throw new ArgumentNullException(nameof(responseFile));

            if (fileName.IsEmpty())
                fileName = responseFile.Name;

            await Dispatch(response, await responseFile.ReadAllBytesAsync(), fileName, contentType);
        }

        /// <summary>
        /// Dispatches a file back to the client.
        /// </summary>
        public static async Task Dispatch(this HttpResponse response, Blob blob, string contentType = "Application/octet-stream")
        {
            await Dispatch(response, await File.ReadAllBytesAsync(blob.LocalPath), blob.FileName, contentType);
        }

        /// <summary>
        /// Dispatches a binary data block back to the client.
        /// </summary>
        public static async Task Dispatch(this HttpResponse response, byte[] responseData, string fileName, string contentType = "Application/octet-stream")
        {
            if (responseData == null)
                throw new ArgumentNullException(nameof(responseData));

            if (fileName.IsEmpty())
                throw new ArgumentNullException(nameof(fileName));

            response.Clear();
            response.ContentType = contentType;

            response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName.Remove("\"").Replace(",", "-")}\"");

            await response.Body.WriteAsync(responseData, 0, responseData.Length);
            await response.Body.FlushAsync();
        }

        /// <summary>
        /// Dispatches a string back to the client as a file.
        /// </summary>
        public static void Dispatch(this HttpResponse response, string responseText, string fileName, string contentType = "Application/octet-stream", System.Text.Encoding encoding = null)
        {
            response.Clear();

            response.Headers.Add("Cache-Control", "no-store");
            response.Headers.Add("Pragma", "no-cache");

            if (fileName.HasValue())
                response.Headers.Add("Content-Disposition", $"attachment;filename={fileName.Replace(" ", "_")}");

            response.ContentType = contentType;

            if (encoding != null)
                response.WriteAsync(responseText, encoding).RunSynchronously();
            else
                response.WriteAsync(responseText).RunSynchronously();
        }

        /// <summary>
        /// Writes the specified message in the response and then ends the response.
        /// </summary>
        public static async Task EndWith(this HttpResponse response, string message, string mimeType = "text/html")
        {
            response.ContentType = mimeType;
            await response.WriteAsync(message);
        }
    }
}