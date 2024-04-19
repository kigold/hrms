using System.Net.Sockets;

var builder = DistributedApplication.CreateBuilder(args);

// Service registration

var messaging = builder.AddRabbitMQ("RabbitMQConnection")
    .WithAnnotation(new ContainerImageAnnotation { Image = "rabbitmq", Tag = "3-management" })
    .WithAnnotation(new EndpointAnnotation(ProtocolType.Tcp, port: 15672, targetPort: 15672, uriScheme: "http"));
    //.WithEnvironment("RABBITMQ_DEFAULT_PASS", "guest")
    //.WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")


var authAPI = builder.AddProject<Projects.Auth_API>("authapi")
    .WithReference(messaging);

// Angular: npm run start
//builder.AddNpmApp("angular", "../HRMS.Angular")
//    .WithReference(authAPI)
//    .WithEndpoint(containerPort: 3000, scheme: "http", env: "PORT");
//AsDockerfileInManifest();

builder.AddProject<Projects.Employees_API>("employeesapi")
    .WithReference(messaging);
    //AsDockerfileInManifest();

builder.Build().Run();

