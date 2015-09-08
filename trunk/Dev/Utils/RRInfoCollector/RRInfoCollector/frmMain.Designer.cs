namespace RRInfoCollector
{
    partial class frmMain
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
            this.treeRecorders = new System.Windows.Forms.TreeView();
            this.cmbSystems = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblFieldCount = new System.Windows.Forms.Label();
            this.lstAssignedFields = new System.Windows.Forms.ListView();
            this.lblCtrl = new System.Windows.Forms.Label();
            this.lblRecorder = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tabEditors = new System.Windows.Forms.TabControl();
            this.tabRREditor = new System.Windows.Forms.TabPage();
            this.tabPropertyEditor = new System.Windows.Forms.TabPage();
            this.label10 = new System.Windows.Forms.Label();
            this.btnSaveQuery = new System.Windows.Forms.Button();
            this.btnClearQuery = new System.Windows.Forms.Button();
            this.txtTableName = new System.Windows.Forms.TextBox();
            this.txtTableDesc = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lstQueryFields = new System.Windows.Forms.ListView();
            this.lblSystem = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.treeProperties = new System.Windows.Forms.TreeView();
            this.tabEditors.SuspendLayout();
            this.tabRREditor.SuspendLayout();
            this.tabPropertyEditor.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeRecorders
            // 
            this.treeRecorders.Dock = System.Windows.Forms.DockStyle.Left;
            this.treeRecorders.Location = new System.Drawing.Point(3, 3);
            this.treeRecorders.Name = "treeRecorders";
            this.treeRecorders.Size = new System.Drawing.Size(287, 593);
            this.treeRecorders.TabIndex = 0;
            this.treeRecorders.DoubleClick += new System.EventHandler(this.treeRecorders_DoubleClick);
            // 
            // cmbSystems
            // 
            this.cmbSystems.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSystems.FormattingEnabled = true;
            this.cmbSystems.Location = new System.Drawing.Point(310, 71);
            this.cmbSystems.Name = "cmbSystems";
            this.cmbSystems.Size = new System.Drawing.Size(240, 21);
            this.cmbSystems.TabIndex = 1;
            this.cmbSystems.SelectedIndexChanged += new System.EventHandler(this.cmbSystems_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label1.Location = new System.Drawing.Point(312, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Recorder Sistem Adı";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label3.Location = new System.Drawing.Point(312, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Atanmış Alanlar";
            // 
            // btnSave
            // 
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(580, 416);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(113, 23);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Kaydet (Ctrl+K)";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnClear
            // 
            this.btnClear.Enabled = false;
            this.btnClear.Location = new System.Drawing.Point(433, 416);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(119, 23);
            this.btnClear.TabIndex = 4;
            this.btnClear.Text = "Temizle (Ctrl+T)";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(580, 97);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(302, 304);
            this.txtDescription.TabIndex = 3;
            this.txtDescription.TextChanged += new System.EventHandler(this.txtDescription_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label2.Location = new System.Drawing.Point(580, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Açıklama Yaz";
            // 
            // lblFieldCount
            // 
            this.lblFieldCount.AutoSize = true;
            this.lblFieldCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblFieldCount.Location = new System.Drawing.Point(412, 55);
            this.lblFieldCount.Name = "lblFieldCount";
            this.lblFieldCount.Size = new System.Drawing.Size(69, 13);
            this.lblFieldCount.TabIndex = 13;
            this.lblFieldCount.Text = "Henüz Yok";
            // 
            // lstAssignedFields
            // 
            this.lstAssignedFields.AutoArrange = false;
            this.lstAssignedFields.FullRowSelect = true;
            this.lstAssignedFields.Location = new System.Drawing.Point(312, 97);
            this.lstAssignedFields.Name = "lstAssignedFields";
            this.lstAssignedFields.Size = new System.Drawing.Size(240, 304);
            this.lstAssignedFields.TabIndex = 2;
            this.lstAssignedFields.UseCompatibleStateImageBehavior = false;
            this.lstAssignedFields.View = System.Windows.Forms.View.List;
            this.lstAssignedFields.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lstAssignedFields_ItemSelectionChanged);
            this.lstAssignedFields.DoubleClick += new System.EventHandler(this.lstAssignedFields_DoubleClick);
            // 
            // lblCtrl
            // 
            this.lblCtrl.AutoSize = true;
            this.lblCtrl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblCtrl.Location = new System.Drawing.Point(309, 404);
            this.lblCtrl.Name = "lblCtrl";
            this.lblCtrl.Size = new System.Drawing.Size(38, 13);
            this.lblCtrl.TabIndex = 15;
            this.lblCtrl.Text = "Press";
            // 
            // lblRecorder
            // 
            this.lblRecorder.AutoSize = true;
            this.lblRecorder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblRecorder.Location = new System.Drawing.Point(312, 27);
            this.lblRecorder.Name = "lblRecorder";
            this.lblRecorder.Size = new System.Drawing.Size(72, 13);
            this.lblRecorder.TabIndex = 16;
            this.lblRecorder.Text = "lblRecorder";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label4.Location = new System.Drawing.Point(581, 45);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(299, 47);
            this.label4.TabIndex = 17;
            this.label4.Text = "Lütfen recordera ait örnek kayıt veya farklı durumlar varsa herbir durum için kay" +
    "ıtları ve bu kayıtların atanmış alanlara nasıl MAP edileceğini yazın";
            // 
            // tabEditors
            // 
            this.tabEditors.Controls.Add(this.tabRREditor);
            this.tabEditors.Controls.Add(this.tabPropertyEditor);
            this.tabEditors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabEditors.Location = new System.Drawing.Point(0, 0);
            this.tabEditors.Name = "tabEditors";
            this.tabEditors.SelectedIndex = 0;
            this.tabEditors.Size = new System.Drawing.Size(896, 625);
            this.tabEditors.TabIndex = 18;
            // 
            // tabRREditor
            // 
            this.tabRREditor.Controls.Add(this.treeRecorders);
            this.tabRREditor.Controls.Add(this.label4);
            this.tabRREditor.Controls.Add(this.txtDescription);
            this.tabRREditor.Controls.Add(this.lblRecorder);
            this.tabRREditor.Controls.Add(this.cmbSystems);
            this.tabRREditor.Controls.Add(this.lblCtrl);
            this.tabRREditor.Controls.Add(this.label1);
            this.tabRREditor.Controls.Add(this.lstAssignedFields);
            this.tabRREditor.Controls.Add(this.label3);
            this.tabRREditor.Controls.Add(this.lblFieldCount);
            this.tabRREditor.Controls.Add(this.btnSave);
            this.tabRREditor.Controls.Add(this.label2);
            this.tabRREditor.Controls.Add(this.btnClear);
            this.tabRREditor.Location = new System.Drawing.Point(4, 22);
            this.tabRREditor.Name = "tabRREditor";
            this.tabRREditor.Padding = new System.Windows.Forms.Padding(3);
            this.tabRREditor.Size = new System.Drawing.Size(888, 599);
            this.tabRREditor.TabIndex = 0;
            this.tabRREditor.Text = "Remote Recorder Bilgileri";
            this.tabRREditor.UseVisualStyleBackColor = true;
            this.tabRREditor.Click += new System.EventHandler(this.tabRREditor_Click);
            // 
            // tabPropertyEditor
            // 
            this.tabPropertyEditor.Controls.Add(this.label10);
            this.tabPropertyEditor.Controls.Add(this.btnSaveQuery);
            this.tabPropertyEditor.Controls.Add(this.btnClearQuery);
            this.tabPropertyEditor.Controls.Add(this.txtTableName);
            this.tabPropertyEditor.Controls.Add(this.txtTableDesc);
            this.tabPropertyEditor.Controls.Add(this.label8);
            this.tabPropertyEditor.Controls.Add(this.label7);
            this.tabPropertyEditor.Controls.Add(this.lstQueryFields);
            this.tabPropertyEditor.Controls.Add(this.lblSystem);
            this.tabPropertyEditor.Controls.Add(this.label6);
            this.tabPropertyEditor.Controls.Add(this.treeProperties);
            this.tabPropertyEditor.Location = new System.Drawing.Point(4, 22);
            this.tabPropertyEditor.Name = "tabPropertyEditor";
            this.tabPropertyEditor.Padding = new System.Windows.Forms.Padding(3);
            this.tabPropertyEditor.Size = new System.Drawing.Size(888, 599);
            this.tabPropertyEditor.TabIndex = 1;
            this.tabPropertyEditor.Text = "Properties2";
            this.tabPropertyEditor.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label10.Location = new System.Drawing.Point(312, 104);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(135, 13);
            this.label10.TabIndex = 30;
            this.label10.Text = "Default Analysis Query";
            // 
            // btnSaveQuery
            // 
            this.btnSaveQuery.Enabled = false;
            this.btnSaveQuery.Location = new System.Drawing.Point(590, 568);
            this.btnSaveQuery.Name = "btnSaveQuery";
            this.btnSaveQuery.Size = new System.Drawing.Size(109, 23);
            this.btnSaveQuery.TabIndex = 28;
            this.btnSaveQuery.Text = "Kaydet (Ctrl+K)";
            this.btnSaveQuery.UseVisualStyleBackColor = true;
            this.btnSaveQuery.Click += new System.EventHandler(this.btnSaveQuery_Click);
            // 
            // btnClearQuery
            // 
            this.btnClearQuery.Enabled = false;
            this.btnClearQuery.Location = new System.Drawing.Point(459, 568);
            this.btnClearQuery.Name = "btnClearQuery";
            this.btnClearQuery.Size = new System.Drawing.Size(109, 23);
            this.btnClearQuery.TabIndex = 27;
            this.btnClearQuery.Text = "Temizle (Ctrl+T)";
            this.btnClearQuery.UseVisualStyleBackColor = true;
            this.btnClearQuery.Click += new System.EventHandler(this.btnClearQuery_Click);
            // 
            // txtTableName
            // 
            this.txtTableName.Location = new System.Drawing.Point(590, 71);
            this.txtTableName.Name = "txtTableName";
            this.txtTableName.Size = new System.Drawing.Size(240, 20);
            this.txtTableName.TabIndex = 24;
            this.txtTableName.TextChanged += new System.EventHandler(this.txtTableName_TextChanged);
            // 
            // txtTableDesc
            // 
            this.txtTableDesc.Location = new System.Drawing.Point(315, 71);
            this.txtTableDesc.Name = "txtTableDesc";
            this.txtTableDesc.Size = new System.Drawing.Size(240, 20);
            this.txtTableDesc.TabIndex = 23;
            this.txtTableDesc.TextChanged += new System.EventHandler(this.txtTableDesc_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label8.Location = new System.Drawing.Point(587, 55);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(61, 13);
            this.label8.TabIndex = 22;
            this.label8.Text = "Tablo Adı";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label7.Location = new System.Drawing.Point(312, 55);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(80, 13);
            this.label7.TabIndex = 21;
            this.label7.Text = "Tablo Tanımı";
            // 
            // lstQueryFields
            // 
            this.lstQueryFields.AutoArrange = false;
            this.lstQueryFields.FullRowSelect = true;
            this.lstQueryFields.Location = new System.Drawing.Point(315, 130);
            this.lstQueryFields.Name = "lstQueryFields";
            this.lstQueryFields.Size = new System.Drawing.Size(515, 432);
            this.lstQueryFields.TabIndex = 20;
            this.lstQueryFields.UseCompatibleStateImageBehavior = false;
            this.lstQueryFields.View = System.Windows.Forms.View.List;
            this.lstQueryFields.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstQueryFields_MouseDoubleClick);
            // 
            // lblSystem
            // 
            this.lblSystem.AutoSize = true;
            this.lblSystem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblSystem.Location = new System.Drawing.Point(312, 22);
            this.lblSystem.Name = "lblSystem";
            this.lblSystem.Size = new System.Drawing.Size(60, 13);
            this.lblSystem.TabIndex = 18;
            this.lblSystem.Text = "lblSystem";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label6.Location = new System.Drawing.Point(312, 3);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(122, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Recorder Sistem Adı";
            // 
            // treeProperties
            // 
            this.treeProperties.Dock = System.Windows.Forms.DockStyle.Left;
            this.treeProperties.Location = new System.Drawing.Point(3, 3);
            this.treeProperties.Name = "treeProperties";
            this.treeProperties.Size = new System.Drawing.Size(287, 593);
            this.treeProperties.TabIndex = 1;
            this.treeProperties.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.treeProperties_MouseDoubleClick);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(896, 625);
            this.Controls.Add(this.tabEditors);
            this.KeyPreview = true;
            this.Name = "frmMain";
            this.Text = "Remote Recorder Alan Eşleştirme";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmMain_KeyDown);
            this.tabEditors.ResumeLayout(false);
            this.tabRREditor.ResumeLayout(false);
            this.tabRREditor.PerformLayout();
            this.tabPropertyEditor.ResumeLayout(false);
            this.tabPropertyEditor.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeRecorders;
        private System.Windows.Forms.ComboBox cmbSystems;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblFieldCount;
        private System.Windows.Forms.ListView lstAssignedFields;
        private System.Windows.Forms.Label lblCtrl;
        private System.Windows.Forms.Label lblRecorder;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabControl tabEditors;
        private System.Windows.Forms.TabPage tabRREditor;
        private System.Windows.Forms.TabPage tabPropertyEditor;
        private System.Windows.Forms.TreeView treeProperties;
        private System.Windows.Forms.Label lblSystem;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtTableName;
        private System.Windows.Forms.TextBox txtTableDesc;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ListView lstQueryFields;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btnSaveQuery;
        private System.Windows.Forms.Button btnClearQuery;
    }
}