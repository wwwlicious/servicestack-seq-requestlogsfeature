# ServiceStack.Seq.RequestLogsFeature

[![Build status](https://ci.appveyor.com/api/projects/status/89pfhb02b0psi80e/branch/master?svg=true)](https://ci.appveyor.com/project/wwwlicious/servicestack-seq-requestlogsfeature/branch/master)

A ServiceStack plugin that logs requests to [Seq](http://getseq.net). For more details view the [blog post](http://wwwlicious.com/2015/10/25/logging-servicestack-requests-with-seq/)

*NB. This version is compatible with ServiceStack v4.x. For v3 compatibility, use the v3 branch*

# Installing

The package is available from nuget.org

`Install-Package ServiceStack.Seq.RequestLogsFeature`

# Requirements

You must have an instance of seq server to post to. You can download and install a copy of Seq [here](http://getseq.net).

# Usage

In your app host add the plugin and specify your seq url and optional seq apiKey.

```csharp
public override void Configure(Container container)
{
  // Config examples
  Plugins.Add(new SeqRequestLogsFeature("http://localhost:5341", "seq-api-key"));
}
```

![Seq Request Logs](assets/Seq.png)
