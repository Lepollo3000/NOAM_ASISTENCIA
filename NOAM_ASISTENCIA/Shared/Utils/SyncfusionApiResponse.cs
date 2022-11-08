using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOAM_ASISTENCIA.Shared.Utils
{
    public class SyncfusionApiResponse
    {
        public IEnumerable<object> Items { get; set; } = null!;
        public int Count { get; set; }
    }
}
