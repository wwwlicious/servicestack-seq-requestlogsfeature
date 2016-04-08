// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ConsoleDemo
{
    using System;

    using ServiceStack;

    public class DemoService : IService, IAny<Hello>
    {
        public object Any(Hello request)
        {
            request.Name.ThrowIfNullOrEmpty();
            if(request.Name.EndsWith("spoon")) throw HttpError.NotFound("there is no spoon");
            if(request.Name.EndsWith("server")) throw new ApplicationException("Entirely deliberate exception");
            return new HelloResponse(request.Name);
        }
    }

    public class Hello : IReturn<HelloResponse>
    {
        public Hello(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    public class HelloResponse
    {
        public HelloResponse(string greeting)
        {
            Greeting = $"Hello {greeting}";
        }

        public string Greeting { get; set; }
    }
}