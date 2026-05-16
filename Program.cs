using Microsoft.EntityFrameworkCore;
using OnlineSchool.Models;
 
var builder = WebApplication.CreateBuilder(args);
 
builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddDbContext<SchoolContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
 
var app = builder.Build();
 
app.UseRouting();
 
app.MapControllers();
 
app.Run();