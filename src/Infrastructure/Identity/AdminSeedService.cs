using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Escrow.Api.Infrastructure.Identity;
public class AdminSeedService
{
    public static async Task EnsureAdminUserExists(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Create roles if they don't exist
        if (!await roleManager.RoleExistsAsync(nameof(Roles.Admin)))
            await roleManager.CreateAsync(new IdentityRole(nameof(Roles.Admin)));

        if (!await roleManager.RoleExistsAsync("User"))
            await roleManager.CreateAsync(new IdentityRole("User"));

        var adminEmail = "admin@Escrow.com";
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin == null)
        {
            var adminUser = new ApplicationUser
            {
                //FullName = "Super Admin",
                Email = adminEmail,
                //Role = "Admin",
                UserName = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123"); // Use a strong password
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, nameof(Roles.Admin));
            }
        }
    }
}
