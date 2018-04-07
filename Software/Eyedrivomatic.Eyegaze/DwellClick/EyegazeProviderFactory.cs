//	Copyright (c) 2018 Eyedrivomatic Authors
//	
//	This file is part of the 'Eyedrivomatic' PC application.
//	
//	This program is intended for use as part of the 'Eyedrivomatic System' for 
//	controlling an electric wheelchair using soley the user's eyes. 
//	
//	Eyedrivomaticis distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  


using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Eyedrivomatic.Logging;
using NullGuard;

namespace Eyedrivomatic.Eyegaze.DwellClick
{
    [Export(typeof(IEyegazeProviderFactory)), PartCreationPolicy(CreationPolicy.Shared)]
    public class EyegazeProviderFactory : IEyegazeProviderFactory
    {
        private readonly IList<Lazy<IEyegazeProvider, IEyegazeProviderMetadata>> _providersFactories;
        private readonly Dictionary<string, Tuple<IEyegazeProvider, Task<bool>>> _providers = new Dictionary<string, Tuple<IEyegazeProvider, Task<bool>>>();

        [ImportingConstructor]
        public EyegazeProviderFactory(IEnumerable<Lazy<IEyegazeProvider, IEyegazeProviderMetadata>> providerFactories)
        {
            _providersFactories = providerFactories.ToList();
        }

        [return:AllowNull]
        public async Task<IEyegazeProvider> CreateAsync(string providerName)
        {
            if (_providers.ContainsKey(providerName)) return await _providers[providerName].Item2 ? _providers[providerName].Item1 : null;

            Log.Debug(this, $"Provider [{providerName}] requested.");

            var providerFactory = _providersFactories.FirstOrDefault(p => p.Metadata.Name == providerName)
                                  ?? _providersFactories.FirstOrDefault(p => p.Metadata.Name == "Mouse")
                                  ?? _providersFactories.First();

            try
            {
                Log.Info(this, $"Creating [{providerFactory.Metadata.Name}] eyegaze provider.");

                var provider = providerFactory.Value;
                var initializeTask = provider.InitializeAsync();
                _providers.Add(providerName, new Tuple<IEyegazeProvider, Task<bool>>(provider, initializeTask));

                if (!await initializeTask) return null;
                return provider;
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Failed to create eyegaze provider [{providerName}] - [{ex}]");
                return null;
            }
        }
    }
}