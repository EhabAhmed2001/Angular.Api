using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Talabat.Core.Entities.Identity;

namespace Talabat.Repository.Identity
{
	public static class AppIdentityDbContextSeed
	{
		public static async Task SeedUserAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
		{

            // Seed the database with roles
            if (!roleManager.Roles.Any())
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
                await roleManager.CreateAsync(new IdentityRole("Customer"));
            }


            if (!userManager.Users.Any())
			{
				var admin = new AppUser()
				{
					DisplayNams = "Ehab Ahmed",
					Email = "e.ahmed2684@gmail.com",
					UserName = "e.ahmed2684",
					PhoneNumber = "01021323989",
				};

                // Create a user with a password with admin role
                var result = await userManager.CreateAsync(admin, "P@ssw0rd");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");

                else
                    throw new Exception("Failed to create user");

                var customer = new AppUser()
                {
                    DisplayNams = "Customer",
                    Email = "customer@gmail.com",
                    UserName = "customer",
                    PhoneNumber = "01234567891",
                };
                // Create a user with a password with customer role
                var result2 = await userManager.CreateAsync(customer, "P@ssw0rd");
                if (result2.Succeeded)
                    await userManager.AddToRoleAsync(customer, "Customer");
                else
                    throw new Exception("Failed to create user");

            }

        }
	}
}
