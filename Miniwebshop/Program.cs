using Application.Interface;
using Application.Interfaces;
using Domain.Entities;
using FluentValidation.AspNetCore;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;

var builder = WebApplication.CreateBuilder(args);

// Logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
    .CreateLogger();
builder.Host.UseSerilog();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// MVC
builder.Services.AddControllersWithViews();

// Swagger
builder.Services.AddSwaggerGen();

// Session for cart
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(30); });

// Validation
builder.Services.AddFluentValidationAutoValidation();

// Response caching
builder.Services.AddResponseCaching();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.UseSwagger();
app.UseSwagger();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Seed roles
    string[] roles = { "Admin", "Customer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Seed admin user
    var adminEmail = "admin@gmail.com";
    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            Name = "Admin User",
            Phone = "1234567890",
            ShippingAddress = "Admin Address"
        };
        var result = await userManager.CreateAsync(admin, "Admin123@#$");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
        else
        {
            throw new Exception("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    // Seed categories and products
    if (!context.Categories.Any())
    {
        var cat1 = new Domain.Entities.Category { Name = "Electronics", Slug = "electronics" };
        var cat2 = new Domain.Entities.Category { Name = "Books", Slug = "books" };
        context.Categories.AddRange(cat1, cat2);

        context.Products.AddRange(
            new Domain.Entities.Product { Name = "Phone", Slug = "phone", Price = 100, Stock = 10, ShortDescription = "Cool phone", ImageUrl = "https://placeholder.com/phone.jpg", Category = cat1 },
            new Domain.Entities.Product { Name = "Book1", Slug = "book1", Price = 20, Stock = 5, ShortDescription = "Good book", ImageUrl = "https://placeholder.com/book.jpg", Category = cat2 }
        );
        context.SaveChanges();
    }
}

app.Run();