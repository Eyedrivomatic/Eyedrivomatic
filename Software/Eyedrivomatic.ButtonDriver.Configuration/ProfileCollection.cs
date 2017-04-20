using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Prism.Logging;

namespace Eyedrivomatic.ButtonDriver.Configuration
{
    [Serializable]
    [XmlRoot(PropertyName)]
    public class ProfileCollection : ObservableCollection<Profile>, IXmlSerializable
    {
        internal const string PropertyName = "profiles";
        private string _currentProfile;

        public Profile CurrentProfile
        {
            get { return this.FirstOrDefault(c => string.Compare(c.Name, _currentProfile, StringComparison.CurrentCultureIgnoreCase) == 0); }
            set
            {
                Contract.Requires<ArgumentNullException>(value != null);
                _currentProfile = value.Name;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(CurrentProfile)));
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            try
            {
                _currentProfile = reader.GetAttribute("currentProfile");

                var serializer = new XmlSerializer(typeof(Profile));

                if (!reader.ReadToFollowing(Profile.PropertyName)) return;
                do
                {
                    Add((Profile)serializer.Deserialize(reader));
                    if (!reader.IsEmptyElement) reader.ReadEndElement();

                } while (reader.NodeType != XmlNodeType.EndElement);

            }
            catch (Exception ex)
            {
                ButtonDriverConfigurationModule.Logger.Log($"Failed to read drive profile configuration - [{ex}]", Category.Exception, Priority.None);
            }

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(CurrentProfile)));
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("currentProfile", CurrentProfile?.Name ?? this.FirstOrDefault()?.Name ?? string.Empty);

            var ns = new XmlSerializerNamespaces();
            ns.Add("", ""); //remove the xsd and xsi namespace declarations. They should be in the merged document.

            var serializer = new XmlSerializer(typeof(Profile));
            foreach (var profile in this)
            {
                serializer.Serialize(writer, profile, ns);
            }
        }
    }
}