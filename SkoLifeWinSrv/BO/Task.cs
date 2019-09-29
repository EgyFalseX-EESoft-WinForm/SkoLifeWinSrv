using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Data.SqlClient;

namespace SkoLifeWinSrv.BO
{
    public class Task
    {
        public Task(Config op)
        {
            Op = op;
            DataList = new List<object>();
        }

        public Config Op { get; set; }

        public List<object> DataList { get; set; }

        public bool Execute()
        {
            try
            {
                SqlConnection con = new SqlConnection(Properties.Settings.Default.SkoLifeDBConnection);
                SqlCommand cmd = new SqlCommand("", con);
                string updateQuery = Op.GetPreparedQuery();
                DataList = new List<object>();

                con.Open();
                //update Dyn values
                foreach (Dyn dyn in Op.dyn_list)
                {
                    cmd.CommandText = dyn.update_query;
                    dyn.op_col_value = cmd.ExecuteScalar().ToString();
                }
                // get data
                cmd.CommandText = updateQuery;
                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    List<object> row = new List<object>();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        row.Add(dr.GetValue(i));
                    }
                    DataList.Add(row);
                }
                con.Close();
               
                cmd.Dispose();
                con.Dispose();
            }
            catch (Exception ex)
            {
                LogsManager.DefaultInstance.LogMsg(LogsManager.LogType.Error, ex.Message, typeof(Task));
                return false;
            }
            return true;
        }

        public bool SendDataToService()
        {
            try
            {
                if (DataList.Count == 0)
                    return true;

                List<object> data = new List<object> {Op.op_id, DataList, Op.dyn_list};

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(Properties.Settings.Default.PostDataSrvURL);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(json);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream stream = httpResponse.GetResponseStream();
                if (stream == null)
                    return false;
                using (var streamReader = new StreamReader(stream))
                {
                    var result = streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                LogsManager.DefaultInstance.LogMsg(LogsManager.LogType.Error, ex.Message, typeof(TaskManager));
                return false;
            }
            return true;
        }

    }
}
