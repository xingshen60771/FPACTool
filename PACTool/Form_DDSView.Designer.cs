namespace FPACTool
{
    partial class Form_DDSView
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
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_BackColor = new System.Windows.Forms.ComboBox();
            this.btn_Properties = new System.Windows.Forms.Button();
            this.btn_SaveAs = new System.Windows.Forms.Button();
            this.btn_ZoomOut = new System.Windows.Forms.Button();
            this.btn_ZoomIn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer.IsSplitterFixed = true;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.AutoScroll = true;
            this.splitContainer.Panel1.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.splitContainer.Panel1.Controls.Add(this.pictureBox);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.label1);
            this.splitContainer.Panel2.Controls.Add(this.comboBox_BackColor);
            this.splitContainer.Panel2.Controls.Add(this.btn_Properties);
            this.splitContainer.Panel2.Controls.Add(this.btn_SaveAs);
            this.splitContainer.Panel2.Controls.Add(this.btn_ZoomOut);
            this.splitContainer.Panel2.Controls.Add(this.btn_ZoomIn);
            this.splitContainer.Size = new System.Drawing.Size(1043, 691);
            this.splitContainer.SplitterDistance = 610;
            this.splitContainer.SplitterWidth = 2;
            this.splitContainer.TabIndex = 0;
            // 
            // pictureBox
            // 
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox.Location = new System.Drawing.Point(3, 2);
            this.pictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(99, 50);
            this.pictureBox.TabIndex = 1;
            this.pictureBox.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(756, 31);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "背景颜色";
            // 
            // comboBox_BackColor
            // 
            this.comboBox_BackColor.FormattingEnabled = true;
            this.comboBox_BackColor.Location = new System.Drawing.Point(834, 27);
            this.comboBox_BackColor.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_BackColor.Name = "comboBox_BackColor";
            this.comboBox_BackColor.Size = new System.Drawing.Size(160, 23);
            this.comboBox_BackColor.TabIndex = 4;
            // 
            // btn_Properties
            // 
            this.btn_Properties.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Properties.Font = new System.Drawing.Font("宋体", 12F);
            this.btn_Properties.Image = global::FPACTool.Properties.Resources.img_ToolStrip_PACProperties;
            this.btn_Properties.Location = new System.Drawing.Point(194, 14);
            this.btn_Properties.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_Properties.Name = "btn_Properties";
            this.btn_Properties.Size = new System.Drawing.Size(50, 50);
            this.btn_Properties.TabIndex = 3;
            this.btn_Properties.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btn_Properties.UseVisualStyleBackColor = false;
            this.btn_Properties.Click += new System.EventHandler(this.btn_Properties_Click);
            // 
            // btn_SaveAs
            // 
            this.btn_SaveAs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_SaveAs.Font = new System.Drawing.Font("宋体", 12F);
            this.btn_SaveAs.Image = global::FPACTool.Properties.Resources.img_DDSView_SaveAs;
            this.btn_SaveAs.Location = new System.Drawing.Point(136, 14);
            this.btn_SaveAs.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_SaveAs.Name = "btn_SaveAs";
            this.btn_SaveAs.Size = new System.Drawing.Size(50, 50);
            this.btn_SaveAs.TabIndex = 2;
            this.btn_SaveAs.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btn_SaveAs.UseVisualStyleBackColor = false;
            this.btn_SaveAs.Click += new System.EventHandler(this.btn_SaveAs_Click);
            // 
            // btn_ZoomOut
            // 
            this.btn_ZoomOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_ZoomOut.Font = new System.Drawing.Font("宋体", 12F);
            this.btn_ZoomOut.Image = global::FPACTool.Properties.Resources.img_DDSView_ZoomOut;
            this.btn_ZoomOut.Location = new System.Drawing.Point(78, 14);
            this.btn_ZoomOut.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_ZoomOut.Name = "btn_ZoomOut";
            this.btn_ZoomOut.Size = new System.Drawing.Size(50, 50);
            this.btn_ZoomOut.TabIndex = 1;
            this.btn_ZoomOut.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btn_ZoomOut.UseVisualStyleBackColor = false;
            this.btn_ZoomOut.Click += new System.EventHandler(this.btn_ZoomOut_Click);
            // 
            // btn_ZoomIn
            // 
            this.btn_ZoomIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_ZoomIn.Font = new System.Drawing.Font("宋体", 12F);
            this.btn_ZoomIn.Image = global::FPACTool.Properties.Resources.img_DDSView_ZoomIn;
            this.btn_ZoomIn.Location = new System.Drawing.Point(20, 14);
            this.btn_ZoomIn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_ZoomIn.Name = "btn_ZoomIn";
            this.btn_ZoomIn.Size = new System.Drawing.Size(50, 50);
            this.btn_ZoomIn.TabIndex = 0;
            this.btn_ZoomIn.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btn_ZoomIn.UseVisualStyleBackColor = false;
            this.btn_ZoomIn.Click += new System.EventHandler(this.btn_ZoomIn_Click);
            // 
            // Form_DDSView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1043, 691);
            this.Controls.Add(this.splitContainer);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Form_DDSView";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form_DDSView";
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Button btn_Properties;
        private System.Windows.Forms.Button btn_SaveAs;
        private System.Windows.Forms.Button btn_ZoomOut;
        private System.Windows.Forms.Button btn_ZoomIn;
        private System.Windows.Forms.ComboBox comboBox_BackColor;
        private System.Windows.Forms.Label label1;
    }
}