using System.Timers;

namespace EventlogRecorder
{
    class ExtendedTimer : Timer
    {
        private int timerIndex;

        public int TimerIndex
        {
            get { return timerIndex; }
            set { timerIndex = value; }
        }

        private string timerLocation;

        public string TimerLocation
        {
            get { return timerLocation; }
            set { timerLocation = value; }
        }

        public ExtendedTimer(int index, string associatedLocation)
            : base()
        {
            this.timerIndex = index;
            this.timerLocation = associatedLocation;
        }

    }
}
