namespace MessageService.Models
{
    public class Message
    {
        public required string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public int SequenceNumber { get; set; }
    }
}
