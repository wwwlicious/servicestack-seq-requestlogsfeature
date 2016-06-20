namespace ServiceStack.Seq.RequestLogsFeature.Tests
{
    using System;

    public class DemoService : IService, IAny<Hello>
    {
        public object Any(Hello request)
        {
            request.Name.ThrowIfNullOrEmpty();
            if (request.Name.EndsWith("spoon")) throw HttpError.NotFound("there is no spoon");
            if (request.Name.EndsWith("server")) throw new ApplicationException("Entirely deliberate exception");
            return new HelloResponse(request.Name);
        }
    }
}