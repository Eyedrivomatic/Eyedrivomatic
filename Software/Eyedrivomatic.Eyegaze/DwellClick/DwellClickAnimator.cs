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

namespace Eyedrivomatic.Eyegaze.DwellClick
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
        [ImportingConstructor]
        public DwellClickAnimator()
        {
        }

        private Storyboard _dwellStoryboard;

        public void StartAnimation(DwellClickAdorner adorner, TimeSpan dwellTime, Action clickCallback)
        {
            //Log.Debug(this, "Creating dwell click animation.");

            _dwellStoryboard?.Stop(); //If the storyboard is already running

            _dwellStoryboard = new Storyboard();

            var dwellAnimation = new DoubleAnimation(0.0, 1.0, dwellTime);
            dwellAnimation.Completed += (s, a) => clickCallback();

            Storyboard.SetTarget(dwellAnimation, adorner);
            Storyboard.SetTargetProperty(dwellAnimation, new PropertyPath(DwellClickAdorner.DwellProgressProperty));

            _dwellStoryboard.Children.Add(dwellAnimation);

            //Log.Debug(this, "Starting dwell click animation.");
            _dwellStoryboard.Begin();
        }

        public void PauseAnimation()
        {
            //Log.Debug(this, "Pausing dwell click animation.");
            _dwellStoryboard?.Pause();
        }

        public void ResumeAnimation()
        {
            //Log.Debug(this, "Resuming the dwell click animation.");
            _dwellStoryboard?.Resume();
        }

        public void StopAnimation()
        {
            _dwellStoryboard?.Stop();
            //Log.Debug(this, "Stopped the dwell click animation.");
        }

    }
}
