using System.Net;
using System.Net.Http;
using System.Text;
using CadenceComponentLibraryAdmin.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace CadenceComponentLibraryAdmin.Tests;

public sealed class LcscOpenApiClientTests
{
    [Fact]
    public void GenerateSignature_MatchesDeterministicFixture()
    {
        var signature = LcscOpenApiClient.GenerateSignature(
            "7dc6035da7874b5b9bc245bd28017290",
            "63yeike7dy6c2kjd",
            "eiru73y343r36fdi",
            "1524662065");

        Assert.Equal("2de80a3a38e9d567326bd5f53599da1c4fba3aa6", signature);
    }

    [Fact]
    public async Task Client_DoesNotLogSecret_OnFailure()
    {
        var logger = new CaptureLogger<LcscOpenApiClient>();
        var client = new LcscOpenApiClient(
            new HttpClient(new ThrowingHandler()),
            Options.Create(new LcscOpenApiOptions
            {
                Enabled = true,
                ApiKey = "public-key",
                ApiSecret = "super-secret-value",
                BaseUrl = "https://ips.lcsc.com"
            }),
            logger);

        await client.GetProductInfoAsync("C2040");

        Assert.DoesNotContain("super-secret-value", string.Join("\n", logger.Messages), StringComparison.Ordinal);
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new HttpRequestException("boom");
    }

    private sealed class CaptureLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
            if (exception is not null)
            {
                Messages.Add(exception.ToString());
            }
        }
    }
}
