using CRMApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure logging to capture SQL queries
builder.Logging.AddConsole();  // Log to Console
builder.Logging.AddDebug();    // Log to Debug Output

//Database connectivity
AppSetting App = Util.AppSetting;
string ConString = builder.Configuration.GetConnectionString(App.DbName)!;
builder.Services.AddDbContext<DbCRM>(options =>
{
    switch (App.DbName) 
    {
        case "Oracle":            
            options.UseOracle(ConString).LogTo(Console.WriteLine, LogLevel.Information);
            break;
        case "SqlServer":
            options.UseSqlServer(ConString);
            break;
        case "MySql":
            options.UseMySql(ConString,
            ServerVersion.AutoDetect(ConString),
            mySqlOptions =>
                mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(300),
                    errorNumbersToAdd: null)
            );
            break;
        default:
            throw new Exception("Invalid Database Type");
    }
});

builder.Logging.AddFile("Logs/SqlLogEGatePass.txt");

//Define Json Naming Policy
builder.Services.AddMvc(options =>
{
    options.MaxModelBindingCollectionSize = 100000;
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(sw =>
{
    sw.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JWT Token",
        Version = "v1",
    });
    sw.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter Bearer Token (Example : Bearer[space]Token Value): "
    });
    sw.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{ }
        }
    });
});
//Add Jwt Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = App.Jwt.Issuer,
        ValidAudience = App.Jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(App.Jwt.Key)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

//Add Cros Policy
builder.Services.AddCors(option =>
    option.AddPolicy("CorsPolicy", build =>
    {
        //build.WithOrigins(Generic.AppSetting.CrosOrigin).AllowAnyMethod().AllowAnyHeader();
        build.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        //build.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    })
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.ConfigObject.AdditionalItems["persistAuthorization"] = true; });
}
else 
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.ConfigObject.AdditionalItems["persistAuthorization"] = true; });
}
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
