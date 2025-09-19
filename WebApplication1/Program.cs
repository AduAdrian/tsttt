using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using WebApplication1.BackgroundServices;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure forwarded headers for reverse proxy support
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

// Add Memory Cache
builder.Services.AddMemoryCache();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
{
    // Configur?ri pentru parol? - îmbun?t??ite
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredUniqueChars = 3;
    
    // Configur?ri pentru user
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true; // Important pentru securitate
    
    // Configur?ri pentru lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // Configur?ri pentru token-urile de resetare parol?
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configurare pentru cookie authentication - îmbun?t??it?
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(2); // Redus pentru securitate
    options.SlidingExpiration = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Add Antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Înregistrare servicii
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IItpNotificationService, ItpNotificationService>();
builder.Services.AddScoped<IDataSeedingService, DataSeedingService>();
builder.Services.AddScoped<IExcelTemplateService, ExcelTemplateService>();

// Înregistrare Background Services
builder.Services.AddHostedService<ItpNotificationBackgroundService>();

// Add Controllers with enhanced security
builder.Services.AddControllersWithViews(options =>
{
    // Add global antiforgery filter
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

// Configure JSON options
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();

// Configure forwarded headers for reverse proxy
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Force HTTPS for production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
else
{
    // Use HTTPS redirection even in development for consistency
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }
    
    await next();
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Apply database migrations and initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Apply any pending migrations
        await context.Database.MigrateAsync();
        
        // Always ensure admin user exists
        await EnsureAdminUserExistsAsync(context, userManager, roleManager, logger);
        
        logger.LogInformation("Database initialization completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or initializing the database");
        
        // In production, you might want to fail fast
        if (!app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

// Method to ensure admin user always exists - îmbun?t??it
static async Task EnsureAdminUserExistsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger logger)
{
    try
    {
        // Create Admin role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            var adminRole = new IdentityRole("Admin");
            await roleManager.CreateAsync(adminRole);
            logger.LogInformation("Admin role created successfully");
        }

        // Create User role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("User"))
        {
            var userRole = new IdentityRole("User");
            await roleManager.CreateAsync(userRole);
            logger.LogInformation("User role created successfully");
        }

        // Check if admin user already exists
        var existingAdmin = await userManager.FindByNameAsync("admin");
        if (existingAdmin == null)
        {
            // Create admin user with predefined credentials
            var adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = "notificari-sms@misedainspect.ro",
                PhoneNumber = "0756396565",
                EmailConfirmed = true,
                DisplayName = "Administrator Miseda Inspect SRL",
                PhoneNumberConfirmed = true
            };

            // Parol? îmbun?t??it? conform cerin?elor Identity
            var result = await userManager.CreateAsync(adminUser, "Admin123!");

            if (result.Succeeded)
            {
                // Add admin user to Admin role
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("Default admin user created successfully:");
                logger.LogInformation("Username: admin");
                logger.LogInformation("Email: notificari-sms@misedainspect.ro");
                logger.LogInformation("Phone: 0756396565");
                logger.LogInformation("Password: Admin123!");
            }
            else
            {
                logger.LogError("Error creating default admin user:");
                foreach (var error in result.Errors)
                {
                    logger.LogError("- {Description}", error.Description);
                }
            }
        }
        else
        {
            // Admin exists, ensure it has Admin role and is confirmed
            if (!await userManager.IsInRoleAsync(existingAdmin, "Admin"))
            {
                await userManager.AddToRoleAsync(existingAdmin, "Admin");
                logger.LogInformation("Admin role added to existing admin user");
            }
            
            // Ensure email is confirmed
            if (!existingAdmin.EmailConfirmed)
            {
                existingAdmin.EmailConfirmed = true;
                await userManager.UpdateAsync(existingAdmin);
                logger.LogInformation("Email confirmed for admin user");
            }

            // Ensure phone is confirmed
            if (!existingAdmin.PhoneNumberConfirmed)
            {
                existingAdmin.PhoneNumberConfirmed = true;
                await userManager.UpdateAsync(existingAdmin);
                logger.LogInformation("Phone number confirmed for admin user");
            }

            // Update display name if empty
            if (string.IsNullOrEmpty(existingAdmin.DisplayName))
            {
                existingAdmin.DisplayName = "Administrator Miseda Inspect SRL";
                await userManager.UpdateAsync(existingAdmin);
                logger.LogInformation("Display name updated for admin user");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error ensuring admin user exists");
    }
}

// Make Program class public for testing
public partial class Program { }
