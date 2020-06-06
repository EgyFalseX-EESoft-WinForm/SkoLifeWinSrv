using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;


namespace SkoLifeWinSrv.BO
{
    public class TaskManager
    {
        public static TaskManager DefaultInstance { get; set; }
        private static Timer _timner;


        public TaskManager()
        {
            Tasks = new Queue<Task>();
            _timner = new Timer(1000 * 5);
            _timner.Elapsed += _timner_Run;

        }

        public  Queue<Task> Tasks { get; set; }
        public bool IsBusy { get; set; }

        private void _timner_Run(object sender, ElapsedEventArgs e)
        {
            if (!IsBusy && Tasks.Count > 0)
            {
                Run();
            }   
        }

        private void Run()
        {
            IsBusy = true;
            Task tsk = Tasks.Dequeue();
            if (tsk.Op.s2w)
            {
                if (tsk.Execute())
                    if (tsk.SendDataToService())
                        LogsManager.DefaultInstance.LogMsg(LogsManager.LogType.Success, $"OP:{tsk.Op.op_id} Success...", GetType());
            }
            else
            {
                if (tsk.GetDataFromW())
                    if (tsk.PostDyn())
                        LogsManager.DefaultInstance.LogMsg(LogsManager.LogType.Success, $"OP:{tsk.Op.op_id} Success...", GetType());
            }
            

            IsBusy = false;
        }

        public void GetTasks()
        {
            try
            {
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Properties.Settings.Default.GetConfigSrvURL);
                request.Timeout = int.MaxValue;
                request.ContinueTimeout = int.MaxValue;
                request.ReadWriteTimeout = int.MaxValue;
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (var stream = response.GetResponseStream())
                {
                    if (stream == null)
                        return;
                    using (var sr = new StreamReader(stream))
                    {
                        string content = sr.ReadToEnd();
                        List<Config> data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Config>>(content, new JsonBooleanConverter());
                        data.ForEach(fe =>
                        {
                            if (Tasks.ToList().OrderBy(o => o.Op.op_order).All(a => a.Op.op_id != fe.op_id))
                                Tasks.Enqueue(new Task(fe));
                        });
                        
                    }
                }
            }
            catch (Exception ex)
            {
                LogsManager.DefaultInstance.LogMsg(LogsManager.LogType.Error, ex.Message, typeof(TaskManager));
            }
        }
        public void Start()
        { _timner.Start(); }
        public void Stop()
        { _timner.Stop(); }

    }
}
