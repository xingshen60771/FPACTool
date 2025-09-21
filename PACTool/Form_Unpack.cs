using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace FPACTool
{
    public partial class Form_Unpack : Form
    {
        #region 初始化变量
        // 初始化变量
        string pacFile;     // PAC文件路径
        string unpackDir;   // 解包目录
        List<Tuple<byte[], string, long, long>> currentPACList;// 现行PAC文件列表
        #endregion

        #region 创建PAC文件打开对话框
        // 创建并初始化PAC文件打开对话框
        OpenFileDialog saveFileDialog_PACFile = new OpenFileDialog()
        {
            Title = "请选择要提取的PAC文件：", // 设置对话框标题
            DefaultExt = "pac", // 设置默认文件扩展名
            Filter = "Sora 1st PAC资源文件 (*.pac)|*.pac|所有文件 (*.*)|*.*", // 设置文件类型过滤器
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)// 设置初始目录为桌面
        };
        #endregion

        #region 创建解包单个文件保存对话框
        // 创建并初始化创建解包单个文件保存对话框
        SaveFileDialog saveFileDialog_UnpackSingle = new SaveFileDialog()
        {
            Title = "请选择文件保存位置：", // 设置对话框标题
            Filter = "所有文件 (*.*)|*.*", // 设置文件类型过滤器
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)// 设置初始目录为桌面
        };
        #endregion

        #region 创建解包文件夹对话框
        // 创建并初始化解包文件夹文件浏览对话框
        FolderBrowserDialog folderBrowserDialog_UnpackDirectory = new FolderBrowserDialog()
        {
            // 设置对话框描述信息
            Description = "请选择要提取的文件夹",
            // 设置初始目录为桌面
            SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };
        #endregion

        #region 窗体初始化操作
        public Form_Unpack()
        {
            InitializeComponent();
            // 修改窗口标题
            this.Text = PublicFunction.GetAPPInformation(1) + " - 提取PAC";
            // 设置状态标签和百分比标签文本
            label_State.Text = "请选择要提取的PAC文件(支持拖放到窗口)";
            label_Percentage.Text = "0%";
        }
        #endregion

        #region 载入PAC列表
        /// <summary>
        /// 载入PAC文件列表
        /// <param name="pacList">(列表&lt;四元组&gt; 欲获取PAC内部文件列表)</param>
        /// </summary>
        private void ShowPACList(List<Tuple<byte[], string, long, long>> pacList)
        {
            // 载入前调用BeginUpdate以提升性能
            listView_FileList.BeginUpdate();

            // 清除现有项（如果不需要保留原有数据）
            listView_FileList.Items.Clear();

            // 取PAC文件数
            int fileCount = pacList.Count;
            // 载入PAC文件列表到列表框
            for (int i = 0; i < fileCount; i++)
            {
                // 显示签名值
                string Signature = LittleEndianToHexString(pacList[i].Item1);
                // 显示文件名
                string FileName = pacList[i].Item2;
                // 显示文件偏移
                string FileOffset = "0x" + pacList[i].Item3.ToString("X8");
                // 显示文件大小
                string FileSize = PublicFunction.BytesToSize(pacList[i].Item4);

                // 载入到列表框
                ListViewItem newItem = new ListViewItem((i + 1).ToString());
                newItem.SubItems.Add(FileName);
                newItem.SubItems.Add(FileOffset);
                newItem.SubItems.Add(FileSize);
                newItem.SubItems.Add(Signature);
                listView_FileList.Items.Add(newItem);
            }
            // 添加完成，刷新控件显示
            listView_FileList.EndUpdate();
        }
        #endregion

        #region PAC载入函数
        /// <summary>
        /// 载入PAC文件
        /// </summary>
        /// <param name="pacFilePath">(文本型 欲载入的PAC文件)</param>
        private void LoadPACFile(string pacFilePath)
        {
            // 清空日志框、文件框
            listBox_OutputLog.Items.Clear();
            listView_FileList.Items.Clear();
            try
            {
                // 获取PAC文件列表
                listBox_OutputLog.Items.Add($"正在载入 \"{pacFilePath}\"");
                currentPACList = FPACHelper.UnPack.GetPACInformation(pacFilePath);
                ShowPACList(currentPACList);

                // 修改窗口标题
                this.Text = $"{PublicFunction.GetAPPInformation(1)} - 提取PAC(当前文件:{System.IO.Path.GetFileName(pacFilePath)})";

                // 载入成功，更新状态标签
                label_State.Text = $"支持的格式，可以提取！";

                // 标签字体加粗并改为绿色
                label_State.Font = new System.Drawing.Font(label_State.Font, System.Drawing.FontStyle.Bold);
                label_State.ForeColor = System.Drawing.Color.FromArgb(76, 175, 80);

                // 播放提示音
                System.Media.SystemSounds.Beep.Play();

                // 更新listbox
                listBox_OutputLog.Items.Add(label_State.Text + $"共 {currentPACList.Count} 个文件");

                // 设置默认解包目录为"文件名+_Unapcked"
                unpackDir = System.IO.Path.Combine(Path.GetDirectoryName(pacFilePath), $"{Path.GetFileNameWithoutExtension(pacFilePath)}_Unpacked");
                textBox_UnpackDirectory.Text = unpackDir;
            }
            catch (Exception ex)
            {
                // 载入失败，更新状态标签
                label_State.Text = $"发生错误，无法提取";
                // 标签字体加粗并改为红色
                label_State.Font = new System.Drawing.Font(label_State.Font, System.Drawing.FontStyle.Bold);
                label_State.ForeColor = System.Drawing.Color.Red;
                // 修改窗口标题
                this.Text = $"{PublicFunction.GetAPPInformation(1)} - 提取PAC(未能载入PAC文件)";

                // 播放提示音
                System.Media.SystemSounds.Hand.Play();
                listBox_OutputLog.Items.Add(label_State.Text);
                listBox_OutputLog.Items.Add($"错误原因: {ex.Message}");

                // 清空解包目录文本
                unpackDir = string.Empty;
                textBox_UnpackDirectory.Text = unpackDir;
            }
        }

        /// <summary>
        /// 将小端存储的字节数组转换为高位在前显示的十六进制字符串。
        /// </summary>
        /// <param name="bytes">小端存储的字节数组</param>
        /// <returns>高位在前的十六进制字符串</returns>
        private string LittleEndianToHexString(byte[] bytes)
        {
            // 参数验证
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0)
                return string.Empty;

            // 创建字节数组的副本以避免修改原始数组
            byte[] reversedBytes = (byte[])bytes.Clone();

            // 反转字节顺序：小端 -> 大端表示
            Array.Reverse(reversedBytes);

            // 将每个字节转换为两位十六进制格式并拼接
            StringBuilder hexString = new StringBuilder();
            foreach (byte b in reversedBytes)
            {
                hexString.Append(b.ToString("X2"));
            }

            return hexString.ToString();
        }
        #endregion

        #region 按钮响应函数
        /// <summary>
        /// 点击“选择PAC文件”按钮响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_PACFilePath_Click(object sender, EventArgs e)
        {
            // 显示文件打开对话框
            if (saveFileDialog_PACFile.ShowDialog() == DialogResult.OK)
            {
                // 获取选择的文件路径
                pacFile = saveFileDialog_PACFile.FileName;
                textBox_PACFilePath.Text = saveFileDialog_PACFile.FileName;
                LoadPACFile(pacFile);
            }
        }

        /// <summary>
        /// 点击“选择解包目录”按钮响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_UnpackDirectory_Click(object sender, EventArgs e)
        {
            // 打开解包文件夹对话框
            if (folderBrowserDialog_UnpackDirectory.ShowDialog() == DialogResult.OK)
            {
                textBox_UnpackDirectory.Text = folderBrowserDialog_UnpackDirectory.SelectedPath;
            }
        }

        /// <summary>
        /// 点击"开始解包"按钮响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Btn_Unpack_Click(object sender, EventArgs e)
        {
            // 防止设置解包目录后又改变了文本框内容
            unpackDir = textBox_UnpackDirectory.Text;

            // 验证输入
            if (string.IsNullOrEmpty(pacFile))
            {
                MessageBox.Show("请先选择要提取的PAC文件！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(pacFile))
            {
                MessageBox.Show("选择的PAC文件不存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(unpackDir))
            {
                MessageBox.Show("请设置提取目录路径！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (currentPACList == null || currentPACList.Count == 0)
            {
                MessageBox.Show("当前没有可提取的文件列表！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 检查解包目录是否存在，不存在则创建
            if (!Directory.Exists(unpackDir))
            {
                try
                {
                    Directory.CreateDirectory(unpackDir);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法创建提取目录：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // 检查解包目录是否为空，不为空则询问是否继续
            if (Directory.GetFiles(unpackDir).Length > 0 || Directory.GetDirectories(unpackDir).Length > 0)
            {
                System.Media.SystemSounds.Beep.Play();
                DialogResult result = MessageBox.Show(
                    $"提取目录 {Path.GetFileName(unpackDir)} 不为空，提取可能会覆盖现有文件，是否继续？",
                    "确认提取",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            // 禁用控件，开始解包
            this.SetControlsEnabled(false);

            label_State.Visible = true;
            label_Percentage.Visible = true;

            progressBar_Unpack.Value = 0;
            label_State.Text = "正在提取，请稍候...";
            
            // label_State字体恢复原状
            label_State.Font = new System.Drawing.Font(label_State.Font, System.Drawing.FontStyle.Regular);
            label_State.ForeColor = System.Drawing.SystemColors.ControlText;

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
                listBox_OutputLog.Items.Add("准备就绪，提取开始！");
                listBox_OutputLog.Items.Add(string.Empty);
                listBox_OutputLog.Items.Add($"PAC文件:{pacFile}");
                listBox_OutputLog.Items.Add($"提取目录:{unpackDir}");
                listBox_OutputLog.Items.Add($"文件总数:{currentPACList.Count}");
                listBox_OutputLog.Items.Add(string.Empty);
                listBox_OutputLog.Items.Add("-------------------------");


                // 异步解包
                await FPACHelper.UnPack.UnpackAllFileAsync(
                    pacFile,
                    currentPACList,
                    unpackDir,
                    OnUnpackProgress);

                // 解包完成
                progressBar_Unpack.Value = 100;
                label_State.Text = "提取完成！";

                listBox_OutputLog.Items.Add("-------------------------");
                listBox_OutputLog.Items.Add(string.Empty);
                listBox_OutputLog.Items.Add(string.Empty);
                listBox_OutputLog.Items.Add("【警告】提取出的文件仅限技术研究，游戏资源文件之版权归相关公司所有，严禁将提取");
                listBox_OutputLog.Items.Add("文件用于其他用途，否则本工具作者概不承担因而造成的一切后果！");
                listBox_OutputLog.TopIndex = listBox_OutputLog.Items.Count - 1;

                // label_State字体蓝色、加粗
                label_State.Font = new System.Drawing.Font(label_State.Font, System.Drawing.FontStyle.Bold);
                label_State.ForeColor = System.Drawing.Color.FromArgb(76, 175, 80);

                // 询问是否打开解包文件夹
                System.Media.SystemSounds.Beep.Play();
                DialogResult openFolder = MessageBox.Show(
                    $"提取完成！\n提取目录：{unpackDir}\n是否打开提取文件所在的文件夹？",
                    "提取完成",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (openFolder == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"\"{unpackDir}\"");
                }
            }
            catch (Exception ex)
            {
                // 解包失败
                label_State.Text = "提取失败！";
                progressBar_Unpack.Value = 0;

                listBox_OutputLog.Items.Add(string.Empty);
                listBox_OutputLog.Items.Add(string.Empty);
                listBox_OutputLog.Items.Add("-------------------------");
                listBox_OutputLog.Items.Add(label_State.Text);
                listBox_OutputLog.Items.Add($"失败原因: {ex.Message}");

                // label_State字体红色、加粗
                label_State.Font = new System.Drawing.Font(label_State.Font, System.Drawing.FontStyle.Bold);
                label_State.ForeColor = System.Drawing.Color.FromArgb(255, 0, 0);

                MessageBox.Show(
                    $"提取失败：{ex.Message}\n\n详细信息：{ex}",
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
        /// 点击"单文件解包"按钮响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_UnpackSingle_Click(object sender, EventArgs e)
        {
            // 检查listView是否有选中项
            if (listView_FileList.SelectedItems.Count == 0)
            {
                MessageBox.Show("请先选择要单独提取的文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 获取选中的文件索引（假设只允许单选）
            int selectedIndex = listView_FileList.SelectedItems[0].Index;
            // 获取选中文件名称，只获取文件名
            string selectedFileName = System.IO.Path.GetFileName(listView_FileList.SelectedItems[0].SubItems[1].Text);

            // 设置保存对话框的默认文件名为选中文件名
            saveFileDialog_UnpackSingle.FileName = selectedFileName;
            // 显示保存对话框
            // 显示保存对话框
            if (saveFileDialog_UnpackSingle.ShowDialog() == DialogResult.OK)
            {
                // 获取保存路径
                string savePath = saveFileDialog_UnpackSingle.FileName;

                try
                {
                    // 执行单文件解包操作
                    FPACHelper.UnPack.UnpackSingle(pacFile, currentPACList, selectedIndex, savePath);

                    // 询问是否打开输出文件夹
                    System.Media.SystemSounds.Beep.Play();
                    DialogResult openFolder = MessageBox.Show(
                        $"文件 '{selectedFileName}' 已成功提取到:\n{Path.GetDirectoryName(savePath)}\n\n是否打开提取出来的文件所在的文件夹？",
                        "提取完成",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (openFolder == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{savePath}\"");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"提取文件时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 点击"关闭"按钮响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region PAC解包操作
        /// <summary>
        /// 设置控件启用状态
        /// <param name="enabled">(逻辑型 是否启用控件)</param>
        /// </summary>
        private void SetControlsEnabled(bool enabled)
        {
            btn_PACFilePath.Enabled = enabled;
            btn_UnpackDirectory.Enabled = enabled;
            btn_Unpack.Enabled = enabled && !string.IsNullOrEmpty(textBox_PACFilePath.Text) && currentPACList != null && currentPACList.Count > 0;
            btn_UnpackSingle.Enabled = enabled;
            textBox_PACFilePath.Enabled = enabled;
            textBox_UnpackDirectory.Enabled = enabled;
        }

        /// <summary>
        /// 解包进度回调
        /// <param name="progress">(文件回调 解包进度信息)</param>
        /// </summary>
        private void OnUnpackProgress(FPACHelper.FileProgress progress)
        {
            // 由于是在后台线程中调用，需要使用Invoke来更新UI
            if (InvokeRequired)
            {
                Invoke(new Action<FPACHelper.FileProgress>(OnUnpackProgress), progress);
                return;
            }

            // 更新进度条
            progressBar_Unpack.Value = Math.Min(progress.percentage, 100);

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
        /// 点击"复制细节到剪切板"响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_OutputLog_Click(object sender, EventArgs e)
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

        #region 拖拽支持
        /// <summary>
        /// 拖拽文件到窗体的处理
        /// </summary>
        private void Form_Unpack_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        /// <summary>
        /// 拖拽放下的处理
        /// </summary>
        private void Form_Unpack_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                string path = files[0];
                if (File.Exists(path) && Path.GetExtension(path).ToLower() == ".pac")
                {
                    pacFile = path;
                    textBox_PACFilePath.Text = path;
                    LoadPACFile(pacFile);
                }
                else
                {
                    MessageBox.Show("请拖拽PAC文件！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        #endregion

        #region ListView事件
        /// <summary>
        /// ListView选择项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_FileList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 更新单文件解包按钮状态
            btn_UnpackSingle.Enabled = listView_FileList.SelectedItems.Count > 0;
        }
              
        /// <summary>
        /// ListView双击事件响应函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_FileList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // 如果进度条百分比不等于0或者不等于100，则触发解包按钮点击事件
            if (progressBar_Unpack.Value != 0 && progressBar_Unpack.Value != 100)
            {
                return;
            }
            else
            {
                Btn_UnpackSingle_Click(sender, e);
            }
        }
        #endregion
    }
}