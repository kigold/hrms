var builder = DistributedApplication.CreateBuilder(args);

var authAPI = builder.AddProject<Projects.Auth_API>("auth.api");

// Angular: npm run start
builder.AddNpmApp("angular", "../HRMS.Angular")
    .WithReference(authAPI)
    .WithEndpoint(containerPort: 3000, scheme: "http", env: "PORT");
    //AsDockerfileInManifest();

builder.Build().Run();
