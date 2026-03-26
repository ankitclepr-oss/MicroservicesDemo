using Polly;
using System.Net.Http;

namespace OrderService.Infrastructure.Resilience
{
    public static class PollyPolicies
    {
        // ✅ Retry (works for gRPC)
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger)
        {
            return Policy<HttpResponseMessage>
                .Handle<Exception>() // 🔥 IMPORTANT: catches RpcException
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (outcome, timespan, retryCount, context) =>
                    {
                        logger.LogWarning(
                            "🔁 Retry {RetryCount} after {Delay}s due to {Error}",
                            retryCount,
                            timespan.TotalSeconds,
                            outcome.Exception?.Message
                        );
                    });
        }

        // ✅ Circuit Breaker (works for gRPC)
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger logger)
        {
            return Policy<HttpResponseMessage>
                .Handle<Exception>() // 🔥 IMPORTANT
                .CircuitBreakerAsync(
                    3,
                    TimeSpan.FromSeconds(10),
                    onBreak: (outcome, breakDelay) =>
                    {
                        logger.LogError(
                            "🚨 Circuit OPEN for {Seconds}s due to {Error}",
                            breakDelay.TotalSeconds,
                            outcome.Exception?.Message
                        );
                    },
                    onReset: () =>
                    {
                        logger.LogInformation("✅ Circuit CLOSED");
                    },
                    onHalfOpen: () =>
                    {
                        logger.LogInformation("⚡ Circuit HALF-OPEN");
                    });
        }
    }
}