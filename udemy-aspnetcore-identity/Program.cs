using Microsoft.EntityFrameworkCore;
using udemy_aspnetcore_identity.Data;
using Microsoft.AspNetCore.Identity;
using udemy_aspnetcore_identity.Service;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

});

builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDBContext>().AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = true;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

    options.SignIn.RequireConfirmedEmail = false;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/SignIn";
    options.AccessDeniedPath = "/Identity/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
});

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

builder.Services.AddSingleton<IEmailSender, EmailSender>();

var issuers = builder.Configuration["Tokens:Issuer"];
var audience = builder.Configuration["Tokens:Audience"];
var key = builder.Configuration["Tokens:Key"];

builder.Services.AddAuthentication().AddFacebook(options =>
{

    options.AppId = builder.Configuration["FacebookAppId"];
    options.AppSecret = builder.Configuration["FacebookAppSecret"];
}).AddJwtBearer(options => {
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidIssuer = issuers,
        ValidAudience=audience,
        IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))


    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MemberDep", p =>
    {
        p.RequireClaim("Department", "Tech").RequireRole("Member");
    });

    options.AddPolicy("AdminDep", p =>
    {
        p.RequireClaim("Department", "Tech").RequireRole("Admin");
    });
});

builder.Services.AddControllersWithViews();





var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
