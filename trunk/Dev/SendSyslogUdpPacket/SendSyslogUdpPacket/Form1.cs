using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SendSyslogUdpPacket.Properties;

namespace SendSyslogUdpPacket
{
    public partial class Form1 : Form
    {
        delegate void updateLabelTextDelegate(string newText);

        public Form1()
        {
            InitializeComponent();
        }

        private void updateLabelText(string newText)
        {
            if (label5.InvokeRequired)
            {
                updateLabelTextDelegate del = updateLabelText;
                label5.Invoke(del, new object[] { newText });
            }
            else
            {
                label5.Text = newText;
            }
        }

        private void btnSendData_Click(object sender, EventArgs e)
        {
            //if (backgroundWorker1.IsBusy == true)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            CancelButtonOperation();
        }

        private void CancelButtonOperation()
        {
            if (backgroundWorker1.WorkerSupportsCancellation)
            {
                backgroundWorker1.CancelAsync();
            }
        } // CancelButtonOperation

        private void SendData(int port, string ipAdress, string line)
        {
            if (port <= 0) return;
            var udpClient = new UdpClient(port);
            try
            {
                udpClient.Connect(ipAdress, port);
                byte[] sendBytes = Encoding.ASCII.GetBytes(line);
                udpClient.Send(sendBytes, sendBytes.Length);
                var udpClientB = new UdpClient();
                udpClientB.Send(sendBytes, sendBytes.Length, ipAdress, port);
                udpClient.Close();
                udpClientB.Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        } // SendData

        private void SendData()
        {
            try
            {
                int port = Convert.ToInt32(txtPort.Text.Trim());
                string ipAdress = !string.IsNullOrEmpty(txtIpAdress.Text) ? txtIpAdress.Text.Trim() : "127.0.0.1";
                var sleepTime = !string.IsNullOrEmpty(txtSleepTime.Text) ? Convert.ToInt64(txtSleepTime.Text.Trim()) : 1;
                //var encoding = Encoding.Default;
                var fullFileName = txtFileLocation.Text.Trim();
                //char ch;
                //var stringBuilder = new StringBuilder();
                var fileStream = new FileStream(fullFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                //var binaryReader = new BinaryReader(fileStream, encoding);
                string line;
                int lineCounter = 1;
                //while (Position < fileStream.Length)
                //{
                //    ch = binaryReader.ReadChar();
                //    stringBuilder.Append(ch);
                //    char carriageReturn = '\r';
                //    char newLine = '\n';
                //    Position = binaryReader.BaseStream.Position;
                //    if (ch == newLine || ch == carriageReturn)
                //    {
                //        line = stringBuilder.ToString();
                //        SendData(port, ipAdress, line);
                //        if (ch != newLine || ch != carriageReturn)
                //        {
                //            lineCounter++;
                //        }
                //        stringBuilder.Remove(0, stringBuilder.Length);
                //        Thread.Sleep((int)sleepTime);
                //    }
                //    //backgroundWorker1.ReportProgress((int)((Position * 100) / fileStream.Length));
                //}

                var streamReader = new StreamReader(fileStream);
                while ((line = streamReader.ReadLine()) != null)
                {
                    SendData(port, ipAdress, line);
                    lineCounter++;
                    Thread.Sleep((int)sleepTime);
                }

                MessageBox.Show(String.Format("{0} Position:", Position));
                MessageBox.Show(String.Format("{0} length: ", fileStream.Length));
                MessageBox.Show(String.Format("{0} lines sended", lineCounter));
            }
            catch (Exception exception)
            {
                MessageBox.Show(Resources.Form1_SendData_Data_gönderilirken_bir_hata_oluştu_ + exception.Message);
            }
        }// SendData

        private bool WriteData(string line)
        {
            const string path = "C:\\tmp\\a.txt";
            try
            {
                var streamWriter = new StreamWriter(File.Open(path, FileMode.Append));
                streamWriter.Write(line);
                streamWriter.Close();
                return true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
                return false;
            }
        } // WriteData

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenButtonOperation();
        }

        private void OpenButtonOperation()
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtFileLocation.Text = openFileDialog1.FileName;
            }
        } // OpenButtonOperation

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker1DoWork();
        }

        private void BackgroundWorker1DoWork()
        {
            //var worker = sender as BackgroundWorker;
            updateLabelText("Okunan satırlar " + txtIpAdress.Text + " makinesine " + txtPort.Text +
                            " portundan gönderiliyor." + "\r\nİptal etmek için Cancel düğmesine basınız.");

            SendData();
        } // BackgroundWorker1DoWork

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BackgroundWorker1ProgressChanged(e);
        }

        private void BackgroundWorker1ProgressChanged(ProgressChangedEventArgs e)
        {
            label5.Text = e.ProgressPercentage.ToString(CultureInfo.InvariantCulture);
        } // BackgroundWorker1ProgressChangedOperation

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker1RunWorkerCompleted(e);
        }

        private void BackgroundWorker1RunWorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                label5.Text = Resources.Form1_BackgroundWorker1RunWorkerCompleted_Canceled_;
            }

            else if (e.Error != null)
            {
                label5.Text = (Resources.Form1_BackgroundWorker1RunWorkerCompleted_Error__ + e.Error.Message);
            }

            else
            {
                label5.Text = Resources.Form1_BackgroundWorker1RunWorkerCompleted_Done_;
            }
        } // BackgroundWorker1RunWorkerCompleted
    }
}
