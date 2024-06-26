﻿namespace Shared.Messaging
{
    //public record PublishMessage(
    //    string Message,
    //    PubMessageType MessageType,
    //    Guid Id = new Guid(),
    //    DateTime? CreatedDate = DateTime.Now
    //    );

    public class PublishMessage
    {
        public PublishMessage(string message,
            string messageType) 
        {
            Message = message;
            MessageType = messageType;
        }

        public string Message { get; set; }
        public string MessageType { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
