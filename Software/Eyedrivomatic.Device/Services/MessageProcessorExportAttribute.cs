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
using System.ComponentModel.Composition;

namespace Eyedrivomatic.Device.Services
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
