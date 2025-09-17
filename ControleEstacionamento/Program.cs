using ControleEstacionamento.Data;
using Microsoft.EntityFrameworkCore;
using ControleEstacionamento.Services;
using Microsoft.OpenApi.Models;  // Add this for Swagger

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ControleEstacionamento API", Version = "v1" });
});

// Add the database context with SQLite
builder.Services.AddDbContext<EstacionamentoContext>(options =>
    options.UseSqlite("Data Source=estacionamento.db"));

builder.Services.AddScoped<TabelaPrecoService>();
builder.Services.AddScoped<EstacionamentoService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ControleEstacionamento API V1");
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
