using System;
using System.Collections.Generic;
using System.Linq;

namespace NOAM_ASISTENCIA.Shared.Utils
{
    public class ApiResponse<TObject>
    {
        public bool Successful { get; set; } = false;
        public TObject? Result { get; set; } = default!;
        public IEnumerable<string> ErrorMessages { get; set; } = null!;
    }
}
