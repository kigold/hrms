using Employees.API.Services;
using MassTransit;
using Shared.Messaging;
using System.Text.Json;

namespace Employees.API.Messaging
{
    public class EmployeeMessageConsumer : IConsumer<PublishMessage>
    {
        private readonly ILogger _logger;
        private readonly IEmployeeService _employeeService;

        public EmployeeMessageConsumer()
        {

        }

        public EmployeeMessageConsumer(ILogger<EmployeeMessageConsumer> logger,
            IEmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;
        }

        public async Task Consume(ConsumeContext<PublishMessage> context)
        {
            try
            {
                _logger.LogInformation(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>Received: {message}", context.Message.Message); 
                if (context.Message.MessageType != PubMessageType.SetStaffId)
                {
                    _logger.LogInformation("No handler for message: {message}", context.Message);
                    return;
                }
                var message = JsonSerializer.Deserialize<EmployeeStaffIdDTO>(context.Message.Message);

                await _employeeService.UpdateEmployeeStaffId(message!);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }
    }
}