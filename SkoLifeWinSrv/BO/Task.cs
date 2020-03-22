using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.IO;
using System.Net;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Configuration;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

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
                cmd.CommandTimeout = int.MaxValue;
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
                httpWebRequest.Timeout = int.MaxValue;
                httpWebRequest.ContinueTimeout = int.MaxValue;
                httpWebRequest.ReadWriteTimeout = int.MaxValue;
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

        #region Request Data
        public bool GetDataFromW()
        {
            DataTable dt = new DataTable();
            try
            {
                string updateQuery = Op.GetPreparedQuery();

                List<object> data = new List<object> { Op.op_id, updateQuery };

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(Properties.Settings.Default.RequestDataSrvURL);
                httpWebRequest.Timeout = int.MaxValue;
                httpWebRequest.ContinueTimeout = int.MaxValue;
                httpWebRequest.ReadWriteTimeout = int.MaxValue;
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
                    dt = JsonConvert.DeserializeObject<DataTable>(result);
                    if (dt?.Rows.Count > 0)
                        UpdateBulk(dt);
                    
                }
            }
            catch (Exception ex)
            {
                LogsManager.DefaultInstance.LogMsg(LogsManager.LogType.Error, ex.Message, typeof(TaskManager));
                return false;
            }
            return true;
        }

        private bool UpdateBulk(DataTable BulkTable)
        {
            bool outPut = false;
            DateTime dtStart = DateTime.Now;
            SqlConnection connection = new SqlConnection(Properties.Settings.Default.SkoLifeDBConnection);
            SqlCommand command = new SqlCommand("", connection) { CommandTimeout = 0 };
            SqlDataAdapter adp = new SqlDataAdapter(command);
            try
            {
                
                List<string> src_col = new List<string>();
                src_col.AddRange(Op.op_dst_col.Split(',').Select(s => s.Trim()));
                //foreach (DataColumn col in BulkTable.Columns)
                //    src_col.Add(col.ColumnName);


                //Recreate Data Table with right Columns Types
                command.CommandText = $"SELECT {string.Join(",", src_col)} FROM {Op.op_dst_tbl} WHERE 1 = 2";
                DataTable dtData = new DataTable();
                adp.Fill(dtData);
                foreach (DataRow row in BulkTable.Rows)
                {
                    DataRow copyRow = dtData.NewRow();
                    for (int i = 0; i < src_col.Count; i++)
                    {
                        if (row[i] == null || row[i].ToString() == string.Empty)
                            continue;

                        if (dtData.Columns[i].DataType == typeof(bool))
                            copyRow[i] = Convert.ToInt32(row[i]);
                        else
                        {
                            TypeConverter typeConverter = TypeDescriptor.GetConverter(dtData.Columns[i].DataType);
                            copyRow[i] = typeConverter.ConvertFromString(row[i].ToString());
                        }
                    }
                    dtData.Rows.Add(copyRow);
                }


                connection.Open();

                //Create tmp table
                string BulkTableName = string.Format("IMP{0}{1}{2}{3}{4}{5}{6}", dtStart.Year, dtStart.Month, dtStart.Day, dtStart.Hour, dtStart.Minute, dtStart.Second, dtStart.Millisecond);
                command.CommandText = $"SELECT {string.Join(",", src_col)} INTO {BulkTableName} FROM {Op.op_dst_tbl} WHERE 1 = 0;";
                command.ExecuteNonQuery();

                //Insert into tmp table
                SqlBulkCopy bulkCopy = new SqlBulkCopy(Properties.Settings.Default.SkoLifeDBConnection);
                bulkCopy.BulkCopyTimeout = 0;
                bulkCopy.ColumnMappings.Clear();
                
                for (int i = 0; i < src_col.Count; i++)
                {
                    bulkCopy.ColumnMappings.Add(src_col[i].Trim(), src_col[i].Trim());
                }

                bulkCopy.DestinationTableName = BulkTableName;
                bulkCopy.BatchSize = dtData.Rows.Count;
                bulkCopy.WriteToServer(dtData);

                //Merage tmp into distnation table
                string convStr = string.Join(" , ", (from q in src_col select $"Target.{q} = Source.{q}"));
                //string matchStr = string.Join(" AND ", (from q in src_col select $"Target.{q} = Source.{q}"));
                string matchStr = string.Join(" AND ", (from q in Op.key_list select $"Target.{q.op_dst_col_name} = Source.{q.op_src_col_name}"));
                string dst_cols = string.Join(",", (from q in src_col select q));
                string src_cols = string.Join(",", (from q in src_col select "Source." + q));
                command.CommandText = $"merge into {Op.op_dst_tbl} as Target using {BulkTableName} AS Source ON {matchStr} when matched then UPDATE SET {convStr} " +
                    $"when not matched then INSERT ({dst_cols}) VALUES ({src_cols});";
                //LogsManager.DefaultInstance.LogMsg(LogsManager.LogType.Debug, $"MERGE : {command.CommandText}", typeof(Task));
                command.ExecuteNonQuery();
                
                //Update Dyn Fields
                foreach (Dyn dyn in Op.dyn_list)
                {
                    DataRow maxRow = BulkTable.Select("1 = 1", dyn.op_col_name + " DESC")[0];
                    dyn.op_col_value = maxRow[dyn.op_col_name].ToString();
                }

                command.CommandText = string.Format(@"DROP TABLE {0}", BulkTableName);
                command.ExecuteNonQuery();

                connection.Close();
                outPut = true;
            }
            catch (SqlException ex)
            {
                LogsManager.DefaultInstance.LogMsg(LogsManager.LogType.Error, ex.Message, this.GetType());
                command.Dispose();
                connection.Dispose();
            }
            return outPut;
        }

        public bool PostDyn()
        {
            try
            {
                if (Op.dyn_list.Count == 0)
                    return true;

                List<object> data = new List<object> { Op.op_id, Op.dyn_list };

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(Properties.Settings.Default.PostDynSrvURL);
                httpWebRequest.Timeout = int.MaxValue;
                httpWebRequest.ContinueTimeout = int.MaxValue;
                httpWebRequest.ReadWriteTimeout = int.MaxValue;
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

        #endregion


    }
}
