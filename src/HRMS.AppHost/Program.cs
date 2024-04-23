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

var employeeAPI = builder.AddProject<Projects.Employees_API>("employeesapi")
    .WithReference(messaging);
//AsDockerfileInManifest();

var gateway = builder.AddProject<Projects.Yarp_Gateway>("yarp-gateway")
    .WithReference(authAPI)
    .WithReference(employeeAPI);

// Angular: npm run start
builder.AddNpmApp("angular", "../HRMS.Angular")
    .WithReference(gateway)
    .WithHttpEndpoint(targetPort: 3000, env: "PORT")
    //.PublishAsDockerFile()
    ;

builder.Build().Run();

