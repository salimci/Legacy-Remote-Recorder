using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;

namespace RemoteRecorderFeedback
{
    public partial class RemoteRecorderFeedBackProgram : Form
    {

        public string serverName { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public string database { get; set; }
        public string virtualHost { get; set; }
        public string dt { get; set; }

        public RemoteRecorderFeedBackProgram()
        {
            InitializeComponent();
        }

        public bool GetNatekRegistryInfo(string FolderName)
        {
            try
            {
                if (string.IsNullOrEmpty(txtCompanyName.Text) && string.IsNullOrEmpty(txtDescription.Text) && string.IsNullOrEmpty(txtVersion.Text))
                {
                    lblMessage.ForeColor = Color.Black;
                    lblMessage.Text = "Lütfen zorunlu alanları doldurunuz.";
                    return false;
                }

                if (!Directory.Exists(FolderName))
                {
                    Directory.CreateDirectory(FolderName);
                }

                if (!ToXML(txtCompanyName.Text, txtDescription.Text, FolderName + "\\BugReport.xml",
                      txtVersion.Text))
                {
                    return false;
                }

                if (!ExportRegistries(FolderName, @"SOFTWARE\NATEK\DAL", FolderName + "\\NatekRegistryList.xml"))
                {
                    return false;
                }

                if (!ExportRegistries("Registries", @"SOFTWARE\NATEK\Security Manager\Remote Recorder",
                                 FolderName + "\\NatekRemoteRecorder.xml"))
                {
                    return false;
                }

                if (!VersionControl())
                    return false;

                //                ZipFolder("Registries.zip", "", "Registries");

                return true;

            }
            catch (Exception exception)
            {
                MessageBox.Show("GetNatekRegistryInfo: " + exception.Message);
                return false;
            }
        }// GetNatekRegistryInfo

        public bool VersionControl()
        {
            try
            {
                string version = null;
                if (File.Exists("Logs\\NatekRemoteRecorder.xml"))
                {
                    using (var stream = new StreamReader("Logs\\NatekRemoteRecorder.xml"))
                    {
                        string line;

                        while ((line = stream.ReadLine()) != null)
                        {
                            if (line != null && line.Trim().Contains("version"))
                            {
                                version = Between(line, "keyValue=", "/>").Replace('"', ' ').Trim();
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(version))
                {
                    if (!String.Equals(version, txtVersion.Text))
                    {
                        MessageBox.Show("Remote Recorder versiyonunuz güncel değil. \r\nLütfen Remote Recorder versiyonunuzu yeni versiyon olarak yükseltin.");
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                MessageBox.Show("VersionControl: " + exception.Message);
                return false;
            }
        }// VersionControl

        public bool ToXML(object oCompanyName, object oDscription, string outPutFileName, object oVersion)
        {
            try
            {
                var doc = new XmlDocument();
                XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(docNode);

                XmlNode productsNode = doc.CreateElement("RemoteRecorderFeedbackOutputFile");
                doc.AppendChild(productsNode);

                XmlNode productNode = doc.CreateElement("CompanyName");
                var productAttribute = doc.CreateAttribute("CompanyName");
                productAttribute.Value = (string)oCompanyName;
                if (productNode.Attributes != null) productNode.Attributes.Append(productAttribute);
                productsNode.AppendChild(productNode);

                productNode = doc.CreateElement("Description");
                productAttribute = doc.CreateAttribute("Description");
                productAttribute.Value = (string)oDscription;

                XmlNode productNode2 = doc.CreateElement("Version");
                var productAttribute2 = doc.CreateAttribute("Version");
                productAttribute2.Value = (string)oVersion;
                if (productNode.Attributes != null) productNode.Attributes.Append(productAttribute);
                productsNode.AppendChild(productNode);

                if (productNode2.Attributes != null) productNode2.Attributes.Append(productAttribute2);
                productsNode.AppendChild(productNode2);

                doc.Save(outPutFileName);
                return true;
            }
            catch (Exception exception)
            {
                MessageBox.Show("ToXML: " + exception.Message);
                return false;
            }
        }// ToXML

        private void RemoteRecorderFeedBackProgram_Load(object sender, EventArgs e)
        {
            FormLoad();
        }

        private void FormLoad()
        {
            var ss = new SQLServerLogin();
            try
            {
                GetDatabaseNames();
            }
            catch (Exception exception)
            {
                MessageBox.Show("RemoteRecorderFeedBackProgram_Load: " + exception.Message);
            }
        }// FormLoad

        public void GetDatabaseNames()
        {
            try
            {
                const string Query = "SELECT name FROM master..sysdatabases";
                var connectionString = "Server=" + serverName +
                                       ";Database=master;User Id=" + userName +
                                       ";Password=" + password + ";";

                var myConnection = new SqlConnection(connectionString);
                try
                {
                    myConnection.Open();
                }
                catch (Exception e)
                {
                    MessageBox.Show("GetDatabaseNames Cannot Open Connection: " + e.Message);
                }

                if (cboSelectDatabase.Items.Count > 0)
                    cboSelectDatabase.Items.Clear();

                var cmd = new SqlCommand(Query, myConnection);
                var dr = cmd.ExecuteReader();

                while (dr.Read())
                    cboSelectDatabase.Items.Add(dr[0].ToString());
                dr.Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show("GetDatabaseNames:" + exception.Message);
            }
        }// GetDatabaseNames

        private void cboSelectDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            database = cboSelectDatabase.SelectedItem.ToString();
            GetRemoteRecorders();
        }

        private void GetRemoteRecorders()
        {
            try
            {
                database = cboSelectDatabase.SelectedItem.ToString();
                var Query =
                    "SELECT ID, RECORDERNAME, TRACELEVEL, VIRTUALHOST FROM REMOTE_RECORDER WHERE STATUS = 1";

                var connectionString = "Server=" + serverName +
                                       ";Database=" + database +
                                       ";User Id=" + userName +
                                       ";Password=" + password + ";";

                var myConnection = new SqlConnection(connectionString);
                try
                {
                    myConnection.Open();
                }
                catch (Exception e)
                {
                    MessageBox.Show("GetRemoteRecorders Cannot Open Connection." + e.Message);
                }

                var cmd = new SqlCommand(Query, myConnection);
                var dr = cmd.ExecuteReader();

                lstRemoteRecorders.View = View.Details;
                lstRemoteRecorders.CheckBoxes = true;
                lstRemoteRecorders.Columns.Add("Remote Recorder ID", 50, HorizontalAlignment.Left);
                lstRemoteRecorders.Columns.Add("Remote Recorder Name", 250, HorizontalAlignment.Left);
                lstRemoteRecorders.Columns.Add("Trace Level", 75, HorizontalAlignment.Left);
                lstRemoteRecorders.Columns.Add("Virtual Host", 125, HorizontalAlignment.Left);

                while (dr.Read())
                {
                    var oItem = new ListViewItem();
                    lstRemoteRecorders.Items.Add(oItem);

                    var id = dr[0].ToString();
                    oItem.Text = id;
                    var traceLevel = dr[2].ToString();
                    oItem.SubItems.Add(dr[1].ToString());
                    oItem.SubItems.Add(traceLevel);
                    oItem.SubItems.Add(dr[3].ToString());
                }
                dr.Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show("GetRemoteRecorders:" + exception.Message);
            }
        }// GetRemoteRecorders

        private static bool ExportRegistries(string exportPath, string registryPath, string outPutFileName)
        {
            try
            {
                var regPath = registryPath;
                var xRegRoot = new XElement("Root", new XAttribute("Registry", regPath));
                ReadRegistry(regPath, xRegRoot);
                var xmlStringReg = xRegRoot.ToString();
                var docR = new XmlDocument();
                docR.LoadXml(xmlStringReg);
                docR.Save(AppDomain.CurrentDomain.BaseDirectory + outPutFileName);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ExportRegistries: " + ex.Message);
                return false;
            }
        }// ExportRegistries

        private static void ReadRegistry(string keyPath, XElement xRegRoot)
        {
            var localMachine = Registry.LocalMachine;
            var RegKey = localMachine.OpenSubKey(keyPath);

            try
            {
                if (RegKey != null)
                {
                    string[] subKeys = RegKey.GetSubKeyNames();
                    foreach (string subKey in subKeys)
                    {
                        string fullPath = keyPath + "\\" + subKey;
                        var xregkey = new XElement("RegKeyName",
                                                   new XAttribute("FullName", fullPath), new XAttribute("Name", subKey));
                        xRegRoot.Add(xregkey);
                        ReadRegistry(fullPath, xRegRoot);
                    }
                }

                if (RegKey != null)
                {
                    var subVals = RegKey.GetValueNames();
                    foreach (var val in subVals)
                    {
                        var keyName = val;
                        var keyType = RegKey.GetValueKind(val).ToString();
                        var keyValue = RegKey.GetValue(val).ToString();

                        var xregvalue = new XElement("RegKeyValue",
                                                     new XAttribute("keyType", keyType),
                                                     new XAttribute("keyName", keyName),
                                                     new XAttribute("keyValue", keyValue));
                        xRegRoot.Add(xregvalue);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }// ReadRegistry

        public void GetCheckedRemoteRecorders()
        {
            dt = DateTime.Now.ToString("yyyyMMddHHmmss");

            if (cboSelectDatabase.SelectedIndex == -1)
            {
                MessageBox.Show("Lütfen Database Seçiniz.");
                return;
            }

            if (lstRemoteRecorders.CheckedItems.Count == 0)
            {
                MessageBox.Show("Lütfen enaz 1 adet Recorder seçiniz.");
                return;
            }
            try
            {
                //if (GetNatekRegistryInfo())
                {
                    var checkedRemoteRecorders = new Dictionary<string, int>();
                    var checkedRemoteRecordersTraceLevel = new Dictionary<string, int>();
                    var checkedRemoteRecordersVirtualHost = new List<string>();

                    if (lstRemoteRecorders.CheckedItems.Count <= 0) return;
                    for (var i = 0; i < lstRemoteRecorders.CheckedItems.Count; i++)
                    {
                        var keyName = Between(lstRemoteRecorders.CheckedItems[i].SubItems[1].ToString(), "{", "}");
                        var valueId = Convert.ToInt32(Between(lstRemoteRecorders.CheckedItems[i].ToString(), "{", "}"));
                        var valueTraceLevel = Between(lstRemoteRecorders.CheckedItems[i].SubItems[2].ToString(), "{",
                                                      "}");
                        checkedRemoteRecorders.Add(keyName + valueId, valueId);
                        checkedRemoteRecordersTraceLevel.Add(keyName + valueId,
                                                             Convert.ToInt32(
                                                                 valueTraceLevel.ToString(CultureInfo.InvariantCulture)));
                        checkedRemoteRecordersVirtualHost.Add(
                            Between(lstRemoteRecorders.CheckedItems[i].SubItems[3].ToString(), "{",
                                    "}"));
                    }

                    string informModRecorders = null;

                    foreach (var s in checkedRemoteRecordersTraceLevel)
                    {
                        if (s.Value != 4)
                        {
                            informModRecorders += s.Key;
                            informModRecorders += "\r\n";
                        }
                    }

                    var message = string.Format(
                        "Seçilen, \r\n{0}recorder yada recorderlar Debug modda değil.\r\n" +
                        "Inform modda loglar toplanacak.\r\n" +
                        "Lütfen Debug modda programı tekrar çalıştırınız.",
                        informModRecorders);

                    if (!string.IsNullOrEmpty(informModRecorders))
                    {
                        var result = MessageBox.Show(message, "Warning", MessageBoxButtons.OK);
                        if (result == DialogResult.OK)
                        {
                            var folderName = "Faz-1" + dt;
                            CollectLogs(checkedRemoteRecorders, checkedRemoteRecordersVirtualHost, folderName);
                            GetNatekRegistryInfo(folderName);
                        }
                    }

                    else
                    {
                        var folderName = "Faz-2" + dt;
                        CollectLogs(checkedRemoteRecorders, checkedRemoteRecordersVirtualHost, folderName);
                        GetNatekRegistryInfo(folderName);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("GetCheckedRemoteRecorders: " + exception.Message);
            }
        }// GetCheckedRemoteRecorders

        public static void ZipFolder(string ZipFileName, string password, string folderName)
        {
            try
            {
                var fsOut = File.Create(ZipFileName + ".zip");
                var zipStream = new ZipOutputStream(fsOut);
                zipStream.SetLevel(3);
                if (!string.IsNullOrEmpty(password))
                    zipStream.Password = password;
                var folderOffset = folderName.Length + (folderName.EndsWith("\\") ? 0 : 1);
                CompressFolder(folderName, zipStream, folderOffset);
                zipStream.IsStreamOwner = true;
                zipStream.Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show("ZipFolder: " + exception.Message);
            }
        }

        public static void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {
            try
            {
                var files = Directory.GetFiles(path);
                foreach (var filename in files)
                {
                    var fi = new FileInfo(filename);
                    var entryName = filename.Substring(folderOffset);
                    entryName = ZipEntry.CleanName(entryName);
                    var newEntry = new ZipEntry(entryName)
                                       {
                                           DateTime = fi.LastWriteTime,
                                           Size = fi.Length
                                       };
                    zipStream.PutNextEntry(newEntry);
                    var buffer = new byte[4096];
                    using (var streamReader = File.OpenRead(filename))
                    {
                        StreamUtils.Copy(streamReader, zipStream, buffer);
                    }
                    zipStream.CloseEntry();
                }
                var folders = Directory.GetDirectories(path);
                foreach (string folder in folders)
                {
                    CompressFolder(folder, zipStream, folderOffset);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("CompressFolder: " + exception.Message);
            }
        }// CompressFolder

        public void CollectLogs(Dictionary<string, int> RecorderList, List<string> VirtualHostList, string LogFolderName)
        {
            if (!Directory.Exists(LogFolderName))
            {
                Directory.CreateDirectory(LogFolderName);
            }

            try
            {
                const string recorderLogFilePath = @"C:\Program Files\Natek\Security Manager\Remote Recorder\log";
                foreach (var kvp in from kvp in RecorderList
                                    let tempPath = recorderLogFilePath + "\\" + kvp.Key + ".log"
                                    where File.Exists(tempPath)
                                    select kvp)
                {
                    File.Copy(recorderLogFilePath + "\\" + kvp.Key + ".log",
                              LogFolderName + "\\" + kvp.Key + "-" + dt + ".log");
                }

                var logList = Directory.GetFiles(recorderLogFilePath, "*.log");

                foreach (var s in logList)
                {
                    var shortFileName = Path.GetFileName(s);
                    if (shortFileName.StartsWith("RemoteRecorder"))
                    {
                        File.Copy(s, LogFolderName + "\\" + shortFileName.Split('.')[0] + "-" + dt + ".log");
                    }
                    if (shortFileName.StartsWith("Reloader"))
                    {
                        File.Copy(s, LogFolderName + "\\" + shortFileName.Split('.')[0] + "-" + dt + ".log");
                    }
                }

                var filterLogFilePath = @"C:\Program Files\Natek\Security Manager\Server\log";
                var filterLogFileList = Directory.GetFiles(filterLogFilePath, "Filter*.log");

                foreach (var s in filterLogFileList)
                {
                    var shortFileName = Path.GetFileName(s);
                    foreach (var virtualHost in VirtualHostList)
                    {
                        if (shortFileName.StartsWith("Filter") && shortFileName.Contains(virtualHost))
                        {
                            var targetFileterLogFilePath = LogFolderName + "\\" + shortFileName.Split('.')[0] + "-" + dt + ".log";
                            if (!File.Exists(targetFileterLogFilePath))
                            {
                                File.Copy(s, targetFileterLogFilePath);
                            }
                        }
                    }
                }

                ZipFolder(LogFolderName, "", LogFolderName);
                lblMessage.ForeColor = Color.Black;
                lblMessage.Text = "Gerekli tüm loglar Logs dizini altına toplandı.";

            }
            catch (Exception exception)
            {
                MessageBox.Show("CollectLogs: " + exception.Message);
            }
        }// CollectLogs

        private void btnCollectLogs_Click(object sender, EventArgs e)
        {
            GetCheckedRemoteRecorders();
        }

        public static string Between(string value, string a, string b)
        {
            var posA = value.IndexOf(a, StringComparison.Ordinal);
            var posB = value.LastIndexOf(b, StringComparison.Ordinal);

            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            var adjustedPosA = posA + a.Length;
            return adjustedPosA >= posB ? "" : value.Substring(adjustedPosA, posB - adjustedPosA);
        }// Between


        private void lblAll_MouseMove(object sender, MouseEventArgs e)
        {
            var font = lblAll.Font;
            lblAll.Font = new Font(font, FontStyle.Underline);
            Cursor = Cursors.Hand;
        }

        private void lblAll_MouseLeave(object sender, EventArgs e)
        {
            var font = lblAll.Font;
            lblAll.Font = new Font(font, FontStyle.Regular);
            Cursor = Cursors.Arrow;

        }

        private void lblNone_MouseMove(object sender, MouseEventArgs e)
        {
            var font = lblNone.Font;
            lblNone.Font = new Font(font, FontStyle.Underline);
            Cursor = Cursors.Hand;
        }

        private void lblNone_MouseLeave(object sender, EventArgs e)
        {
            var font = lblNone.Font;
            lblNone.Font = new Font(font, FontStyle.Regular);
            Cursor = Cursors.Arrow;
        }

        private void lblNone_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lstRemoteRecorders.Items.Count; i++)
            {
                lstRemoteRecorders.Items[i].Checked = false;
            }
        }

        private void lblAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lstRemoteRecorders.Items.Count; i++)
            {
                lstRemoteRecorders.Items[i].Checked = true;
            }
        }

        private void RemoteRecorderFeedBackProgram_FormClosed(object sender, FormClosedEventArgs e)
        {
            FormClosedEvent();
        }

        private static void FormClosedEvent()
        {
            Application.Exit();
        }//FormClosedEvent

        private void lblRefresh_Click(object sender, EventArgs e)
        {
            lstRemoteRecorders.Items.Clear();
            GetRemoteRecorders();
        }

        private void lblRefresh_MouseLeave(object sender, EventArgs e)
        {
            var font = lblRefresh.Font;
            lblRefresh.Font = new Font(font, FontStyle.Regular);
            Cursor = Cursors.Arrow;
        }

        private void lblRefresh_MouseMove(object sender, MouseEventArgs e)
        {
            var font = lblRefresh.Font;
            lblRefresh.Font = new Font(font, FontStyle.Underline);
            Cursor = Cursors.Hand;
        }
    }
}
