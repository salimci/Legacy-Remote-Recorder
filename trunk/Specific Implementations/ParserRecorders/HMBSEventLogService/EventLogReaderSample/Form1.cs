using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics;
using System.Timers;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Parser;
using Microsoft.Win32;
using System.Globalization;

namespace EventLogReaderSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HMBSEventLogRecorder testParse = new Parser.HMBSEventLogRecorder();
            //testParse.PrivateParse(textBox1.Text);

        }
    }
}
