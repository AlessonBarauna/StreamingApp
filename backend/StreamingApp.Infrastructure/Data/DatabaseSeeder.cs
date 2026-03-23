using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StreamingApp.Domain.Entities;
using StreamingApp.Domain.Enums;

namespace StreamingApp.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        await context.Database.MigrateAsync();

        // Roles
        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Admin user
        if (await userManager.FindByEmailAsync("admin@streaming.local") == null)
        {
            var admin = new User { UserName = "admin@streaming.local", Email = "admin@streaming.local", DisplayName = "Administrator", IsAdmin = true, SubscriptionPlan = SubscriptionPlan.Premium, EmailConfirmed = true };
            var result = await userManager.CreateAsync(admin, "Admin@123456");
            if (result.Succeeded) await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Test user
        if (await userManager.FindByEmailAsync("user@streaming.local") == null)
        {
            var user = new User { UserName = "user@streaming.local", Email = "user@streaming.local", DisplayName = "Test User", SubscriptionPlan = SubscriptionPlan.Basic, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, "User@123456");
            if (result.Succeeded) await userManager.AddToRoleAsync(user, "User");
        }

        // Categories
        if (!await context.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category { Name = "Ação", Slug = "acao", IconName = "local_fire_department" },
                new Category { Name = "Comédia", Slug = "comedia", IconName = "sentiment_very_satisfied" },
                new Category { Name = "Drama", Slug = "drama", IconName = "theater_comedy" },
                new Category { Name = "Ficção Científica", Slug = "ficcao-cientifica", IconName = "rocket_launch" },
                new Category { Name = "Terror", Slug = "terror", IconName = "skull" },
                new Category { Name = "Documentário", Slug = "documentario", IconName = "camera_roll" },
                new Category { Name = "Animação", Slug = "animacao", IconName = "animation" },
                new Category { Name = "Romance", Slug = "romance", IconName = "favorite" },
            };
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // Sample content
        if (!await context.Contents.AnyAsync())
        {
            var category = await context.Categories.FirstAsync(c => c.Slug == "acao");
            var contents = new[]
            {
                new Content { Title = "Aventura Espacial", Description = "Uma jornada épica pelo cosmos em busca de um novo lar para a humanidade.", Type = ContentType.Movie, ReleaseYear = 2024, AgeRating = "12", Status = TranscodingStatus.Draft, CategoryId = category.Id, IsFeatured = true, ThumbnailUrl = "/assets/placeholder-thumb.jpg", DurationMinutes = 120 },
                new Content { Title = "O Último Herói", Description = "Em um mundo pós-apocalíptico, um guerreiro solitário luta pela sobrevivência.", Type = ContentType.Movie, ReleaseYear = 2023, AgeRating = "16", Status = TranscodingStatus.Draft, CategoryId = category.Id, ThumbnailUrl = "/assets/placeholder-thumb.jpg", DurationMinutes = 95 },
                new Content { Title = "Mundos Paralelos", Description = "Série de ficção científica sobre viagens entre dimensões alternativas.", Type = ContentType.Series, ReleaseYear = 2024, AgeRating = "14", Status = TranscodingStatus.Draft, CategoryId = category.Id, ThumbnailUrl = "/assets/placeholder-thumb.jpg" },
            };
            await context.Contents.AddRangeAsync(contents);
            await context.SaveChangesAsync();
        }
    }
}
