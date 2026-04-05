using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using recrutementapp.Data;
using recrutementapp.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Application services
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddTransient<IEmailSender, LocalFileEmailSender>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

// Authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CandidateOnly", p => p.RequireRole("Candidate"));
    options.AddPolicy("RecruiterAccess", p => p.RequireRole("Recruiter", "Admin"));
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Ensure wwwroot/uploads/resumes exists before any request is served
var webRoot = builder.Environment.WebRootPath
                 ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
Directory.CreateDirectory(Path.Combine(webRoot, "uploads", "resumes"));
Directory.CreateDirectory(Path.Combine(webRoot, "uploads", "logos"));

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve wwwroot (css, js, lib, etc.)
app.UseStaticFiles();

// Serve uploaded files from wwwroot/uploads with inline Content-Disposition
// so PDFs open in the browser instead of downloading
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(webRoot, "uploads")),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        var ext = Path.GetExtension(ctx.File.Name).ToLowerInvariant();
        if (ext == ".pdf")
        {
            ctx.Context.Response.Headers["Content-Type"] = "application/pdf";
            ctx.Context.Response.Headers["Content-Disposition"] = "inline";
        }
    }
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed database on first run
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
    DbSeeder.Seed(db);
}

app.Run();