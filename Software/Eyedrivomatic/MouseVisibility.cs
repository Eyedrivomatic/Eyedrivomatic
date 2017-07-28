using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using Eyedrivomatic.Configuration;

namespace Eyedrivomatic
{

    [Export(typeof(IMouseVisibility))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class MouseVisibility : IMouseVisibility, IDisposable
    {
        private readonly Cursor _smallCursor;
        private readonly IAppearanceConfigurationService _configurationService;

        [ImportingConstructor]
        public MouseVisibility(IAppearanceConfigurationService configurationService)
        {
            _configurationService = configurationService;
            var streamResourceInfo = Application.GetResourceStream(
                new Uri("pack://application:,,,/Eyedrivomatic.Resources;component/Images/SmallCursor.cur"));

            if (streamResourceInfo != null)
            {
                _smallCursor = new Cursor(streamResourceInfo.Stream);
            }

            _configurationService.PropertyChanged += (sender, args) =>
            {
                if (string.Equals(args.PropertyName, nameof(_configurationService.HideMouseCursor)))
                    OverrideMouseVisibility(_configurationService.HideMouseCursor);
            };

            OverrideMouseVisibility(_configurationService.HideMouseCursor);
        }

        public bool IsMouseHidden => Mouse.OverrideCursor == _smallCursor;

        public void OverrideMouseVisibility(bool hideMouse)
        {
            Mouse.OverrideCursor = hideMouse && _configurationService.HideMouseCursor ? _smallCursor : null;
        }

        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Mouse.OverrideCursor = null;
            _smallCursor.Dispose();
        }

    }
}
