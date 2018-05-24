using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;

namespace Olive
{
    partial class ApiClient
    {
        int retries, ExceptionsBeforeBreakingCircuit;
        TimeSpan CircuitBreakDuration, RetryPauseDuration;

        static readonly Dictionary<string, Policy> CircuitBreakerPolicies = new Dictionary<string, Policy>();

        /// <summary>
        /// Sets the number of retries before giving up. Default is zero.
        /// </summary>
        public ApiClient Retries(int paramRetries, int pauseMilliseconds = 100)
        {
            this.retries = paramRetries;
            this.RetryPauseDuration = pauseMilliseconds.Milliseconds();
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

        async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpRequestMessage request)
        {
            Policy policy;

            var policyKey = request.RequestUri.Host + "|" + ExceptionsBeforeBreakingCircuit + "|" + CircuitBreakDuration;

            // Get a shared policy for the Api base url:
            lock (CircuitBreakerPolicies)
            {
                if (!CircuitBreakerPolicies.TryGetValue(policyKey, out policy))
                {
                    if (ExceptionsBeforeBreakingCircuit <= 0)
                        policy = Policy.Handle<HttpRequestException>()
                            .WaitAndRetryAsync(retries, attempt => RetryPauseDuration);
                    else
                        policy = Policy.Handle<HttpRequestException>()
                                   .CircuitBreakerAsync(ExceptionsBeforeBreakingCircuit, CircuitBreakDuration);

                    CircuitBreakerPolicies.Add(policyKey, policy);
                }
            }

            return await policy.ExecuteAsync(() => client.SendAsync(request));
        }
    }
}