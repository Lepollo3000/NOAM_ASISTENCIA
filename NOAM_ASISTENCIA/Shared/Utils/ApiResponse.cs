using System;
using System.Collections.Generic;
using System.Linq;

namespace NOAM_ASISTENCIA.Shared.Utils
{
    public class ApiResponse
    {
        public bool Successful { get; set; } = false;
        public object Result { get; set; } = null!;
        public IEnumerable<string> ErrorMessages { get; set; } = null!;
    }
}
