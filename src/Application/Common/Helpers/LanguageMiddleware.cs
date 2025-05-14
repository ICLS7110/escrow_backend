using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Escrow.Api.Application.Common.Constants;

namespace Escrow.Api.Application.Common.Helpers
{
    public class LanguageMiddleware
    {
        private readonly RequestDelegate _next;
        private const string DefaultLang = "en";
        public const string LanguageKey = "CurrentLanguage";

        public LanguageMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get language from header or query parameter
            string lang = context.Request.Headers["Language"].ToString();

            if (string.IsNullOrEmpty(lang))
            {
                lang = context.Request.Query["lang"].ToString();
            }

            lang = string.IsNullOrEmpty(lang) ? DefaultLang : lang.ToLower();

            // Set the selected language
            Language selectedLang = lang.StartsWith("ar") ? Language.Arabic : Language.English;

            // Set language in context to be accessible throughout the request
            context.Items[LanguageKey] = selectedLang;

            // Set culture info for the application
            var culture = new CultureInfo(selectedLang == Language.Arabic ? "ar-SA" : "en-US");//"en-US");
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            await _next(context);
        }

    }

}
