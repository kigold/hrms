using Auth.API.Data.Models;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Shared.Messaging;
using System.Text.Json;
using static Shared.Messaging.PubMessageType;

namespace Auth.API.Messaging
{
    public class AuthMessageConsumer : IConsumer<PublishMessage>
    {
        private readonly ILogger _logger;
        private readonly UserManager<User> _userManager;
        private readonly IBus _bus;

        public AuthMessageConsumer(ILogger<AuthMessageConsumer> logger,
            UserManager<User> userManager,
            IBus bus)
        {
            _logger = logger;
            _userManager = userManager;
            _bus = bus;
        }

        public async Task Consume(ConsumeContext<PublishMessage> context)
        {
            try
            {
                _logger.LogDebug("Received: {message}", context.Message.Message);
                switch (context.Message.MessageType)
                {
                    case EMPLOYEE_CREATE_USER:
                        await CreateUser(context.Message);
                        break;
                    case EMPLOYEE_UPDATE_USER:
                        await UpdateUser(context.Message);
                        break;
                    case EMPLOYEE_DELETE_USER:
                        await DeleteUser(context.Message);
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

        private async Task CreateUser(PublishMessage message)
        {
            var request = JsonSerializer.Deserialize<EmployeeDTO>(message.Message);
            if (await _userManager.FindByNameAsync(request!.Email) is not null)
                _logger.LogInformation("User already exists {email}", request!.Email);

            var user = new User
            {
                Email = request!.Email,
                UserName = request!.Email,
                Firstname = request!.FirstName,
                Lastname = request!.LastName,
            };

            var hash = _userManager.PasswordHasher.HashPassword(user, "P@ssw0rd"); //TODO
            user.PasswordHash = hash;
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogInformation("Failed to create User {user} {errors}", user, result.Errors);
            }

            var pubMessage = JsonSerializer.Serialize(new EmployeeStaffIdDTO(user.Id, user.Email));
            await _bus.Publish(new PublishMessage(pubMessage, EMPLOYEE_SET_STAFF_ID));
        }

        private async Task UpdateUser(PublishMessage message)
        {
            var request = JsonSerializer.Deserialize<EmployeeDTO>(message.Message);
            var user = await _userManager.FindByNameAsync(request!.Email);
            if (user is null)
            {
                _logger.LogInformation("User does not exists {email}", request!.Email);
                return;
            }

            user.Firstname = request!.FirstName;
            user.Lastname = request.LastName;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogInformation("Failed to Update User {user} {errors}", user, result.Errors);
            }
        }

        private async Task DeleteUser(PublishMessage message)
        {
            var request = JsonSerializer.Deserialize<EmployeeDTO>(message.Message);
            var user = await _userManager.FindByNameAsync(request!.Email);
            if (user is null)
            {
                _logger.LogInformation($"User not found{request!.Email}");
                return;
            }

            var result = await _userManager.DeleteAsync(user!);
            if (!result.Succeeded)
            {
                _logger.LogInformation($"Failed to delete User {user.Email}", result.Errors, user);
            }
        }
    }
}