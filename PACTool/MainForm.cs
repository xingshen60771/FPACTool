using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FPACTool
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            // 修改窗口标题
            this.Text = PublicFunction.GetAPPInformation(1);
        }

        /// <summary>
        /// 打包器按钮响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Pack_Click(object sender, EventArgs e)
        {
            // 询问用户是否继续
            DialogResult result = MessageBox.Show("警告！PAC重建功能尚不完善，可能会因哈希值错乱导致游戏闪退，建议直接把改好的解包内容放在游戏根目录下，确需重新重建的务必备份好原文件。\n\n是否继续？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.No)
            {
                return;
            }
            // 打开打包器窗口
            Form_Pack packForm = new Form_Pack();
            packForm.ShowDialog();
            packForm.Dispose();
        }

        /// <summary>
        /// 解包器按钮响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Unpack_Click(object sender, EventArgs e)
        {
            // 打开解包器窗口
            Form_Unpack unpackForm = new Form_Unpack();
            unpackForm.ShowDialog();
        }
    }
}
