using Employees.API.Services;
using MassTransit;
using Shared.Data;
using Shared.Messaging;
using System.Text.Json;
using static Shared.Messaging.PubMessageType;

namespace Employees.API.Messaging
{
    public class FileProcessorConsumer : IConsumer<PublishMessage>
    {
        private readonly ILogger _logger;
        private readonly IEmployeeService _employeeService;

        public FileProcessorConsumer() { }

        public FileProcessorConsumer(ILogger<EmployeeMessageConsumer> logger,
            IEmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;
        }

        public async Task Consume(ConsumeContext<PublishMessage> context)
        {
            try
            {
                _logger.LogDebug("Received: {message}", context.Message.Message);
                switch (context.Message.MessageType)
                {
                    case FILE_PROCESS_DELETE_FILE:
                        await DeleteFile(context.Message);
                        break;
                    default:
                        _logger.LogWarning("No handler for message: {message}", context.Message);
                        return;
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        private async Task DeleteFile(PublishMessage message)
        {
            var request = JsonSerializer.Deserialize<MediaFile>(message.Message);
            await _employeeService.DeleteFile(request!);
        }
    }
}
