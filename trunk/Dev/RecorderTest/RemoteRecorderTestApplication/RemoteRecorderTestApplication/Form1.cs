using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace RemoteRecorderTestApplication
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void OpenRemoteRecorderClick(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                txtRecordersPath.Text = openFileDialog1.FileName;
            }
        }

        public bool OpenFile(string fileName)
        {
            MessageBox.Show(Path.GetDirectoryName(fileName));
            OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Path.GetDirectoryName(fileName) + ";Extended Properties=dBASE IV;User ID=;Password=;");
            try
            {
                if (con.State == ConnectionState.Closed) { con.Open(); }
                OleDbDataAdapter da = new OleDbDataAdapter("select * from " + Path.GetFileName(fileName), con);
                DataSet ds = new DataSet();
                da.Fill(ds);
                con.Close();
                int i = ds.Tables[0].Rows.Count;
                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return true;
        }

        private void LoadRemoteRecorderClick(object sender, EventArgs e)
        {
            //try
            //{
            //    Assembly myDllAssembly = Assembly.LoadFile(txtRecordersPath.Text.Trim());
            //    MyDLLFormType = myDllAssembly.GetType("MyDLLNamespace.MyDLLForm");
            //    myDllAssembly.CreateInstance()
            //}
            //catch (Exception exception)
            //{
            //    MessageBox.Show("Can not load assembly.");
            //}

            OpenFile(txtRecordersPath.Text);
        }
    }
}
