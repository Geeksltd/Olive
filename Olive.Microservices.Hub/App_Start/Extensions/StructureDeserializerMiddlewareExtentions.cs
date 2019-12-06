using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Microsoft.AspNetCore.Builder;

namespace System
{
    public static class StructureDeserializerMiddlewareExtentions
    {
        public static IApplicationBuilder UseStructureDeserializer(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                await StructureDeserializer.Load();

                await next();
            });
        }
    }
}
