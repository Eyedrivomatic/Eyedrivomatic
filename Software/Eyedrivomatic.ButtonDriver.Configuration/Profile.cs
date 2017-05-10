using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
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
        private bool _diagonalSpeedReduction;
        private TimeSpan _xDuration;
        private TimeSpan _yDuration;
        private TimeSpan _nudgeDuration;
        internal const string PropertyName = "profile";
        internal const string DefaultProfileName = "Default";

        public void AddDefaultSpeeds()
        {
            var otherSpeeds = Speeds.Select(speed => speed.Name).ToList();
            Speeds.AddRange(
                new[]
                {
                    //Previous version = {Name="Slow"
                    new ProfileSpeed {Name=Strings.DrivingView_SpeedSlow.NextPostfix(otherSpeeds),
                    //  X=22, YForward=9, YBackward=9, XDiag=14, YForwardDiag=6, YBackwardDiag=6, XDiagReduced=4, YForwardDiagReduced=6, Nudge=6}, Previous Version
                        X=22, YForward=9, YBackward=9, XDiag=14, YForwardDiag=6, YBackwardDiag=6, XDiagReduced=4, YForwardDiagReduced=6, Nudge=6},

                    //Previous version = {Name="Walk"
                    new ProfileSpeed {Name=Strings.DrivingView_SpeedWalk.NextPostfix(otherSpeeds),
                    //  X=22, YForward=13, YBackward=13, XDiag=15, YForwardDiag=10, YBackwardDiag=10, XDiagReduced=5, YForwardDiagReduced=10, Nudge=6}, Previous Version
                        X=22, YForward=13, YBackward=13, XDiag=15, YForwardDiag=10, YBackwardDiag=10, XDiagReduced=5, YForwardDiagReduced=10, Nudge=6},

                    //Previous version = {Name="Fast",                           
                    new ProfileSpeed {Name=Strings.DrivingView_SpeedFast.NextPostfix(otherSpeeds),
                    //  X=22, YForward=17, YBackward=17, XDiag=17, YForwardDiag=14, YBackwardDiag=14, XDiagReduced=7, YForwardDiagReduced=14, Nudge=6}, Previous Version
                        X=22, YForward=17, YBackward=17, XDiag=17, YForwardDiag=14, YBackwardDiag=14, XDiagReduced=7, YForwardDiagReduced=14, Nudge=6},

                    //Previous version = {Name="Manic"
                    //new ProfileSpeed {Name=Strings.DrivingView_SpeedManic.NextPostfix(otherSpeeds),
                    //    X=22, YForward=21, YBackward=21, XDiag=22, YForwardDiag=18, YBackwardDiag=18, XDiagReduced=12, YForwardDiagReduced=18, Nudge=6}, Previous Version
                    //    X=22, YForward=21, YBackward=21, XDiag=22, YForwardDiag=18, YBackwardDiag=18, XDiagReduced=12, YForwardDiagReduced=18, Nudge=6},
                });
        }

        public string Name
        {
            get => _name = _name ?? Resources.Strings.ProfileName_Drive;
            set => SetProperty(ref _name, value);
        }

        [AllowNull]
        public ProfileSpeed CurrentSpeed
        {
            get => Speeds.SingleOrDefault(c => string.Compare(c.Name, _currentSpeed, StringComparison.CurrentCultureIgnoreCase) == 0);
            set => SetProperty(ref _currentSpeed, value.Name);
        }

        /// <summary>
        /// True if diagnonal speed reduction is enabled.
        /// </summary>
        public bool DiagonalSpeedReduction
        {
            get => _diagonalSpeedReduction;
            set => SetProperty(ref _diagonalSpeedReduction, value);
        }

        #region Duration

        /// <summary>
        /// The duration of left/right movements.
        /// </summary>
        public TimeSpan XDuration
        {
            get => _xDuration;
            set => SetProperty(ref _xDuration, value);
        }

        /// <summary>
        /// The duration of forward/backward movements.
        /// </summary>
        public TimeSpan YDuration
        {
            get => _yDuration;
            set => SetProperty(ref _yDuration,  value);
        }

        /// <summary>
        /// The duration of nudge movements.
        /// </summary>
        public TimeSpan NudgeDuration
        {
            get => _nudgeDuration;
            set => SetProperty(ref _nudgeDuration, value);
        }

        #endregion Duration

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
            DiagonalSpeedReduction = bool.Parse(reader.GetAttribute("diagonalSpeedReduction") ?? "False");
            NudgeDuration = TimeSpan.FromMilliseconds(double.Parse(reader.GetAttribute("nudgeDuration") ?? "1000"));
            XDuration = TimeSpan.FromMilliseconds(double.Parse(reader.GetAttribute("xDuration") ?? "2000"));
            YDuration = TimeSpan.FromMilliseconds(double.Parse(reader.GetAttribute("yDuration") ?? "2000"));
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
            writer.WriteAttributeString("diagonalSpeedReduction", _diagonalSpeedReduction.ToString());
            writer.WriteAttributeString("nudgeDuration", _nudgeDuration.TotalMilliseconds.ToString("0"));
            writer.WriteAttributeString("xDuration", _xDuration.TotalMilliseconds.ToString("0"));
            writer.WriteAttributeString("yDuration", _yDuration.TotalMilliseconds.ToString("0"));
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