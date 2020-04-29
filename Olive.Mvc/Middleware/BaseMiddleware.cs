using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    abstract class BaseMiddleware
    {
        protected readonly RequestDelegate Next;

        public BaseMiddleware(RequestDelegate next) => Next = next;

        public abstract Task Invoke(HttpContext context);
    }
}
