using Microsoft.Extensions.FileProviders;
using WorkoutTrackerAPI.Providers;
using WorkoutTrackerAPI.Services.FileServices;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

var services = builder.Services;

services.AddControllersWithOptions();

services.AddDistributedMemoryCache();
services.AddSession();

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

services.AddMSSQLServer(config);

services.AddTransient<IFileService, FileService>();
services.AddIdentityModels();

services.AddJWTAuthentication(config);
services.AddHttpContextAccessor();
services.AddAccountServices();

services.AddWorkoutModelServices();

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
