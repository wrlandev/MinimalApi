using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("connect/token", (Authentication authentication, IConfiguration configuration) =>
{
    if(authentication.user == "client" && authentication.password == "123")
    {
        return Token.Create(configuration, "Client");
    }

    return "user not found";
});

app.Run();

record Authentication(string user, string password);

public static class Token
{
    public static object Create(IConfiguration configuration, string role)
    {
        var key = Encoding.ASCII.GetBytes(configuration["Authentication:Key"]);

        var tokenConfig = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, role)
            }),
            Expires = DateTime.UtcNow.AddHours(3),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenConfig);
        var tokenString = tokenHandler.WriteToken(token);

        return new
        {
            token = tokenString,
        };
    }
}
