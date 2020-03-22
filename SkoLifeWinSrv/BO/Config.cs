using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkoLifeWinSrv.BO
{
    public class Config
    {
        public string op_id { get; set; }
        public int op_order { get; set; }
        public string op_desc { get; set; }
        public string op_dst_tbl { get; set; }
        public string op_dst_col { get; set; }
        public string sql_query { get; set; }
        public string op_src_filter { get; set; }
        public bool s2w { get; set; }
        public List<Dyn> dyn_list { get; set; }
        public List<Key> key_list { get; set; }

        public string GetPreparedQuery()
        {
            string[] values = dyn_list.OrderBy(o => o.dyn_order).Select(s => s.op_col_value).ToArray();
            string where = string.Format($@"{op_src_filter}", values);
            return $@"{sql_query} WHERE {where}";
            
        }
    }
}
