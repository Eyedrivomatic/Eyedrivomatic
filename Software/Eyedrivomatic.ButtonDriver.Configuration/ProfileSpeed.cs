using System;
using System.Xml.Serialization;
using Prism.Mvvm;
using System.IO;
using NullGuard;

namespace Eyedrivomatic.ButtonDriver.Configuration
{
    [Serializable]
    [XmlRoot(PropertyName)]
    public class ProfileSpeed : BindableBase
    {
        internal const string PropertyName = "speed";

        private string _name;
        private int _x;
        private int _yForward;
        private int _yBackward;
        private int _xDiag;
        private int _yForwardDiag;
        private int _yBackwardDiag;
        private int _nudge;

        public static ProfileSpeed Clone([AllowNull] ProfileSpeed from)
        {
            if (from == null) return new ProfileSpeed();

            var stream = new MemoryStream();
            //easiest way to do this is to just serialize it.
            var serializer = new XmlSerializer(typeof(ProfileSpeed));
            serializer.Serialize(stream, from);
            stream.Seek(0, SeekOrigin.Begin);
            var to = serializer.Deserialize(stream) as ProfileSpeed;
            return to ?? new ProfileSpeed();
        }

        /// <summary>
        /// The name of the profile speed. This will be used to identify this speed to the user.
        /// </summary>
        [XmlAttribute("name")]
        public string Name
        {
            get => _name = _name ?? Resources.Strings.ProfileSpeed_Default;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// The X deflection when the right or left buttons are pressed.
        /// </summary>
        [XmlAttribute("x")]
        public int X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }

        /// <summary>
        /// The Y deflection when the forward button is presssed.
        /// </summary>
        [XmlAttribute("yForward")]
        public int YForward
        {
            get => _yForward;
            set => SetProperty(ref _yForward, value);
        }

        /// <summary>
        /// The Y deflection when the backward button is presssed.
        /// </summary>
        [XmlAttribute("yBackward")]
        public int YBackward
        {
            get => _yBackward;
            set => SetProperty(ref _yBackward, value);
        }

        /// <summary>
        /// The X deflection when one of the diagonal buttons are pressed.
        /// </summary>
        [XmlAttribute("xDiag")]
        public int XDiag
        {
            get => _xDiag;
            set => SetProperty(ref _xDiag, value);
        }

        /// <summary>
        /// The Y deflection when one of the forward diagonal buttons are pressed.
        /// </summary>
        [XmlAttribute("yForwardDiag")]
        public int YForwardDiag
        {
            get => _yForwardDiag;
            set => SetProperty(ref _yForwardDiag, value);
        }

        /// <summary>
        /// The Y deflection when one of the backward diagonal buttons are pressed.
        /// </summary>
        [XmlAttribute("yBackwardDiag")]
        public int YBackwardDiag
        {
            get => _yBackwardDiag;
            set => SetProperty(ref _yBackwardDiag, value);
        }

        /// <summary>
        /// The X deflection when one of the nudge buttons are pressed while moving forward.
        /// </summary>
        [XmlAttribute("nudge")]
        public int Nudge
        {
            get => _nudge;
            set => SetProperty(ref _nudge, value);
        }
    }
}