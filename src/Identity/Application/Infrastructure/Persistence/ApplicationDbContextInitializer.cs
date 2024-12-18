using System.Text.Json;
using Identity.Application.Common.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Infrastructure.Persistence;

public class ApplicationDbContextInitializer
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IPasswordHasher<User> _passwordHasher;

    public ApplicationDbContextInitializer(
        ILogger<ApplicationDbContextInitializer> logger, 
        ApplicationDbContext context,
        UserManager<User> userManager, 
        RoleManager<IdentityRole<Guid>> roleManager,
        IPasswordHasher<User> passwordHasher
    )
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _passwordHasher = passwordHasher;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Start migrating database...");
            await _context.Database.MigrateAsync();
            _logger.LogInformation("End migrating database.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Start seeding database...");
            await TrySeedAsync();
            _logger.LogInformation("End seeding database.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        /* Seeding system users */
        if (await _userManager.Users.AnyAsync())
        {
            _logger.LogInformation("System Users already exists.");
            return;
        }

        var contentRootPath = $@"{Directory.GetParent(Environment.CurrentDirectory)?.Parent?.FullName}\Identity\Application\Infrastructure";
        string sourcePath = Path.Combine(contentRootPath, "Seeds", "users.json");
        string sourceJson = File.ReadAllText(sourcePath);
        User[]? sourceItems = JsonSerializer.Deserialize<User[]>(sourceJson);

        if (sourceItems is null || sourceItems.Length == 0)
        {
            _logger.LogError("Cannot read users items from json file.");
            return;
        }

        _logger.LogInformation("Seeding identity users...");

        List<User> systemUsers = sourceItems.ToList();
        systemUsers.ForEach(user => 
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, "Password@BuyEZ");
            user.SecurityStamp = Guid.NewGuid().ToString();
        });

        await _context.AddRangeAsync(systemUsers);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added {total} system user record(s)", systemUsers.Count());

        /* Seeding system roles */
        if (await _roleManager.Roles.AnyAsync())
        {
            _logger.LogInformation("Roles already exists.");
            return;
        }

        _logger.LogInformation("Seeding identity roles...");

        var createSystemAdminResult = await _roleManager.CreateAsync(new IdentityRole<Guid>
        { Id = Guid.NewGuid(), Name = Common.Constants.IdentityConstants.Role.SYSTEM_ADMIN, ConcurrencyStamp = Guid.NewGuid().ToString() });
        if (createSystemAdminResult.Succeeded)
        {
            var systemAdminUsers = systemUsers.Where(u => u.UserName!.Contains("_administrator"));
            foreach (var user in systemAdminUsers)
                await _userManager.AddToRoleAsync(user, Common.Constants.IdentityConstants.Role.SYSTEM_ADMIN);
        }

        var createAdminSupportResult = await _roleManager.CreateAsync(new IdentityRole<Guid>
        { Id = Guid.NewGuid(), Name = Common.Constants.IdentityConstants.Role.SYSTEM_SUPPORT, ConcurrencyStamp = Guid.NewGuid().ToString() });
        if (createAdminSupportResult.Succeeded)
        {
            var systemSupportUsers = systemUsers.Where(u => u.UserName!.Contains("_supporter"));
            foreach (var user in systemSupportUsers)
                await _userManager.AddToRoleAsync(user, Common.Constants.IdentityConstants.Role.SYSTEM_SUPPORT);
        }

        await _roleManager.CreateAsync(new IdentityRole<Guid>
        { Id = Guid.NewGuid(), Name = Common.Constants.IdentityConstants.Role.TENANT_ADMIN, ConcurrencyStamp = Guid.NewGuid().ToString() });

        await _roleManager.CreateAsync(new IdentityRole<Guid>
        { Id = Guid.NewGuid(), Name = Common.Constants.IdentityConstants.Role.TENANT_MANAGER, ConcurrencyStamp = Guid.NewGuid().ToString() });

        await _roleManager.CreateAsync(new IdentityRole<Guid>
        { Id = Guid.NewGuid(), Name = Common.Constants.IdentityConstants.Role.TENANT_STAFF, ConcurrencyStamp = Guid.NewGuid().ToString() });

        await _roleManager.CreateAsync(new IdentityRole<Guid>
        { Id = Guid.NewGuid(), Name = Common.Constants.IdentityConstants.Role.USER, ConcurrencyStamp = Guid.NewGuid().ToString() });
        
        _logger.LogInformation("Seeded identity roles.");
    }
}
