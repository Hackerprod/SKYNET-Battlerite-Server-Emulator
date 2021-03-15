using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKYNET.Models
{
    [Serializable]
    public class Auth
    {
        public string AccountName { get; set; }
        public string Password { get; set; }
    }
}
