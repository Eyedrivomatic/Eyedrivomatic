using System;

namespace Eyedrivomatic
{
    public interface IAutoUpdateService
    {
        void Start(TimeSpan checkInterval);
    }
}