using System;
using System.Xml.Serialization;
using Prism.Mvvm;

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
        private int _xDiagReduced;
        private int _yForwardDiag;
        private int _yForwardDiagReduced;
        private int _yBackwardDiag;
        private int _yBackwardDiagReduced;
        private int _nudge;

        /// <summary>
        /// The name of the profile. This will be displayed to the user.
        /// </summary>
        [XmlAttribute("name")]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// The X deflection when the right or left buttons are pressed.
        /// </summary>
        [XmlAttribute("x")]
        public int X
        {
            get { return _x; }
            set { SetProperty(ref _x, value); }
        }

        /// <summary>
        /// The Y deflection when the forward button is presssed.
        /// </summary>
        [XmlAttribute("yForward")]
        public int YForward
        {
            get { return _yForward; }
            set { SetProperty(ref _yForward, value); }
        }

        /// <summary>
        /// The Y deflection when the backward button is presssed.
        /// </summary>
        [XmlAttribute("yBackward")]
        public int YBackward
        {
            get { return _yBackward; }
            set { SetProperty(ref _yBackward, value); }
        }

        /// <summary>
        /// The X deflection when one of the diagonal buttons are pressed.
        /// </summary>
        [XmlAttribute("xDiag")]
        public int XDiag
        {
            get { return _xDiag; }
            set { SetProperty(ref _xDiag, value); }
        }

        /// <summary>
        /// The X deflection when one of the diagonal buttons are pressed while diagonal reduction is enabled.
        /// </summary>
        [XmlAttribute("xDiagReduced")]
        public int XDiagReduced
        {
            get { return _xDiagReduced; }
            set { SetProperty(ref _xDiagReduced, value); }
        }

        /// <summary>
        /// The Y deflection when one of the forward diagonal buttons are pressed.
        /// </summary>
        [XmlAttribute("yForwardDiag")]
        public int YForwardDiag
        {
            get { return _yForwardDiag; }
            set { SetProperty(ref _yForwardDiag, value); }
        }

        /// <summary>
        /// The Y deflection when one of the forward diagonal buttons are pressed while diagonal reduction is enabled.
        /// </summary>
        [XmlAttribute("yForwardDiagReduced")]
        public int YForwardDiagReduced
        {
            get { return _yForwardDiagReduced; }
            set { SetProperty(ref _yForwardDiagReduced, value); }
        }

        /// <summary>
        /// The Y deflection when one of the backward diagonal buttons are pressed.
        /// </summary>
        [XmlAttribute("yBackwardDiag")]
        public int YBackwardDiag
        {
            get { return _yBackwardDiag; }
            set { SetProperty(ref _yBackwardDiag, value); }
        }

        /// <summary>
        /// The Y deflection when one of the bacward diagonal buttons are pressed while diagonal reduction is enabled.
        /// </summary>
        [XmlAttribute("yBackwardDiagReduced")]
        public int YBackwardDiagReduced
        {
            get { return _yBackwardDiagReduced; }
            set { SetProperty(ref _yBackwardDiagReduced, value); }
        }


        /// <summary>
        /// The X deflection when one of the nudge buttons are pressed while moving forward.
        /// </summary>
        [XmlAttribute("nudge")]
        public int Nudge
        {
            get { return _nudge; }
            set { SetProperty(ref _nudge, value); }
        }
    }
}