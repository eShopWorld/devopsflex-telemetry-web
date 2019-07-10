using System;

namespace Eshopworld.Web
{
    /// <summary>
    /// configuration type for EswDnsPolicy as built by <see cref="EvoFallbackPollyPolicyBuilder"/>
    /// </summary>
    public class EvoFallbackPollyPolicyConfiguration
    {
        public TimeSpan WaitTimeBetweenRetries { get; set; }
        public int RetriesPerLevel { get; set; }
        public TimeSpan RetryTimeOut { get; set; }

        internal bool Validate() =>
            WaitTimeBetweenRetries != default && RetriesPerLevel != default && RetryTimeOut != default;
    }

    public class EvoFallbackPollyPolicyConfigurationBuilder : IEvoFallbackPollyPolicyConfigurationBuilder
    {
        private readonly EvoFallbackPollyPolicyConfiguration _config = new EvoFallbackPollyPolicyConfiguration();

        public IEvoFallbackPollyPolicyConfigurationBuilder SetWaitTimeBetweenRetries(TimeSpan time)
        {
            _config.WaitTimeBetweenRetries = time;
            return this;
        }

        public IEvoFallbackPollyPolicyConfigurationBuilder SetRetriesPerLevel(int count)
        {
            _config.RetriesPerLevel = count;
            return this;
        }

        public IEvoFallbackPollyPolicyConfigurationBuilder SetRetryTimeOut(TimeSpan timeout)
        {
            _config.RetryTimeOut = timeout;
            return this;
        }

        public EvoFallbackPollyPolicyConfiguration Build()
        {
            return _config;
        }
    }

    public interface IEvoFallbackPollyPolicyConfigurationBuilder
    {
        IEvoFallbackPollyPolicyConfigurationBuilder SetWaitTimeBetweenRetries(TimeSpan time);
        IEvoFallbackPollyPolicyConfigurationBuilder SetRetriesPerLevel(int count);
        IEvoFallbackPollyPolicyConfigurationBuilder SetRetryTimeOut(TimeSpan timeout);
        EvoFallbackPollyPolicyConfiguration Build();
    }
}
