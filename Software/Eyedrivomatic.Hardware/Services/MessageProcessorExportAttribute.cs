using System;
using System.ComponentModel.Composition;

namespace Eyedrivomatic.Hardware.Services
{
    public interface IMessageProcessorMetadata
    {
        string Name { get; }
    }


    [MetadataAttribute, AttributeUsage(AttributeTargets.Class)]
    public class MessageProcessorAttribute : ExportAttribute, IMessageProcessorMetadata
    {
        public string Name{ get; }

        public MessageProcessorAttribute(string name)
            : base(typeof(IBrainBoxMessageProcessor))
        {
            Name = name;
        }
    }
}
