﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOAM_ASISTENCIA.Shared.Utils.AuthModels
{
    public class ConfirmEmailResult
    {
        public bool IsTokenError { get; set; }
        public string Username { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
    }
}
