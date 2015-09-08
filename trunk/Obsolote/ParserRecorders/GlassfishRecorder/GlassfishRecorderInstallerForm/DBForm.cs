/*
 * Database Installer Form
 * Copyright (C) 2008 Erdoðan Kalemci <olligan@gmail.com>
 * You have no rights to distrubute, modify and use this code unless writer gives permission
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Configuration.Install;
using Microsoft.Win32;
using DAL;

namespace GlassfishRecorderInstallerForm
{
    public partial class DBForm : Form
    {
        private bool normalClose;
        private Installer pInstaller;
        public String dbName;

        public DBForm()
        {
            InitializeComponent();
            LoadProvider();
            normalClose = false;
        }

        public DBForm(Installer pInstallerP)
        {
            InitializeComponent();
            LoadProvider();
            normalClose = false;
            pInstaller = pInstallerP;
            dbName = "";
        }

        private void LoadProvider()
        {
            cBoxProvider.Items.Add(Database.Provider.SQLServer);
            cBoxProvider.Items.Add(Database.Provider.Oracle);
            cBoxProvider.Items.Add(Database.Provider.MySQL);
            cBoxProvider.SelectedItem = Database.Provider.SQLServer;
        }

        private void butTest_Click(object sender, EventArgs e)
        {
            Database.AddProviderToRegister((Database.Provider)cBoxProvider.SelectedItem, "temp", txtHost.Text, txtDB.Text, txtUser.Text, txtPassword.Text);
            try
            {
                Database.Fast = false;
                switch ((Database.Provider)cBoxProvider.SelectedItem)
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
                statusStripLabel.Text = "Error in connection check parameters!";
                RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Natek\\DAL",true);
                key.DeleteSubKeyTree("temp");
                MessageBox.Show("Error in connection check parameters!");
                return;
            }

            RegistryKey keyAdd = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Natek\\DAL",true);
            keyAdd.DeleteSubKeyTree("temp");
            butUpdate.Enabled = true;
            statusStripLabel.Text = "Connected...";
        }

        private void butUpdate_Click(object sender, EventArgs e)
        {
            Database.AddProviderToRegister((Database.Provider)cBoxProvider.SelectedItem, dbName, txtHost.Text, txtDB.Text, txtUser.Text, txtPassword.Text);
            butUpdate.Enabled = false;
            normalClose = true;
            Close();
        }

        private void txtHost_TextChanged(object sender, EventArgs e)
        {
            if (butUpdate.Enabled)
                butUpdate.Enabled = false;
        }

        private void txtDB_TextChanged(object sender, EventArgs e)
        {
            if (butUpdate.Enabled)
                butUpdate.Enabled = false;
        }

        private void txtUser_TextChanged(object sender, EventArgs e)
        {
            if (butUpdate.Enabled)
                butUpdate.Enabled = false;
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            if (butUpdate.Enabled)
                butUpdate.Enabled = false;
        }

        private void cBoxProvider_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (butUpdate.Enabled)
                butUpdate.Enabled = false;
        }

        private void DBForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (normalClose)
                return;
        }
    }
}