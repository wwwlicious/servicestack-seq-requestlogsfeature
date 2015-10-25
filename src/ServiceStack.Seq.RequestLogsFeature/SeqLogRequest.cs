namespace ServiceStack.Seq.RequestLogsFeature
{
    public class SeqLogRequest
    {
        public SeqLogRequest(params SeqRequestLogEntry[] events)
        {
            Events = events;
        }

        public SeqRequestLogEntry[] Events { get; set; }
    }
}