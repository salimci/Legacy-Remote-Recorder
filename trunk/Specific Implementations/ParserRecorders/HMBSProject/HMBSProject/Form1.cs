using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DAL;
using Microsoft.Win32;
using System.Data.Common;


namespace NatekLogService
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //FormFeatures formFeatures = new FormFeatures();
            //LoadActionType();
            //formFeatures.LoadRuntimeandPeriod();
            //formFeatures.LoadFilterName();
            //FilterService deneme = new FilterService();
        }

        private void LoadActionType()
        {
            cmBoxActionType.Items.Add("MAIL");
            cmBoxActionType.SelectedIndex = 0;
        }

        private void cmBoxFilterName_SelectedIndexChanged(object sender, EventArgs e)
        {
            FormFeatures formFeatures = new FormFeatures(this, cmBoxFilterName.SelectedItem.ToString());
        }

        private void btnAddFilter_Click(object sender, EventArgs e)
        {
            Filter filter = new Filter(this);
            filter.AddFilter();
        }

        private void btnUpdateFilter_Click(object sender, EventArgs e)
        {
            Filter filter = new Filter(this);
            filter.UpdateFilter();
        }

        private void btnDeleteFilter_Click(object sender, EventArgs e)
        {
            Filter filter = new Filter(this);
            filter.DeleteFilter();
            cmBoxFilterName.Items.Remove(cmBoxFilterName.SelectedItem);
        }

        private void btnUpdateRuntime_Click(object sender, EventArgs e)
        {
            Filter filter = new Filter(this);
            filter.UpdateRuntime();
        }

        private void btnGetTableValues_Click(object sender, EventArgs e)
        {
            try
            {
                DisposeControls();
                cmBoxFilterName.Items.Clear();
                Rsc.LogTbl = txtBoxTblName.Text;
                FormFeatures formFeatures = new FormFeatures(this);
                LoadActionType();
                formFeatures.LoadRuntimeandPeriod();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DisposeControls()
        {
            try
            {
                foreach (Control item in panel1.Controls)
                {
                    item.Dispose();
                }

                if (panel1.Controls.Count > 0)
                {
                    DisposeControls();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

    }
}
