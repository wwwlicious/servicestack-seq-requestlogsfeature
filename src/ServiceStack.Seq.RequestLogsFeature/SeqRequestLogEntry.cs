namespace ServiceStack.Seq.RequestLogsFeature
{
    using System.Collections.Generic;

    /// <summary>
    /// A log entry added by the IRequestLogger
    /// </summary>
    public class SeqRequestLogEntry
    {
        public SeqRequestLogEntry()
        {
            MessageTemplate = "Servicestack SeqRequestLogsFeature";
            Properties = new Dictionary<string, object>();
        }
        public string Timestamp { get; set; }
        public string Level { get; set; } = "Debug";
        public Dictionary<string, object> Properties { get; }
        public string MessageTemplate { get; set; }
    }
}