using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web2App.Middlewares
{
    public static class EnableBufferingMiddlewareExtension
    {
        public static IApplicationBuilder UseBuffering(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<EnableBufferingMiddleware>();
        }
    }

    public class EnableBufferingMiddleware
    {
        private readonly RequestDelegate _next;
        public EnableBufferingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try { context.Request.EnableBuffering(); } catch { }
            await _next(context);
        }
    }
}
