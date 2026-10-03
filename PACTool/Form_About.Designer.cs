using System;
using System.Configuration.Assemblies;
using System.Reflection;
using System.Runtime.InteropServices;


namespace FPACTool
{
    partial class Form_About
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
            this.label_About_title = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label_licence = new System.Windows.Forms.Label();
            this.label_changelog = new System.Windows.Forms.Label();
            this.btn_ContactMe = new System.Windows.Forms.Button();
            this.btn_OK = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label_WebsiteURL = new System.Windows.Forms.Label();
            this.img_logo = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label_ViedSource = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.img_logo)).BeginInit();
            this.SuspendLayout();
            // 
            // label_About_title
            // 
            this.label_About_title.BackColor = System.Drawing.Color.Transparent;
            this.label_About_title.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_About_title.Location = new System.Drawing.Point(252, 9);
            this.label_About_title.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_About_title.Name = "label_About_title";
            this.label_About_title.Size = new System.Drawing.Size(400, 100);
            this.label_About_title.TabIndex = 2;
            this.label_About_title.Text = "label1";
            this.label_About_title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(27, 108);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(850, 92);
            this.label3.TabIndex = 3;
            this.label3.Text = "    本软件可自由传播，不收取任何费用！转载请注明出处，并“依样”打包转载。\r\n    友情提示：如果您是通过付费途径获得的（如：某宝购买、付费积分下载、VIP" +
    "免费下载、付费论坛下载等）证明您已经上当受骗，这样得到的文件安全性无法保证，转载者很可能向此软件植入广告推广、第三方联系方式，甚至还会有病毒木马，故不承担任何责" +
    "任。为确保您的合法权益，请您到软件发布帖获取本软件。\r\n";
            // 
            // label_licence
            // 
            this.label_licence.AutoSize = true;
            this.label_licence.BackColor = System.Drawing.Color.Transparent;
            this.label_licence.Font = new System.Drawing.Font("宋体", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_licence.ForeColor = System.Drawing.Color.Blue;
            this.label_licence.Location = new System.Drawing.Point(8, 390);
            this.label_licence.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_licence.Name = "label_licence";
            this.label_licence.Size = new System.Drawing.Size(80, 17);
            this.label_licence.TabIndex = 6;
            this.label_licence.Text = "许可协议";
            this.label_licence.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label_licence.Click += new System.EventHandler(this.Label_licence_Click);
            // 
            // label_changelog
            // 
            this.label_changelog.AutoSize = true;
            this.label_changelog.BackColor = System.Drawing.Color.Transparent;
            this.label_changelog.Font = new System.Drawing.Font("宋体", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_changelog.ForeColor = System.Drawing.Color.Blue;
            this.label_changelog.Location = new System.Drawing.Point(109, 390);
            this.label_changelog.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_changelog.Name = "label_changelog";
            this.label_changelog.Size = new System.Drawing.Size(80, 17);
            this.label_changelog.TabIndex = 7;
            this.label_changelog.Text = "更新日志";
            this.label_changelog.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label_changelog.Click += new System.EventHandler(this.Label_changelog_Click);
            // 
            // btn_ContactMe
            // 
            this.btn_ContactMe.Location = new System.Drawing.Point(775, 346);
            this.btn_ContactMe.Margin = new System.Windows.Forms.Padding(4);
            this.btn_ContactMe.Name = "btn_ContactMe";
            this.btn_ContactMe.Size = new System.Drawing.Size(100, 29);
            this.btn_ContactMe.TabIndex = 9;
            this.btn_ContactMe.Text = "与我联系";
            this.btn_ContactMe.UseVisualStyleBackColor = true;
            this.btn_ContactMe.Click += new System.EventHandler(this.Btn_ContactMe_Click);
            // 
            // btn_OK
            // 
            this.btn_OK.Location = new System.Drawing.Point(775, 384);
            this.btn_OK.Margin = new System.Windows.Forms.Padding(4);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(100, 29);
            this.btn_OK.TabIndex = 10;
            this.btn_OK.Text = "确定";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.Btn_OK_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("宋体", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(209, 390);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(178, 17);
            this.label4.TabIndex = 13;
            this.label4.Text = "吾爱破解论坛欢迎您：";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label_WebsiteURL
            // 
            this.label_WebsiteURL.AutoSize = true;
            this.label_WebsiteURL.BackColor = System.Drawing.Color.Transparent;
            this.label_WebsiteURL.Font = new System.Drawing.Font("宋体", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_WebsiteURL.ForeColor = System.Drawing.Color.Blue;
            this.label_WebsiteURL.Location = new System.Drawing.Point(423, 390);
            this.label_WebsiteURL.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_WebsiteURL.Name = "label_WebsiteURL";
            this.label_WebsiteURL.Size = new System.Drawing.Size(148, 17);
            this.label_WebsiteURL.TabIndex = 14;
            this.label_WebsiteURL.Text = "www.52Pojie.cn";
            this.label_WebsiteURL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label_WebsiteURL.Click += new System.EventHandler(this.Label_WebsiteURL_Click);
            // 
            // img_logo
            // 
            this.img_logo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.img_logo.Image = global::FPACTool.Properties.Resources.img_52pojielogo;
            this.img_logo.Location = new System.Drawing.Point(647, 341);
            this.img_logo.Name = "img_logo";
            this.img_logo.Size = new System.Drawing.Size(121, 75);
            this.img_logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.img_logo.TabIndex = 16;
            this.img_logo.TabStop = false;
            this.img_logo.Click += new System.EventHandler(this.Img_logo_Click);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.Blue;
            this.label1.Location = new System.Drawing.Point(27, 210);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(850, 30);
            this.label1.TabIndex = 18;
            this.label1.Text = "本工具开源并遵守CC0-1.0许可协议，使用时请遵守相关法律法规！";
            // 
            // label_ViedSource
            // 
            this.label_ViedSource.BackColor = System.Drawing.Color.Transparent;
            this.label_ViedSource.Font = new System.Drawing.Font("宋体", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))));
            this.label_ViedSource.ForeColor = System.Drawing.Color.Purple;
            this.label_ViedSource.Location = new System.Drawing.Point(27, 245);
            this.label_ViedSource.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_ViedSource.Name = "label_ViedSource";
            this.label_ViedSource.Size = new System.Drawing.Size(121, 23);
            this.label_ViedSource.TabIndex = 19;
            this.label_ViedSource.Text = "访问源代码";
            this.label_ViedSource.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label_ViedSource.Click += new System.EventHandler(this.Label_ViedSource_Click);
            // 
            // Form_About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(906, 429);
            this.Controls.Add(this.label_ViedSource);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.img_logo);
            this.Controls.Add(this.label_WebsiteURL);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.btn_ContactMe);
            this.Controls.Add(this.label_changelog);
            this.Controls.Add(this.label_licence);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label_About_title);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_About";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "关于";
            ((System.ComponentModel.ISupportInitialize)(this.img_logo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label_About_title;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label_licence;
        private System.Windows.Forms.Label label_changelog;
        private System.Windows.Forms.Button btn_ContactMe;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label_WebsiteURL;
        private System.Windows.Forms.PictureBox img_logo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label_ViedSource;
    }
}