using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using AMLCustomerPaymentsPortal.Server.Data; 
using AspNetCoreRateLimit; 
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer; 

var builder = WebApplication.CreateBuilder(args);

// 1. Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// 2. Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

// 3. Configure Rate Limiting
builder.Services.AddMemoryCache(); // Required by AspNetCoreRateLimit
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddHttpContextAccessor(); // Required by AspNetCoreRateLimit

// 4. Add Controllers and other services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 5. Configure Swagger to use JWT
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "AML Customer Payments API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});


var app = builder.Build();

// --- Configure the HTTP request pipeline ---

// Serve static files for the React frontend
app.UseDefaultFiles(); // Serves index.html for root path
app.UseStaticFiles();  // Serves files from wwwroot (where your React build output should go)

// Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY"); // Or "SAMEORIGIN"
    context.Response.Headers.Add("Referrer-Policy", "no-referrer-when-downgrade");
    await next();
});

// Use Rate Limiting
app.UseIpRateLimiting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AML API V1");
    });
    app.UseDeveloperExceptionPage(); // More detailed errors for development
}
else
{
    app.UseHsts(); // HTTP Strict Transport Security
}

app.UseHttpsRedirection();

app.UseRouting(); 


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.MapFallbackToFile("/index.html");

app.Run();