namespace CartPaymentAPI.Models
{
    public class LoggingEntry
    {
        public DateTime Timestamp { get; set; }
        public string LogType { get; set; }
        public string Url { get; set; }
        public string CorrelationId { get; set; }
        public string ApplicationName { get; set; }
        public string Message { get; set; }
    }
}
