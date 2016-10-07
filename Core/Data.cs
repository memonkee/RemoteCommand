using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace RemoteCommand.Core.Data
{
    class Task
    {
        public string id { get; set; }
        public string executePath { get; set; }
        public string name { get; set; }
    }

    class Data
    {
        List<Task> tasks { get; set; }

        private void Save()
        {
            Data obj = this;
            XmlSerializer xs = new XmlSerializer(typeof(Data));
            TextWriter tw = new StreamWriter(app.settingsFile);
            xs.Serialize(tw, obj);
        }

        private Data Get()
        {
            XmlSerializer xs = new XmlSerializer(typeof(Data));
            var data = new Data();
            using(var sr = new StreamReader(app.settingsFile))
            {
                 return (Data)xs.Deserialize(sr);
            }
        }

        public void addTask(Task task)
        {
            var data = Get();
            if(tasks == null)
            {
                tasks = new List<Task>();
            }

            tasks.Add(task);
            Save();
        }

        public void removeTask(Task task)
        {
            var data = Get();
            if (tasks != null)
            {
                tasks.Remove(task);
            }
            Save();
        }

        public List<Task> getTasks()
        {
            var data = Get();
            if (tasks == null)
            {
                tasks = new List<Task>();
            }
            return data.tasks;
        }
    }

    class app
    {
        
        public static string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
        public static string directory = System.IO.Path.GetDirectoryName(path);
        public static string settingsFile = directory + @"\settings.xml";
    }
}
