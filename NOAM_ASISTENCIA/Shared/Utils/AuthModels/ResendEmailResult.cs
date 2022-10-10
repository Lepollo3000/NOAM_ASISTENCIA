using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOAM_ASISTENCIA.Shared.Utils.AuthModels
{
    public class ResendEmailResult
    {
        public string Username { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
    }
}
