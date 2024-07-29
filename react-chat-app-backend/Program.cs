using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using react_chat_app_backend;
using react_chat_app_backend.Context;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration["ConnectionString"];

// Add services to the container
builder.Services.AddDbContext<AppDbContext>(optionsBuilder => 
    optionsBuilder.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    //options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    //options.JsonSerializerOptions.PropertyNamingPolicy = new LowerCaseNamingPolicy();
    //options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

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

app.UseWebSockets();

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();