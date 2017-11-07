using Prism.Interactivity.InteractionRequest;

namespace Eyedrivomatic.Infrastructure
{
    public class ConfirmationWithCustomButtons : Confirmation, IConfirmationWithCustomButtons
    {
        public object ContinueButtonContent { get; set; } = Translate.TranslationFor("CommandText_Continue");
        public object CancelButtonContent { get; set; } = Translate.TranslationFor("CommandText_Cancel");
    }
}