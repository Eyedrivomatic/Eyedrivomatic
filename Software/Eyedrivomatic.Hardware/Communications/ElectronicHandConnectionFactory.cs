using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Eyedrivomatic.Hardware.Communications
{
    [Export(typeof(IElectronicHandConnectionFactory))]
    public class ElectronicHandConnectionFactory : IElectronicHandConnectionFactory
    {
        private readonly IList<IElectronicHandDeviceInfo> _infos;

        [ImportingConstructor]
        public ElectronicHandConnectionFactory([ImportMany] IEnumerable<IElectronicHandDeviceInfo> infos)
        {
            _infos = infos.ToList();
        }

        public IDeviceConnection CreateConnection(string connectionString)
        {
            return new ElectronicHandConnection(_infos, connectionString);
        }
    }
}