using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QRToolsClient
{
    public class Group
    {
        public Guid ID { get; set; }

        public string Code { get; set; }

        public string Remark { get; set; }

        public DateTime EntryTime { get; set; }

        public List<Log> LogList { get; set; }
    }

    public class Log
    {
        public Guid ID { get; set; }

        public string Code { get; set; }

        public string Remark { get; set; }

        public DateTime EntryTime { get; set; }

    }
}
