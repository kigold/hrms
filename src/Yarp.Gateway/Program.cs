using Microsoft.Extensions.FileProviders;
using Shared.FileStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();

var fileSetting = builder.Configuration.GetSection(nameof(FileStorageSetting)).Get<FileStorageSetting>()!;
app.UseFileServer(new FileServerOptions
{
    FileProvider = new PhysicalFileProvider(fileSetting.BaseDirectory),
    //EnableDirectoryBrowsing = true
});

app.Run();
