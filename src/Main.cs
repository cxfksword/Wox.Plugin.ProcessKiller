using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Management;

namespace Wox.Plugin.ProcessKiller
{
    public class Main : IPlugin
    {
        private static List<string> systemProcessList = new List<string>(){
            "svchost",
            "idle",
            "system",
            "rundll32",
            "csrss",
            "lsass",
            "lsm",
            "smss",
            "wininit",
            "winlogon",
            "services",
            "spoolsv",
            "explorer"};

        public List<Result> Query(Query query)
        {
            var results = new List<Result>();

            var keyword = "";
            List<Process> processlist = new List<Process>();
            if (query.ActionParameters.Count == 0)
            {
                // show all process order by cpu
                var processes = Process.GetProcesses();
                foreach (Process p in processes)
                {
                    // filter system process
                    var name = p.ProcessName.ToLower();
                    if (systemProcessList.Contains(name))
                        continue;

                    processlist.Add(p);
                }
            }
            else
            {
                keyword = query.ActionParameters[0].ToLower();
                var processes = Process.GetProcesses();
                foreach (Process p in processes)
                {
                    // filter system process
                    var name = p.ProcessName.ToLower();
                    if (systemProcessList.Contains(name))
                        continue;

                    if (p.ProcessName.ToLower().Contains(keyword))
                    {
                        processlist.Add(p);
                    }
                }

            }

            
            if (processlist.Count > 0)
            {
                foreach(Process proc in processlist){
                    var p = proc;
                    var path = GetPath(p);
                    results.Add(new Result()
                    {
                        IcoPath = path,
                        Title = p.ProcessName + " - " + p.Id.ToString(),
                        SubTitle = path,
                        Action = (c) =>
                        {
                            p.Kill();
                            return true;
                        }
                    });
                }


                if (processlist.Count > 1 && !string.IsNullOrEmpty(keyword))
                {
                    results.Insert(0, new Result()
                    {
                        IcoPath = "Images\\app.png",
                        Title = "kill all \"" + keyword + "\" process",
                        SubTitle = "",
                        Action = (c) =>
                           {
                               foreach (var p in processlist)
                               {
                                   p.Kill();
                               }
                               return true;
                           }
                    });
                }
            }

            return results;
        }

        private string GetPath(Process p)
        {
            var path = "";
            try
            {
                path = p.Modules != null && p.Modules.Count > 0 ? p.Modules[0].FileName : "";
            }
            catch
            {}

            return path.ToLower();
        }

        public void Init(PluginInitContext context)
        {
        }
    }
}
