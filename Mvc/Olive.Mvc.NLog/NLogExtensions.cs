using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using NLog.Web;

namespace Olive.Mvc
{
    public static class NLogExtensions
    {
        /// <summary>
        /// Registers NLog listener to receive log messages sent to Olive.Log class.
        /// It does the configuration based on NLog.config file.
        /// See https://github.com/nlog/NLog/wiki/Configuration-file
        /// </summary>
        public static IApplicationBuilder UseNLog(this IApplicationBuilder app, IHostingEnvironment env)
        {
            // TODO: add a default configuration for nlog.config.

            // TODO: Take environment into account.

            var logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
            Log.RegisterLogger(new NLogger(logger));
            return app;
        }
    }
}
