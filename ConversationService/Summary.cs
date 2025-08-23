namespace ConversationService.Models;

public class Summary
{
    public string id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public float[]? Embedding { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}