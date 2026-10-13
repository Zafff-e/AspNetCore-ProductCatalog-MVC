using Microsoft.EntityFrameworkCore;
using AspNetCore_ProductCatalog_MVC.Models;
using AspNetCore_ProductCatalog_MVC.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(op => op.UseSqlServer(builder.Configuration.GetConnectionString("con")));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(name: "Custom", pattern: "{controller=Products}/{action=Index}/{id?}");
app.MapControllerRoute(name:"default", pattern:"{controller=Home}/{action=Index}/{id?}");

app.Run();
