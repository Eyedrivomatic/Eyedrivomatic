using System;
using System.ComponentModel.Composition;
using System.Windows.Controls.Primitives;
using Prism.Regions;

namespace Eyedrivomatic.Controls
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RegionNavigationButton : ToggleButton
    {
        private readonly IRegionManager _regionManager;
        private string _regionName;

        [ImportingConstructor]
        public RegionNavigationButton(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

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

        private void SetRegionEvent()
        {
            if (string.IsNullOrEmpty(_regionName)) return;

            var region = _regionManager.Regions[RegionName];
            if (region?.NavigationService != null)
            {
                region.NavigationService.Navigated += MainContentRegion_Navigated;
            }
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

        protected override void OnClick()
        {
            base.OnClick();
            _regionManager.RequestNavigate(RegionName, Target);
        }
    }
}
