using WebApiPolly.Services.Provider1;
using WebApiPolly.Services.Provider2;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Add the Providers integration services
builder.Services.AddTransient<IProvider1Integration, Provider1Integration>();
builder.Services.AddTransient<IProvider2Integration, Provider2Integration>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
