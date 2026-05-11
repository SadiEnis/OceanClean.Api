using OceanClean.Api.Data;
using OceanClean.Api.Repositories;
using OceanClean.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<MySqlConnectionFactory>();

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<PlayerProfileRepository>();
builder.Services.AddScoped<PlayerProfileService>();

builder.Services.AddScoped<MatchRepository>();
builder.Services.AddScoped<MatchService>();

builder.Services.AddScoped<ShopRepository>();
builder.Services.AddScoped<ShopService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();