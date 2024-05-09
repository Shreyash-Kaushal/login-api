using loginapi.Repository;
using loginapi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
byte[] keyBytes = new byte[32];
using (var rng = RandomNumberGenerator.Create())
{
    rng.GetBytes(keyBytes);
}

string secretKey = Convert.ToBase64String(keyBytes);
string issuer = builder.Configuration["JwtSettings:Issuer"];
string audience = builder.Configuration["JwtSettings:Audience"];
int expiryInMinutes = builder.Configuration.GetValue<int>("JwtSettings:ExpiryInMinutes"); 
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Add services to the container.
builder.Services.AddSingleton<UserRepository>(new UserRepository(connectionString));
builder.Services.AddSingleton<JwtService>(new JwtService(secretKey, issuer, audience, expiryInMinutes));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("allow frontend", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
                      .AllowAnyMethod()
                      .AllowAnyHeader();
    });
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "https://localhost:44360",
        ValidAudience = "https://localhost:44360/api", 
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("local-testing")) 
    };
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("allow frontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.Run();
