using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Prism.Mvvm;

namespace Eyedrivomatic.ButtonDriver.Configuration
{
    [Serializable]
    [XmlRoot(PropertyName)]
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

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public ProfileSpeed CurrentSpeed
        {
            get { return Speeds.SingleOrDefault(c => string.Compare(c.Name, _currentSpeed, StringComparison.CurrentCultureIgnoreCase) == 0); }
            set
            {
                Contract.Requires<ArgumentNullException>(value != null, nameof(value));
                SetProperty(ref _currentSpeed, value.Name);
            }
        }

        /// <summary>
        /// True if diagnonal speed reduction is enabled.
        /// </summary>
        public bool DiagonalSpeedReduction
        {
            get { return _diagonalSpeedReduction; }
            set { SetProperty(ref _diagonalSpeedReduction, value); }
        }

        #region Duration

        /// <summary>
        /// The duration of left/right movements.
        /// </summary>
        public TimeSpan XDuration
        {
            get { return _xDuration; }
            set { SetProperty(ref _xDuration, value); }
        }

        /// <summary>
        /// The duration of forward/backward movements.
        /// </summary>
        public TimeSpan YDuration
        {
            get { return _yDuration; }
            set { SetProperty(ref _yDuration,  value); }
        }

        /// <summary>
        /// The duration of nudge movements.
        /// </summary>
        public TimeSpan NudgeDuration
        {
            get { return _nudgeDuration; }
            set { SetProperty(ref _nudgeDuration, value); }
        }

        #endregion Duration

        public ObservableCollection<ProfileSpeed> Speeds { get; } = new ObservableCollection<ProfileSpeed>();

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