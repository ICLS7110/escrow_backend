using System.Globalization;
using System.Linq;
using Escrow.Api.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.Common.Helpers
{
    public class LanguageService
    {
        private readonly IApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LanguageService(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserLanguagePreference(string userId)
        {
            // Parse userId if necessary
            if (!int.TryParse(userId, out int uid)) return "en";

            var user = _context.UserDetails
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == uid);

            return user?.Language ?? "en";
        }

        //public string GetLocalizedMessage(string userId, string messageKey)
        //{
        //    string language = GetUserLanguagePreference(userId);

        //    // Check Accept-Language header if user language is not set or default
        //    var headerLang = _httpContextAccessor.HttpContext?.Request?.Headers["Accept-Language"].ToString();
        //    if ((string.IsNullOrEmpty(language) || language == "en") && headerLang?.Contains("ar") == true)
        //    {
        //        language = "ar";
        //    }

        //    return _localizer.WithCulture(new CultureInfo(language))[messageKey];
        //}
    }
}
