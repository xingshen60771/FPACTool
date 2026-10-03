using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Forms;

namespace FPACTool
{

    public partial class Form_License : Form
    {
        /// <summary>
        /// 用户是否接受了许可协议（同意且点了"开始使用"）
        /// </summary>
        public bool LicenseAccepted { get; private set; } = false;

        /// <summary>
        /// 实例化当前窗体
        /// </summary>
        public Form_License()
        {
            InitializeComponent();

            // 动态标题
            this.Text = $"《{PublicFunction.GetAPPInformation(1)}》用户许可协议";
            // 载入许可协议 RTF 文本
            richText_License.LoadFile(new MemoryStream(Properties.Resources.license), RichTextBoxStreamType.RichText);

            // "同意协议"复选框未选中时，"确定"按钮标题为"退出"，隐藏"撤销同意协议"标签
            if (check_agree.Checked == false)
            {
                btn_OK.Text = "退出";
                label_licencedisagree.Visible = false;
            }

            // 遍历已打开窗口，如果是在"关于"窗体打开的则表示无需同意
            foreach (Form frm in Application.OpenForms)
            {
                if (frm is Form_About)
                {
                    check_agree.Visible = false;
                    label_licencedisagree.Visible = true;
                    btn_OK.Text = "确定";
                }
            }
        }

        /// <summary>
        /// "同意协议"复选框选中状态改变时的响应函数
        /// </summary>
        private void Check_agree_CheckedChanged(object sender, EventArgs e)
        {
            btn_OK.Text = check_agree.Checked ? "开始使用" : "退出";
        }

        /// <summary>
        /// 点击"确定/开始使用/退出"按钮响应函数
        /// </summary>
        private void Btn_OK_Click(object sender, EventArgs e)
        {
            // 若由"关于"窗体调用（无需同意），直接关闭
            foreach (Form frm in Application.OpenForms)
            {
                if (frm is Form_About)
                {
                    this.Close();
                    return;
                }
            }

            // 检查注册表是否已有许可记录（用于判断"不同意"时的提示分支）
            const string regKeyPath = @"SOFTWARE\52pojie\FPACTool\Main";
            RegistryKey keyExist = Registry.CurrentUser.OpenSubKey(regKeyPath);

            if (check_agree.Checked)
            {
                // 写入注册表：标记已同意
                RegistryKey key = Registry.CurrentUser;
                RegistryKey software = key.CreateSubKey(regKeyPath);
                software.SetValue("License", 0);
                key.Close();

                LicenseAccepted = true;
                this.Close();  // 销毁本窗体并令 ShowDialog() 返回，Program.cs 据此显示主界面
            }
            else
            {
                // 未勾选同意
                if (keyExist == null)
                {
                    // 首次运行，用户未同意就直接点退出 → 询问确认
                    DialogResult r = MessageBox.Show(
                        "警告，你必须仔细阅读并无条件接受《许可协议》才可以继续使用。\n如果不接受《许可协议》则会导致工具退出，现在要退出工具吗？",
                        "即将退出",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    if (r == DialogResult.Yes)
                    {
                        Environment.Exit(0);
                    }
                    // 用户选 No → 留在窗体，用户可重新勾选
                }
                else
                {
                    // 注册表已有记录但复选框未勾（用户故意取消勾选），按"不同意"处理
                    DialogResult r = MessageBox.Show(
                        "撤回对本工具许可协议的同意，本工具将终止服务，不能使用。\n是否要撤回同意？",
                        "撤回许可协议同意",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    if (r == DialogResult.Yes)
                    {
                        // 删除注册表项并退出
                        RegistryKey delKey = Registry.CurrentUser;
                        delKey.DeleteSubKey(regKeyPath, true);
                        delKey.Close();
                        MessageBox.Show("已撤回同意工具许可协议！点击\"确定\"按钮终止运行。", "终止服务", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Environment.Exit(0);
                    }
                    // 用户选 No → 留在窗体
                }
            }
        }

        /// <summary>
        /// 点击"撤销同意协议"标签的响应函数（About 窗体中调用）
        /// </summary>
        private void Label_licencedisagree_Click(object sender, EventArgs e)
        {
            DialogResult q = MessageBox.Show(
                "撤回对本工具许可协议的同意，本工具将终止服务，不能使用，是否要撤回同意吗？",
                "撤回许可协议同意",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (q == DialogResult.Yes)
            {
                const string regKeyPath = @"SOFTWARE\52pojie\FPACTool\Main";
                RegistryKey key = Registry.CurrentUser;
                key.DeleteSubKey(regKeyPath, true);
                key.Close();
                MessageBox.Show("已撤回同意工具许可协议！点击\"确定\"按钮终止运行。", "终止服务", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Environment.Exit(0);
            }
        }
    }
}
