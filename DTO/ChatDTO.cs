namespace reviewApi.DTO
{
    public class ChatMessageDTO
    {
        public int id { get; set; }
        public string sender { get; set; }
        public string? avatar { get; set; }
        public string message { get; set; }
        public string timestamp { get; set; }
        public List<ChatAttachmentDTO>? attachments { get; set; }
        public int? replyToMessageId { get; set; }
        public string? replyToMessagePreview { get; set; }
        public string? typeChat { get; set; }
    }

    public class ChatAttachmentDTO
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}
