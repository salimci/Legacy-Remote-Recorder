namespace NatekLogService
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.btnAddFilter = new System.Windows.Forms.Button();
            this.txtBoxFunctionName = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.lblColName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtBoxDescription = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cmBoxActionType = new System.Windows.Forms.ComboBox();
            this.cmBoxFilterName = new System.Windows.Forms.ComboBox();
            this.btnDeleteFilter = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label10 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.btnUpdateRuntime = new System.Windows.Forms.Button();
            this.numUpDownHour = new System.Windows.Forms.NumericUpDown();
            this.btnGetTableValues = new System.Windows.Forms.Button();
            this.numUpDownPeriod = new System.Windows.Forms.NumericUpDown();
            this.txtBoxTblName = new System.Windows.Forms.TextBox();
            this.numUpDownMinute = new System.Windows.Forms.NumericUpDown();
            this.lblTarget = new System.Windows.Forms.Label();
            this.txtBoxTarget = new System.Windows.Forms.TextBox();
            this.btnUpdateFilter = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numUpDownHour)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUpDownPeriod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUpDownMinute)).BeginInit();
            this.SuspendLayout();
            // 
            // btnAddFilter
            // 
            this.btnAddFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnAddFilter.Location = new System.Drawing.Point(117, 219);
            this.btnAddFilter.Name = "btnAddFilter";
            this.btnAddFilter.Size = new System.Drawing.Size(75, 23);
            this.btnAddFilter.TabIndex = 12;
            this.btnAddFilter.Text = "Add";
            this.btnAddFilter.UseVisualStyleBackColor = true;
            this.btnAddFilter.Click += new System.EventHandler(this.btnAddFilter_Click);
            // 
            // txtBoxFunctionName
            // 
            this.txtBoxFunctionName.Location = new System.Drawing.Point(189, 93);
            this.txtBoxFunctionName.Name = "txtBoxFunctionName";
            this.txtBoxFunctionName.Size = new System.Drawing.Size(166, 20);
            this.txtBoxFunctionName.TabIndex = 8;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.lblColName);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(371, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(360, 461);
            this.panel1.TabIndex = 2;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label8.Location = new System.Drawing.Point(150, 18);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(105, 17);
            this.label8.TabIndex = 31;
            this.label8.Text = "Constant Name";
            // 
            // lblColName
            // 
            this.lblColName.AutoSize = true;
            this.lblColName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblColName.Location = new System.Drawing.Point(20, 18);
            this.lblColName.Name = "lblColName";
            this.lblColName.Size = new System.Drawing.Size(96, 17);
            this.lblColName.TabIndex = 30;
            this.lblColName.Text = "Column Name";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label1.Location = new System.Drawing.Point(12, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Filter Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label2.Location = new System.Drawing.Point(12, 97);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Function Name";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label3.Location = new System.Drawing.Point(27, 353);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 17);
            this.label3.TabIndex = 7;
            this.label3.Text = "Run Time";
            this.label3.Visible = false;
            // 
            // txtBoxDescription
            // 
            this.txtBoxDescription.Location = new System.Drawing.Point(190, 121);
            this.txtBoxDescription.Multiline = true;
            this.txtBoxDescription.Name = "txtBoxDescription";
            this.txtBoxDescription.Size = new System.Drawing.Size(165, 92);
            this.txtBoxDescription.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label4.Location = new System.Drawing.Point(12, 121);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Description";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label5.Location = new System.Drawing.Point(198, 393);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 17);
            this.label5.TabIndex = 11;
            this.label5.Text = "Action Type";
            this.label5.Visible = false;
            // 
            // cmBoxActionType
            // 
            this.cmBoxActionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmBoxActionType.FormattingEnabled = true;
            this.cmBoxActionType.Location = new System.Drawing.Point(218, 413);
            this.cmBoxActionType.MaxDropDownItems = 30;
            this.cmBoxActionType.Name = "cmBoxActionType";
            this.cmBoxActionType.Size = new System.Drawing.Size(18, 21);
            this.cmBoxActionType.TabIndex = 9;
            this.cmBoxActionType.Visible = false;
            // 
            // cmBoxFilterName
            // 
            this.cmBoxFilterName.FormattingEnabled = true;
            this.cmBoxFilterName.Location = new System.Drawing.Point(189, 66);
            this.cmBoxFilterName.Name = "cmBoxFilterName";
            this.cmBoxFilterName.Size = new System.Drawing.Size(165, 21);
            this.cmBoxFilterName.TabIndex = 7;
            this.cmBoxFilterName.SelectedIndexChanged += new System.EventHandler(this.cmBoxFilterName_SelectedIndexChanged);
            // 
            // btnDeleteFilter
            // 
            this.btnDeleteFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnDeleteFilter.Location = new System.Drawing.Point(198, 219);
            this.btnDeleteFilter.Name = "btnDeleteFilter";
            this.btnDeleteFilter.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnDeleteFilter.Size = new System.Drawing.Size(75, 23);
            this.btnDeleteFilter.TabIndex = 13;
            this.btnDeleteFilter.Text = "Delete Filter";
            this.btnDeleteFilter.UseVisualStyleBackColor = true;
            this.btnDeleteFilter.Click += new System.EventHandler(this.btnDeleteFilter_Click);
            // 
            // panel2
            // 
            this.panel2.AutoScroll = true;
            this.panel2.Controls.Add(this.label11);
            this.panel2.Controls.Add(this.label10);
            this.panel2.Controls.Add(this.label7);
            this.panel2.Controls.Add(this.label9);
            this.panel2.Controls.Add(this.btnUpdateRuntime);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.numUpDownHour);
            this.panel2.Controls.Add(this.btnGetTableValues);
            this.panel2.Controls.Add(this.numUpDownPeriod);
            this.panel2.Controls.Add(this.txtBoxTblName);
            this.panel2.Controls.Add(this.numUpDownMinute);
            this.panel2.Controls.Add(this.lblTarget);
            this.panel2.Controls.Add(this.txtBoxTarget);
            this.panel2.Controls.Add(this.btnUpdateFilter);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.btnDeleteFilter);
            this.panel2.Controls.Add(this.btnAddFilter);
            this.panel2.Controls.Add(this.cmBoxFilterName);
            this.panel2.Controls.Add(this.txtBoxFunctionName);
            this.panel2.Controls.Add(this.cmBoxActionType);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.txtBoxDescription);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(365, 461);
            this.panel2.TabIndex = 19;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label10.Location = new System.Drawing.Point(105, 372);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(37, 13);
            this.label10.TabIndex = 31;
            this.label10.Text = "Period";
            this.label10.Visible = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label7.Location = new System.Drawing.Point(28, 372);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(30, 13);
            this.label7.TabIndex = 30;
            this.label7.Text = "Hour";
            this.label7.Visible = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label9.Location = new System.Drawing.Point(60, 372);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(39, 13);
            this.label9.TabIndex = 29;
            this.label9.Text = "Minute";
            this.label9.Visible = false;
            // 
            // btnUpdateRuntime
            // 
            this.btnUpdateRuntime.Location = new System.Drawing.Point(71, 398);
            this.btnUpdateRuntime.Name = "btnUpdateRuntime";
            this.btnUpdateRuntime.Size = new System.Drawing.Size(116, 10);
            this.btnUpdateRuntime.TabIndex = 6;
            this.btnUpdateRuntime.Text = "Update Runtime";
            this.btnUpdateRuntime.UseVisualStyleBackColor = true;
            this.btnUpdateRuntime.Visible = false;
            this.btnUpdateRuntime.Click += new System.EventHandler(this.btnUpdateRuntime_Click);
            // 
            // numUpDownHour
            // 
            this.numUpDownHour.Location = new System.Drawing.Point(31, 388);
            this.numUpDownHour.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
            this.numUpDownHour.Name = "numUpDownHour";
            this.numUpDownHour.Size = new System.Drawing.Size(10, 20);
            this.numUpDownHour.TabIndex = 3;
            this.numUpDownHour.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numUpDownHour.Visible = false;
            // 
            // btnGetTableValues
            // 
            this.btnGetTableValues.Location = new System.Drawing.Point(189, 36);
            this.btnGetTableValues.Name = "btnGetTableValues";
            this.btnGetTableValues.Size = new System.Drawing.Size(166, 23);
            this.btnGetTableValues.TabIndex = 2;
            this.btnGetTableValues.Text = "Get Columns and Filter Name";
            this.btnGetTableValues.UseVisualStyleBackColor = true;
            this.btnGetTableValues.Click += new System.EventHandler(this.btnGetTableValues_Click);
            // 
            // numUpDownPeriod
            // 
            this.numUpDownPeriod.Increment = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.numUpDownPeriod.Location = new System.Drawing.Point(47, 388);
            this.numUpDownPeriod.Maximum = new decimal(new int[] {
            1440,
            0,
            0,
            0});
            this.numUpDownPeriod.Name = "numUpDownPeriod";
            this.numUpDownPeriod.Size = new System.Drawing.Size(10, 20);
            this.numUpDownPeriod.TabIndex = 5;
            this.numUpDownPeriod.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numUpDownPeriod.Visible = false;
            // 
            // txtBoxTblName
            // 
            this.txtBoxTblName.Location = new System.Drawing.Point(12, 38);
            this.txtBoxTblName.Name = "txtBoxTblName";
            this.txtBoxTblName.Size = new System.Drawing.Size(160, 20);
            this.txtBoxTblName.TabIndex = 1;
            this.txtBoxTblName.Text = "RECORD_HMBS";
            // 
            // numUpDownMinute
            // 
            this.numUpDownMinute.Location = new System.Drawing.Point(63, 388);
            this.numUpDownMinute.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.numUpDownMinute.Name = "numUpDownMinute";
            this.numUpDownMinute.Size = new System.Drawing.Size(10, 20);
            this.numUpDownMinute.TabIndex = 4;
            this.numUpDownMinute.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numUpDownMinute.Visible = false;
            // 
            // lblTarget
            // 
            this.lblTarget.AutoSize = true;
            this.lblTarget.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblTarget.Location = new System.Drawing.Point(206, 353);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(50, 17);
            this.lblTarget.TabIndex = 23;
            this.lblTarget.Text = "Target";
            this.lblTarget.Visible = false;
            // 
            // txtBoxTarget
            // 
            this.txtBoxTarget.Location = new System.Drawing.Point(209, 373);
            this.txtBoxTarget.Name = "txtBoxTarget";
            this.txtBoxTarget.Size = new System.Drawing.Size(27, 20);
            this.txtBoxTarget.TabIndex = 10;
            this.txtBoxTarget.Visible = false;
            // 
            // btnUpdateFilter
            // 
            this.btnUpdateFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnUpdateFilter.Location = new System.Drawing.Point(279, 219);
            this.btnUpdateFilter.Name = "btnUpdateFilter";
            this.btnUpdateFilter.Size = new System.Drawing.Size(75, 23);
            this.btnUpdateFilter.TabIndex = 14;
            this.btnUpdateFilter.Text = "Update Filter";
            this.btnUpdateFilter.UseVisualStyleBackColor = true;
            this.btnUpdateFilter.Click += new System.EventHandler(this.btnUpdateFilter_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(12, 20);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(65, 13);
            this.label11.TabIndex = 32;
            this.label11.Text = "Table Name";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(731, 461);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MaximumSize = new System.Drawing.Size(737, 489);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Natek HMBS Log Alert";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numUpDownHour)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUpDownPeriod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUpDownMinute)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnAddFilter;
        private System.Windows.Forms.TextBox txtBoxFunctionName;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtBoxDescription;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmBoxActionType;
        private System.Windows.Forms.ComboBox cmBoxFilterName;
        private System.Windows.Forms.Button btnDeleteFilter;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.NumericUpDown numUpDownHour;
        private System.Windows.Forms.Button btnUpdateFilter;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.TextBox txtBoxTarget;
        private System.Windows.Forms.NumericUpDown numUpDownMinute;
        private System.Windows.Forms.NumericUpDown numUpDownPeriod;
        private System.Windows.Forms.Button btnGetTableValues;
        private System.Windows.Forms.TextBox txtBoxTblName;
        private System.Windows.Forms.Button btnUpdateRuntime;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblColName;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label11;
    }
}

