using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Eyedrivomatic.Hardware
{
    public interface IHardwareService
    {
        Task InitializeAsync();

        IDriver CurrentDriver { get; set; }
        ObservableCollection<IDriver> AvailableDrivers { get; }
    }
}
