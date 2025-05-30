using Microsoft.Extensions.FileProviders;
using WorkoutTracker.API.Extensions;
using WorkoutTracker.API.Middlewares;
using WorkoutTracker.Infrastructure.Providers;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

var services = builder.Services;

services.AddControllersWithOptions();

services.AddDistributedMemoryCache();
services.AddSession();

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddHttpContextAccessor();
services.AddProjectServices(config);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStaticFiles();

await app.InitializeDataAsync(config);

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();
app.MapControllers();

app.Run();
