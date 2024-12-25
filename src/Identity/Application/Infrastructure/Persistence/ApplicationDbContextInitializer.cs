using Identity.Application.Domain.Identity;

using IdentityConstants = Shared.Common.Constants.IdentityConstants;
using Shared.Infrastructure.Persistence;

using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Infrastructure.Persistence;

public class ApplicationDbContextInitializer : IApplicationDbContextInitializer
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

    private async Task TrySeedAsync()
    {
        /*  Seeding general data   */
        await SeedUserRoles();

        /*  Seeding system data   */
        _logger.LogInformation("Start seeding system data...");
        var systemUsers = await SeedUsers("users.json", "Password@BuyEZ");
        await SeedSystemUserRolesAsync(systemUsers);

        /*  Seeding tenant data   */
        _logger.LogInformation("Start seeding tenant data...");
        var tenantUsers = await SeedUsers("lucy-users.json", "Password@Lucy", user => user.TenantId == "e7585613-e43e-48b5-8479-ef87556de30d");
        await SeedTenantUserRolesAsync(tenantUsers);
    }

    private async Task<IEnumerable<User>> SeedUsers(string jsonFileName, string password, Expression<Func<User, bool>>? checkExistPredicate = null)
    {
        /* Seeding system users */
        var isUserExists = checkExistPredicate != null
            ? await _userManager.Users.AnyAsync(checkExistPredicate)
            : await _userManager.Users.AnyAsync();
        if (isUserExists)
        {
            _logger.LogInformation("Users already exists.");
            return Enumerable.Empty<User>();
        }

        var contentRootPath = $@"{Directory.GetParent(Environment.CurrentDirectory)?.Parent?.FullName}\Identity\Application\Infrastructure";
        string sourcePath = Path.Combine(contentRootPath, "Seeds", jsonFileName);
        string sourceJson = File.ReadAllText(sourcePath);
        User[]? sourceItems = JsonSerializer.Deserialize<User[]>(sourceJson);

        if (sourceItems is null || sourceItems.Length == 0)
        {
            _logger.LogError("Cannot read users items from json file.");
            return Enumerable.Empty<User>();
        }

        _logger.LogInformation("Seeding users...");

        List<User> users = sourceItems.ToList();
        users.ForEach(user => 
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.NormalizedUserName = user.UserName!.ToUpper();
            user.NormalizedEmail = user.Email!.ToUpper();
            user.Created = DateTimeOffset.Now;
        });

        await _context.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added {total} user record(s)", users.Count());

        return users;
    }

    private async Task SeedUserRoles()
    {
        /* Seeding system roles */
        if (await _roleManager.Roles.AnyAsync())
        {
            _logger.LogInformation("Roles already exists.");
            return;
        }

        _logger.LogInformation("Seeding identity roles...");

        await _roleManager.CreateAsync(new IdentityRole<Guid>
        { Id = new Guid("60070ab8-1454-4a20-9f85-5db4d0a3fcab"), Name = IdentityConstants.Role.SYSTEM_ADMIN, ConcurrencyStamp = Guid.NewGuid().ToString() });

        await _roleManager.CreateAsync(new IdentityRole<Guid>
        { Id = new Guid("10593c52-9800-4828-a9b3-151cee17f4d5"), Name = IdentityConstants.Role.SYSTEM_SUPPORT, ConcurrencyStamp = Guid.NewGuid().ToString() });

        await _roleManager.CreateAsync(new IdentityRole<Guid>
        { Id = new Guid("c870fd41-883d-4755-8c13-4d77ecefcd98"), Name = IdentityConstants.Role.TENANT_ADMIN, ConcurrencyStamp = Guid.NewGuid().ToString() });

        await _roleManager.CreateAsync(new IdentityRole<Guid>
        { Id = new Guid("31ec5efa-bf55-40e2-a3bc-ba7a16dada24"), Name = IdentityConstants.Role.TENANT_MANAGER, ConcurrencyStamp = Guid.NewGuid().ToString() });

        await _roleManager.CreateAsync(new IdentityRole<Guid>
        { Id = new Guid("c4be35d4-7d5a-4bee-9abd-d207ce93cb8c"), Name = IdentityConstants.Role.TENANT_STAFF, ConcurrencyStamp = Guid.NewGuid().ToString() });

        await _roleManager.CreateAsync(new IdentityRole<Guid>
        { Id = new Guid("e4c8ebc9-e6ea-4de3-88b5-bde0d43a771c"), Name = IdentityConstants.Role.USER, ConcurrencyStamp = Guid.NewGuid().ToString() });
        
        _logger.LogInformation("Seeded identity roles.");
    }

    private async Task SeedSystemUserRolesAsync(IEnumerable<User> systemUsers)
    {
        var systemAdminUsers = systemUsers.Where(u => u.UserName!.Contains("buyez_administrator"));
        foreach (var user in systemAdminUsers)
            await _userManager.AddToRoleAsync(user, IdentityConstants.Role.SYSTEM_ADMIN);

        var systemSupportUsers = systemUsers.Where(u => u.UserName!.Contains("buyez_supporter"));
        foreach (var user in systemSupportUsers)
            await _userManager.AddToRoleAsync(user, IdentityConstants.Role.SYSTEM_SUPPORT);
    }

    private async Task SeedTenantUserRolesAsync(IEnumerable<User> tenantUsers)
    {
        var tenantAdminUsers = tenantUsers.Where(u => u.UserName!.Contains("lucy_store_administrator"));
        foreach (var user in tenantAdminUsers)
            await _userManager.AddToRoleAsync(user, IdentityConstants.Role.TENANT_ADMIN);

        var tenantManagerUsers = tenantUsers.Where(u => u.UserName!.Contains("lucy_store_manager"));
        foreach (var user in tenantManagerUsers)
            await _userManager.AddToRoleAsync(user, IdentityConstants.Role.TENANT_MANAGER);

        var tenantStaffUsers = tenantUsers.Where(u => u.UserName!.Contains("lucy_store_staff"));
        foreach (var user in tenantManagerUsers)
            await _userManager.AddToRoleAsync(user, IdentityConstants.Role.TENANT_STAFF);
    }
}
