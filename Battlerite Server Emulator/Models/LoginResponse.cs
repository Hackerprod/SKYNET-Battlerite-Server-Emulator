using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKYNET.Models
{
    [Serializable]
    public class LoginResponse
    {
        public bool Result { get; set; }
        public string ErrorMessage { get; set; }
    }
}
