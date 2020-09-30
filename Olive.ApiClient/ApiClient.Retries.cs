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

        Task<HttpResponseMessage> SendAsync(HttpClient client, HttpRequestMessage request)
        {
            return CreateExecutionPolicy(request).ExecuteAsync(() => client.SendAsync(request));
        }

        AsyncPolicy CreateExecutionPolicy(HttpRequestMessage request)
        {
            var retryPolicy = Policy.Handle<HttpRequestException>()
                             .WaitAndRetryAsync(retries, attempt => RetryPauseDuration);

            if (ExceptionsBeforeBreakingCircuit <= 0) return retryPolicy;

            var policyKey = request.RequestUri.Host + "|" + ExceptionsBeforeBreakingCircuit + "|" + CircuitBreakDuration;

            return retryPolicy.WrapAsync(GetOrCreateCircuitBreakerPolicy(policyKey));
        }

        AsyncCircuitBreakerPolicy GetOrCreateCircuitBreakerPolicy(string policyKey)
        {
            if (CircuitBreakerPolicies.TryGetValue(policyKey, out var policy))
                return policy;

            lock (CircuitBreakerPolicies)
            {
                if (CircuitBreakerPolicies.TryGetValue(policyKey, out policy))
                    return policy;

                policy = Policy.Handle<HttpRequestException>()
                    .CircuitBreakerAsync(ExceptionsBeforeBreakingCircuit, CircuitBreakDuration);

                CircuitBreakerPolicies.Add(policyKey, policy);
                return policy;
            }
        }
    }
}