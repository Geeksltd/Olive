using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Olive.Entities;

namespace Olive.Mvc
{
    public class BlobViewModel
    {

        public BlobViewModelAction Action { get; set; }

        public string TempFileId { get; set; }

        public string Filename { get; set; }

        public string Url { get; set; }

        public bool HasValue => !IsEmpty;
        
        public bool IsEmpty { get; set; }

        internal string ItemId { get; set; }

        public static BlobViewModel From(Blob blob)
        {
            return new BlobViewModel
            {
                Action = BlobViewModelAction.Unchanged,
                Filename = blob?.FileName,
                ItemId = blob?.OwnerId(),
                Url = blob.Url(),
                IsEmpty = blob.IsEmpty()
            };
        }

        public Task<Blob> ToBlob()
        {
            if (TempFileId.HasValue())
                return Context.Current.GetService<IFileRequestService>().Bind(TempFileId);

            Blob result;

            if (Action == BlobViewModelAction.Unchanged) result = Blob.Unchanged();
            else result = Blob.Empty();

            return Task.FromResult(result);
        }
    }

    public enum BlobViewModelAction
    {
        New,
        Removed,
        Unchanged
    }
}
