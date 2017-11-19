using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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
            get
            {
                var text = Translate.Key(nameof(Strings.Disclaimer_Text));
                var rtb = new RichTextBox
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    BorderThickness = new Thickness(0)
                };

                var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
                rtb.Selection.Load(stream, DataFormats.Rtf);
                return rtb;
            }

            set => throw new NotSupportedException();
        }

        public object ButtonContent
        {
            get => Translate.Key(nameof(Strings.Disclaimer_Button));
            set => throw new NotSupportedException();
        }

        public bool IgnoreDwellPause { get; set; } = true;
    }
}