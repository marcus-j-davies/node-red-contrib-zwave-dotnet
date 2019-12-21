using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;

namespace Server
{
   

    
    class Request
    {
        public Request() { }


        public byte node { get; set; }
        public string operation { get; set; }
        public object[] operation_vars { get; set; }

        public byte[] raw { get; set; }


    }
}
