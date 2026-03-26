using Polly;
using Polly.Extensions.Http;

namespace OrderService.Infrastructure.Resilience
{
    public static class PollyPolicies
    {
        // ✅ Retry Policy
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<Grpc.Core.RpcException>()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (outcome, timespan, retryCount, context) =>
                    {
                        logger.LogWarning(
                            "🔁 Retry {RetryCount} after {Delay}s due to {Error}",
                            retryCount,
                            timespan.TotalSeconds,
                            outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()
                        );
                    });
        }

        // ✅ Circuit Breaker Policy
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger logger)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<Grpc.Core.RpcException>()
                .CircuitBreakerAsync(
                    3,
                    TimeSpan.FromSeconds(10),
                    onBreak: (outcome, breakDelay) =>
                    {
                        logger.LogError(
                            "🚨 Circuit OPEN for {Seconds}s due to {Error}",
                            breakDelay.TotalSeconds,
                            outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()
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
