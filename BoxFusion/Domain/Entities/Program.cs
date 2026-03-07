using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BoxFusion.API.BoxFusion.Domain.Entities;
using BoxFusion.API.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. მონაცემთა ბაზა - PostgreSQL (Npgsql)
builder.Services.AddDbContext<BoxFusionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity კონფიგურაცია
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<BoxFusionDbContext>()
    .AddDefaultTokenProviders();

// 3. CORS - React-თან დასაკავშირებლად
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://box-fusion-frontend.vercel.app" // დაამატე შენი ფრონტის ლინკი
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// 4. აპლიკაციის სერვისები
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CloudinaryService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 5. Swagger კონფიგურაცია + JWT მხარდაჭერა
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "შეიყვანე ფორმატით: Bearer {შენი_თოკენი}"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// 6. JWT ავთენტიფიკაცია
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "BoxFusion",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "BoxFusionUsers",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "BoxFusionSuperSecretKey1234567890!"))
    };
});

var app = builder.Build();

// 7. მონაცემთა ბაზის მიგრაცია და როლების შექმნა (ავტომატური)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<BoxFusionDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // ავტომატური მიგრაცია
        db.Database.Migrate();

        // როლების შექმნა
        string[] roles = { "Admin", "Customer" };
        foreach (var role in roles)
        {
            if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "ბაზის მიგრაციისას ან როლების შექმნისას მოხდა შეცდომა.");
    }
}

// 8. Middleware-ების რიგითობა
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Production-შიც რომ ჩანდეს Swagger (თუ გინდა), ამოიტანე if-ის გარეთ
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();