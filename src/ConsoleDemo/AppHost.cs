// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ConsoleDemo
{
    using System.Collections.Generic;

    using Funq;

    using ServiceStack;
    using ServiceStack.Seq.RequestLogsFeature;

    public class AppHost : AppHostBase
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
            // Config examples
            //Plugins.Add(new RequestCorrelationFeature());
            Plugins.Add(new SeqRequestLogsFeature
            {
                ApiKey = "Overwrite-app-setting",
                // add additional properties to Seq log entry.
                AppendProperties =
                    (request, dto, response, duration) =>
                    new Dictionary<string, object> { { "NewCustomProperty", "42" } },
                ExcludeRequestDtoTypes = new[] { typeof(SeqRequestLogConfig) }, // add your own type exclusions
                HideRequestBodyForRequestDtoTypes = new[] { typeof(SeqRequestLogConfig) } // add your own exclusions for bodyrequest logging
            });
        }
    }
}
