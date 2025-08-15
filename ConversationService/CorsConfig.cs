namespace ConversationService
{
    public class CorsConfig
    {
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
        public string[] AllowedHeaders { get; set; } = Array.Empty<string>();
        public string[] AllowedMethods { get; set; } = Array.Empty<string>();
        public bool AllowCredentials { get; set; }
    }
}