namespace ServiceStack.Seq.RequestLogsFeature
{
    [Route("/SeqRequestLogConfig")]
    public class SeqRequestLogConfig : IReturn<SeqRequestLogConfig>
    {
        public bool? Enabled { get; set; }
        public bool? EnableSessionTracking { get; set; }
        public bool? EnableRequestBodyTracking { get; set; }
        public bool? EnableResponseTracking { get; set; }
        public bool? EnableErrorTracking { get; set; }
    }
}
