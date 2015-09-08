using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RemoteRecorderFeedback
{
    public partial class SQLServerLogin : Form
    {
        private static RemoteRecorderFeedBackProgram instance;
        public bool connectionOpen;

        public static RemoteRecorderFeedBackProgram Instance
        {
            get { return instance ?? (instance = new RemoteRecorderFeedBackProgram()); }
        }

        private static CEncryptDecrypt instanceEncryptDecrypt;

        public static CEncryptDecrypt InstanceEncryptDecrypt
        {
            get { return instanceEncryptDecrypt ?? (instanceEncryptDecrypt = new CEncryptDecrypt()); }
        }


        public string serverName { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        private SqlConnection myConnection;

        public SQLServerLogin()
        {
            InitializeComponent();
            var remoteRecorderFeedBackProgram = Instance;
            var encryptDecrypt = InstanceEncryptDecrypt;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OperationComplete();
        }

        private void OperationComplete()
        {
            try
            {
                connectionOpen = TestConnection();
                if (connectionOpen)
                {
                    serverName = txtServerName.Text;
                    userName = txtUserName.Text;
                    password = txtPassword.Text;

                    instance.serverName = serverName;
                    instance.userName = userName;
                    instance.password = password;

                    using (var stream = new StreamWriter("DBConnection.conf"))
                    {
                        var dbParameters = string.Format("ServerName {0}, UserName {1}, Password {2} ", serverName,
                                                         userName, instanceEncryptDecrypt.Encrypt(password, "key123"));
                        stream.WriteLine(dbParameters);
                    }

                    instance.ShowDialog();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("OperationComplete: " + exception.Message);
            }
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            connectionOpen = TestConnection();
        }

        private bool TestConnection()
        {
            try
            {
                serverName = txtServerName.Text;
                userName = txtUserName.Text;
                password = txtPassword.Text;

                const string Query = "SELECT name FROM master..sysdatabases";
                var connectionString = "Server=" + serverName +
                                       ";Database=master;User Id=" + userName +
                                       ";Password=" + password + ";";

                myConnection = new SqlConnection(connectionString);
                try
                {
                    myConnection.Open();
                    label1.ForeColor = Color.Black;
                    label1.Text = "Successful Test Connection.";
                    myConnection.Close();
                    return true;
                }
                catch (Exception)
                {
                    label1.ForeColor = Color.Red;
                    label1.Text = "Test Connection Failed.";
                    return false;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("TestConnection: " + exception.Message);
                return false;
            }
        }

        private void SQLServerLogin_Load(object sender, EventArgs e)
        {
            MaximizeBox = false;
            MinimizeBox = false;

            try
            {
                if (File.Exists("DBConnection.conf"))
                {
                    using (var stream = new StreamReader("DBConnection.conf"))
                    {
                        var line = stream.ReadLine();
                        var lineArr = line.Split(',');
                        serverName = lineArr[0].Split(' ')[1];
                        userName = lineArr[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1];
                        password = instanceEncryptDecrypt.Decrypt(lineArr[2].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1], "key123");
                    }

                    txtUserName.Text = userName;
                    txtServerName.Text = serverName;
                    txtPassword.Text = password;

                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("SQLServerLogin_Load: " + exception.Message);
            }
        }
    }
}
