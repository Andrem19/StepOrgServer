using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;
using StepOrg.Entities;

namespace StepOrg.Data
{
    public class StoreContextSeed
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<User> userManager, ILoggerFactory loggerFactory)
        {
            try
            {
                if (!userManager.Users.Any())
                {
                    var user = new User
                    {
                        UserName = "bob",
                        Email = "bob@test.com",
                        EmailConfirmed = true,
                        InviteCode = "YT65FCVBPO009GFQA3WSF65G"
                    };

                    await userManager.CreateAsync(user, "Pa$$w0rd");
                    await userManager.AddToRoleAsync(user, "Member");

                    var admin = new User
                    {
                        UserName = "admin",
                        Email = "admin@test.com",
                        InviteCode = "YT88FCVBPO009GFQA3WSF55G",
                        EmailConfirmed = true,
                    };
                    await userManager.CreateAsync(admin, "Pa$$w0rd");
                    await userManager.AddToRoleAsync(admin, "Admin");

                    var moderator = new User
                    {
                        UserName = "moderator",
                        Email = "moderator@test.com",
                        InviteCode = "YT65FCVBPO449GFQA3WSF65G",
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(moderator, "Pa$$w0rd");
                    await userManager.AddToRoleAsync(moderator, "Moderator");

                    var delivery = new User
                    {
                        UserName = "delivery",
                        InviteCode = "YT65FCVBPO009GFQA3WSF23G",
                        Email = "delivery@test.com",
                        EmailConfirmed = true,
                    };

                    await userManager.CreateAsync(delivery, "Pa$$w0rd");
                    await userManager.AddToRoleAsync(delivery, "Delivery");
                };
                //if (!context.Groups.Any())
                //{
                //    var groupData =
                //        File.ReadAllText("../HomeOrganizer/Data/SeedData/groups.json");
                //    var groups = JsonSerializer.Deserialize<List<Group>>(groupData);


                //    foreach (var item in groups)
                //    {
                //        context.Groups.Add(item);
                //    }
                //    await context.SaveChangesAsync();
                //}
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<StoreContextSeed>();
                logger.LogError(ex.Message);
            }
        }
    }
}
