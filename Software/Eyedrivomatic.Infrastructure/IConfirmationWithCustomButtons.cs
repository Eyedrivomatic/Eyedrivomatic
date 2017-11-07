using Prism.Interactivity.InteractionRequest;

namespace Eyedrivomatic.Infrastructure
{
    public interface IConfirmationWithCustomButtons : IConfirmation
    {
        object ContinueButtonContent { get; set; }
        object CancelButtonContent { get; set; }
    }
}