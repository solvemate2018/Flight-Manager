using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Web.Data
{
    public class SeedData
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedData(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedDefaultAdminUser()
        {
            // Check if the admin role exists, and create it if not
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                var adminRole = new IdentityRole("Admin");
                await _roleManager.CreateAsync(adminRole);
            }

            // Check if the admin user exists, and create it if not
            if (await _userManager.FindByNameAsync("admin") == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                };

                var result = await _userManager.CreateAsync(adminUser, "Adm!n123");

                if (result.Succeeded)
                {
                    // Assign the admin role to the admin user
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
