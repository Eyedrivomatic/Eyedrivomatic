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
using System.Windows;
using System.Windows.Controls;
using Prism.Regions;

namespace Eyedrivomatic.Controls
{
    [Export]
    public class RegionNavigationButtonFactory
    {
        private readonly ExportFactory<RegionNavigationButton> _buttonExport;

        [ImportingConstructor]
        public RegionNavigationButtonFactory(ExportFactory<RegionNavigationButton> buttonExport)
        {
            _buttonExport = buttonExport;
        }

        public RegionNavigationButton Create(object content, string regionName, Uri target, int sortOrder)
        {
            var button = _buttonExport.CreateExport().Value;
            button.Content = content;
            button.RegionName = regionName;
            button.Target = target;
            button.SortOrder = sortOrder;
            return button;
        }
    }

    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RegionNavigationButton : RadioButton
    {
        private readonly IRegionManager _regionManager;
        private string _regionName;

        [ImportingConstructor]
        public RegionNavigationButton(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public int SortOrder { get; set; }

        public string RegionName
        {
            get => _regionName;
            set
            {
                ClearRegionEvent();
                _regionName = value;
                SetRegionEvent();
            }
        }

        public Func<bool> CanNavigate = () => true; 

        private void SetRegionEvent()
        {
            if (string.IsNullOrEmpty(_regionName)) return;

            var region = _regionManager.Regions[RegionName];
            if (region?.NavigationService != null)
            {
                region.NavigationService.Navigated += MainContentRegion_Navigated;
            }
            GroupName = _regionName;
        }

        private void ClearRegionEvent()
        {
            if (string.IsNullOrEmpty(_regionName)) return;

            var region = _regionManager.Regions[RegionName];
            if (region?.NavigationService != null)
            {
                region.NavigationService.Navigated -= MainContentRegion_Navigated;
            }
        }

        public Uri Target { get; set; }

        public void MainContentRegion_Navigated(object sender, RegionNavigationEventArgs e)
        {
            UpdateNavigationButtonState(e.Uri);
        }

        private void UpdateNavigationButtonState(Uri uri)
        {
            IsChecked = (uri == Target);
        }

        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);

            if (IsChecked ?? false) _regionManager.RequestNavigate(RegionName, Target);
        }
    }
}
