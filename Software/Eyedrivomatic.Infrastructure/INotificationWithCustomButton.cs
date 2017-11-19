using Prism.Interactivity.InteractionRequest;

namespace Eyedrivomatic.Infrastructure
{
    public interface INotificationWithCustomButton : INotification
    {
        object ButtonContent { get; set; }
        bool IgnoreDwellPause { get; set; }
    }
}