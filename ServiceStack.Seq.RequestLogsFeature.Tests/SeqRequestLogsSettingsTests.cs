namespace ServiceStack.Seq.RequestLogsFeature.Tests
{
    using FluentAssertions;
    using Xunit;

    public class SeqRequestLogsSettingsTests
    {
        private const string Url = "http://github.com:12345";
        private readonly SeqRequestLogsSettings seqRequestLogsSettings;

        public SeqRequestLogsSettingsTests()
        {
            seqRequestLogsSettings = new SeqRequestLogsSettings(Url);
        }

        [Fact]
        public void PopulateProperties_SetsUriInFeature()
        {
            var feature = new SeqRequestLogsFeature();
            feature.SeqUrl.Should().BeNullOrEmpty();

            seqRequestLogsSettings.PopulateProperties(feature);

            feature.SeqUrl.Should().Be(Url);
        }

        [Fact]
        public void PopulateProperties_SetsApiKeyInFeature()
        {
            const string apiKey = "445566";
            var feature = new SeqRequestLogsFeature();

            seqRequestLogsSettings.ApiKey(apiKey);

            feature.ApiKey.Should().BeNullOrEmpty();

            seqRequestLogsSettings.PopulateProperties(feature);

            feature.ApiKey.Should().Be(apiKey);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void PopulateProperties_DoesNotSetsApiKey_IfNotSet(string settingsValue)
        {
            const string apiKey = "445566";
            var feature = new SeqRequestLogsFeature { ApiKey = apiKey };

            seqRequestLogsSettings.PopulateProperties(feature);

            feature.ApiKey.Should().Be(apiKey);
        }

        [Fact]
        public void PopulateProperties_SetsAppendPropertiesInFeature()
        {
            var feature = new SeqRequestLogsFeature();

            seqRequestLogsSettings.AppendProperties((request, dto, response, duration) => null);

            feature.AppendProperties.Should().BeNull();

            seqRequestLogsSettings.PopulateProperties(feature);

            feature.AppendProperties.Should().NotBeNull();
        }
    }
}
