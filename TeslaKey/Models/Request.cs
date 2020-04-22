using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeslaKey.Models
{
    public class Request
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
