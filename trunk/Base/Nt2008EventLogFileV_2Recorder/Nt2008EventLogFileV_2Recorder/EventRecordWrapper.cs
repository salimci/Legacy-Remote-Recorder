using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NT2008EventLogFileRecorder
{
    public class EventRecordWrapper
    {
        public EventRecordWrapper()
        {
            KeywordsDisplayNames = new List<string>();
        }

        public ushort EventId { get; set; }
        public string Description { get; set; }
        public string TaskDisplayName { get; set; }
        public string LevelDisplayName { get; set; }

        public string MachineName { get; set; }

        public DateTime? TimeCreated { get; set; }

        public string LogName { get; set; }

        public ulong RecordId { get; set; }

        public List<string> KeywordsDisplayNames { get; set; }

        public void Reset()
        {
            EventId = 0;
            Description = null;
            TaskDisplayName = null;
            LevelDisplayName = null;
            MachineName = null;
            TimeCreated = null;
            LogName = null;
            RecordId = 0;
            KeywordsDisplayNames.Clear();
        }
    }
}
