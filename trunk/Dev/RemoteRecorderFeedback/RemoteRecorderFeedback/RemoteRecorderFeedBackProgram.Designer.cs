namespace RemoteRecorderFeedback
{
    partial class RemoteRecorderFeedBackProgram
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label2 = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.cboSelectDatabase = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCompanyName = new System.Windows.Forms.TextBox();
            this.btnCollectLogs = new System.Windows.Forms.Button();
            this.txtVersion = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.lstRemoteRecorders = new System.Windows.Forms.ListView();
            this.btnTest = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblAll = new System.Windows.Forms.Label();
            this.lblNone = new System.Windows.Forms.Label();
            this.lblMessage = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblRefresh = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Description";
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(113, 65);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(230, 166);
            this.txtDescription.TabIndex = 2;
            this.txtDescription.Text = "TEST DRIVE";
            // 
            // cboSelectDatabase
            // 
            this.cboSelectDatabase.FormattingEnabled = true;
            this.cboSelectDatabase.Location = new System.Drawing.Point(119, 249);
            this.cboSelectDatabase.Name = "cboSelectDatabase";
            this.cboSelectDatabase.Size = new System.Drawing.Size(230, 21);
            this.cboSelectDatabase.TabIndex = 0;
            this.cboSelectDatabase.Text = "Select Database";
            this.cboSelectDatabase.SelectedIndexChanged += new System.EventHandler(this.cboSelectDatabase_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(6, 253);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 27);
            this.label3.TabIndex = 7;
            this.label3.Text = "Select Configuration Database";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Company Name";
            // 
            // txtCompanyName
            // 
            this.txtCompanyName.Location = new System.Drawing.Point(113, 39);
            this.txtCompanyName.Name = "txtCompanyName";
            this.txtCompanyName.Size = new System.Drawing.Size(230, 20);
            this.txtCompanyName.TabIndex = 1;
            this.txtCompanyName.Text = "NATEK";
            // 
            // btnCollectLogs
            // 
            this.btnCollectLogs.Location = new System.Drawing.Point(753, 248);
            this.btnCollectLogs.Name = "btnCollectLogs";
            this.btnCollectLogs.Size = new System.Drawing.Size(75, 23);
            this.btnCollectLogs.TabIndex = 2;
            this.btnCollectLogs.Text = "Collect Logs";
            this.btnCollectLogs.UseVisualStyleBackColor = true;
            this.btnCollectLogs.Click += new System.EventHandler(this.btnCollectLogs_Click);
            // 
            // txtVersion
            // 
            this.txtVersion.Location = new System.Drawing.Point(113, 13);
            this.txtVersion.Name = "txtVersion";
            this.txtVersion.Size = new System.Drawing.Size(230, 20);
            this.txtVersion.TabIndex = 0;
            this.txtVersion.Text = "5.4.6";
            // 
            // label7
            // 
            this.label7.ForeColor = System.Drawing.Color.Red;
            this.label7.Location = new System.Drawing.Point(6, 12);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(94, 30);
            this.label7.TabIndex = 17;
            this.label7.Text = "RemoteRecorder Latest Version";
            // 
            // lstRemoteRecorders
            // 
            this.lstRemoteRecorders.Location = new System.Drawing.Point(369, 29);
            this.lstRemoteRecorders.Name = "lstRemoteRecorders";
            this.lstRemoteRecorders.Size = new System.Drawing.Size(465, 214);
            this.lstRemoteRecorders.TabIndex = 1;
            this.lstRemoteRecorders.UseCompatibleStateImageBehavior = false;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(0, 0);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 31;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtDescription);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.txtCompanyName);
            this.groupBox2.Controls.Add(this.txtVersion);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Location = new System.Drawing.Point(6, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(357, 240);
            this.groupBox2.TabIndex = 25;
            this.groupBox2.TabStop = false;
            // 
            // lblAll
            // 
            this.lblAll.AutoSize = true;
            this.lblAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblAll.Location = new System.Drawing.Point(372, 12);
            this.lblAll.Name = "lblAll";
            this.lblAll.Size = new System.Drawing.Size(18, 13);
            this.lblAll.TabIndex = 19;
            this.lblAll.Text = "All";
            this.lblAll.Click += new System.EventHandler(this.lblAll_Click);
            this.lblAll.MouseLeave += new System.EventHandler(this.lblAll_MouseLeave);
            this.lblAll.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lblAll_MouseMove);
            // 
            // lblNone
            // 
            this.lblNone.AutoSize = true;
            this.lblNone.Location = new System.Drawing.Point(399, 12);
            this.lblNone.Name = "lblNone";
            this.lblNone.Size = new System.Drawing.Size(33, 13);
            this.lblNone.TabIndex = 26;
            this.lblNone.Text = "None";
            this.lblNone.Click += new System.EventHandler(this.lblNone_Click);
            this.lblNone.MouseLeave += new System.EventHandler(this.lblNone_MouseLeave);
            this.lblNone.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lblNone_MouseMove);
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.ForeColor = System.Drawing.Color.Red;
            this.lblMessage.Location = new System.Drawing.Point(369, 253);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(0, 13);
            this.lblMessage.TabIndex = 27;
            // 
            // label4
            // 
            this.label4.ForeColor = System.Drawing.Color.Red;
            this.label4.Location = new System.Drawing.Point(349, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(10, 10);
            this.label4.TabIndex = 28;
            this.label4.Text = "*";
            // 
            // label5
            // 
            this.label5.ForeColor = System.Drawing.Color.Red;
            this.label5.Location = new System.Drawing.Point(349, 44);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(10, 10);
            this.label5.TabIndex = 29;
            this.label5.Text = "*";
            // 
            // label6
            // 
            this.label6.ForeColor = System.Drawing.Color.Red;
            this.label6.Location = new System.Drawing.Point(349, 72);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(10, 10);
            this.label6.TabIndex = 30;
            this.label6.Text = "*";
            // 
            // lblRefresh
            // 
            this.lblRefresh.AutoSize = true;
            this.lblRefresh.Location = new System.Drawing.Point(442, 12);
            this.lblRefresh.Name = "lblRefresh";
            this.lblRefresh.Size = new System.Drawing.Size(44, 13);
            this.lblRefresh.TabIndex = 32;
            this.lblRefresh.Text = "Refresh";
            this.lblRefresh.Click += new System.EventHandler(this.lblRefresh_Click);
            this.lblRefresh.MouseLeave += new System.EventHandler(this.lblRefresh_MouseLeave);
            this.lblRefresh.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lblRefresh_MouseMove);
            // 
            // RemoteRecorderFeedBackProgram
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(836, 289);
            this.Controls.Add(this.lblRefresh);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.lblNone);
            this.Controls.Add(this.lblAll);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.lstRemoteRecorders);
            this.Controls.Add(this.btnCollectLogs);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cboSelectDatabase);
            this.Name = "RemoteRecorderFeedBackProgram";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RemoteRecorderFeedBackProgram";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RemoteRecorderFeedBackProgram_FormClosed);
            this.Load += new System.EventHandler(this.RemoteRecorderFeedBackProgram_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.ComboBox cboSelectDatabase;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCompanyName;
        private System.Windows.Forms.Button btnCollectLogs;
        private System.Windows.Forms.TextBox txtVersion;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ListView lstRemoteRecorders;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblAll;
        private System.Windows.Forms.Label lblNone;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblRefresh;
    }
}