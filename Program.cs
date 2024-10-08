using BackendChat.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text;
using BackendChat.Services.UploadFilesServices;
using BackendChat.Hubs;
using BackendChat.Repositories.Interfaces;
using BackendChat.Services.Interfaces;
using BackendChat.Services.MailJet;
using BackendChat.Services.SendEmail;
using BackendChat.Helpers.Interfaces;
using BackendChat.Helpers;
using BackendChat.Repositories.UserAccount;
using BackendChat.Repositories.ChatRepository;
using BackendChat.Repositories.UserConnections;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
    options.ListenAnyIP(7210, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

//Configure a password secret using an environment variable
var conStrBuilder = new SqlConnectionStringBuilder(
        builder.Configuration.GetConnectionString("SQLString"));
conStrBuilder.Password = builder.Configuration["Chat:DbPassword"];
var connection = conStrBuilder.ConnectionString;

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IMailJet, MailJet>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISendEmailService, SendEmailService>();
builder.Services.AddScoped<ILoginRepository,  LoginRepository>();
builder.Services.AddScoped<IAdminTokenCode, AdminTokenCode>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IUploadImageService, UploadImageService>();
builder.Services.AddScoped<IUploadMediaService, UploadMediaService>();
builder.Services.AddScoped<IUserConnectionContext, UserConnectionContext>();

//Configure the JWT authentication
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
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Chat:JwtIssuer"],
        ValidAudience = builder.Configuration["Chat:JwtAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Chat:JwtKey"]!))
    };
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Connection String
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connection);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.Run();
