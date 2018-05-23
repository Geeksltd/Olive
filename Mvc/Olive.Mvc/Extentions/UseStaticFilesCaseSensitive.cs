using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Mvc
{
    partial class OliveMvcExtensions
    {
        /// <summary>
        /// Enforces case-correct requests on Windows to make it compatible with Linux.
        /// </summary>
        public static IApplicationBuilder UseStaticFilesCaseSensitive(this IApplicationBuilder app)
        {
            var fileOptions = new StaticFileOptions
            {
                OnPrepareResponse = x =>
                {
                    if (!x.File.PhysicalPath.AsFile().Exists()) return;
                    var requested = x.Context.Request.Path.Value;
                    if (requested.IsEmpty()) return;

                    var onDisk = x.File.PhysicalPath.AsFile().GetExactFullName().Replace("\\", "/");
                    if (!onDisk.EndsWith(requested))
                    {
                        throw new Exception("The requested file has incorrect casing and will fail on Linux servers." +
                            Environment.NewLine + "Requested: " + requested + Environment.NewLine +
                            "On disk: " + onDisk.Right(requested.Length));
                    }
                }
            };

            return app.UseStaticFiles(fileOptions);
        }
    }
}
