using Polly;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Olive
{
    partial class ApiClient
    {
        static readonly Dictionary<string, AsyncCircuitBreakerPolicy> CircuitBreakerPolicies
            = new Dictionary<string, AsyncCircuitBreakerPolicy>();

        int retries, ExceptionsBeforeBreakingCircuit;
        TimeSpan CircuitBreakDuration, RetryPauseDuration;

        /// <summary>
        /// Sets the number of retries before giving up. Default is zero.
        /// </summary>
        public ApiClient Retries(int retries, int pauseMilliseconds = 100)
        {
            this.retries = retries;
            RetryPauseDuration = pauseMilliseconds.Milliseconds();
            return this;
        }

        /// <summary>
        /// Prevents sending too many requests to an already failed remote service.
        /// If http exceptions are raised consecutively for the specified number of times,
        /// it will 'break the circuit' for the specified duration.
        /// During the break period, any attempt to execute a new request will immediately
        /// throw a BrokenCircuitException. Once the duration is over, if the first action
        /// throws http exception again, the circuit will break again for the same duration.
        /// Otherwise the circuit will reset.
        /// </summary>
        public ApiClient CircuitBreaker(int exceptionsBeforeBreaking = 5, int breakDurationSeconds = 10)
        {
            if (exceptionsBeforeBreaking < 1)
                throw new ArgumentException("exceptionsBeforeBreaking should be 1 or more.");

            if (breakDurationSeconds < 1)
                throw new ArgumentException("breakDurationSeconds should be 1 or more.");

            ExceptionsBeforeBreakingCircuit = exceptionsBeforeBreaking;
            CircuitBreakDuration = breakDurationSeconds.Seconds();

            return this;
        }

        /// <summary>
        /// Sends a request, retrying as configured. The request is created afresh for each attempt, as
        /// HttpClient refuses to send the same HttpRequestMessage twice.
        /// </summary>
        Task<HttpResponseMessage> SendAsync(HttpClient client, Func<HttpRequestMessage> createRequest, Action<Exception, int> onRetry)
        {
            return CreateExecutionPolicy(onRetry).ExecuteAsync(() => client.SendAsync(createRequest()));
        }

        AsyncPolicy CreateExecutionPolicy(Action<Exception, int> onRetry)
        {
            var retryPolicy = Policy.Handle<HttpRequestException>()
                             .WaitAndRetryAsync(retries, attempt => RetryPauseDuration,
                             (exception, pause, attempt, context) => onRetry?.Invoke(exception, attempt));

            if (ExceptionsBeforeBreakingCircuit <= 0) return retryPolicy;

            var host = Url.AsUri().Host;
            var policyKey = host + "|" + ExceptionsBeforeBreakingCircuit + "|" + CircuitBreakDuration;

            return retryPolicy.WrapAsync(GetOrCreateCircuitBreakerPolicy(policyKey, host));
        }

        AsyncCircuitBreakerPolicy GetOrCreateCircuitBreakerPolicy(string policyKey, string host)
        {
            if (CircuitBreakerPolicies.TryGetValue(policyKey, out var policy))
                return policy;

            lock (CircuitBreakerPolicies)
            {
                if (CircuitBreakerPolicies.TryGetValue(policyKey, out policy))
                    return policy;

                var failures = ExceptionsBeforeBreakingCircuit;

                // Shared by every client calling the host, so the log names the host rather than a url.
                policy = Policy.Handle<HttpRequestException>()
                    .CircuitBreakerAsync(ExceptionsBeforeBreakingCircuit, CircuitBreakDuration,
                        onBreak: (exception, duration) => Log.For<ApiClient>().Warning(exception,
                            $"Stopped calling {host} for {duration.ToNaturalTime()} after {failures} consecutive failures."),
                        onReset: () => Log.For<ApiClient>().Info($"Resumed calling {host}."));

                CircuitBreakerPolicies.Add(policyKey, policy);
                return policy;
            }
        }
    }
}