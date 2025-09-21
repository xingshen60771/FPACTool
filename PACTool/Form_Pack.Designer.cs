namespace FPACTool
{
    partial class Form_Pack
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
            this.components = new System.ComponentModel.Container();
            this.label_PackDirectory = new System.Windows.Forms.Label();
            this.label_OutputFile = new System.Windows.Forms.Label();
            this.textBox_PackDirectory = new System.Windows.Forms.TextBox();
            this.textBox_OutputFile = new System.Windows.Forms.TextBox();
            this.btn_OutputFile = new System.Windows.Forms.Button();
            this.btn_PackDirectory = new System.Windows.Forms.Button();
            this.listBox_OutputLog = new System.Windows.Forms.ListBox();
            this.btn_Pack = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.label_State = new System.Windows.Forms.Label();
            this.label_Percentage = new System.Windows.Forms.Label();
            this.progressBar_Pack = new System.Windows.Forms.ProgressBar();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuItem_CopyOutputLog = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // label_PackDirectory
            // 
            this.label_PackDirectory.AutoSize = true;
            this.label_PackDirectory.Location = new System.Drawing.Point(25, 25);
            this.label_PackDirectory.Name = "label_PackDirectory";
            this.label_PackDirectory.Size = new System.Drawing.Size(67, 15);
            this.label_PackDirectory.TabIndex = 0;
            this.label_PackDirectory.Text = "目标目录";
            // 
            // label_OutputFile
            // 
            this.label_OutputFile.AutoSize = true;
            this.label_OutputFile.Location = new System.Drawing.Point(25, 63);
            this.label_OutputFile.Name = "label_OutputFile";
            this.label_OutputFile.Size = new System.Drawing.Size(67, 15);
            this.label_OutputFile.TabIndex = 1;
            this.label_OutputFile.Text = "输出路径";
            // 
            // textBox_PackDirectory
            // 
            this.textBox_PackDirectory.Location = new System.Drawing.Point(103, 20);
            this.textBox_PackDirectory.Name = "textBox_PackDirectory";
            this.textBox_PackDirectory.Size = new System.Drawing.Size(514, 25);
            this.textBox_PackDirectory.TabIndex = 2;
            // 
            // textBox_OutputFile
            // 
            this.textBox_OutputFile.Location = new System.Drawing.Point(103, 58);
            this.textBox_OutputFile.Name = "textBox_OutputFile";
            this.textBox_OutputFile.Size = new System.Drawing.Size(514, 25);
            this.textBox_OutputFile.TabIndex = 3;
            // 
            // btn_OutputFile
            // 
            this.btn_OutputFile.Location = new System.Drawing.Point(636, 55);
            this.btn_OutputFile.Name = "btn_OutputFile";
            this.btn_OutputFile.Size = new System.Drawing.Size(120, 30);
            this.btn_OutputFile.TabIndex = 4;
            this.btn_OutputFile.Text = "输出到(&O)…";
            this.btn_OutputFile.UseVisualStyleBackColor = true;
            this.btn_OutputFile.Click += new System.EventHandler(this.Btn_OutputFile_Click);
            // 
            // btn_PackDirectory
            // 
            this.btn_PackDirectory.Location = new System.Drawing.Point(636, 17);
            this.btn_PackDirectory.Name = "btn_PackDirectory";
            this.btn_PackDirectory.Size = new System.Drawing.Size(120, 30);
            this.btn_PackDirectory.TabIndex = 5;
            this.btn_PackDirectory.Text = "浏览(&B)…";
            this.btn_PackDirectory.UseVisualStyleBackColor = true;
            this.btn_PackDirectory.Click += new System.EventHandler(this.Btn_PackDirectory_Click);
            // 
            // listBox_OutputLog
            // 
            this.listBox_OutputLog.FormattingEnabled = true;
            this.listBox_OutputLog.ItemHeight = 15;
            this.listBox_OutputLog.Location = new System.Drawing.Point(27, 180);
            this.listBox_OutputLog.Name = "listBox_OutputLog";
            this.listBox_OutputLog.Size = new System.Drawing.Size(730, 169);
            this.listBox_OutputLog.TabIndex = 6;
            this.listBox_OutputLog.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listBox_OutputLog_MouseDown);
            // 
            // btn_Pack
            // 
            this.btn_Pack.Location = new System.Drawing.Point(650, 361);
            this.btn_Pack.Name = "btn_Pack";
            this.btn_Pack.Size = new System.Drawing.Size(120, 30);
            this.btn_Pack.TabIndex = 7;
            this.btn_Pack.Text = "重建PAC(&P)";
            this.btn_Pack.UseVisualStyleBackColor = true;
            this.btn_Pack.Click += new System.EventHandler(this.Btn_Pack_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Location = new System.Drawing.Point(510, 361);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(120, 30);
            this.btn_Cancel.TabIndex = 8;
            this.btn_Cancel.Text = "关闭(&X)";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.Btn_Cancel_Click);
            // 
            // label_State
            // 
            this.label_State.AutoSize = true;
            this.label_State.Location = new System.Drawing.Point(25, 105);
            this.label_State.Name = "label_State";
            this.label_State.Size = new System.Drawing.Size(55, 15);
            this.label_State.TabIndex = 9;
            this.label_State.Text = "label1";
            // 
            // label_Percentage
            // 
            this.label_Percentage.AutoSize = true;
            this.label_Percentage.Location = new System.Drawing.Point(702, 136);
            this.label_Percentage.Name = "label_Percentage";
            this.label_Percentage.Size = new System.Drawing.Size(55, 15);
            this.label_Percentage.TabIndex = 10;
            this.label_Percentage.Text = "label2";
            // 
            // progressBar_Pack
            // 
            this.progressBar_Pack.Location = new System.Drawing.Point(27, 132);
            this.progressBar_Pack.Name = "progressBar_Pack";
            this.progressBar_Pack.Size = new System.Drawing.Size(669, 23);
            this.progressBar_Pack.TabIndex = 11;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_CopyOutputLog});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(219, 28);
            // 
            // MenuItem_CopyOutputLog
            // 
            this.MenuItem_CopyOutputLog.Name = "MenuItem_CopyOutputLog";
            this.MenuItem_CopyOutputLog.Size = new System.Drawing.Size(218, 24);
            this.MenuItem_CopyOutputLog.Text = "复制细节至剪切板(&C)";
            this.MenuItem_CopyOutputLog.Click += new System.EventHandler(this.MenuItem_CopyOutputLog_Click);
            // 
            // Form_Pack
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(782, 403);
            this.Controls.Add(this.progressBar_Pack);
            this.Controls.Add(this.label_Percentage);
            this.Controls.Add(this.label_State);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_Pack);
            this.Controls.Add(this.listBox_OutputLog);
            this.Controls.Add(this.btn_PackDirectory);
            this.Controls.Add(this.btn_OutputFile);
            this.Controls.Add(this.textBox_OutputFile);
            this.Controls.Add(this.textBox_PackDirectory);
            this.Controls.Add(this.label_OutputFile);
            this.Controls.Add(this.label_PackDirectory);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form_Pack";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form_Pack";
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_PackDirectory;
        private System.Windows.Forms.Label label_OutputFile;
        private System.Windows.Forms.TextBox textBox_PackDirectory;
        private System.Windows.Forms.TextBox textBox_OutputFile;
        private System.Windows.Forms.Button btn_OutputFile;
        private System.Windows.Forms.Button btn_PackDirectory;
        private System.Windows.Forms.ListBox listBox_OutputLog;
        private System.Windows.Forms.Button btn_Pack;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Label label_State;
        private System.Windows.Forms.Label label_Percentage;
        private System.Windows.Forms.ProgressBar progressBar_Pack;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_CopyOutputLog;
    }
}