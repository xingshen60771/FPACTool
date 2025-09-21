namespace FPACTool
{
    partial class Form_Unpack
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Unpack));
            this.progressBar_Unpack = new System.Windows.Forms.ProgressBar();
            this.label_Percentage = new System.Windows.Forms.Label();
            this.label_State = new System.Windows.Forms.Label();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.btn_Unpack = new System.Windows.Forms.Button();
            this.listBox_OutputLog = new System.Windows.Forms.ListBox();
            this.btn_PACFilePath = new System.Windows.Forms.Button();
            this.btn_UnpackDirectory = new System.Windows.Forms.Button();
            this.textBox_UnpackDirectory = new System.Windows.Forms.TextBox();
            this.textBox_PACFilePath = new System.Windows.Forms.TextBox();
            this.label_UnpackDirectory = new System.Windows.Forms.Label();
            this.label_PACFilePath = new System.Windows.Forms.Label();
            this.listView_FileList = new System.Windows.Forms.ListView();
            this.columnHeader_Num = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1_FileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_FileOffset = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_FileSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_Signature = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btn_UnpackSingle = new System.Windows.Forms.Button();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuItem_CopyOutputLog = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressBar_Unpack
            // 
            resources.ApplyResources(this.progressBar_Unpack, "progressBar_Unpack");
            this.progressBar_Unpack.Name = "progressBar_Unpack";
            // 
            // label_Percentage
            // 
            resources.ApplyResources(this.label_Percentage, "label_Percentage");
            this.label_Percentage.Name = "label_Percentage";
            // 
            // label_State
            // 
            resources.ApplyResources(this.label_State, "label_State");
            this.label_State.Name = "label_State";
            // 
            // btn_Cancel
            // 
            resources.ApplyResources(this.btn_Cancel, "btn_Cancel");
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.Btn_Cancel_Click);
            // 
            // btn_Unpack
            // 
            resources.ApplyResources(this.btn_Unpack, "btn_Unpack");
            this.btn_Unpack.Name = "btn_Unpack";
            this.btn_Unpack.UseVisualStyleBackColor = true;
            this.btn_Unpack.Click += new System.EventHandler(this.Btn_Unpack_Click);
            // 
            // listBox_OutputLog
            // 
            this.listBox_OutputLog.FormattingEnabled = true;
            resources.ApplyResources(this.listBox_OutputLog, "listBox_OutputLog");
            this.listBox_OutputLog.Name = "listBox_OutputLog";
            this.listBox_OutputLog.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listBox_OutputLog_MouseDown);
            // 
            // btn_PACFilePath
            // 
            resources.ApplyResources(this.btn_PACFilePath, "btn_PACFilePath");
            this.btn_PACFilePath.Name = "btn_PACFilePath";
            this.btn_PACFilePath.UseVisualStyleBackColor = true;
            this.btn_PACFilePath.Click += new System.EventHandler(this.Btn_PACFilePath_Click);
            // 
            // btn_UnpackDirectory
            // 
            resources.ApplyResources(this.btn_UnpackDirectory, "btn_UnpackDirectory");
            this.btn_UnpackDirectory.Name = "btn_UnpackDirectory";
            this.btn_UnpackDirectory.UseVisualStyleBackColor = true;
            this.btn_UnpackDirectory.Click += new System.EventHandler(this.Btn_UnpackDirectory_Click);
            // 
            // textBox_UnpackDirectory
            // 
            resources.ApplyResources(this.textBox_UnpackDirectory, "textBox_UnpackDirectory");
            this.textBox_UnpackDirectory.Name = "textBox_UnpackDirectory";
            // 
            // textBox_PACFilePath
            // 
            resources.ApplyResources(this.textBox_PACFilePath, "textBox_PACFilePath");
            this.textBox_PACFilePath.Name = "textBox_PACFilePath";
            // 
            // label_UnpackDirectory
            // 
            resources.ApplyResources(this.label_UnpackDirectory, "label_UnpackDirectory");
            this.label_UnpackDirectory.Name = "label_UnpackDirectory";
            // 
            // label_PACFilePath
            // 
            resources.ApplyResources(this.label_PACFilePath, "label_PACFilePath");
            this.label_PACFilePath.Name = "label_PACFilePath";
            // 
            // listView_FileList
            // 
            this.listView_FileList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_Num,
            this.columnHeader1_FileName,
            this.columnHeader_FileOffset,
            this.columnHeader_FileSize,
            this.columnHeader_Signature});
            this.listView_FileList.FullRowSelect = true;
            this.listView_FileList.GridLines = true;
            this.listView_FileList.HideSelection = false;
            resources.ApplyResources(this.listView_FileList, "listView_FileList");
            this.listView_FileList.MultiSelect = false;
            this.listView_FileList.Name = "listView_FileList";
            this.listView_FileList.UseCompatibleStateImageBehavior = false;
            this.listView_FileList.View = System.Windows.Forms.View.Details;
            this.listView_FileList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListView_FileList_MouseDoubleClick);
            // 
            // columnHeader_Num
            // 
            resources.ApplyResources(this.columnHeader_Num, "columnHeader_Num");
            // 
            // columnHeader1_FileName
            // 
            resources.ApplyResources(this.columnHeader1_FileName, "columnHeader1_FileName");
            // 
            // columnHeader_FileOffset
            // 
            resources.ApplyResources(this.columnHeader_FileOffset, "columnHeader_FileOffset");
            // 
            // columnHeader_FileSize
            // 
            resources.ApplyResources(this.columnHeader_FileSize, "columnHeader_FileSize");
            // 
            // columnHeader_Signature
            // 
            resources.ApplyResources(this.columnHeader_Signature, "columnHeader_Signature");
            // 
            // btn_UnpackSingle
            // 
            resources.ApplyResources(this.btn_UnpackSingle, "btn_UnpackSingle");
            this.btn_UnpackSingle.Name = "btn_UnpackSingle";
            this.btn_UnpackSingle.UseVisualStyleBackColor = true;
            this.btn_UnpackSingle.Click += new System.EventHandler(this.Btn_UnpackSingle_Click);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_CopyOutputLog});
            this.contextMenuStrip.Name = "contextMenuStrip";
            resources.ApplyResources(this.contextMenuStrip, "contextMenuStrip");
            // 
            // MenuItem_CopyOutputLog
            // 
            this.MenuItem_CopyOutputLog.Name = "MenuItem_CopyOutputLog";
            resources.ApplyResources(this.MenuItem_CopyOutputLog, "MenuItem_CopyOutputLog");
            this.MenuItem_CopyOutputLog.Click += new System.EventHandler(this.MenuItem_OutputLog_Click);
            // 
            // Form_Unpack
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btn_UnpackSingle);
            this.Controls.Add(this.listView_FileList);
            this.Controls.Add(this.progressBar_Unpack);
            this.Controls.Add(this.label_Percentage);
            this.Controls.Add(this.label_State);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_Unpack);
            this.Controls.Add(this.listBox_OutputLog);
            this.Controls.Add(this.btn_PACFilePath);
            this.Controls.Add(this.btn_UnpackDirectory);
            this.Controls.Add(this.textBox_UnpackDirectory);
            this.Controls.Add(this.textBox_PACFilePath);
            this.Controls.Add(this.label_UnpackDirectory);
            this.Controls.Add(this.label_PACFilePath);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form_Unpack";
            this.ShowIcon = false;
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form_Unpack_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form_Unpack_DragEnter);
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar_Unpack;
        private System.Windows.Forms.Label label_Percentage;
        private System.Windows.Forms.Label label_State;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_Unpack;
        private System.Windows.Forms.ListBox listBox_OutputLog;
        private System.Windows.Forms.Button btn_PACFilePath;
        private System.Windows.Forms.Button btn_UnpackDirectory;
        private System.Windows.Forms.TextBox textBox_UnpackDirectory;
        private System.Windows.Forms.TextBox textBox_PACFilePath;
        private System.Windows.Forms.Label label_UnpackDirectory;
        private System.Windows.Forms.Label label_PACFilePath;
        private System.Windows.Forms.ListView listView_FileList;
        private System.Windows.Forms.ColumnHeader columnHeader_Num;
        private System.Windows.Forms.ColumnHeader columnHeader1_FileName;
        private System.Windows.Forms.ColumnHeader columnHeader_FileOffset;
        private System.Windows.Forms.ColumnHeader columnHeader_FileSize;
        private System.Windows.Forms.ColumnHeader columnHeader_Signature;
        private System.Windows.Forms.Button btn_UnpackSingle;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_CopyOutputLog;
    }
}