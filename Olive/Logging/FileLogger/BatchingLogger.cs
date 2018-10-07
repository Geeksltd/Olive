﻿using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace Olive.Logging
{
    class BatchingLogger : ILogger
    {
        BatchingLoggerProvider Provider;
        string Category;

        public BatchingLogger(BatchingLoggerProvider provider, string category)
        {
            Provider = provider;
            Category = category;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(DateTimeOffset timestamp, LogLevel logLevel, EventId _, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var r = new StringBuilder();
            r.Append(timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"));
            r.Append(" [");
            r.Append(logLevel.ToString());
            r.Append("] ");
            r.Append(Category);
            r.Append(": ");
            r.AppendLine(formatter(state, exception));

            if (exception != null) r.AppendLine(exception.ToString());

            Provider.AddMessage(timestamp, r.ToString());
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Log(DateTimeOffset.Now, logLevel, eventId, state, exception, formatter);
        }
    }
}