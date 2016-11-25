namespace ServiceStack.Seq.RequestLogsFeature.Tests
{
    using System;
    using System.Collections.Generic;
    using Configuration;
    using FakeItEasy;
    using FluentAssertions;
    using Web;
    using Xunit;
    using static FakeItEasy.A;

    [Collection("AppHost")]
    public class SeqRequestLogsFeatureTests
    {
        private readonly IAppSettings settings;
        private readonly IAppHost appHost;
        private const string Url = "http://8.8.8.8:1234";

        public SeqRequestLogsFeatureTests()
        {
            settings = Fake<IAppSettings>();

            appHost = Fake<IAppHost>();
            CallTo(() => appHost.PreRequestFilters).Returns(new List<Action<IRequest, IResponse>>());
            CallTo(() => appHost.AppSettings).Returns(settings);
            CallTo(() => settings.GetString(ConfigKeys.SeqUrl)).Returns(Url);
        }

        private SeqRequestLogsFeature GetFeature()
        {
            var feature = new SeqRequestLogsFeature();
            feature.Register(appHost);
            return feature;
        }

        [Fact]
        public void SeqUrl_Get_GetsFromAppSettings()
        {
            CallTo(() => settings.GetString(ConfigKeys.SeqUrl)).Returns(Url);

            GetFeature().SeqUrl.Should().Be(Url);
            CallTo(() => settings.GetString(ConfigKeys.SeqUrl)).MustHaveHappened();
        }

        [Fact]
        public void SeqUrl_Set_SetsInAppSettings()
        {
            GetFeature().SeqUrl = Url;
            CallTo(() => settings.Set(ConfigKeys.SeqUrl, Url)).MustHaveHappened();
        }

        [Fact]
        public void ApiKey_Get_GetsFromAppSettings()
        {
            const string apiKey = "apiapi";
            CallTo(() => settings.GetString(ConfigKeys.ApiKey)).Returns(apiKey);

            GetFeature().ApiKey.Should().Be(apiKey);
            CallTo(() => settings.GetString(ConfigKeys.ApiKey)).MustHaveHappened();
        }

        [Fact]
        public void ApiKey_Set_SetsInAppSettings()
        {
            const string apiKey = "apiapi";

            GetFeature().ApiKey = apiKey;
            CallTo(() => settings.Set(ConfigKeys.ApiKey, apiKey)).MustHaveHappened();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Enabled_Get_GetsFromAppSettings(bool enabled)
        {
            CallTo(() => settings.Get(ConfigKeys.Enabled, true)).Returns(enabled);

            GetFeature().Enabled.Should().Be(enabled);
            CallTo(() => settings.Get(ConfigKeys.Enabled, true)).MustHaveHappened();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Enabled_Set_SetsInAppSettings(bool enabled)
        {
            GetFeature().Enabled = enabled;
            CallTo(() => settings.Set(ConfigKeys.Enabled, enabled)).MustHaveHappened();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EnableErrorTracking_Get_GetsFromAppSettings(bool enabled)
        {
            CallTo(() => settings.Get(ConfigKeys.EnableErrorTracking, true)).Returns(enabled);

            GetFeature().EnableErrorTracking.Should().Be(enabled);
            CallTo(() => settings.Get(ConfigKeys.EnableErrorTracking, true)).MustHaveHappened();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EnableErrorTracking_Set_SetsInAppSettings(bool enabled)
        {
            GetFeature().EnableErrorTracking = enabled;
            CallTo(() => settings.Set(ConfigKeys.EnableErrorTracking, enabled)).MustHaveHappened();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EnableRequestBodyTracking_Get_GetsFromAppSettings(bool enabled)
        {
            CallTo(() => settings.Get<bool>(ConfigKeys.EnableRequestBodyTracking)).Returns(enabled);

            GetFeature().EnableRequestBodyTracking.Should().Be(enabled);
            CallTo(() => settings.Get<bool>(ConfigKeys.EnableRequestBodyTracking)).MustHaveHappened();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EnableRequestBodyTracking_Set_SetsInAppSettings(bool enabled)
        {
            GetFeature().EnableRequestBodyTracking = enabled;
            CallTo(() => settings.Set(ConfigKeys.EnableRequestBodyTracking, enabled)).MustHaveHappened();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EnableSessionTracking_Get_GetsFromAppSettings(bool enabled)
        {
            CallTo(() => settings.Get<bool>(ConfigKeys.EnableSessionTracking)).Returns(enabled);

            GetFeature().EnableSessionTracking.Should().Be(enabled);
            CallTo(() => settings.Get<bool>(ConfigKeys.EnableSessionTracking)).MustHaveHappened();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EnableSessionTracking_Set_SetsInAppSettings(bool enabled)
        {
            GetFeature().EnableSessionTracking = enabled;
            CallTo(() => settings.Set(ConfigKeys.EnableSessionTracking, enabled)).MustHaveHappened();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EnableResponseTracking_Get_GetsFromAppSettings(bool enabled)
        {
            CallTo(() => settings.Get<bool>(ConfigKeys.EnableResponseTracking)).Returns(enabled);

            GetFeature().EnableResponseTracking.Should().Be(enabled);
            CallTo(() => settings.Get<bool>(ConfigKeys.EnableResponseTracking)).MustHaveHappened();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EnableResponseTracking_Set_SetsInAppSettings(bool enabled)
        {
            GetFeature().EnableResponseTracking = enabled;
            CallTo(() => settings.Set(ConfigKeys.EnableResponseTracking, enabled)).MustHaveHappened();
        }
        
        [Fact]
        public void RequiredRoles_Get_GetsFromAppSettings()
        {
            var list = new List<string> { "foo", "bar" };
            CallTo(() => settings.GetList(ConfigKeys.RequiredRoles)).Returns(list);

            GetFeature().RequiredRoles.Should().BeEquivalentTo(list);
            CallTo(() => settings.GetList(ConfigKeys.RequiredRoles)).MustHaveHappened();
        }

        [Fact]
        public void RequiredRoles_Set_SetsInAppSettings()
        {
            var list = new List<string> { "foo", "bar" };

            GetFeature().RequiredRoles = list;
            CallTo(() => settings.Set(ConfigKeys.RequiredRoles, list)).MustHaveHappened();
        }
    }
}
