using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace FPACTool
{
    public partial class Form_Pack : Form
    {
        #region 初始化变量
        // 初始化变量
        string packDir;     // 打包目录
        string outputFile;  // 输出文件
        #endregion

        #region 创建打包文件夹对话框
        // 创建并初始化打包文件夹文件浏览对话框
        FolderBrowserDialog folderBrowserDialog_PackDirectory = new FolderBrowserDialog()
        {
            // 设置对话框描述信息
            Description = "请选择目标目录：",
            // 设置初始目录为桌面
            SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };
        #endregion

        #region 创建PAC文件保存对话框
        // 创建并初始化PAC文件保存对话框
        SaveFileDialog saveFileDialog_OutputFile = new SaveFileDialog()
        {
            Title = "请选择PAC文件保存位置：", // 设置对话框标题
            DefaultExt = "pac", // 设置默认文件扩展名
            Filter = "Sora 1st PAC资源文件 (*.pac)|*.pac|所有文件 (*.*)|*.*", // 设置文件类型过滤器
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)// 设置初始目录为桌面
        };
        #endregion

        #region 窗体初始化操作
        public Form_Pack()
        {
            InitializeComponent();
            // 修改窗口标题
            this.Text = PublicFunction.GetAPPInformation(1) + " - 重建PAC";
            // 设置状态标签和百分比标签文本
            label_State.Text = "准备就绪";
            label_Percentage.Text = "0%";
        }
        #endregion

        #region 按钮响应函数
        /// <summary>
        /// 点击“选择打包文件夹”按钮响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_PackDirectory_Click(object sender, EventArgs e)
        {
            // 设置打包路径
            if (folderBrowserDialog_PackDirectory.ShowDialog() == DialogResult.OK)
            {
                packDir = folderBrowserDialog_PackDirectory.SelectedPath;
                textBox_PackDirectory.Text = folderBrowserDialog_PackDirectory.SelectedPath;
            }
        }

        /// <summary>
        /// 点击“选择输出文件”按钮响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_OutputFile_Click(object sender, EventArgs e)
        {
            // 获取当前系统时间
            DateTime currentTime = DateTime.Now;
            // 格式化为指定的字符串格式 (yyyy=年, MM=月, dd=日, HH=时(24小时制), mm=分, ss=秒)
            string formattedTime = currentTime.ToString("yyyyMMddHHmmss");
            // 设置默认文件名为 "NewPack_yyyyMMddHHmmss.pac"
            saveFileDialog_OutputFile.FileName = "NewPack_" + formattedTime + ".pac";
            // 设置默认文件类型
            saveFileDialog_OutputFile.FilterIndex = 1;
            if (saveFileDialog_OutputFile.ShowDialog() == DialogResult.OK)
            {
                // 设置输出文件名
                outputFile = saveFileDialog_OutputFile.FileName;
                textBox_OutputFile.Text = saveFileDialog_OutputFile.FileName;
            }
        }

        /// <summary>
        /// 点击“开始打包”按钮响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Btn_Pack_Click(object sender, EventArgs e)
        {
            // 防止设置打包目录和输出文件后又改变了文本框内容
            packDir = textBox_PackDirectory.Text;
            outputFile = textBox_OutputFile.Text;

            if (string.IsNullOrEmpty(packDir))
            {
                MessageBox.Show("请先选择要目标目录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(packDir))
            {
                MessageBox.Show("选择的文件夹不存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                MessageBox.Show("请设置输出PAC文件路径！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 检查输出目录是否存在，不存在则创建
            string outputDir = Path.GetDirectoryName(outputFile);
            if (!Directory.Exists(outputDir))
            {
                try
                {
                    Directory.CreateDirectory(outputDir);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法创建输出目录：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // 检查是否会覆盖现有文件
            if (File.Exists(outputFile))
            {
                System.Media.SystemSounds.Beep.Play();
                DialogResult result = MessageBox.Show(
                    $"文件 {Path.GetFileName(outputFile)} 已存在，是否覆盖？",
                    "确认覆盖",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            // 禁用控件，开始打包
            this.SetControlsEnabled(false);

            label_State.Visible = true;
            label_Percentage.Visible = true;

            progressBar_Pack.Value = 0;
            label_State.Text = "正在重建PAC，请稍候...";

            try
            {
                // 检查列表框是否有内容，有则清空
                if (listBox_OutputLog.Items.Count > 0)
                {
                    listBox_OutputLog.Items.Clear();
                }

                listBox_OutputLog.Items.Add("- - - - - - - - - - - - -");
                listBox_OutputLog.Items.Add($"{PublicFunction.GetAPPInformation(1)}   {PublicFunction.GetAPPInformation(2)}");
                listBox_OutputLog.Items.Add("- - - - - - - - - - - - -");
                listBox_OutputLog.Items.Add("准备就绪，重建PAC开始！");
                listBox_OutputLog.Items.Add(string.Empty);
                listBox_OutputLog.Items.Add($"目标目录:{packDir}");
                listBox_OutputLog.Items.Add($"PAC文件保存路径{outputFile}");
                listBox_OutputLog.Items.Add(string.Empty);
                listBox_OutputLog.Items.Add("-------------------------");
                // 异步打包
                await FPACHelper.Pack.PackPACAsync(
                                    packDir,
                                    outputFile,
                                    OnPackProgress);

                // 打包完成
                progressBar_Pack.Value = 100;
                label_State.Text = "重建PAC完成！";

                listBox_OutputLog.Items.Add("-------------------------");
                listBox_OutputLog.Items.Add(string.Empty);
                listBox_OutputLog.Items.Add(string.Empty);
                listBox_OutputLog.Items.Add("【警告】修改过的PAC文件仅限技术研究，游戏资源文件之版权归相关公司所有，严禁将修改");
                listBox_OutputLog.Items.Add("版PAC文件用于其他用途，否则本工具作者概不承担因而造成的一切后果！");
                listBox_OutputLog.TopIndex = listBox_OutputLog.Items.Count - 1;

                // 询问是否打开输出文件夹
                System.Media.SystemSounds.Beep.Play();
                DialogResult openFolder = MessageBox.Show(
                    $"重建PAC完成！\n输出文件：{outputFile}\n是否打开输出文件所在的文件夹？",
                    "重建PAC完成",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (openFolder == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{outputFile}\"");
                }
            }
            catch (Exception ex)
            {
                // 打包失败
                label_State.Text = "重建PAC失败！";
                progressBar_Pack.Value = 0;

                MessageBox.Show(
                    $"重建PAC失败：{ex.Message}\n\n详细信息：{ex}",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                // 恢复控件状态
                SetControlsEnabled(true);
            }
        }

        /// <summary>
        /// 点击“关闭”按钮响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region PAC打包操作
        /// <summary>
        /// 设置控件启用状态
        /// </summary>
        private void SetControlsEnabled(bool enabled)
        {
            btn_PackDirectory.Enabled = enabled;
            btn_OutputFile.Enabled = enabled;
            btn_Pack.Enabled = enabled && !string.IsNullOrEmpty(textBox_PackDirectory.Text);
            textBox_PackDirectory.Enabled = enabled;
            textBox_OutputFile.Enabled = enabled;

        }

        /// <summary>
        /// 打包进度回调
        /// </summary>
        private void OnPackProgress(FPACHelper.FileProgress progress)
        {
            // 由于是在后台线程中调用，需要使用Invoke来更新UI
            if (InvokeRequired)
            {
                Invoke(new Action<FPACHelper.FileProgress>(OnPackProgress), progress);
                return;
            }

            // 更新进度条
            progressBar_Pack.Value = Math.Min(progress.percentage, 100);

            // 列表框始终在底部
            listBox_OutputLog.TopIndex = listBox_OutputLog.Items.Count - 1;

            // 更新状态文本
            label_State.Text = progress.state;
            listBox_OutputLog.Items.Add(progress.state);
            label_Percentage.Text = progress.percentage.ToString() + "%";

            // 强制刷新界面
            Application.DoEvents();
        }
        #endregion

        #region 列表框右键菜单
        /// <summary>
        /// 列表框鼠标右键点击事件响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox_OutputLog_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && listBox_OutputLog.Items.Count != 0)
            {
                contextMenuStrip.Show(listBox_OutputLog, e.Location);
            }
        }

        /// <summary>
        /// 点击“复制细节到剪切板”响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_CopyOutputLog_Click(object sender, EventArgs e)
        {
            if (listBox_OutputLog.Items.Count == 0)
            {
                MessageBox.Show("当前输出日志框是空的！", "空日志", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (var dataItem in listBox_OutputLog.Items)
            {
                sb.AppendLine(dataItem.ToString());
            }

            System.Windows.Forms.Clipboard.SetText(sb.ToString());
        }
        #endregion
    }
}
