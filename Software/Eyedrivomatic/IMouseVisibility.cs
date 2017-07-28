namespace Eyedrivomatic
{
    public interface IMouseVisibility
    {
        bool IsMouseHidden { get; }

        void OverrideMouseVisibility(bool hideMouse);
    }
}