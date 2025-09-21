namespace FPACTool
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.btn_Pack = new System.Windows.Forms.Button();
            this.btn_Unpack = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btn_Pack
            // 
            this.btn_Pack.Font = new System.Drawing.Font("宋体", 24F);
            this.btn_Pack.Location = new System.Drawing.Point(30, 44);
            this.btn_Pack.Name = "btn_Pack";
            this.btn_Pack.Size = new System.Drawing.Size(150, 84);
            this.btn_Pack.TabIndex = 0;
            this.btn_Pack.Text = "重建";
            this.btn_Pack.UseVisualStyleBackColor = true;
            this.btn_Pack.Click += new System.EventHandler(this.Btn_Pack_Click);
            // 
            // btn_Unpack
            // 
            this.btn_Unpack.Font = new System.Drawing.Font("宋体", 24F);
            this.btn_Unpack.Location = new System.Drawing.Point(202, 44);
            this.btn_Unpack.Name = "btn_Unpack";
            this.btn_Unpack.Size = new System.Drawing.Size(150, 84);
            this.btn_Unpack.TabIndex = 1;
            this.btn_Unpack.Text = "提取";
            this.btn_Unpack.UseVisualStyleBackColor = true;
            this.btn_Unpack.Click += new System.EventHandler(this.Btn_Unpack_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 9F);
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "对PAC如何操作？";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 153);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_Unpack);
            this.Controls.Add(this.btn_Pack);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_Pack;
        private System.Windows.Forms.Button btn_Unpack;
        private System.Windows.Forms.Label label1;
    }
}

