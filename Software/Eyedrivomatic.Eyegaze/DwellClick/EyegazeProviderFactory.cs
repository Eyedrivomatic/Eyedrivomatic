using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Eyedrivomatic.Logging;
using NullGuard;

namespace Eyedrivomatic.Eyegaze.DwellClick
{
    [Export(typeof(IEyegazeProviderFactory)), PartCreationPolicy(CreationPolicy.Shared)]
    public class EyegazeProviderFactory : IEyegazeProviderFactory
    {
        private readonly IList<Lazy<IEyegazeProvider, IEyegazeProviderMetadata>> _providersFactories;
        private readonly Dictionary<string, IEyegazeProvider> _providers = new Dictionary<string, IEyegazeProvider>();

        [ImportingConstructor]
        public EyegazeProviderFactory(IEnumerable<Lazy<IEyegazeProvider, IEyegazeProviderMetadata>> providerFactories)
        {
            _providersFactories = providerFactories.ToList();
        }

        [return:AllowNull]
        public IEyegazeProvider Create(string providerName)
        {
            if (_providers.ContainsKey(providerName)) return _providers[providerName];

            Log.Debug(this, $"Provider [{providerName}] requested.");

            var providerFactory = _providersFactories.FirstOrDefault(p => p.Metadata.Name == providerName)
                                  ?? _providersFactories.FirstOrDefault(p => p.Metadata.Name == "Mouse")
                                  ?? _providersFactories.First();

            IEyegazeProvider provider;

            try
            {
                Log.Info(this, $"Creating [{providerFactory.Metadata.Name}] eyegaze provider.");
                if (!providerFactory.IsValueCreated) providerFactory.Value.Initialize();
                provider = providerFactory.Value;
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Failed to create eyegaze provider [{providerName}] - [{ex}]");
                provider = null;
            }
            _providers.Add(providerName, provider);
            return provider;
        }
    }
}