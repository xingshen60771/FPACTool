namespace FPACTool
{
    partial class Form_License
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
            this.richText_License = new System.Windows.Forms.RichTextBox();
            this.btn_OK = new System.Windows.Forms.Button();
            this.check_agree = new System.Windows.Forms.CheckBox();
            this.label_licencedisagree = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // richText_License
            // 
            this.richText_License.BackColor = System.Drawing.Color.White;
            this.richText_License.Location = new System.Drawing.Point(12, 12);
            this.richText_License.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.richText_License.Name = "richText_License";
            this.richText_License.ReadOnly = true;
            this.richText_License.Size = new System.Drawing.Size(751, 450);
            this.richText_License.TabIndex = 0;
            this.richText_License.Text = "";
            // 
            // btn_OK
            // 
            this.btn_OK.Location = new System.Drawing.Point(668, 511);
            this.btn_OK.Margin = new System.Windows.Forms.Padding(4);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(100, 29);
            this.btn_OK.TabIndex = 1;
            this.btn_OK.Text = "button1";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.Btn_OK_Click);
            // 
            // check_agree
            // 
            this.check_agree.AutoSize = true;
            this.check_agree.Location = new System.Drawing.Point(36, 515);
            this.check_agree.Margin = new System.Windows.Forms.Padding(4);
            this.check_agree.Name = "check_agree";
            this.check_agree.Size = new System.Drawing.Size(314, 19);
            this.check_agree.TabIndex = 2;
            this.check_agree.Text = "我已经仔细阅读《许可协议》并无条件接受";
            this.check_agree.UseVisualStyleBackColor = true;
            this.check_agree.CheckedChanged += new System.EventHandler(this.Check_agree_CheckedChanged);
            // 
            // label_licencedisagree
            // 
            this.label_licencedisagree.BackColor = System.Drawing.Color.Transparent;
            this.label_licencedisagree.Font = new System.Drawing.Font("宋体", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_licencedisagree.ForeColor = System.Drawing.Color.Red;
            this.label_licencedisagree.Location = new System.Drawing.Point(489, 515);
            this.label_licencedisagree.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_licencedisagree.Name = "label_licencedisagree";
            this.label_licencedisagree.Size = new System.Drawing.Size(167, 19);
            this.label_licencedisagree.TabIndex = 9;
            this.label_licencedisagree.Text = "撤销同意协议";
            this.label_licencedisagree.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_licencedisagree.Click += new System.EventHandler(this.Label_licencedisagree_Click);
            // 
            // Form_License
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(781, 552);
            this.Controls.Add(this.label_licencedisagree);
            this.Controls.Add(this.check_agree);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.richText_License);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_License";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form_License";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richText_License;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.CheckBox check_agree;
        private System.Windows.Forms.Label label_licencedisagree;
    }
}