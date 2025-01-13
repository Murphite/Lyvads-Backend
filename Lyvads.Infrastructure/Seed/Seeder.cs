using Lyvads.Domain.Constants;
using Lyvads.Domain.Entities;
using Lyvads.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lyvads.Infrastructure.Seed;

public class Seeder
{
    public static async Task Run(IApplicationBuilder app)
    {
        var context = app.ApplicationServices.CreateScope().ServiceProvider
            .GetRequiredService<AppDbContext>();

        //// Apply pending migrations if any
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
            await context.Database.MigrateAsync();

        // Seed roles if not already present
        var roleManager = app.ApplicationServices.CreateScope().ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>(); // Inject RoleManager


        // Seed roles if not already present
        if (!context.Roles.Any())
        {
            var roles = new List<IdentityRole>
            {
                new() { Name = RolesConstant.RegularUser, NormalizedName = RolesConstant.RegularUser.ToUpper() },
                new() { Name = RolesConstant.Creator, NormalizedName = RolesConstant.Creator.ToUpper() },
                new() { Name = RolesConstant.Admin, NormalizedName = RolesConstant.Admin.ToUpper() },
                new() { Name = RolesConstant.SuperAdmin, NormalizedName = RolesConstant.SuperAdmin.ToUpper() }
            };
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // Ensure SUPERADMIN role exists
        var superAdminRole = await roleManager.FindByNameAsync(RolesConstant.SuperAdmin);
        if (superAdminRole == null)
        {
            superAdminRole = new IdentityRole
            {
                Name = RolesConstant.SuperAdmin,
                NormalizedName = RolesConstant.SuperAdmin.ToUpper()
            };
            await roleManager.CreateAsync(superAdminRole);
        }

        var userManager = app.ApplicationServices.CreateScope().ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        // Create an Admin user and assign roles
        var user = new ApplicationUser
        {
            FirstName = "Murphy",
            LastName = "Admin",
            Email = "ogbeidemurphy@gmail.com",
            UserName = "ogbeidemurphy@gmail.com",
            PhoneNumber = "080123456789"
        };

        // Create the user first
        var createResult = await userManager.CreateAsync(user, "Admin@123");

        // Ensure the user is created successfully before adding to a role
        if (createResult.Succeeded)
        {
            // Save the user before adding roles
            await userManager.AddToRoleAsync(user, RolesConstant.SuperAdmin);

            // Reference the user in the SuperAdmin table
            var superAdmin = new SuperAdmin 
            {
                ApplicationUserId = user.Id, 
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await context.SuperAdmins.AddAsync(superAdmin);
            await context.SaveChangesAsync(); // Save the SuperAdmin record
        }
        else
        {
            // Handle the error (you can log the error or throw an exception)
            Console.WriteLine("Error creating user: " + string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }

        // Generate additional data
        var config = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        try
        {
            var dataGenerator = new DataGenerator(userManager, context, config);
            await dataGenerator.Run();
        }
        catch (Exception ex)
        {
            // Log the error for further investigation
            Console.WriteLine($"Error in DataGenerator: {ex.Message}");
        }
    }
}
