using System;
using System.Drawing;
using System.Windows.Forms;

namespace FPACTool
{
    public partial class Form_ChangeLog : Form
    {
        /// <summary>
        /// 实例化当前窗体
        /// </summary>
        public Form_ChangeLog()
        {
            InitializeComponent();
            //设置更新日志文本框大小，并从resource中载入文本
            text_ChangeLog.Size = new System.Drawing.Size(this.Width - 30, this.Height - 80);
            text_ChangeLog.Text = Properties.Resources.text_changelog;
            text_ChangeLog.BackColor = Color.FromArgb(199, 237, 204);
        }

        /// <summary>
        /// 点击“确定”按钮响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_OK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
