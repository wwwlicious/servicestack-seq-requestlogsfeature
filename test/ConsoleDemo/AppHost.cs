namespace ConsoleDemo
{
    using Funq;

    using ServiceStack;
    using ServiceStack.Seq.RequestLogsFeature;

    public class AppHost : AppSelfHostBase
    {
        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public AppHost() : base("ConsoleDemo", typeof(DemoService).Assembly)
        {
        }

        /// <summary>
        /// Application specific configuration
        /// This method should initialize any IoC resources utilized by your web service classes.
        /// </summary>
        /// <param name="container"></param>
        public override void Configure(Container container)
        {
            //Config examples
            string seqApiKey = null; // optional, enter a seq api key
            Plugins.Add(new SeqRequestLogsFeature("http://localhost:5341", seqApiKey,true,true,true,true));
        }
    }
}
