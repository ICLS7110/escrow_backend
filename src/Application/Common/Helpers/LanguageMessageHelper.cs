using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Constants;
using Microsoft.AspNetCore.Http;
using Escrow.Api.Infrastructure.Security;


namespace Escrow.Api.Application.Common.Helpers;
public static class LanguageMessageHelper
{
    // Get localized message based on current language in HttpContext
    public static string GetMessage(HttpContext context, string englishMessage, string arabicMessage)
    {
        if (context.Items.TryGetValue(LanguageMiddleware.LanguageKey, out var langObj) &&
            langObj is Language lang && lang == Language.Arabic)
        {
            return arabicMessage;
        }

        return englishMessage;
    }
}
