using Grpc.Core;
using Grpc.Core.Interceptors;
using Polly;
using Polly.Timeout;

namespace OrderService.Infrastructure.Resilience
{
    public class GrpcResilienceInterceptor : Interceptor
    {
        private readonly ILogger<GrpcResilienceInterceptor> _logger;

        // Circuit breaker must be SINGLETON (important!)
        private readonly IAsyncPolicy _circuitBreaker;

        public GrpcResilienceInterceptor(ILogger<GrpcResilienceInterceptor> logger)
        {
            _logger = logger;

            _circuitBreaker = Policy
                .Handle<RpcException>()
                .CircuitBreakerAsync(
                    3,
                    TimeSpan.FromSeconds(10),
                    onBreak: (ex, breakDelay) =>
                    {
                        _logger.LogError("🚨 Circuit OPEN for {Seconds}s due to {Error}",
                            breakDelay.TotalSeconds,
                            ex.Message);
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("✅ Circuit CLOSED");
                    },
                    onHalfOpen: () =>
                    {
                        _logger.LogInformation("⚡ Circuit HALF-OPEN");
                    });
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
            where TRequest : class
            where TResponse : class
        {
            // 🔁 Retry Policy
            var retryPolicy = Policy
                .Handle<RpcException>()
                .WaitAndRetryAsync(
                    3,
                    retry => TimeSpan.FromSeconds(Math.Pow(2, retry)),
                    (ex, delay, retryCount, ctx) =>
                    {
                        _logger.LogWarning(
                            "🔁 Retry {Retry} after {Delay}s due to {Error}",
                            retryCount,
                            delay.TotalSeconds,
                            ex.Message);
                    });

            // ⏱ Timeout Policy
            var timeoutPolicy = Policy
                .TimeoutAsync(
                    TimeSpan.FromSeconds(5),
                    TimeoutStrategy.Pessimistic,
                    onTimeoutAsync: (context, timespan, task, ex) =>
                    {
                        _logger.LogError("⏱ Timeout after {Seconds}s", timespan.TotalSeconds);
                        return Task.CompletedTask;
                    });

            // 🔥 Combine policies
            var policyWrap = Policy.WrapAsync(retryPolicy, _circuitBreaker, timeoutPolicy);

            async Task<TResponse> ExecuteAsync()
            {
                return await policyWrap.ExecuteAsync(async () =>
                {
                    var call = continuation(request, context);
                    return await call.ResponseAsync;
                });
            }

            return new AsyncUnaryCall<TResponse>(
                ExecuteAsync(),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
        }
    }
}