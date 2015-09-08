namespace WindowsFormsApplication1
{
    partial class RecorderProjectsCsprojUpdater
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
            this.LstListBox = new System.Windows.Forms.ListBox();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.btnCompile = new System.Windows.Forms.Button();
            this.chkRelease = new System.Windows.Forms.CheckBox();
            this.txtFileExtension = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // LstListBox
            // 
            this.LstListBox.FormattingEnabled = true;
            this.LstListBox.Location = new System.Drawing.Point(4, 58);
            this.LstListBox.Name = "LstListBox";
            this.LstListBox.Size = new System.Drawing.Size(869, 420);
            this.LstListBox.TabIndex = 0;
            this.LstListBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LstListBoxMouseDoubleClick);
            // 
            // txtFilePath
            // 
            this.txtFilePath.Location = new System.Drawing.Point(113, 4);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(679, 20);
            this.txtFilePath.TabIndex = 2;
            this.txtFilePath.Text = "M:\\Recorders2011SonVersiyon\\trunk\\Specific Implementations";
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.Location = new System.Drawing.Point(798, 3);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(75, 23);
            this.btnOpenFolder.TabIndex = 3;
            this.btnOpenFolder.Text = "Open Folder";
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Click += new System.EventHandler(this.BtnOpenFolderClick);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(798, 484);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 4;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStartClick);
            // 
            // btnCompile
            // 
            this.btnCompile.Location = new System.Drawing.Point(4, 484);
            this.btnCompile.Name = "btnCompile";
            this.btnCompile.Size = new System.Drawing.Size(103, 23);
            this.btnCompile.TabIndex = 5;
            this.btnCompile.Text = "CompileAllProjects";
            this.btnCompile.UseVisualStyleBackColor = true;
            this.btnCompile.Click += new System.EventHandler(this.BtnCompileClick);
            // 
            // chkRelease
            // 
            this.chkRelease.AutoSize = true;
            this.chkRelease.Location = new System.Drawing.Point(113, 487);
            this.chkRelease.Name = "chkRelease";
            this.chkRelease.Size = new System.Drawing.Size(65, 17);
            this.chkRelease.TabIndex = 6;
            this.chkRelease.Text = "Release";
            this.chkRelease.UseVisualStyleBackColor = true;
            this.chkRelease.CheckedChanged += new System.EventHandler(this.chkRelease_CheckedChanged);
            // 
            // txtFileExtension
            // 
            this.txtFileExtension.Location = new System.Drawing.Point(113, 30);
            this.txtFileExtension.Name = "txtFileExtension";
            this.txtFileExtension.Size = new System.Drawing.Size(679, 20);
            this.txtFileExtension.TabIndex = 7;
            this.txtFileExtension.Text = "*.cs";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Project File Directory";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "File Extension";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(689, 483);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(103, 24);
            this.button1.TabIndex = 10;
            this.button1.Text = "Find LastRecDate";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 510);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "fsdfsdf";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button4);
            this.panel1.Controls.Add(this.button3);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.txtFileExtension);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.chkRelease);
            this.panel1.Controls.Add(this.btnCompile);
            this.panel1.Controls.Add(this.btnStart);
            this.panel1.Controls.Add(this.btnOpenFolder);
            this.panel1.Controls.Add(this.txtFilePath);
            this.panel1.Controls.Add(this.LstListBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(879, 531);
            this.panel1.TabIndex = 12;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(566, 483);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(117, 24);
            this.button2.TabIndex = 12;
            this.button2.Text = "Find Delete Function";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(443, 484);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(117, 24);
            this.button3.TabIndex = 13;
            this.button3.Text = "Find After Function";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(320, 484);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(117, 24);
            this.button4.TabIndex = 14;
            this.button4.Text = "Find Before Function";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // RecorderProjectsCsprojUpdater
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(879, 531);
            this.Controls.Add(this.panel1);
            this.Name = "RecorderProjectsCsprojUpdater";
            this.Text = "Updade and Compile Recorder Project App";
            this.Load += new System.EventHandler(this.Form1Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox LstListBox;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button btnCompile;
        private System.Windows.Forms.CheckBox chkRelease;
        private System.Windows.Forms.TextBox txtFileExtension;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
    }
}

