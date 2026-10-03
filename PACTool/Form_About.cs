using System;
using System.Windows.Forms;

namespace FPACTool
{
    public partial class Form_About : Form
    {

        /// <summary>
        /// 实例化当前窗体
        /// </summary>
        public Form_About()
        {
            InitializeComponent();
            //写入软件标题、版本信息、作者
            label_About_title.Text = PublicFunction.GetAPPInformation(1) + "\r\n" + PublicFunction.GetAPPInformation(2) + "\r\n\r\nBy：" + PublicFunction.GetAPPInformation(3);
        }

        /// <summary>
        /// 点击左侧软件图标的响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IMG_icon_Click(object sender, EventArgs e)
        {
            MessageBox.Show("本logo为作者亲自手绘的图！不存在侵权行为，未经本人允许，任何人也不可以拿这张图进行商业盈利活动！", "这张图不侵权！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// 点击“许可协议”标签响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Label_licence_Click(object sender, EventArgs e)
        {
            Form_License Form_License = new Form_License();
            Form_License.ShowDialog();
        }

        /// <summary>
        /// 点击“更新日志”响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Label_changelog_Click(object sender, EventArgs e)
        {
            Form_ChangeLog Form_ChangeLog = new Form_ChangeLog();
            Form_ChangeLog.ShowDialog();
        }

        /// <summary>
        /// 点击“吾爱破解网址”响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Label_WebsiteURL_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://" + label_WebsiteURL.Text + "/?fromuid=368698");
        }

        /// <summary>
        /// 点击“访问源代码”标签的响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Label_ViedSource_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/xingshen60771/FPACTool");
        }

        /// <summary>
        /// 点击“吾爱破解Logo”响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Img_logo_Click(object sender, EventArgs e)
        {
            //执行点击“吾爱破解网址”响应函数
            Label_WebsiteURL_Click(sender, e);
        }

        /// <summary>
        /// 点击“与我联系”按钮响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_ContactMe_Click(object sender, EventArgs e)
        {
            //询问是否打开作者主页
            DialogResult contactmeOK = MessageBox.Show("联系我需要登录吾爱破解论坛账号，然后给我发站内信。\r\n发送站内信的时候请遵守论坛版规，禁止留QQ、微信等联系方式，对利用私信留联系方式的行为将从重处罚！", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (contactmeOK == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start("https://www.52pojie.cn/home.php?mod=space&uid=368698");
            }
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
