// Copyright (c) 2016 Eyedrivomatic Authors
//
// This file is part of the 'Eyedrivomatic' PC application.
//
//    This program is intended for use as part of the 'Eyedrivomatic System' for 
//    controlling an electric wheelchair using soley the user's eyes. 
//
//    Eyedrivomatic is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Eyedrivomaticis distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

using Prism.Logging;
using Microsoft.Practices.ServiceLocation;

namespace Eyedrivomatic.Hardware
{
    public struct BrainBoxStatusMessage
    {
        private static ILoggerFacade Logger => ServiceLocator.Current.GetInstance<ILoggerFacade>();

        public static Regex DefaultPattern = new Regex($"^(?<{nameof(ManoeuvreState)}>[0-9])," +
                                                       $"(?<{nameof(SpeedState)}>[0-4]+)," +
                                                       $"(?<{nameof(SafetyBypass)}>[0-1])," +
                                                       $"(?<{nameof(LastDirbuttpress)}>[0-8])," +
                                                       $"(?<{nameof(ContinueState)}>[0-3])," +
                                                       $"(?<{nameof(DriveSwitchStateForwardBackward)}>[0-1])," +
                                                       $"(?<{nameof(YMidPos)}>[0-9]+)," +
                                                       $"(?<{nameof(JoystickStateForwardBackward)}>[0-2])," +
                                                       $"(?<{nameof(DurationTimeForwardBackward)}>[0-9]+)," +
                                                       $"(?<{nameof(XMidPos)}>[0-9]+)," +
                                                       $"(?<{nameof(DriveSwitchStateLeftRight)}>[0-1])," +
                                                       $"(?<{nameof(DiagonalReducer)}>[0-3]+)," +
                                                       $"(?<{nameof(JoystickStateLeftRight)}>[0-2])," +
                                                       $"(?<{nameof(DurationTimeLeftRight)}>[0-9]+)," +
                                                       $"(?<{nameof(NudgeSpeed)}>0|4|8|12)," +
                                                       $"(?<{nameof(NudgeDuration)}>[0-9]+)$");

        public static BrainBoxStatusMessage BuildStatusMessage(Regex statusPattern, string message)
        {
            Contract.Requires<ArgumentNullException>(statusPattern != null, nameof(statusPattern));
            if (String.IsNullOrEmpty(message)) return BrainBoxStatusMessage.Empty;

            var match = statusPattern.Match(message);
            if (!match.Success)
            {
                Logger.Log($"Invalid status message received - '{message}'.", Category.Exception, Priority.None);
                return BrainBoxStatusMessage.Empty;
            }

            try
            {
                return new BrainBoxStatusMessage
                {
                    IsValid = true,
                    ManoeuvreState = int.Parse(match.Groups[nameof(BrainBoxStatusMessage.ManoeuvreState)].Value),
                    SpeedState = ToSpeed(int.Parse(match.Groups[nameof(BrainBoxStatusMessage.SpeedState)].Value)),
                    SafetyBypass = ToSafetyBypassState(int.Parse(match.Groups[nameof(BrainBoxStatusMessage.SafetyBypass)].Value)),
                    LastDirbuttpress = ToDirection(int.Parse(match.Groups[nameof(BrainBoxStatusMessage.LastDirbuttpress)].Value)),
                    ContinueState = (ContinueState)int.Parse(match.Groups[nameof(BrainBoxStatusMessage.ContinueState)].Value),
                    DriveSwitchStateForwardBackward = (DriveStateSwitchValue)int.Parse(match.Groups[nameof(BrainBoxStatusMessage.DriveSwitchStateForwardBackward)].Value),
                    XMidPos = int.Parse(match.Groups[nameof(BrainBoxStatusMessage.XMidPos)].Value),
                    JoystickStateForwardBackward = (JoystickStateForwardBackwardValue)int.Parse(match.Groups[nameof(BrainBoxStatusMessage.JoystickStateForwardBackward)].Value),
                    DurationTimeForwardBackward = ulong.Parse(match.Groups[nameof(BrainBoxStatusMessage.DurationTimeForwardBackward)].Value) * 100,
                    YMidPos = int.Parse(match.Groups[nameof(BrainBoxStatusMessage.YMidPos)].Value),
                    DriveSwitchStateLeftRight = (DriveStateSwitchValue)int.Parse(match.Groups[nameof(BrainBoxStatusMessage.DriveSwitchStateLeftRight)].Value),
                    DiagonalReducer = (DiagonalReducerState)int.Parse(match.Groups[nameof(BrainBoxStatusMessage.DiagonalReducer)].Value),
                    JoystickStateLeftRight = (JoystickStateLeftRightValue)int.Parse(match.Groups[nameof(BrainBoxStatusMessage.JoystickStateLeftRight)].Value),
                    DurationTimeLeftRight = ulong.Parse(match.Groups[nameof(BrainBoxStatusMessage.DurationTimeLeftRight)].Value) * 100,
                    NudgeSpeed = ToNudgeSpeed(int.Parse(match.Groups[nameof(BrainBoxStatusMessage.NudgeSpeed)].Value)),
                    NudgeDuration = ulong.Parse(match.Groups[nameof(BrainBoxStatusMessage.NudgeDuration)].Value) * 100,
                };
            }
            catch (FormatException ex)
            {
                Logger.Log($"Failed to parse status message '{message}': {ex}", Category.Exception, Priority.None);
                return BrainBoxStatusMessage.Empty;
            }
        }

        private static Direction ToDirection(int fromDevice)
        {
            switch (fromDevice)
            {
                case 0:
                    return Direction.None;
                case 1:
                    return Direction.ForwardLeft;
                case 2:
                    return Direction.Forward;
                case 3:
                    return Direction.ForwardRight;
                case 4:
                    return Direction.Right;
                case 5:
                    return Direction.BackwardRight;
                case 6:
                    return Direction.Backward;
                case 7:
                    return Direction.BackwardLeft;
                case 8:
                    return Direction.Left;
            }
            return Direction.None;
        }

        private static Speed ToSpeed(int fromDevice)
        {
            switch (fromDevice)
            {
                case 0:
                    return Speed.None;
                case 1:
                    return Speed.Slow;
                case 2:
                    return Speed.Walk;
                case 3:
                    return Speed.Fast;
                case 4:
                    return Speed.Manic;
                default:
                    return Speed.None;
            }
        }

        private static Speed ToNudgeSpeed(int fromDevice)
        {
            switch (fromDevice)
            {
                case 0:
                    return Speed.Slow;
                case 4:
                    return Speed.Walk;
                case 8:
                    return Speed.Fast;
                case 12:
                    return Speed.Manic;
                default:
                    return Speed.None;
            }
        }

        private static SafetyBypassState ToSafetyBypassState(int fromDevice)
        {
            if (fromDevice == 0) return SafetyBypassState.Safe;
            return SafetyBypassState.Unsafe;
        }

        static Lazy<BrainBoxStatusMessage> _lazyEmpty = new Lazy<BrainBoxStatusMessage>(() => new BrainBoxStatusMessage());
        public static BrainBoxStatusMessage Empty
        {
            get
            {
                return _lazyEmpty.Value;
            }
        }

        public enum DriveStateSwitchValue
        {
            NotDriving = 0,
            Driving = 1
        }

        public enum JoystickStateForwardBackwardValue
        {
            None = 0,
            Forward = 1,
            Backward = 2
        }

        public enum JoystickStateLeftRightValue
        {
            None = 0,
            Right = 1,
            Left = 2
        }

        public enum DiagonalReducerState
        {
            Unknown = 0,
            Disabled = 1,
            Enabled = 2
        }

        public bool IsValid;

        public int ManoeuvreState;
        public Speed SpeedState;
        public SafetyBypassState SafetyBypass;
        public Direction LastDirbuttpress;
        public ContinueState ContinueState;
        public DriveStateSwitchValue DriveSwitchStateForwardBackward;
        public int XMidPos;
        public JoystickStateForwardBackwardValue JoystickStateForwardBackward;
        public ulong DurationTimeForwardBackward;
        public int YMidPos;
        public DriveStateSwitchValue DriveSwitchStateLeftRight;
        public DiagonalReducerState DiagonalReducer;
        public JoystickStateLeftRightValue JoystickStateLeftRight;
        public ulong DurationTimeLeftRight;
        public Speed NudgeSpeed;
        public ulong NudgeDuration;

        public override string ToString()
        {
            return $"{nameof(ManoeuvreState)}={ManoeuvreState}, " +
                   $"{nameof(SpeedState)}={SpeedState}, " +
                   $"{nameof(SafetyBypass)}={SafetyBypass}, " +
                   $"{nameof(LastDirbuttpress)}={LastDirbuttpress}, " +
                   $"{nameof(ContinueState)}={ContinueState}, " +
                   $"{nameof(DriveSwitchStateForwardBackward)}={DriveSwitchStateForwardBackward}, " +
                   $"{nameof(XMidPos)}={XMidPos}, " +
                   $"{nameof(JoystickStateForwardBackward)}={JoystickStateForwardBackward}, " +
                   $"{nameof(DurationTimeForwardBackward)}={DurationTimeForwardBackward}, " +
                   $"{nameof(YMidPos)}={YMidPos}, " +
                   $"{nameof(DriveSwitchStateLeftRight)}={DriveSwitchStateLeftRight}, " +
                   $"{nameof(DiagonalReducer)}={DiagonalReducer}, " +
                   $"{nameof(JoystickStateLeftRight)}={JoystickStateLeftRight}, " +
                   $"{nameof(DurationTimeLeftRight)}={DurationTimeLeftRight}, " +
                   $"{nameof(NudgeSpeed)}={NudgeSpeed}, " +
                   $"{nameof(NudgeDuration)}={NudgeDuration}";
        }
    }
}
