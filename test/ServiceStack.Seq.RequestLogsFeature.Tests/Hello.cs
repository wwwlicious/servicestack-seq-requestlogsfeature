namespace ServiceStack.Seq.RequestLogsFeature.Tests
{
    [Route("/hello/{Name}")]
    public class Hello : IReturn<HelloResponse>, IGet
    {
        public Hello(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}