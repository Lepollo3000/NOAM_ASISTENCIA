﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOAM_ASISTENCIA.Shared.Utils.AuthModels
{
    public class ConfirmEmailRequest
    {
        public string UserId { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
