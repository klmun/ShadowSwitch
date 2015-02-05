using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowSwitch
{
    public class Config
    {
        public string server { get; set; }
        public int server_port { get; set; }
        public int local_port { get; set; }
        public string password { get; set; }
        public string method { get; set; }
        public int timeout { get; set; }
    }
}
