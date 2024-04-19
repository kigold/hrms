using Employees.API;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.ConfigureAuth(builder.Configuration);
builder.Services.AddApplicationDependencies(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.MapCustomEnpoints();

//app.UseCors(x =>
//{
//    x.WithOrigins("http://localhost:4200")
//    //x.AllowAnyOrigin()
//        .AllowAnyMethod()
//        .AllowAnyHeader();
//    //.AllowCredentials();
//});

app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.Run();
