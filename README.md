# ServiceStack.Seq.RequestLogsFeature

[![Build status](https://ci.appveyor.com/api/projects/status/89pfhb02b0psi80e/branch/master?svg=true)](https://ci.appveyor.com/project/wwwlicious/servicestack-seq-requestlogsfeature/branch/master)
[![NuGet version](https://badge.fury.io/nu/ServiceStack.Seq.RequestLogsFeature.svg)](https://badge.fury.io/nu/ServiceStack.Seq.RequestLogsFeature)

A ServiceStack plugin that logs requests to [Seq](http://getseq.net). For more details view the [blog post](http://wwwlicious.com/2015/10/25/logging-servicestack-requests-with-seq/)

*NB. This version is compatible with ServiceStack v4.x. For v3 compatibility, use the v3 branch*

# Installing

The package is available from nuget.org

`Install-Package ServiceStack.Seq.RequestLogsFeature`

# Requirements

You must have an instance of seq server to post to. You can download and install a copy of Seq [here](http://getseq.net).

Check if you have it running locally on the default port [http://localhost:5341](http://localhost:5341)

# Quick Start

In your `AppHost` class `Configure` method, add the plugin and specify your seq url.
You can futher customise the configuration options using the example below

```csharp
public override void Configure(Container container)
{
    // Config examples
    Plugins.Add(
        new SeqRequestLogsFeature(
            new SeqRequestLogsSettings("http://localhost:5341") // required seq server url:port
                // everything else is optional
                .ApiKey("seqApiKey")            // api key for seq
                .Enabled()                      // default true
                .EnableErrorTracking()          // default true
                .EnableSessionTracking()        // default false
                .EnableRequestBodyTracking()    // default false
                .EnableResponseTracking()       // default false
                .ExcludeRequestDtoTypes(typeof(SeqRequestLogConfig)) // add your own type exclusions
                .HideRequestBodyForRequestDtoTypes(typeof(SeqRequestLogConfig)) // add your own exclusions for bodyrequest logging
                .RequiredRoles("admin", "ops") // restrict the runtime configuration to specific roles
                .UseCustomLogger(new CustomLogger()) // swap out the seq logger for your own implementation
                .ClearExcludeRequestDtoTypes()  // remove default exclusions (RequestLog)
                .ClearHideRequestBodyForRequestDtoTypes() // remove default request body exclusions (Auth, Registration)
                .AppendProperties(
                     (request, dto, response, duration) =>
                        {
                            return new Dictionary<string, object>() { { "NewCustomProperty", "42" } }; //add additional properties to Seq log entry.
                        })
                .AddLogEvent(
                    (request, dto, response, duration) =>
                        {
                            // your custom log event
                        })));

}
```

### Request Correlation

This plugin will detect the default header `x-mac-requestid` created by [ServiceStack.Request.Correlation](https://github.com/MacLeanElectrical/servicestack-request-correlation)
and add this as a property. This is useful for tracking requests from their point of origin across multiple services

### Runtime configuration

You can change the logging configuration at runtime 

```csharp
var request = new SeqRequestLogConfig
                    {
                        Enabled = false,
                        EnableRequestBodyTracking = false,
                        EnableErrorTracking = false,
                        EnableSessionTracking = false,
                        EnableResponseTracking = false
                    };

var client = new JsonServiceClient("http://myservice");
client.Send(request);
```

### Metadata page

![Metadata](assets/SeqRequestLogger_Metadata.png)


### Logging in action

![Seq Request Logs](assets/Seq.png)
