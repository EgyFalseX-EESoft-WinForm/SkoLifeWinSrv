using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SkoLifeWinSrv.BO
{
    public class Dyn
    {
        [ScriptIgnore]
        public int dyn_order { get; set; }
        public string op_col_name { get; set; }
        public string op_col_value { get; set; }
        [ScriptIgnore]
        public string update_query { get; set; }
    }
}
