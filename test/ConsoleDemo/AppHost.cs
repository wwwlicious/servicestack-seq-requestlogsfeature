namespace ConsoleDemo
{
    using Funq;

    using ServiceStack.Seq.RequestLogsFeature;
    using ServiceStack.WebHost.Endpoints;

    public class AppHost : AppHostHttpListenerBase
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
            Plugins.Add(new SeqRequestLogsFeature("http://localhost:5341"));
        }
    }
}
