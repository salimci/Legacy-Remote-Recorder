using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace SiteProtector20Recorder
{
    class CompositeLogFormat
    {

        public void setMainLog(List<object> mainLogList)
        {
            mainLog = new List<object>();
            foreach (object var in mainLogList)
            {
                mainLog.Add(var);
            }
        }

        public void setChildLog(List<ChildLogFormat> childLogList)
        {
            childLog = new List<ChildLogFormat>();
            foreach (ChildLogFormat var in childLogList)
            {
                childLog.Add(var);
            }
        }

        private List<object> mainLog;

        public List<object> MainLog
        {
            get { return mainLog; }
            set { mainLog = value; }
        }

        private List<ChildLogFormat> childLog;

        public List<ChildLogFormat> ChildLog
        {
            get
            {
                if (childLog != null)
                {
                    if (childLog.Count == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return childLog;
                    }
                }
                else
                {
                    return null;
                }
            }
            set { childLog = value; }
        }
    }
}
