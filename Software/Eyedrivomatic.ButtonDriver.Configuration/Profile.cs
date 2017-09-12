using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Eyedrivomatic.Infrastructure;
using Prism.Mvvm;
using Eyedrivomatic.Infrastructure.Extensions;
using Eyedrivomatic.Resources;
using NullGuard;

namespace Eyedrivomatic.ButtonDriver.Configuration
{
    [Serializable]
    [XmlRoot(PropertyName)]
    [Export]
    public class Profile : BindableBase, IXmlSerializable
    {
        private string _name;
        private string _currentSpeed;
        internal const string PropertyName = "profile";
        internal const string DefaultProfileName = "Default";

        public void AddDefaultSpeeds()
        {
            var otherSpeeds = Speeds.Select(speed => speed.Name).ToList();
            Speeds.AddRange(
                new[]
                {
                    new ProfileSpeed {Name=Strings.DrivingView_Speed_Slow.NextPostfix(otherSpeeds),
                        X=60, YForward=40, YBackward=40, XDiag=50, YForwardDiag=25, YBackwardDiag=25, Nudge=10},

                    new ProfileSpeed {Name=Strings.DrivingView_Speed_Walk.NextPostfix(otherSpeeds),
                        X=80, YForward=60, YBackward=60, XDiag=70, YForwardDiag=50, YBackwardDiag=45, Nudge=10},

                    new ProfileSpeed {Name=Strings.DrivingView_Speed_Fast.NextPostfix(otherSpeeds),
                        X=100, YForward=100, YBackward=100, XDiag=75, YForwardDiag=75, YBackwardDiag=65, Nudge=10},
                });
        }

        public string Name
        {
            get => _name = _name ?? Strings.ProfileName_Drive;
            set => SetProperty(ref _name, value);
        }

        [AllowNull]
        public ProfileSpeed CurrentSpeed
        {
            get => Speeds.SingleOrDefault(c => string.Compare(c.Name, _currentSpeed, StringComparison.CurrentCultureIgnoreCase) == 0);
            set => SetProperty(ref _currentSpeed, value.Name);
        }

        private bool _safetyBypass;
        public bool SafetyBypass
        {
            get => _safetyBypass;
            set
            {
                Log.Warn(this, $"Safety bypass [{(value ? "Enabled (Unsafe)" : "Disabled (Safe)")}].");
                SetProperty(ref _safetyBypass, value);
            }
        }

        public ObservableCollection<ProfileSpeed> Speeds { get; } = new ObservableCollection<ProfileSpeed>();

        public ProfileSpeed AddSpeed(ProfileSpeed cloneFrom = null)
        {
            var newSpeed = ProfileSpeed.Clone(cloneFrom ?? Speeds.LastOrDefault());
            newSpeed.Name = newSpeed.Name.NextPostfix(Speeds.Select(speed => speed.Name));
            Speeds.Add(newSpeed);
            return newSpeed;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Name = reader.GetAttribute("name") ?? DefaultProfileName;
            // ReSharper disable once ExplicitCallerInfoArgument
            SetProperty(ref _currentSpeed, reader.GetAttribute("currentSpeed") ?? "None", nameof(CurrentSpeed));

            var serializer = new XmlSerializer(typeof(ProfileSpeed));

            if (!reader.ReadToFollowing(ProfileSpeed.PropertyName)) return;
            do
            {
                if (reader.Name != ProfileSpeed.PropertyName) reader.ReadToNextSibling(ProfileSpeed.PropertyName);
                Speeds.Add((ProfileSpeed) serializer.Deserialize(reader));

            } while (reader.NodeType != XmlNodeType.EndElement);
            _currentSpeed = Speeds.Select(s => s.Name).FirstOrDefault();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString(nameof(CurrentSpeed), CurrentSpeed?.Name ?? string.Empty);

            var ns = new XmlSerializerNamespaces();
            ns.Add("", ""); //remove the xsd and xsi namespace declarations. They should be in the merged document.

            var serializer = new XmlSerializer(typeof(ProfileSpeed));
            
            foreach (var speed in Speeds)
            {
                serializer.Serialize(writer, speed, ns);
            }
        }
    }
}