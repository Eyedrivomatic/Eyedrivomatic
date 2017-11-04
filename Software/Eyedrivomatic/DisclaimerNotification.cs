using System;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Resources;

namespace Eyedrivomatic
{
    public class DisclaimerNotification : INotificationWithCustomButton
    {
        public string Title
        {
            get => Translate.Key(nameof(Strings.Disclaimer_Title));
            set => throw new NotSupportedException();
        }

        public object Content
        {
            get => Translate.Key(nameof(Strings.Disclaimer_Text));
            set => throw new NotSupportedException();
        }

        public object ButtonContent
        {
            get => Translate.Key(nameof(Strings.Disclaimer_Button));
            set => throw new NotSupportedException();
        }
    }
}