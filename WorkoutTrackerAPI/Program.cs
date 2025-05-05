using Microsoft.Extensions.FileProviders;
using WorkoutTracker.API.Extensions;
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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Data/Source")),
    RequestPath = "/Resources"
});

await app.InitializeDataAsync(config);

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();
app.MapControllers();

app.Run();
