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
//    Eyedrivomatic is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media.Animation;

using Prism.Logging;

namespace Eyedrivomatic.Controls.DwellClick
{
    public interface IDwellClickAnimator
    { 
        void StartAnimation(DwellClickAdorner adorner, TimeSpan dwellTime, Action clickCallback);
        void PauseAnimation();
        void ResumeAnimation();
        void StopAnimation();
    }

    [Export(typeof(IDwellClickAnimator)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class DwellClickAnimator : IDwellClickAnimator
    {
        [Import]
        public static ILoggerFacade Logger { get; set; }

        private Storyboard _dwellStoryboard;

        public void StartAnimation(DwellClickAdorner adorner, TimeSpan dwellTime, Action clickCallback)
        {
            Logger?.Log("Creating dwell click animation.", Category.Debug, Priority.None);

            _dwellStoryboard?.Stop(); //If the storyboard is already running

            _dwellStoryboard = new Storyboard();

            var dwellAnimation = new DoubleAnimation(0.0, 1.0, dwellTime);
            dwellAnimation.Completed += (s, a) => clickCallback();

            Storyboard.SetTarget(dwellAnimation, adorner);
            Storyboard.SetTargetProperty(dwellAnimation, new PropertyPath(DwellClickAdorner.DwellProgressProperty));

            _dwellStoryboard.Children.Add(dwellAnimation);

            Logger?.Log("Starting dwell click animation.", Category.Debug, Priority.None);
            _dwellStoryboard?.Begin();
        }

        public void PauseAnimation()
        {
            Logger?.Log("Pausing dwell click animation.", Category.Debug, Priority.None);
            _dwellStoryboard?.Pause();
        }

        public void ResumeAnimation()
        {
            Logger?.Log("Resuming the dwell click animation.", Category.Debug, Priority.None);
            _dwellStoryboard?.Resume();
        }

        public void StopAnimation()
        {
            _dwellStoryboard?.Stop();
            Logger?.Log("Stopped the dwell click animation.", Category.Debug, Priority.None);
        }

    }
}
