using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using DAL;

namespace GlassfishRecorderDBTool
{
    public partial class RegisteryTool : Form
    {
        public string registryPath = "SOFTWARE\\Natek\\DAL";
        public RegistryKey names;

        public RegisteryTool()
        {
            names = Registry.LocalMachine.OpenSubKey(registryPath, true);
            InitializeComponent();
            LoadKeys();
            LoadProvider();
        }

        private void LoadKeys()
        {
            foreach (string s in names.GetSubKeyNames())
            {
                lstKeys.Items.Add(s);
            }
        }

        private void LoadProvider()
        {
            cBoxProvider.Items.Add(Database.Provider.SQLServer);
            cBoxProvider.Items.Add(Database.Provider.Oracle);
            cBoxProvider.Items.Add(Database.Provider.MySQL);
            cBoxProvider.SelectedItem = Database.Provider.SQLServer;
        }

        private void butDeleteKey_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = MessageBox.Show(lstKeys.SelectedItem.ToString() + " anahtarýný siliyorsunuz, devam etmek istiyor musunuz?", "Uyarý", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dr == DialogResult.OK)
                {
                    names.DeleteSubKey(lstKeys.SelectedItem.ToString());
                    lstKeys.Items.Clear();
                    LoadKeys();
                }
            }
            catch
            {
            }
        }

        private void butAddKey_Click(object sender, EventArgs e)
        {
            if (txtName.Text != "" 
                && txtHost.Text != ""
                && txtDB.Text != ""
                && txtUser.Text != "")
            {
                Database.Provider p = (Database.Provider)(Convert.ToInt32(cBoxProvider.SelectedItem));
                Database.AddProviderToRegister(p, "temp", txtHost.Text, txtDB.Text, txtUser.Text, txtPassword.Text);
                try
                {
                    Database.Fast = false;
                    switch (p)
                    {
                        case Database.Provider.SQLServer:
                            Database.ExecuteNonQuery("temp", "SELECT * FROM SYSOBJECTS");
                            break;
                        default:
                            Database.ExecuteNonQuery("temp", "Show tables");
                            break;
                    }
                }
                catch
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Natek\\DAL", true);
                    key.DeleteSubKeyTree("temp");
                    MessageBox.Show("Bilgileriniz yanlýþ baþtan girin");
                    return;
                }

                RegistryKey keyAdd = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Natek\\DAL", true);
                keyAdd.DeleteSubKeyTree("temp");

                if(!lstKeys.Items.Contains(cBoxProvider.SelectedItem+txtDB.Text))
                    Database.AddProviderToRegister((Database.Provider)cBoxProvider.SelectedItem, txtName.Text, txtHost.Text, txtDB.Text, txtUser.Text, txtPassword.Text);
                lstKeys.Items.Clear();
                LoadKeys();
                txtDB.Text = "";
                txtHost.Text = "";
                txtPassword.Text = "";
                txtName.Text = "";
                txtUser.Text = "";
            }
        }
    }
}