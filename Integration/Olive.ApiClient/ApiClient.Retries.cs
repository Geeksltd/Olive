using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;

namespace Olive
{
    partial class ApiClient
    {
        int retries;
        PolicyBuilder Policy = Polly.Policy.Handle<HttpRequestException>();

        int ExceptionsBeforeBreakingCircuit;
        TimeSpan CircuitBreakDuration, RetryPauseDuration;

        static Dictionary<string, Polly.CircuitBreaker.CircuitBreakerPolicy> CircuitBreakers
            = new Dictionary<string, Polly.CircuitBreaker.CircuitBreakerPolicy>();

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
            CircuitBreakDuration = TimeSpan.FromSeconds(breakDurationSeconds);

            return this;
        }

        async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpRequestMessage request)
        {
            var policyBuilder = Polly.Policy.Handle<HttpRequestException>();

            Policy policy = policyBuilder.WaitAndRetryAsync(retries, attempt => RetryPauseDuration);

            if (ExceptionsBeforeBreakingCircuit > 0)
            {
                // Get a shared policy for the Api base url:
                var breakKey = request.RequestUri.Host + ExceptionsBeforeBreakingCircuit + "|" + CircuitBreakDuration;

                lock (CircuitBreakers)
                {
                    if (!CircuitBreakers.TryGetValue(breakKey, out var breakPolicy))
                    {
                        CircuitBreakers[breakKey] = breakPolicy =
                            policyBuilder.CircuitBreakerAsync(ExceptionsBeforeBreakingCircuit, CircuitBreakDuration);
                    }

                    policy = policy.WrapAsync(breakPolicy);
                }
            }

            return await policy.ExecuteAsync(() => client.SendAsync(request));
        }
    }
}