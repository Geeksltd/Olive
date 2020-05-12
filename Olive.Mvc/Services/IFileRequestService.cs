using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Olive.Entities;
using System;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public interface IFileRequestService
    {
        Task DeleteTempFiles(TimeSpan olderThan);
        Task<Blob> Bind(string fileKey);
        Task<object> TempSaveUploadedFile(IFormFile file, bool allowUnsafeExtension = false);
        Task<object> CreateDownloadAction(byte[] data, string filename);
        Task<ActionResult> Download(string key);
    }
}