using System;
using System.ComponentModel.Composition;

namespace Eyedrivomatic.Eyegaze
{
    public interface IEyegazeProviderMetadata
    {
        string Name { get; }
    }

    [MetadataAttribute, AttributeUsage(AttributeTargets.Class)]
    public class ExportEyegazeProviderAttribute : ExportAttribute, IEyegazeProviderMetadata
    {
        public ExportEyegazeProviderAttribute(string name) : base(typeof(IEyegazeProvider))
        {
            Name = name;
        }

        public string Name { get; }
    }
}