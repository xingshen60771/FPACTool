using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FPACTool
{
    public partial class MainForm : Form
    {

        #region 字段与状态

        // 窗体标题
        string appTitle = $"{PublicFunction.GetAPPInformation(1)}  {PublicFunction.GetAPPInformation(2).Substring(4, 3)}";

        // PAC 运行状态
        string currentPACFile;          // 现行 PAC 文件完整路径
        bool areUnpackMode = false;     // 是否为解包模式
        bool arePackMode = false;       // 是否为打包模式
        bool packRunning = false;       // 是否正在打包中

        // PAC 数据集合
        List<Tuple<byte[], string, long, long>> pacPackList;

        // 待打包条目：Item1 为源文件完整路径，Item2 为 PAC 内部完整相对路径。
        List<Tuple<string, string>> packListSum;

        List<Tuple<byte[], string, long, long>> pacUnPackList;

        // 文件浏览状态与图标缓存
        string currentDirPath = "";
        ImageList pacImageList;
        Dictionary<string, int> fileIconIndex = new Dictionary<string, int>();

        // PAC 浏览列表采用分批异步载入。每发起一次新的目录载入就递增版本号，
        // 旧任务完成后若版本已变化会自动放弃更新，避免快速切换目录/关闭 PAC 时覆盖新界面。
        int pacListLoadVersion = 0;
        const int PacListLoadBatchSize = 250;

        #endregion

        #region 类型定义

        /// <summary>
        /// 日志类型枚举
        /// </summary>
        public enum LogType
        {
            Info,    // 信息
            Normal,  // 正常
            Warning, // 警告
            Error    // 错误
        }

        /// <summary>
        /// PAC 浏览列表的轻量行数据。后台线程只准备这些普通数据，
        /// 真正的 WinForms ListViewItem 始终在 UI 线程创建。
        /// </summary>
        private sealed class PacBrowserRow
        {
            public string Text;
            public string Offset;
            public string Size;
            public string Signature;
            public string Tag;
            public string Name;
            public int IconKind; // 0=文件夹, 1=上级目录, 2=普通文件
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 实例化当前窗体
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            // 置窗口图标
            this.Icon = Properties.Resources.APP;
            // 置窗口标题
            this.Text = appTitle;
            // 置窗口初始大小
            this.Size = new System.Drawing.Size(1024, 640);
            this.statusStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
            // 日志栏准备就绪
            InitLogListView();
            DrawLog($"欢迎使用 {PublicFunction.GetAPPInformation(1)} !    ", LogType.Info);
            DrawLog($"工具版本:{PublicFunction.GetAPPInformation(2)}; {PublicFunction.GetAPPInformation(3)} 出品", LogType.Info);
            DrawLog("准备就绪", LogType.Info);
            DrawLog("要开始，请择选并打开一个 PAC 文件或者新建一个 PAC 文件。", LogType.Info);
            stripStatusLabel_Tips.Text = "准备就绪";
            stripProgressBar.Value = 0;
            stripStatusLabel_Percent.Text = "";
            // “文件”菜单项控制
            MenuItem_File_Open.Enabled = true;
            MenuItem_File_New.Enabled = true;
            MenuItem_File_Close.Enabled = false;
            MenuItem_File_PACProperties.Enabled = false;

            // “命令”菜单项控制
            MenuItem_Command.Enabled = false;
            MenuItem_Command_Pack.Visible = true;
            MenuItem_Command_Add.Visible = true;
            MenuItem_Command_Delete.Visible = true;
            MenuItem_Command_Unpack.Visible = true;
            MenuItem_Command_Add_UnpackSelect.Visible = true;
            // 工具条控制
            ToolStrip_New.Enabled = true;
            ToolStrip_Open.Enabled = true;
            ToolStrip_Close.Enabled = false;
            ToolStrip_Add.Visible = false;
            ToolStrip_Delete.Visible = false;
            ToolStrip_Pack.Visible = false;
            ToolStrip_UnpackSelect.Visible = false;
            ToolStrip_UnpackAll.Visible = false;
            ToolStrip_PACProperties.Visible = false;

            // 工具栏图标保持原来的 32x32 显示尺寸。
            this.ToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);

            // 统一工具栏按钮的“图标在上、文字在下”布局。
            NormalizeToolStripItemLayout();

            // 设计器里两个 Dock 控件在 SplitContainer.Panel1 中会重叠,
            // 这里在运行时把 ListView 强制放到 ToolStrip 下方并跟随尺寸变化。
            this.ToolStrip.Dock = DockStyle.Top;
            this.ListView_PAC.Dock = DockStyle.None;
            this.SplitContainer.Panel1.Resize += SplitContainer_Panel1_Resize;
            LayoutpacUnPackListView();

            // 初始化文件浏览框的树状导航：整行选中、双击进入/返回、文件夹/文件图标
            this.ListView_PAC.FullRowSelect = true;
            this.ListView_PAC.MultiSelect = true;
            this.ListView_PAC.DoubleClick += ListView_PAC_DoubleClick;
            this.ListView_PAC.SelectedIndexChanged += ListView_PAC_SelectedIndexChanged;
            this.ListView_PAC.KeyDown += ListView_PAC_KeyDown;
            SetuppacUnPackListViewIcons();
            InitializeDragAndDrop();
            UpdatePackDeleteCommandState();
        }

        /// <summary>
        /// 统一主工具栏按钮布局。
        /// <para>只调整显示关系，不改变设计器里的按钮尺寸，这样 WinForms 仍可按系统 DPI/字体缩放原来的 65x65 ToolStripItem。</para>
        /// </summary>
        private void NormalizeToolStripItemLayout()
        {
            ToolStripItem[] items =
            {
                ToolStrip_New,
                ToolStrip_Open,
                ToolStrip_Close,
                ToolStrip_Add,
                ToolStrip_Delete,
                ToolStrip_Pack,
                ToolStrip_UnpackSelect,
                ToolStrip_UnpackAll,
                ToolStrip_PACProperties,
                ToolStrip_Help,
                ToolStrip_About
            };

            foreach (ToolStripItem item in items)
            {
                item.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                item.ImageScaling = ToolStripItemImageScaling.SizeToFit;
                item.TextImageRelation = TextImageRelation.ImageAboveText;
                item.ImageAlign = ContentAlignment.BottomCenter;
                item.TextAlign = ContentAlignment.BottomCenter;
            }
        }

        /// <summary>
        /// 初始化文件拖放。窗体的大部分可见区域实际上由子控件覆盖，
        /// <para>因此不能只给 Form 设置 AllowDrop；否则拖到 PAC 列表或日志区时不会收到事件。</para>
        /// <para>所有目标共用同一套 DragEnter/DragDrop 处理，避免各区域出现不同业务逻辑。</para>
        /// </summary>
        private void InitializeDragAndDrop()
        {
            Control[] dropTargets =
            {
                this,
                this.MenuStrip,
                this.ToolStrip,
                this.SplitContainer,
                this.SplitContainer.Panel1,
                this.SplitContainer.Panel2,
                this.ListView_PAC,
                this.ListView_Log,
                this.statusStrip
            };

            foreach (Control control in dropTargets)
            {
                control.AllowDrop = true;
                control.DragEnter += MainForm_DragEnter;
                control.DragOver += MainForm_DragEnter;
                control.DragDrop += MainForm_DragDrop;
            }
        }

        #endregion

        #region 窗体初始化后操作

        #region 布局初始化

        /// <summary>
        /// 根据工具条停靠位置重排 PAC 文件列表的位置与尺寸
        /// </summary>
        private void LayoutpacUnPackListView()
        {
            // 如果工具条停靠在顶部，则 PAC 文件列表的 Y 坐标从工具条底部开始；否则从 0 开始。
            int top = this.ToolStrip.Dock == DockStyle.Top ? this.ToolStrip.Height : 0;
            this.ListView_PAC.SetBounds(
                x: 0,
                y: top,
                width: this.SplitContainer.Panel1.ClientSize.Width,
                height: this.SplitContainer.Panel1.ClientSize.Height - top);
        }

        #endregion

        #region PAC 文件浏览初始化

        /// <summary>
        /// 初始化文件浏览框的图标
        /// <para>如：文件夹(0)、上级".."</para>
        /// <para>文件图标不在此处初始化,而是在浏览时按扩展名动态加入(见 GetFileIconIndex)。
        /// 图标提取失败时自动退化为纯文本导航,不影响功能。</para>
        /// </summary>
        private void SetuppacUnPackListViewIcons()
        {
            try
            {
                Image folder = ToolStripIconShow.GetShell32Image(ToolStripIconShow.Shell32Index.FolderClosed, large: false);
                Image up = ToolStripIconShow.GetShell32Image(ToolStripIconShow.Shell32Index.FolderOpen, large: false);
                if (folder == null || up == null)
                {
                    if (folder != null) folder.Dispose();
                    if (up != null) up.Dispose();
                    return; // 图标提取失败,退化为纯文本导航
                }

                pacImageList = new ImageList();
                pacImageList.ImageSize = new Size(16, 16);
                pacImageList.ColorDepth = ColorDepth.Depth32Bit;
                pacImageList.Images.Add(folder);   // 0: 文件夹
                pacImageList.Images.Add(up);       // 1: 上级 ".."
                this.ListView_PAC.SmallImageList = pacImageList;

                // 注意:此处绝不能 Dispose 这些 Image!
                // ImageList 在 CreateHandle(控件真正显示)时才读取位图的尺寸/像素,
                // 若提前 Dispose,届时 get_Width() 会抛 "参数无效"(ArgumentException)。
                // ImageList 自身 Dispose 时会释放其持有的图片,无需手动释放。
            }
            catch
            {
                // 图标仅作装饰,任何异常都静默降级为无图标导航,不影响程序运行。
            }
        }

        #endregion

        #region 日志列表初始化

        /// <summary>
        /// 初始化日志框
        /// <para>单列占满宽度、自绘图标+文本、双缓冲防闪烁、自建日志图标列表</para>
        /// </summary>
        private void InitLogListView()
        {
            // 采用 Details 视图 + 单列占满宽度:让每一行铺满整行,
            // 从而背景色覆盖整行、点击整行任意位置都能选中(FullRowSelect)。
            ListView_Log.View = View.Details;
            ListView_Log.HeaderStyle = ColumnHeaderStyle.None;   // 隐藏列头
            ListView_Log.FullRowSelect = true;
            ListView_Log.HideSelection = false;
            ListView_Log.MultiSelect = false;
            ListView_Log.Columns.Add("");
            ListView_Log.Resize += ListView_Log_Resize;
            ListView_Log_Resize(null, null);

            // 自绘子项(图标 + 文本),避免默认绘制与 DrawItem 冲突
            ListView_Log.DrawSubItem += ListView_Log_DrawSubItem;

            // 启用双缓冲避免闪烁
            typeof(ListView).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(ListView_Log, true, null);

            // 准备日志图标(全部使用自绘 Bitmap,16x16、32 位色以保留透明通道)
            ImageList imageList = new ImageList();
            imageList.ImageSize = new Size(16, 16);
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.Images.Add(CreateLogIcon(LogType.Info));    // 0: 信息图标
            imageList.Images.Add(CreateLogIcon(LogType.Normal));  // 1: 正常/成功图标
            imageList.Images.Add(CreateLogIcon(LogType.Warning)); // 2: 警告图标
            imageList.Images.Add(CreateLogIcon(LogType.Error));   // 3: 错误图标
            ListView_Log.SmallImageList = imageList;
        }

        #endregion

        #endregion

        #region 控件事件

        #region 菜单事件

        /// <summary>
        /// 退出按钮响应函数
        /// </summary>
        private void MenuItem_File_Exit_Click(object sender, EventArgs e)
        {
            Exit();
        }

        /// <summary>
        /// 打开按钮响应函数
        /// </summary>
        private void MenuItem_File_Open_Click(object sender, EventArgs e)
        {
            LoadPACFile();
        }

        /// <summary>
        /// 新建按钮响应函数
        /// </summary>
        private void MenuItem_File_New_Click(object sender, EventArgs e)
        {
            NewPAC();
        }

        /// <summary>
        /// 关闭按钮响应函数
        /// </summary>
        private void MenuItem_File_Close_Click(object sender, EventArgs e)
        {
            ClosePACFile();
        }

        /// <summary>
        /// PAC属性按钮响应函数
        /// </summary>
        private void MenuItem_File_PACProperties_Click(object sender, EventArgs e)
        {
            ShowPacProperties();
        }

        /// <summary>
        /// 打包按钮响应函数
        /// </summary>
        private void MenuItem_Command_Pack_Click(object sender, EventArgs e)
        {
            PackPAC();
        }

        /// <summary>
        /// 添加文件按钮响应函数
        /// </summary>
        private void MenuItem_Command_Add_File_Click(object sender, EventArgs e)
        {
            AddFile();
        }

        /// <summary>
        /// 添加文件夹按钮响应函数
        /// </summary>
        private void MenuItem_Command_Add_Folder_Click(object sender, EventArgs e)
        {
            AddFoledr();
        }

        /// <summary>
        /// 解包按钮响应函数
        /// </summary>
        private void MenuItem_Command_Unpack_Click(object sender, EventArgs e)
        {
            UnPackall();
        }

        /// <summary>
        /// 解包选中按钮响应函数
        /// </summary>
        private void MenuItem_Command_Add_UnpackSelect_Click(object sender, EventArgs e)
        {
            UnpackSelectedFile();
        }

        /// <summary>
        /// 帮助按钮响应函数
        /// </summary>
        private void MenuItem_Help_Main_Click(object sender, EventArgs e)
        {
            ShowHelp();
        }

        /// <summary>
        /// 关于按钮响应函数
        /// </summary>
        private void MenuItem_Help_About_Click(object sender, EventArgs e)
        {
            ShowAbout();
        }
        #endregion

        #region 工具栏按钮事件

        /// <summary>
        /// 打开按钮
        /// </summary>
        private void ToolStrip_Open_Click(object sender, EventArgs e)
        {
            LoadPACFile();
        }

        /// <summary>
        /// 新建按钮
        /// </summary>
        private void ToolStrip_New_Click(object sender, EventArgs e)
        {
            NewPAC();
        }

        private void ToolStrip_Add_File_Click(object sender, EventArgs e)
        {
            AddFile();
        }

        /// <summary>
        /// 添加文件夹按钮响应函数
        /// </summary>
        private void ToolStrip_Add_Folder_Click(object sender, EventArgs e)
        {
            AddFoledr();
        }

        /// <summary>
        /// 打包按钮响应函数
        /// </summary>
        private void ToolStrip_Pack_Click(object sender, EventArgs e)
        {
            PackPAC();
        }

        /// <summary>
        /// 关闭文件
        /// </summary>
        private void ToolStrip_Close_Click(object sender, EventArgs e)
        {
            ClosePACFile();
        }

        /// <summary>
        /// 解包单独按钮
        /// </summary>
        private void ToolStrip_UnpackSelect_Click(object sender, EventArgs e)
        {
            try
            {
                UnpackSelectedFile();
            }
            catch (Exception ex)
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 提取选中文件失败:{ex.Message}", LogType.Error);
                System.Media.SystemSounds.Hand.Play();
            }
        }

        /// <summary>
        /// 解包全部按钮响应函数
        /// </summary>
        private void ToolStrip_UnpackAll_Click(object sender, EventArgs e)
        {
            UnPackall();
        }

        /// <summary>
        /// PAC属性按钮响应函数
        /// </summary>
        private void ToolStrip_PACProperties_Click(object sender, EventArgs e)
        {
            ShowPacProperties();
        }

        /// <summary>
        /// 帮助按钮响应函数
        /// </summary>
        private void ToolStrip_Help_Click(object sender, EventArgs e)
        {
            ShowHelp();
        }

        /// <summary>
        /// 关于按钮响应函数
        /// </summary>
        private void ToolStrip_About_Click(object sender, EventArgs e)
        {
            ShowAbout();
        }

        /// <summary>
        /// 更新日志按钮响应函数
        /// </summary>
        private void 更新日志LToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowChangeLog();
        }

        #endregion

        #region 右键菜单事件

        /// <summary>
        /// 打包列表右键弹出响应函数
        /// </summary>
        private void contextMenu_Pack_MouseUp(object sender, MouseEventArgs e)
        {
            // 仅右键且处于打包模式时弹出菜单
            if (e.Button != MouseButtons.Right || !arePackMode)
                return;
            contextMenu_Pack.Show(ListView_PAC, e.Location);
        }

        /// <summary>
        /// 预览选中文件响应函数
        /// </summary>
        private void contextMenu_Unpack_Preview_Click(object sender, EventArgs e)
        {
            // 将选中的文件映射成完整的 PAC 内部路径,并传入 PreviewFile,
            // 先解包到临时目录,再使用当前 Windows 默认打开方式打开。
            string pacPath;
            int targetIndex;
            if (!TryGetSelectedPacFile(out pacPath, out targetIndex))
                return;

            try
            {
                PreviewFile(pacPath);
            }
            catch (Exception ex)
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 预览失败:{ex.Message}", LogType.Error);
                System.Media.SystemSounds.Hand.Play();
            }
        }

        /// <summary>
        /// 提取单个文件响应函数
        /// </summary>
        private void contextMenu_Unpack_Single_Click(object sender, EventArgs e)
        {
            try
            {
                UnpackSelectedFile();
            }
            catch (Exception ex)
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 提取选中文件失败:{ex.Message}", LogType.Error);
                System.Media.SystemSounds.Hand.Play();
            }
        }

        /// <summary>
        /// 提取全部文件响应函数
        /// </summary>
        private void contextMenu_Unpack_All_Click(object sender, EventArgs e)
        {
            UnPackall();
        }

        /// <summary>
        /// PAC属性响应函数
        /// </summary>
        private void contextMenu_Unpack_PACProperties_Click(object sender, EventArgs e)
        {
            ShowPacProperties();
        }

        /// <summary>
        /// 打包PAC响应函数
        /// </summary>
        private void contextMenu_Pack_PACPack_Click(object sender, EventArgs e)
        {
            PackPAC();
        }

        /// <summary>
        /// 添加文件响应函数
        /// </summary>
        private void contextMenu_Pack_AddFile_Click(object sender, EventArgs e)
        {
            AddFile();
        }

        /// <summary>
        /// 添加文件夹响应函数
        /// </summary>
        private void contextMenu_Pack_AddFolder_Click(object sender, EventArgs e)
        {
            AddFoledr();
        }

        /// <summary>
        /// 打包模式删除命令统一入口：工具栏、命令菜单和右键菜单共用同一套业务逻辑。
        /// </summary>
        private void PackDeleteCommand_Click(object sender, EventArgs e)
        {
            DeletePackSelectedItems();
        }

        #endregion

        #region PAC 文件列表事件

        /// <summary>
        /// PAC 浏览框右键菜单入口：根据当前模式显示打包或解包菜单。
        /// </summary>
        private void ListView_PAC_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            // 打包模式下，PAC 浏览框用于维护待打包文件列表。
            // 右键未选中的项目时，先把它切换为当前选择；右键已选中的项目时保留现有多选，
            // 这样“删除选中项”既不会误删之前的项目，也支持 Ctrl/Shift 多选批量移除。
            if (arePackMode)
            {
                // 注意这里使用 packHit，避免与后面的解包模式命中变量处于同一方法作用域时发生 CS0136。
                ListViewHitTestInfo packHit = ListView_PAC.HitTest(e.Location);
                if (packHit.Item != null && !packHit.Item.Selected)
                {
                    // 先复制再取消选择，避免一边枚举 SelectedItems 一边修改该集合。
                    var previousSelection = new List<ListViewItem>();
                    foreach (ListViewItem selectedItem in ListView_PAC.SelectedItems)
                        previousSelection.Add(selectedItem);
                    foreach (ListViewItem selectedItem in previousSelection)
                        selectedItem.Selected = false;

                    packHit.Item.Selected = true;
                }

                UpdatePackDeleteCommandState();
                contextMenu_Pack.Show(ListView_PAC, e.Location);
                return;
            }

            // 只有解包模式才显示预览、提取和 PAC 属性菜单。
            if (!areUnpackMode)
                return;

            // 命中测试：右键哪一行就先选中哪一行，保证"提取选中"提取的是右键目标项。
            ListViewHitTestInfo unpackHit = ListView_PAC.HitTest(e.Location);
            if (unpackHit.Item != null)
            {
                unpackHit.Item.Selected = true;
            }

            bool hasSelectedFile = unpackHit.Item != null
                && string.Equals(unpackHit.Item.Tag as string, "file", StringComparison.Ordinal);
            contextMenu_Unpack_Preview.Enabled = hasSelectedFile;
            contextMenu_Unpack_Single.Enabled = hasSelectedFile;
            contextMenu_Unpack_All.Enabled = pacUnPackList != null && pacUnPackList.Count > 0;
            contextMenu_Unpack_PACProperties.Enabled = pacUnPackList != null && pacUnPackList.Count > 0;

            // 以控件相对坐标在鼠标位置弹出菜单(无参 Show() 会按屏幕坐标弹到鼠标所在处,
            // 且缺乏命中选中,此处显式指定锚点控件与坐标更稳妥)
            contextMenu_Unpack.Show(ListView_PAC, e.Location);
        }

        /// <summary>
        /// 选中项变化时同步“删除”命令状态。
        /// </summary>
        private void ListView_PAC_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePackDeleteCommandState();
        }

        /// <summary>
        /// 打包模式下支持键盘 Delete，从待打包列表移除当前选中项。
        /// </summary>
        private void ListView_PAC_KeyDown(object sender, KeyEventArgs e)
        {
            if (!arePackMode || e.KeyCode != Keys.Delete)
                return;

            DeletePackSelectedItems();
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        /// <summary>
        /// 双击文件浏览框:进入子目录 / 返回上级目录 / (文件暂不处理)。
        /// </summary>
        private void ListView_PAC_DoubleClick(object sender, EventArgs e)
        {
            if (ListView_PAC.SelectedItems.Count == 0)
                return;

            var item = ListView_PAC.SelectedItems[0];
            string tag = item.Tag as string;

            if (tag == "up")
            {
                // 返回上级目录
                int idx = currentDirPath.LastIndexOf('/');
                string parent = idx < 0 ? "" : currentDirPath.Substring(0, idx);
                LoadListViewByDirectory(parent);
            }
            else if (tag == "dir")
            {
                // 进入子目录
                string dirName = item.Text;
                string newPath = string.IsNullOrEmpty(currentDirPath)
                    ? dirName
                    : currentDirPath + "/" + dirName;
                LoadListViewByDirectory(newPath);
            }
            else if (tag == "pack-up")
            {
                // 打包模式返回上级目录
                int idx = currentDirPath.LastIndexOf('/');
                string parent = idx < 0 ? "" : currentDirPath.Substring(0, idx);
                currentDirPath = parent;
                RefreshPackListView();
            }
            else if (tag == "pack-dir")
            {
                // 打包模式进入子目录
                string dirName = item.Text;
                currentDirPath = string.IsNullOrEmpty(currentDirPath)
                    ? dirName
                    : currentDirPath + "/" + dirName;
                RefreshPackListView();
            }
            else if (tag == "pack-file")
            {
                // 打包模式文件双击暂不执行预览,保留为列表选择操作。
                return;
            }
            else if (tag == "file")
            {
                // 文件双击:先释放到临时目录；DDS 用内置浏览器，其他文件用默认程序打开。
                try
                {
                    PreviewFile(item.Name);
                }
                catch (Exception ex)
                {
                    DrawLog($"{PublicFunction.GetCurrentTime()} 预览失败:{ex.Message}", LogType.Error);
                    System.Media.SystemSounds.Hand.Play();
                }
            }
        }

        #endregion

        #region 文件拖放事件

        /// <summary>
        /// 拖入/拖动过程中只判断当前状态是否允许接收。
        /// 打包模式：接收文件和文件夹，作为待打包项添加。
        /// 非打包模式：只接收单个 .pac 文件，用于快捷打开。
        /// </summary>
        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = CanAcceptDrop(e.Data) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        /// <summary>
        /// 统一拖放入口。打包模式优先解释为“添加待打包项”，
        /// 因而即使拖入的是 .pac 文件，也只会把它当普通文件加入列表，不会切换当前 PAC。
        /// </summary>
        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] droppedPaths = GetDroppedPaths(e.Data);
            if (droppedPaths == null || droppedPaths.Length == 0)
                return;

            if (arePackMode)
            {
                AddDroppedItemsToPackList(droppedPaths);
                return;
            }

            // 非打包模式下只允许拖入一个 PAC 文件作为快捷打开。
            if (droppedPaths.Length != 1 || !IsPacFilePath(droppedPaths[0]))
                return;

            LoadPACFile(droppedPaths[0]);
        }

        /// <summary>
        /// 根据当前工作模式判断拖入内容是否有效。
        /// </summary>
        private bool CanAcceptDrop(IDataObject data)
        {
            string[] droppedPaths = GetDroppedPaths(data);
            if (droppedPaths == null || droppedPaths.Length == 0)
                return false;

            // 后台打包时禁止修改待打包清单。
            if (packRunning)
                return false;

            if (arePackMode)
            {
                foreach (string path in droppedPaths)
                {
                    if (File.Exists(path) || Directory.Exists(path))
                        return true;
                }

                return false;
            }

            return droppedPaths.Length == 1 && IsPacFilePath(droppedPaths[0]);
        }

        /// <summary>
        /// 从 Windows 文件拖放数据中提取路径列表。
        /// </summary>
        private static string[] GetDroppedPaths(IDataObject data)
        {
            if (data == null || !data.GetDataPresent(DataFormats.FileDrop))
                return null;

            return data.GetData(DataFormats.FileDrop) as string[];
        }

        /// <summary>
        /// 判断路径是否为一个实际存在的 PAC 文件。拖放快捷打开只接受 .pac，
        /// 手工“打开”对话框仍保留原来的“所有文件”选项，由 PAC 解析器判断格式。
        /// </summary>
        private static bool IsPacFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return false;

            return string.Equals(Path.GetExtension(path), ".pac", StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region 日志列表事件

        /// <summary>
        /// 日志框右键弹出响应函数
        /// </summary>
        private void ListView_Log_MouseUp(object sender, MouseEventArgs e)
        {
            // 仅响应右键并弹出日志菜单
            if (e.Button != MouseButtons.Right)
                return;
            contextMenu_Log.Show(ListView_Log, e.Location);
        }

        /// <summary>
        /// 日志行背景绘制响应函数
        /// </summary>
        private void ListView_Log_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            // 整行背景:选中用系统高亮,否则用该日志类型设定的背景色
            Color backColor = e.Item.Selected
                ? (e.Item.ListView.Focused ? SystemColors.Highlight : SystemColors.Control)
                : e.Item.BackColor;

            using (SolidBrush bgBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }
        }

        /// <summary>
        /// 日志行内容（图标+文本）绘制响应函数
        /// </summary>
        private void ListView_Log_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            // 单列布局,只绘制第一列(列头已隐藏)
            if (e.ColumnIndex != 0)
                return;

            // 选中状态用系统高亮前景色,否则用该日志类型设定的前景色
            Color foreColor = e.Item.Selected ? SystemColors.HighlightText : e.Item.ForeColor;

            // 绘制图标(如果有)
            int iconWidth = 0;
            ImageList imgList = ListView_Log.SmallImageList;
            if (imgList != null && e.Item.ImageIndex >= 0 && e.Item.ImageIndex < imgList.Images.Count)
            {
                Image img = imgList.Images[e.Item.ImageIndex];
                if (img != null)
                {
                    int iconY = e.Bounds.Y + (e.Bounds.Height - img.Height) / 2;
                    e.Graphics.DrawImage(img, e.Bounds.X + 4, iconY, img.Width, img.Height);
                    iconWidth = img.Width + 4;
                }
            }

            // 绘制文本(从图标右侧开始)
            Rectangle textRect = new Rectangle(
                e.Bounds.X + 4 + iconWidth,
                e.Bounds.Y,
                e.Bounds.Width - 4 - iconWidth,
                e.Bounds.Height);
            TextRenderer.DrawText(
                e.Graphics,
                e.Item.Text,
                e.Item.Font,
                textRect,
                foreColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis
            );
        }

        /// <summary>
        /// 日志框尺寸变化时调整列宽响应函数
        /// </summary>
        private void ListView_Log_Resize(object sender, EventArgs e)
        {
            // 让唯一的列始终占满 ListView 宽度,保证整行可点击、背景铺满整行
            if (ListView_Log.Columns.Count > 0)
            {
                int width = ListView_Log.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
                ListView_Log.Columns[0].Width = width > 0 ? width : 0;
            }
        }

        #endregion

        #region 布局事件

        /// <summary>
        /// Panel1 尺寸变化时，让 ListView_PAC 始终位于 ToolStrip 下方并填满剩余空间。
        /// </summary>
        private void SplitContainer_Panel1_Resize(object sender, EventArgs e)
        {
            LayoutpacUnPackListView();
        }

        #endregion

        #region 状态栏事件

        /// <summary>
        /// 状态栏进度条点击响应函数
        /// </summary>
        private void stripProgressBar_Click(object sender, EventArgs e)
        {
        }

        #endregion

        #endregion

        #region 动作实现

        #region 应用程序操作

        /// <summary>
        /// 退出应用程序
        /// </summary>
        private void Exit()
        {
            Application.Exit();
        }

        #endregion

        #region PAC 打开

        /// <summary>
        /// 载入 PAC
        /// <para>通过“打开”对话框选择并载入 PAC 文件。
        /// 实际载入逻辑集中在 LoadPACFile(string)，拖放快捷打开也复用同一套代码</para>
        /// </summary>
        private void LoadPACFile()
        {
            using (OpenFileDialog pacOpen = new OpenFileDialog
            {
                Title = "请选择要打开的 PAC 文件",
                Filter = "PAC 文件 (*.pac)|*.pac|所有文件 (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            })
            {                            
                if (pacOpen.ShowDialog() != DialogResult.OK)
                {
                    // 此流程已弃用
                    // MessageBox.Show("未选择文件", "未选择", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                // 载入 PAC
                LoadPACFile(pacOpen.FileName);
            }
        }


        /// <summary> 
        /// &lt;逻辑型&gt;从指定路径载入 PAC
        /// <param name="pacFilePath">(文本型 欲载入的 PAC文件)</param>
        /// <para>先解析到临时列表，确认成功后再替换当前界面状态；
        /// 这样拖入损坏/不支持的 PAC 时，不会把当前已打开的 PAC 状态清掉</para>
        /// <returns>成功返回真，否则返回假</returns>
        /// </summary>
        private bool LoadPACFile(string pacFilePath)
        {
            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(pacFilePath);
                if (!File.Exists(fullPath))
                    throw new FileNotFoundException("PAC 文件不存在。", fullPath);

                // 先解析，成功后再提交到窗体状态。避免失败时破坏当前会话。
                List<Tuple<byte[], string, long, long>> loadedPacList =
                    FPACHelper.UnPack.GetPACInformation(fullPath);

                currentPACFile = fullPath;
                pacUnPackList = loadedPacList;
                areUnpackMode = true;
                arePackMode = false;
                currentDirPath = "";
                SetPacListViewMode(false);

                // 日志框是 Details 视图 + 单列，只清项目，不能用 Clear() 清掉列。
                ListView_Log.Items.Clear();

                DrawLog($"{PublicFunction.GetCurrentTime()}支持的格式，可以提取！", LogType.Normal);
                DrawLog($"{PublicFunction.GetCurrentTime()}共有 {pacUnPackList.Count} 个文件。", LogType.Normal);
                if (pacUnPackList.Count >= 5000)
                {
                    DrawLog($"{PublicFunction.GetCurrentTime()}文件数量较多，文件浏览框将分批载入，请耐心地等待！", LogType.Warning);
                }

                this.Text = $"{appTitle} -- 正在浏览: {Path.GetFileName(currentPACFile)} ";
                stripStatusLabel_Tips.Text = $"已打开文件: {Path.GetFileName(currentPACFile)}";
                stripProgressBar.Value = 0;

                // 相关控件状态设置:菜单栏、工具条、文件浏览框
                SetPackControlsEnabled(true);
                MenuItem_File_Open.Enabled = false;
                MenuItem_File_New.Enabled = false;
                MenuItem_File_Close.Enabled = true;
                MenuItem_File_PACProperties.Enabled = true;

                MenuItem_Command.Enabled = true;
                MenuItem_Command_Pack.Visible = false;
                MenuItem_Command_Add.Visible = false;
                MenuItem_Command_Delete.Visible = false;
                MenuItem_Command_Unpack.Visible = true;
                MenuItem_Command_Add_UnpackSelect.Visible = true;

                // 工具条控制
                ToolStrip_New.Enabled = false;
                ToolStrip_Open.Enabled = false;
                ToolStrip_Close.Enabled = true;
                ToolStrip_Add.Visible = false;
                ToolStrip_Delete.Visible = false;
                ToolStrip_Pack.Visible = false;
                ToolStrip_UnpackSelect.Visible = true;
                ToolStrip_UnpackAll.Visible = true;
                ToolStrip_PACProperties.Visible = true;

                UpdatePackDeleteCommandState();

                System.Media.SystemSounds.Beep.Play();

                ListView_PAC.MultiSelect = false;

                // 其余界面状态提交完成后再开始异步渲染根目录，
                // 避免载入中的状态栏信息被“已打开文件”覆盖。
                LoadListViewByDirectory("");
                return true;
            }
            catch (Exception ex)
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 打开 PAC 失败:{ex.Message}", LogType.Error);
                System.Media.SystemSounds.Hand.Play();
                stripStatusLabel_Tips.Text = "发生错误";
                stripProgressBar.Value = 0;
                return false;
            }
        }

        #endregion

        #region PAC 新建

        /// <summary>
        /// 新建PAC
        /// </summary>
        private void NewPAC()
        {
            // 创建PAC文件新建对话框
            using (SaveFileDialog pacNew = new SaveFileDialog
            {
                Title = "请选择 PAC 文件保存路径",                       // 置对话框标题文本
                Filter = "PAC 文件 (*.pac)|*.pac|所有文件 (*.*)|*.*",    // 置对话框过滤器
                FilterIndex = 1,                                         // 默认选中类型
                RestoreDirectory = true
            })
            {
                // 置现行PAC文件路径变量
                if (pacNew.ShowDialog() != DialogResult.OK)
                    return; // 取消新建:不进入打包模式,不清空现有状态

                currentPACFile = pacNew.FileName;
            }

            // 新建 PAC 时清空上一次的待打包列表,避免把旧文件带入新 PAC。
            if (packListSum != null)
                packListSum.Clear();

            arePackMode = true;
            areUnpackMode = false;
            SetPacListViewMode(true);

            // 清空文件浏览框并切换为打包模式列表
            ListView_PAC.Items.Clear();
            currentDirPath = "";
            RefreshPackListView();

            // 只清空日志项目,保留 Details 视图所需的列。
            ListView_Log.Items.Clear();
            DrawLog($"{PublicFunction.GetCurrentTime()}已创建新文件!", LogType.Normal);
            DrawLog($"{PublicFunction.GetCurrentTime()}请添加要打包的文件或文件夹!(支持拖拽添加)", LogType.Normal);

            this.Text = $"{appTitle} -- 正在打包: {Path.GetFileName(currentPACFile)} ";
            stripStatusLabel_Tips.Text = $"已创建文件: {Path.GetFileName(currentPACFile)}";
            stripProgressBar.Value = 0;
            // 设置“命令”菜单栏状态
            SetPackControlsEnabled(true);
            MenuItem_File_Open.Enabled = false;
            MenuItem_File_New.Enabled = false;
            MenuItem_File_Close.Enabled = true;
            MenuItem_File_PACProperties.Enabled = false;

            MenuItem_Command.Enabled = !false;
            MenuItem_Command_Pack.Visible = true;
            MenuItem_Command_Add.Visible = true;
            MenuItem_Command_Delete.Visible = true;
            MenuItem_Command_Unpack.Visible = !true;
            MenuItem_Command_Add_UnpackSelect.Visible = !true;
            // 工具条控制
            ToolStrip_New.Enabled = !true;
            ToolStrip_Open.Enabled = !true;
            ToolStrip_Close.Enabled = !false;
            ToolStrip_Add.Visible = !false;
            ToolStrip_Delete.Visible = !false;
            ToolStrip_Pack.Visible = !false;
            ToolStrip_UnpackSelect.Visible = false;
            ToolStrip_UnpackAll.Visible = false;
            ToolStrip_PACProperties.Visible = false;

            ListView_PAC.MultiSelect = true;
        }

        #endregion

        #region 文件与文件夹添加

        /// <summary>
        /// 添加文件到 PAC 待打包列表
        /// <para>文件会加入“当前正在浏览的 PAC 目录”，而不是固定加入 PAC 根目录</para>
        /// </summary>
        private void AddFile()
        {
            if (!arePackMode || string.IsNullOrEmpty(currentPACFile))
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 请先新建一个 PAC 文件再添加文件。", LogType.Warning);
                System.Media.SystemSounds.Hand.Play();
                return;
            }

            if (packRunning)
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 正在打包中,请等待完成后再添加文件。", LogType.Warning);
                System.Media.SystemSounds.Hand.Play();
                return;
            }

            if (packListSum == null)
                packListSum = new List<Tuple<string, string>>();

            // 固定本次添加动作的目标目录，避免以后代码中途改变 currentDirPath 时产生歧义。
            string targetDirectoryPath = NormalizePackInternalPath(currentDirPath);

            using (OpenFileDialog fileDialog = new OpenFileDialog
            {
                Title = "请选择要添加到 PAC 的文件",
                Filter = "所有文件 (*.*)|*.*",
                Multiselect = true,
                RestoreDirectory = true
            })
            {
                if (fileDialog.ShowDialog() != DialogResult.OK)
                    return;

                int addedCount = 0;
                int skippedCount = 0;

                foreach (string filePath in fileDialog.FileNames)
                {
                    string fullPath;
                    try
                    {
                        fullPath = Path.GetFullPath(filePath);
                    }
                    catch
                    {
                        skippedCount++;
                        continue;
                    }

                    if (!File.Exists(fullPath))
                    {
                        skippedCount++;
                        continue;
                    }

                    string internalPath = CombinePackInternalPath(
                        targetDirectoryPath,
                        Path.GetFileName(fullPath));

                    if (TryAddPackEntry(fullPath, internalPath))
                        addedCount++;
                    else
                        skippedCount++;
                }

                string targetText = string.IsNullOrEmpty(targetDirectoryPath)
                    ? "PAC 根目录"
                    : targetDirectoryPath;

                DrawLog(
                    $"{PublicFunction.GetCurrentTime()} 已添加 {addedCount} 个文件到待打包目录:{targetText}",
                    LogType.Normal);

                if (skippedCount > 0)
                    DrawLog($"{PublicFunction.GetCurrentTime()} 有 {skippedCount} 个文件因路径重复或无效而跳过。", LogType.Warning);
            }

            RefreshPackListView();
        }

        /// <summary>
        /// 添加文件夹到 PAC 待打包列表
        /// <para>保留所选文件夹本体，并把它加入当前正在浏览的 PAC 目录。
        /// 例如当前目录为 game/data，选择 C:\123 时，PAC 内部生成 game/data/123/...</para>
        /// </summary>
        private void AddFoledr()
        {
            // 打包模式下才允许添加文件夹，且必须先新建 PAC 文件。
            if (!arePackMode || string.IsNullOrEmpty(currentPACFile))
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 请先新建一个 PAC 文件再添加文件夹。", LogType.Warning);
                System.Media.SystemSounds.Hand.Play();
                return;
            }

            // 后台打包时禁止修改待打包清单。
            if (packRunning)
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 正在打包中,请等待完成后再添加文件夹。", LogType.Warning);
                System.Media.SystemSounds.Hand.Play();
                return;
            }

            // packListSum 可能在新建 PAC 时被清空，或在关闭 PAC 时被置 null
            if (packListSum == null)
                packListSum = new List<Tuple<string, string>>();

            // 固定本次添加动作的目标目录，避免以后代码中途改变 currentDirPath 时产生歧义
            string targetDirectoryPath = NormalizePackInternalPath(currentDirPath);

            // 以下是文件夹相关的 UI 交互和业务逻辑，使用 FolderBrowserDialog 选择文件夹
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog
            {
                Description = "请选择要添加到 PAC 的文件夹",
                ShowNewFolderButton = false
            })
            {
                if (folderDialog.ShowDialog() != DialogResult.OK)
                    return;

                int addedCount;
                int skippedCount;
                string folderName;
                string errorMessage;

                if (!AddFolderToPackList(
                    folderDialog.SelectedPath,
                    targetDirectoryPath,
                    out addedCount,
                    out skippedCount,
                    out folderName,
                    out errorMessage))
                {
                    DrawLog($"{PublicFunction.GetCurrentTime()} 添加文件夹失败:{errorMessage}", LogType.Warning);
                    MessageBox.Show(
                        errorMessage,
                        "无法添加文件夹",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    System.Media.SystemSounds.Hand.Play();
                    return;
                }

                if (addedCount > 0)
                {
                    string destinationPath = CombinePackInternalPath(targetDirectoryPath, folderName);
                    DrawLog(
                        $"{PublicFunction.GetCurrentTime()} 已添加文件夹“{folderName}”到 {destinationPath}，新增 {addedCount} 个待打包文件。",
                        LogType.Normal);
                }
                else
                {
                    DrawLog(
                        $"{PublicFunction.GetCurrentTime()} 文件夹“{folderName}”没有新增可打包文件（内容可能已存在）。",
                        LogType.Warning);
                }

                if (skippedCount > 0)
                    DrawLog($"{PublicFunction.GetCurrentTime()} 有 {skippedCount} 个文件因路径重复或无效而跳过。", LogType.Warning);
            }

            RefreshPackListView();
        }

        /// <summary>
        /// 拖放文件/文件夹加入待打包列表
        /// <param name="droppedPaths">(文本型数组 拖放得到的文件/文件夹完整路径列表)</param>
        /// <para>与“添加文件/文件夹”按钮共用同一套目标目录规则：文件直接加入当前 PAC 目录，
        /// 文件夹则保留目录本体并整体加入当前 PAC 目录</para>
        /// </summary>
        private void AddDroppedItemsToPackList(string[] droppedPaths)
        {
            // 前置校验：非打包模式或未打开 PAC 时，拖放不生效，直接返回。
            if (!arePackMode || string.IsNullOrEmpty(currentPACFile))
                return;

            // 打包进行中禁止修改待打包清单，提示并返回。
            if (packRunning)
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 正在打包中,请等待完成后再添加文件。", LogType.Warning);
                System.Media.SystemSounds.Hand.Play();
                return;
            }

            // 待打包清单惰性初始化；Item1=源文件完整路径，Item2=PAC 内部路径。
            if (packListSum == null)
                packListSum = new List<Tuple<string, string>>();

            // 统一将当前 PAC 目录规范化为“/”分隔的内部路径，作为本次添加的目标目录。
            string targetDirectoryPath = NormalizePackInternalPath(currentDirPath);
            // 统计本次拖放结果：成功加入数、跳过数、文件夹失败数。
            int addedCount = 0;
            int skippedCount = 0;
            int failedFolderCount = 0;

            // 逐个处理拖放的路径：文件、文件夹、无效路径分别处理。
            foreach (string droppedPath in droppedPaths)
            {
                string fullPath;
                try
                {
                    // 将拖放路径解析为绝对路径。
                    fullPath = Path.GetFullPath(droppedPath);
                }
                catch
                {
                    // 路径非法（如格式错误）按跳过处理，不中断整个拖放流程。
                    skippedCount++;
                    continue;
                }

                // 分支一：单个文件——以文件名作为内部路径，直接加入当前 PAC 目录。
                if (File.Exists(fullPath))
                {
                    string internalPath = CombinePackInternalPath(
                        targetDirectoryPath,
                        Path.GetFileName(fullPath));

                    // TryAddPackEntry 内部会做去重与有效性校验，据此累计成功/跳过数。
                    if (TryAddPackEntry(fullPath, internalPath))
                        addedCount++;
                    else
                        skippedCount++;

                    continue;
                }

                // 分支二：文件夹——交给 AddFolderToPackList 递归处理，保留目录本体。
                if (Directory.Exists(fullPath))
                {
                    int folderAddedCount;
                    int folderSkippedCount;
                    string folderName;
                    string errorMessage;

                    // 成功时累加该文件夹内部的新增/跳过文件数。
                    if (AddFolderToPackList(
                        fullPath,
                        targetDirectoryPath,
                        out folderAddedCount,
                        out folderSkippedCount,
                        out folderName,
                        out errorMessage))
                    {
                        addedCount += folderAddedCount;
                        skippedCount += folderSkippedCount;
                    }
                    else
                    {
                        // 失败（空文件夹、无法读取等）单独计数并告警。
                        failedFolderCount++;
                        DrawLog(
                            $"{PublicFunction.GetCurrentTime()} 拖拽文件夹失败:{fullPath}; {errorMessage}",
                            LogType.Warning);
                    }

                    continue;
                }

                // 既不是文件也不是目录（被删除、权限不足等），按跳过处理。
                skippedCount++;
            }

            // 刷新待打包列表视图，让新增项立即显示。
            RefreshPackListView();

            // 目标目录为空表示添加到 PAC 根目录，用于日志展示。
            string targetText = string.IsNullOrEmpty(targetDirectoryPath)
                ? "PAC 根目录"
                : targetDirectoryPath;

            // 汇总日志：有新增则报告添加数量与目标目录。
            if (addedCount > 0)
            {
                DrawLog(
                    $"{PublicFunction.GetCurrentTime()} 拖拽添加完成:已添加 {addedCount} 个文件到 {targetText}。",
                    LogType.Normal);
            }
            else
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 拖拽添加完成:没有新增可打包文件。", LogType.Warning);
            }

            // 跳过项提示（重复或无效路径）。
            if (skippedCount > 0)
                DrawLog($"{PublicFunction.GetCurrentTime()} 有 {skippedCount} 个文件或路径因重复、无效而跳过。", LogType.Warning);

            // 文件夹失败汇总，并播放提示音提醒用户注意。
            if (failedFolderCount > 0)
            {
                DrawLog(
                    $"{PublicFunction.GetCurrentTime()} 有 {failedFolderCount} 个文件夹无法加入待打包列表（空文件夹或无法读取）,已跳过。",
                    LogType.Warning);
                System.Media.SystemSounds.Hand.Play();
            }
        }

        /// <summary>
        /// 将一个磁盘目录及其全部文件加入指定 PAC 目录，并保留所选目录本体。
        /// packListSum.Item1 保存“源文件完整路径”，Item2 保存“PAC 内部完整路径”，
        /// 因而源文件位置和 PAC 目标目录完全解耦，可以在任意 PAC 子目录中继续添加。
        /// </summary>
        private bool AddFolderToPackList(
            string folderPath,
            string targetDirectoryPath,
            out int addedCount,
            out int skippedCount,
            out string folderName,
            out string errorMessage)
        {
            addedCount = 0;
            skippedCount = 0;
            folderName = "";
            errorMessage = "";

            string fullFolderPath;
            DirectoryInfo folderInfo;
            try
            {
                fullFolderPath = Path.GetFullPath(folderPath);
                if (!Directory.Exists(fullFolderPath))
                {
                    errorMessage = "目录不存在。";
                    return false;
                }

                folderInfo = new DirectoryInfo(fullFolderPath);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }

            // 驱动器根目录/共享根目录不适合作为 PAC 内部的单一文件夹名。
            if (folderInfo.Parent == null || string.IsNullOrWhiteSpace(folderInfo.Name))
            {
                errorMessage = "不能直接添加驱动器或共享根目录，请选择其下的具体文件夹。";
                return false;
            }

            folderName = folderInfo.Name;

            string[] files;
            try
            {
                files = Directory.GetFiles(fullFolderPath, "*", SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                errorMessage = "遍历目录失败:" + ex.Message;
                return false;
            }

            // PAC 文件表只记录文件，不存在合法的“空目录项”。
            // 因此完全没有文件的目录不能进入待打包结构。
            if (files.Length == 0)
            {
                errorMessage = "PAC 里不能有空文件夹，所选文件夹中没有任何可打包文件。";
                return false;
            }

            string destinationFolderPath = CombinePackInternalPath(
                targetDirectoryPath,
                folderName);

            foreach (string filePath in files)
            {
                string relativeInsideFolder;
                try
                {
                    relativeInsideFolder = GetRelativeToBase(fullFolderPath, filePath);
                }
                catch
                {
                    skippedCount++;
                    continue;
                }

                if (string.IsNullOrEmpty(relativeInsideFolder))
                {
                    skippedCount++;
                    continue;
                }

                string internalPath = CombinePackInternalPath(
                    destinationFolderPath,
                    relativeInsideFolder);

                if (TryAddPackEntry(filePath, internalPath))
                    addedCount++;
                else
                    skippedCount++;
            }

            return true;
        }

        /// <summary>
        /// &lt;逻辑型&gt;向 packListSum 添加一条源文件到 PAC 内部路径的映射
        /// <param name="sourceFilePath">(文本型 原文件路径, </param>
        /// <param name="internalPath">文本型 相对文件路径）</param>
        /// <para>Item1 始终保存源文件的完整路径；Item2 始终保存 PAC 内部完整相对路径
        /// PAC 内部路径按不区分大小写去重</para>
        /// <returns>成功返回真，否则返回假</returns>
        /// </summary>
        private bool TryAddPackEntry(string sourceFilePath, string internalPath)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath) || string.IsNullOrWhiteSpace(internalPath))
                return false;

            string fullSourcePath;
            try
            {
                fullSourcePath = Path.GetFullPath(sourceFilePath);
            }
            catch
            {
                return false;
            }

            if (!File.Exists(fullSourcePath))
                return false;

            string normalizedPath = NormalizePackInternalPath(internalPath);
            if (string.IsNullOrEmpty(normalizedPath))
                return false;

            if (packListSum == null)
                packListSum = new List<Tuple<string, string>>();

            foreach (Tuple<string, string> item in packListSum)
            {
                if (item == null || string.IsNullOrEmpty(item.Item2))
                    continue;

                string existingPath = NormalizePackInternalPath(item.Item2);
                if (string.Equals(existingPath, normalizedPath, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            packListSum.Add(new Tuple<string, string>(fullSourcePath, normalizedPath));
            return true;
        }

        /// <summary>
        /// &lt;文本型&gt;拼接 PAC 内部路径
        /// <param name="directoryPath">(文本型 主路径, </param>
        /// <param name="childPath">文本型 子路径, )</param>
        /// <para>统一使用 '/'，并自动处理根目录。</para>
        /// <returns>成功返回处理好的文本</returns>
        /// </summary>
        private static string CombinePackInternalPath(string directoryPath, string childPath)
        {
            string directory = NormalizePackInternalPath(directoryPath);
            string child = NormalizePackInternalPath(childPath);

            if (string.IsNullOrEmpty(directory))
                return child;
            if (string.IsNullOrEmpty(child))
                return directory;

            return directory + "/" + child;
        }

        #endregion

        #region 待打包列表删除

        /// <summary>
        /// 移除待打包列表中的选中项目
        /// <para>从待打包清单中移除当前选中的文件或目录。
        /// 这里只修改 packListSum，不删除任何磁盘上的源文件/源目录。
        /// PAC 目录由文件路径动态生成，因此某目录的最后一个文件被删除后，
        /// 该目录会自动从待打包目录树中消失，不会留下空文件夹。</para>
        /// </summary>
        private void DeletePackSelectedItems()
        {
            // 前置校验：非打包模式或未打开 PAC 时无法移除，提示并返回。
            if (!arePackMode || string.IsNullOrEmpty(currentPACFile))
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 当前不在打包模式，无法移除待打包项目。", LogType.Warning);
                System.Media.SystemSounds.Hand.Play();
                return;
            }

            // 打包进行中禁止修改待打包清单。
            if (packRunning)
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 正在打包中，请等待完成后再修改待打包列表。", LogType.Warning);
                System.Media.SystemSounds.Hand.Play();
                return;
            }

            // 清单为空时无可移除项目。
            if (packListSum == null || packListSum.Count == 0)
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 待打包列表为空，没有可移除的项目。", LogType.Warning);
                System.Media.SystemSounds.Hand.Play();
                return;
            }

            // 收集本次删除目标，区分“文件”与“目录”两类：
            // fileTargets 存文件 PAC 内部完整路径；directoryTargets 存目录 PAC 内部路径。
            // 用 HashSet 自动去重，大小写不敏感（PAC 路径统一用“/”分隔）。
            var fileTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var directoryTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 遍历当前选中的列表项，按 Tag 区分是文件还是目录，分别归类。
            foreach (ListViewItem item in ListView_PAC.SelectedItems)
            {
                string tag = item.Tag as string;
                if (string.Equals(tag, "pack-file", StringComparison.Ordinal))
                {
                    // 文件项：直接取其内部路径（Name 即 PAC 内部完整路径）。
                    string filePath = NormalizePackInternalPath(item.Name);
                    if (!string.IsNullOrEmpty(filePath))
                        fileTargets.Add(filePath);
                }
                else if (string.Equals(tag, "pack-dir", StringComparison.Ordinal))
                {
                    // 目录项：RefreshPackListView 会把目录的 PAC 内部完整路径保存在 Name；
                    // 下面的回退计算只用于兼容异常/旧列表项。
                    string directoryPath = NormalizePackInternalPath(item.Name);
                    if (string.IsNullOrEmpty(directoryPath))
                    {
                        directoryPath = CombinePackInternalPath(currentDirPath, item.Text);
                    }

                    if (!string.IsNullOrEmpty(directoryPath))
                        directoryTargets.Add(directoryPath);
                }
            }

            // 目标总项目数 = 文件数 + 目录数。
            int targetCount = fileTargets.Count + directoryTargets.Count;
            if (targetCount == 0)
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 请选择要从待打包列表移除的文件或文件夹。", LogType.Warning);
                System.Media.SystemSounds.Hand.Play();
                UpdatePackDeleteCommandState();
                return;
            }

            // 预统计受影响的文件数：目录会连带其所有后代文件，故需遍历整个清单判断。
            int affectedFileCount = 0;
            foreach (Tuple<string, string> entry in packListSum)
            {
                if (IsPackEntryTargetedForRemoval(entry, fileTargets, directoryTargets))
                    affectedFileCount++;
            }

            // 影响数为 0（选中项实际已不在清单中）时，刷新列表并返回。
            if (affectedFileCount == 0)
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 选中项目已不在待打包列表中，列表已刷新。", LogType.Warning);
                EnsureCurrentPackDirectoryExists();
                RefreshPackListView();
                return;
            }

            // 记录删除前的当前目录，并预判删除后当前目录是否会被清空（即不再含任何后代文件）。
            string currentDirectoryBeforeDelete = NormalizePackInternalPath(currentDirPath);
            bool willRemoveCurrentDirectory =
                !string.IsNullOrEmpty(currentDirectoryBeforeDelete) &&
                !HasRemainingPackFileUnderDirectory(
                    currentDirectoryBeforeDelete,
                    fileTargets,
                    directoryTargets);

            // 场景一：当前文件夹只剩最后一个文件时，删除该文件会让目录本身一并消失。
            // 使用明确警告，避免用户误以为删除后仍会保留一个空目录。
            if (willRemoveCurrentDirectory &&
                targetCount == 1 &&
                fileTargets.Count == 1)
            {
                const string emptyFolderWarning =
                    "警告，PAC里不能有空文件夹，删除该文件夹的最后一个文件将删除当前整个文件夹，确定吗？";

                if (MessageBox.Show(
                    emptyFolderWarning,
                    "警告",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                {
                    return;
                }
            }
            else if (willRemoveCurrentDirectory)
            {
                // 场景二：删除后当前文件夹将被清空（非“最后一个文件”的单一情况，
                // 如删除一个目录导致当前目录无后代），提示当前文件夹也会一并移除。
                string message = string.Format(
                    "警告，PAC里不能有空文件夹，本次删除会使当前文件夹“{0}”中不再包含任何文件，" +
                    "因此当前文件夹也会一并移除。\r\n\r\n确定继续吗？",
                    currentDirectoryBeforeDelete);

                if (MessageBox.Show(
                    message,
                    "警告",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                {
                    return;
                }
            }
            else if (directoryTargets.Count > 0 || targetCount > 1)
            {
                // 场景三：删除目录或多选，仍保留影响范围确认，但不再与“空目录”警告重复弹窗。
                string message;
                if (targetCount == 1 && directoryTargets.Count == 1)
                {
                    string directoryPath = GetFirstPath(directoryTargets);
                    message = string.Format(
                        "确定从待打包列表中移除文件夹“{0}”吗？\r\n\r\n" +
                        "该操作将移除其中 {1} 个待打包文件。\r\n" +
                        "不会删除磁盘上的原始文件。",
                        directoryPath,
                        affectedFileCount);
                }
                else
                {
                    message = string.Format(
                        "确定移除选中的 {0} 个项目吗？\r\n\r\n" +
                        "共影响 {1} 个待打包文件。\r\n" +
                        "不会删除磁盘上的原始文件。",
                        targetCount,
                        affectedFileCount);
                }

                if (MessageBox.Show(
                    message,
                    "移除待打包项目",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                {
                    return;
                }
            }

            // 真正执行移除：从 packListSum 中删除所有命中目标（文件精确匹配 / 目录前缀匹配后代）。
            int removedCount = packListSum.RemoveAll(delegate (Tuple<string, string> entry)
            {
                return IsPackEntryTargetedForRemoval(entry, fileTargets, directoryTargets);
            });

            // 删除最后一个文件后，当前目录可能已不再存在。
            // 自动退回最近仍有文件的祖先目录，绝不让界面停留在一个“空文件夹”中。
            EnsureCurrentPackDirectoryExists();
            // 对比删除前后当前目录是否变化，用于后续日志拼接提示语。
            bool currentDirectoryRemoved = !string.Equals(
                currentDirectoryBeforeDelete,
                NormalizePackInternalPath(currentDirPath),
                StringComparison.OrdinalIgnoreCase);

            RefreshPackListView();

            // 按删除类型输出汇总日志，并附带“当前目录已自动移除/退回”的说明。
            if (targetCount == 1 && fileTargets.Count == 1)
            {
                string suffix = currentDirectoryRemoved
                    ? "；当前文件夹已无文件并自动移除。"
                    : "";
                DrawLog(
                    $"{PublicFunction.GetCurrentTime()} 已从待打包列表移除文件:{GetFirstPath(fileTargets)}{suffix}",
                    LogType.Normal);
            }
            else if (targetCount == 1 && directoryTargets.Count == 1)
            {
                string suffix = currentDirectoryRemoved
                    ? " 当前浏览目录已为空并自动退回上级。"
                    : "";
                DrawLog(
                    $"{PublicFunction.GetCurrentTime()} 已从待打包列表移除目录:{GetFirstPath(directoryTargets)}，共移除 {removedCount} 个文件。{suffix}",
                    LogType.Normal);
            }
            else
            {
                string suffix = currentDirectoryRemoved
                    ? " 当前浏览目录已为空并自动退回上级。"
                    : "";
                DrawLog(
                    $"{PublicFunction.GetCurrentTime()} 已从待打包列表移除 {targetCount} 个选中项目，共移除 {removedCount} 个文件。{suffix}",
                    LogType.Normal);
            }
        }

        /// <summary>
        /// 判断一条待打包映射是否属于本次删除目标。
        /// 文件按完整 PAC 内部路径精确匹配；目录按“目录路径/”前缀匹配全部后代文件。
        /// </summary>
        private static bool IsPackEntryTargetedForRemoval(
            Tuple<string, string> entry,
            HashSet<string> fileTargets,
            HashSet<string> directoryTargets)
        {
            if (entry == null || string.IsNullOrEmpty(entry.Item2))
                return false;

            string internalPath = NormalizePackInternalPath(entry.Item2);
            if (fileTargets.Contains(internalPath))
                return true;

            foreach (string directoryPath in directoryTargets)
            {
                string prefix = NormalizePackInternalPath(directoryPath) + "/";
                if (internalPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 判断执行当前删除操作后，指定 PAC 目录下是否仍至少存在一个后代文件。
        /// 目录存在与否只由其后代文件决定，因此这个判断同时覆盖直接文件和所有子目录。
        /// </summary>
        private bool HasRemainingPackFileUnderDirectory(
            string directoryPath,
            HashSet<string> fileTargets,
            HashSet<string> directoryTargets)
        {
            string normalizedDirectory = NormalizePackInternalPath(directoryPath);
            if (string.IsNullOrEmpty(normalizedDirectory) || packListSum == null)
                return false;

            string prefix = normalizedDirectory + "/";

            foreach (Tuple<string, string> entry in packListSum)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Item2))
                    continue;

                string internalPath = NormalizePackInternalPath(entry.Item2);
                if (!internalPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!IsPackEntryTargetedForRemoval(entry, fileTargets, directoryTargets))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 判断当前 packListSum 中某个 PAC 目录是否仍有至少一个后代文件。
        /// </summary>
        private bool PackDirectoryContainsFile(string directoryPath)
        {
            string normalizedDirectory = NormalizePackInternalPath(directoryPath);
            if (string.IsNullOrEmpty(normalizedDirectory) || packListSum == null)
                return false;

            string prefix = normalizedDirectory + "/";

            foreach (Tuple<string, string> entry in packListSum)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Item2))
                    continue;

                string internalPath = NormalizePackInternalPath(entry.Item2);
                if (internalPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 如果当前正在浏览的打包目录因删除最后一个文件而消失，
        /// 自动退回最近仍然存在的祖先目录；根目录始终作为最终回退位置。
        /// </summary>
        private void EnsureCurrentPackDirectoryExists()
        {
            if (!arePackMode)
                return;

            string candidate = NormalizePackInternalPath(currentDirPath);

            while (!string.IsNullOrEmpty(candidate) && !PackDirectoryContainsFile(candidate))
            {
                candidate = GetParentPackInternalPath(candidate);
            }

            currentDirPath = candidate;
        }

        /// <summary>
        /// &lt;文本型&gt;获取 PAC 内部目录的父路径
        /// <param name="path">(文本型 PAC 内部路径)</param>
        /// <para>一级目录的父路径为根目录空字符串</para>
        /// <returns>成功返回父路径</returns>
        /// </summary>
        private static string GetParentPackInternalPath(string path)
        {
            string normalizedPath = NormalizePackInternalPath(path);
            int slashIndex = normalizedPath.LastIndexOf('/');

            return slashIndex < 0
                ? string.Empty
                : normalizedPath.Substring(0, slashIndex);
        }

        /// <summary>
        /// &lt;文本型&gt;统一 PAC 内部路径格式
        /// <param name="path">(文本型 欲处理的路径)</param>
        /// <para>PAC 内部路径统一使用 '/'，并去掉首尾分隔符，供待打包列表比较使用。</para>
        /// <returns>成功返回处理好的文本</returns>
        /// </summary>
        private static string NormalizePackInternalPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            return path.Replace('\\', '/').Trim('/');
        }


        /// <summary>
        /// &lt;文本型&gt;取首层路径
        /// <param name="paths"></param>
        /// <para>HashSet 仅在日志/确认框需要单项名称时取第一项，避免为了这一处引入 LINQ 依赖</para>
        /// <returns>成功返回首层路径名</returns>
        /// </summary>
        private static string GetFirstPath(HashSet<string> paths)
        {
            foreach (string path in paths)
                return path;
            return string.Empty;
        }

        #endregion

        #region PAC 打包

        /// <summary>
        /// 执行打包操作的主方法
        /// </summary>
        private async void PackPAC()
        {
            // 参考我们之前写的UnPackall函数，打包的细节也要从日志框显示，进度条参照执行，DrawLog输出：正在写入XXX文件，蓝色信息图标。
            // 调用 FPACHelper.Pack.PackPACAsync 的列表版重载：Item1=源文件完整路径，Item2=PAC 内部完整相对路径。

            if (packRunning)
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 正在打包中,请勿重复操作。", LogType.Warning);
                System.Media.SystemSounds.Hand.Play();
                return;
            }

            if (!arePackMode || string.IsNullOrEmpty(currentPACFile))
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 请先新建一个 PAC 文件再执行打包。", LogType.Warning);
                System.Media.SystemSounds.Hand.Play();
                return;
            }

            if (packListSum == null || packListSum.Count == 0)
            {
                DrawLog($"{PublicFunction.GetCurrentTime()}待打包列表为空，请先添加文件或文件夹。", LogType.Warning);
                System.Media.SystemSounds.Hand.Play();
                return;
            }

            // 打包期间锁定相关控件,防止重复启动或在后台打包时修改列表。
            packRunning = true;
            SetPackControlsEnabled(false);

            // 快照:打包期间列表不可再修改,且快照数量与最终写入数量一致。
            List<Tuple<string, string>> packSnapshot = new List<Tuple<string, string>>(packListSum);

            stripStatusLabel_Tips.Text = "正在打包";
            stripProgressBar.Value = 0;
            DrawLog($"{PublicFunction.GetCurrentTime()} 开始打包，共 {packSnapshot.Count} 个文件，目标文件:{Path.GetFileName(currentPACFile)}", LogType.Normal);

            try
            {
                await FPACHelper.Pack.PackPACAsync(
                    packSnapshot,
                    currentPACFile,
                    (progress) =>
                    {
                        // 进度回调来自后台线程,必须通过 Invoke 切回 UI 线程更新控件;
                        // 用同步 Invoke 保证日志输出顺序与打包进度一致。
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action<FPACHelper.FileProgress>(OnPackProgress), progress);
                            return;
                        }
                        OnPackProgress(progress);
                    });

                DrawLog($"{PublicFunction.GetCurrentTime()} 打包完成！共 {packSnapshot.Count} 个文件已写入:{currentPACFile}", LogType.Normal);
                stripStatusLabel_Tips.Text = "打包完成";
                stripProgressBar.Value = 100;
                System.Media.SystemSounds.Beep.Play();

                // 打包完成后询问是否打开输出文件夹。
                DialogResult openFolderResult = MessageBox.Show(
                    "打包完成！是否打开输出文件夹？",
                    "打包完成",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);
                if (openFolderResult == DialogResult.Yes)
                {
                    try
                    {
                        OpenFolderInExplorer(Path.GetDirectoryName(currentPACFile));
                    }
                    catch (Exception openEx)
                    {
                        DrawLog($"{PublicFunction.GetCurrentTime()} 打开输出文件夹失败:{openEx.Message}", LogType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // 整体失败（如文件无法写入等）：输出错误原因并播放错误提示音
                DrawLog($"{PublicFunction.GetCurrentTime()} 打包失败:{ex.Message}", LogType.Error);
                stripStatusLabel_Tips.Text = "发生错误";
                stripProgressBar.Value = 0;
                System.Media.SystemSounds.Hand.Play();
            }
            finally
            {
                packRunning = false;
                SetPackControlsEnabled(true);
            }
        }

        /// <summary>
        /// 在 UI 线程上处理打包进度回调（更新状态栏与日志框）。
        /// 若某个文件写入失败，progress.error 非空，此时输出错误原因但不中断整体任务。
        /// </summary>
        private void OnPackProgress(FPACHelper.FileProgress progress)
        {
            if (!string.IsNullOrEmpty(progress.error))
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} {progress.error}", LogType.Error);
                System.Media.SystemSounds.Hand.Play();
                return;
            }
            int percentage = Math.Max(0, Math.Min(100, progress.percentage));
            // 状态栏只保留进度条（轻量）；完整进度信息以日志框为准。
            stripProgressBar.Value = percentage;
            stripStatusLabel_Tips.Text = progress.state;
            // 打包进度信息（含“正在写入: XXX”）以蓝色信息图标输出到日志框。
            DrawLog($"{PublicFunction.GetCurrentTime()} {progress.state}", LogType.Info);
        }

        #endregion

        #region PAC 解包

        /// <summary>
        /// 解包整个 PAC
        /// <para>将当前 PAC 全部文件提取到用户选择的目录（自动创建“PAC名_unpacked”子文件夹）</para>
        /// </summary>
        private async void UnPackall()
        {
            if (areUnpackMode != true || currentPACFile == null || pacUnPackList == null || pacUnPackList.Count == 0)
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 请先打开一个 PAC 文件再执行提取。", LogType.Warning);
                System.Media.SystemSounds.Hand.Play();
                return;
            }

            // 创建提取PAC到指定路径对话框。
            // 用户选择的是父目录，程序会自动在其中创建“PAC文件名_unpacked”文件夹。
            FolderBrowserDialog unpackDir = new FolderBrowserDialog
            {
                Description = "请选择提取文件的保存位置，程序将自动创建“PAC文件名_unpacked”文件夹",
                ShowNewFolderButton = true,
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            if (unpackDir.ShowDialog() != DialogResult.OK)
                return;

            // “提取全部”始终在用户选择的目录下创建独立输出目录，
            // 避免 PAC 内的大量文件直接散落到用户选择的文件夹中。
            string pacName = Path.GetFileNameWithoutExtension(currentPACFile);
            string savePath = Path.Combine(unpackDir.SelectedPath, pacName + "_unpacked");
            stripStatusLabel_Tips.Text = "正在提取";
            stripProgressBar.Value = 0;
            DrawLog($"{PublicFunction.GetCurrentTime()} 开始提取文件,共 {pacUnPackList.Count} 个,目标路径:{savePath}", LogType.Normal);

            try
            {
                await FPACHelper.UnPack.UnpackAllFileAsync(
                    currentPACFile,
                    pacUnPackList,
                    savePath,
                    (progress) =>
                    {
                        // 进度回调来自后台线程,必须通过 Invoke 切回 UI 线程更新控件
                        if (this.InvokeRequired)
                        {
                            this.BeginInvoke(new Action<FPACHelper.FileProgress>(OnUnpackProgress), progress);
                            return;
                        }
                        OnUnpackProgress(progress);
                    });

                DrawLog($"{PublicFunction.GetCurrentTime()} 提取完成!共 {pacUnPackList.Count} 个文件已提取到:{savePath}", LogType.Normal);
                stripStatusLabel_Tips.Text = "提取完成";
                stripProgressBar.Value = 100;
                System.Media.SystemSounds.Beep.Play();

                // 提取完成后询问是否打开输出文件夹。
                DialogResult openFolderResult = MessageBox.Show(
                    "提取完成！是否打开输出文件夹？",
                    "提取完成",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);
                if (openFolderResult == DialogResult.Yes)
                {
                    try
                    {
                        OpenFolderInExplorer(savePath);
                    }
                    catch (Exception openEx)
                    {
                        DrawLog($"{PublicFunction.GetCurrentTime()} 打开输出文件夹失败:{openEx.Message}", LogType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // 整体失败(如 PAC 文件无法打开等):输出错误原因并播放错误提示音
                DrawLog($"{PublicFunction.GetCurrentTime()} 提取失败:{ex.Message}", LogType.Error);
                stripStatusLabel_Tips.Text = "发生错误";
                stripProgressBar.Value = 0;
                System.Media.SystemSounds.Hand.Play();
            }
        }

        /// <summary>
        /// 在资源管理器中打开指定文件夹。
        /// <para>传入目录路径，使用 explorer.exe 的 /select 参数定位并打开该文件夹。</para>
        /// </summary>
        private void OpenFolderInExplorer(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return;

            // 确保目录存在；不存在则不打开（交由调用方决定是否提示）。
            string fullPath = Path.GetFullPath(folderPath);
            if (!Directory.Exists(fullPath))
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = "\"" + fullPath + "\"",
                UseShellExecute = true
            });
        }

        /// <summary>
        /// 提取当前选中的 PAC 文件
        /// </summary>
        private void UnpackSelectedFile()
        {
            string pacPath;
            int targetIndex;
            if (!TryGetSelectedPacFile(out pacPath, out targetIndex))
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} 请先选择一个文件再执行提取。", LogType.Warning);
                return;
            }

            string defaultFileName = Path.GetFileName(
                pacPath.Replace('/', Path.DirectorySeparatorChar));
            string extension = Path.GetExtension(defaultFileName);

            using (SaveFileDialog saveFile = new SaveFileDialog
            {
                Title = "另存为",
                FileName = defaultFileName,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = string.IsNullOrEmpty(extension)
                    ? "所有文件 (*.*)|*.*"
                    : string.Format("{0} 文件 (*{0})|*{0}|所有文件 (*.*)|*.*", extension),
                FilterIndex = 1,
                AddExtension = false,
                OverwritePrompt = true,
                RestoreDirectory = true
            })
            {
                if (saveFile.ShowDialog() != DialogResult.OK)
                    return;

                string outputPath = Path.GetFullPath(saveFile.FileName);
                string outputDirectory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);

                FPACHelper.UnPack.UnpackSingle(currentPACFile, pacUnPackList, targetIndex, outputPath);
                DrawLog($"{PublicFunction.GetCurrentTime()} 已提取文件:{pacPath},保存为:{outputPath}", LogType.Normal);
                System.Media.SystemSounds.Beep.Play();
            }
        }

        /// <summary>
        /// 在 UI 线程上处理解包进度回调(更新状态栏与日志框)。
        /// 若某个文件提取失败,progress.error 非空,此时输出错误原因但不中断整体任务。
        /// </summary>
        private void OnUnpackProgress(FPACHelper.FileProgress progress)
        {
            if (!string.IsNullOrEmpty(progress.error))
            {
                DrawLog($"{PublicFunction.GetCurrentTime()} {progress.error}", LogType.Error);
                System.Media.SystemSounds.Hand.Play();
                return;
            }
            int percentage = Math.Max(0, Math.Min(100, progress.percentage));
            // 状态栏只保留进度条(轻量);百分比数字已取消,完整进度信息以日志框为准。
            stripProgressBar.Value = percentage;
            stripStatusLabel_Tips.Text = progress.state;
            // 日志框完整输出每个文件的提取状态(progress.state 已包含当前文件名等完整信息)。
            DrawLog($"{PublicFunction.GetCurrentTime()} {progress.state}", LogType.Info);
        }

        #endregion

        #region PAC 关闭

        /// <summary>
        /// 关闭当前 PAC
        /// <para>打包模式二次确认，失效后台浏览任务，重置界面与控件状态</para>
        /// </summary>
        private void ClosePACFile()
        {
            // 打包模式关闭警告
            if (arePackMode)
            {
                DialogResult result = MessageBox.Show("确定要关闭 PAC 文件吗？\n请注意，执行<打包 PAC> 操作前添加的文件和文件夹将不会保存。", "关闭 PAC 文件", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            // 使仍在后台准备/分批写入的浏览任务立即失效，防止关闭后旧任务继续回填列表。
            pacListLoadVersion++;
            ListView_PAC.Enabled = true;

            // ListView.Clear() 会连列一起清空,导致后续无法正常显示文本;这里只清空项目。
            ListView_PAC.Items.Clear();
            ListView_Log.Items.Clear();
            arePackMode = false;
            areUnpackMode = false;
            packRunning = false;
            SetPacListViewMode(false);
            if (packListSum != null)
                packListSum.Clear();
            currentPACFile = null;
            currentDirPath = "";
            this.Text = appTitle;
            DrawLog("文件已关闭", LogType.Info);
            stripStatusLabel_Tips.Text = "准备就绪";
            stripProgressBar.Value = 0;
            stripStatusLabel_Percent.Text = "";
            // “文件”菜单项控制
            MenuItem_File_Open.Enabled = true;
            MenuItem_File_New.Enabled = true;
            MenuItem_File_Close.Enabled = false;
            MenuItem_File_PACProperties.Enabled = false;
            // “命令”菜单项控制
            MenuItem_Command.Enabled = false;
            MenuItem_Command_Pack.Visible = true;
            MenuItem_Command_Add.Visible = true;
            MenuItem_Command_Delete.Visible = true;
            MenuItem_Command_Unpack.Visible = true;
            MenuItem_Command_Add_UnpackSelect.Visible = true;
            // 工具条控制
            ToolStrip_New.Enabled = true;
            ToolStrip_Open.Enabled = true;
            ToolStrip_Close.Enabled = false;
            ToolStrip_Add.Visible = false;
            ToolStrip_Delete.Visible = false;
            ToolStrip_Pack.Visible = false;
            ToolStrip_UnpackSelect.Visible = false;
            ToolStrip_UnpackAll.Visible = false;
            ToolStrip_PACProperties.Visible = false;
            UpdatePackDeleteCommandState();

            ListView_PAC.MultiSelect = false;
        }

        #endregion

        #region 文件预览


        /// <summary>
        /// 预览指定文件
        /// <param name="path">(文本型 文件名)</param>
        /// <para>预览选中的 PAC 文件。DDS 使用内置 DDS 浏览器；其他文件仍交给 Windows 默认程序
        /// 两种路径都先复用现有 UnpackSingle 解包到独立临时目录</para>
        /// <exception cref="InvalidDataException">目标文件不在待解包清单中时抛出。</exception>
        /// </summary>
        private void PreviewFile(string path)
        {
            // 路径为空时直接返回，不预览。
            if (string.IsNullOrEmpty(path))
                return;

            // DDS 文件走内置浏览器预览，单独处理，不进入后面的默认程序流程。
            if (string.Equals(Path.GetExtension(path), ".dds", StringComparison.OrdinalIgnoreCase))
            {
                PreviewDDS(path);
                return;
            }

            // 为本次预览创建独立临时目录（用文件路径生成唯一目录名，避免冲突）。
            string tempRoot = Path.Combine(
                Path.GetTempPath(),
                PublicFunction.GenerateFolderName(path));
            Directory.CreateDirectory(tempRoot);

            // 在待解包清单中定位目标文件的索引（按 PAC 内部路径精确匹配）。
            int targetIndex = pacUnPackList.FindIndex(delegate (Tuple<byte[], string, long, long> entry)
            {
                return string.Equals(entry.Item2, path, StringComparison.Ordinal);
            });
            if (targetIndex < 0)
                throw new InvalidDataException("未找到选中的 PAC 文件。");

            // 预览文件直接放在本次临时目录中，避免把 PAC 内部路径当成系统绝对路径。
            string previewPath = CombinePacPath(tempRoot, Path.GetFileName(
                path.Replace('/', Path.DirectorySeparatorChar)));
            // 将目标单文件从 PAC 解包到临时目录。
            FPACHelper.UnPack.UnpackSingle(currentPACFile, pacUnPackList, targetIndex, previewPath);

            // 非 DDS 文件继续使用当前 Windows 文件关联的默认程序打开。
            Process.Start(new ProcessStartInfo
            {
                FileName = previewPath,
                UseShellExecute = true
            });

            DrawLog($"{PublicFunction.GetCurrentTime()} 已预览文件:{path}", LogType.Normal);
        }

        /// <summary>
        /// 预览 DDS
        /// <para>将 PAC 内的 DDS 单文件释放到专用临时目录，再交给内置 DDSView
        /// DDSView 关闭时负责释放 Bitmap 并清理该临时目录</para>
        /// </summary>
        private void PreviewDDS(string path)
        {
            if (!areUnpackMode || pacUnPackList == null || string.IsNullOrEmpty(currentPACFile))
                throw new InvalidOperationException("当前没有打开可浏览的 PAC 文件。");

            int targetIndex = pacUnPackList.FindIndex(delegate (Tuple<byte[], string, long, long> entry)
            {
                return string.Equals(entry.Item2, path, StringComparison.Ordinal);
            });
            if (targetIndex < 0)
                throw new InvalidDataException("未找到选中的 DDS 文件。");

            string tempRoot = Path.Combine(
                Path.GetTempPath(),
                PublicFunction.GenerateFolderName(path));
            Directory.CreateDirectory(tempRoot);

            string previewPath = CombinePacPath(tempRoot, Path.GetFileName(
                path.Replace('/', Path.DirectorySeparatorChar)));

            try
            {
                FPACHelper.UnPack.UnpackSingle(
                    currentPACFile,
                    pacUnPackList,
                    targetIndex,
                    previewPath);

                Form_DDSView viewer = new Form_DDSView(previewPath, tempRoot);
                viewer.Show(this);

                DrawLog($"{PublicFunction.GetCurrentTime()} 已使用内置 DDS 浏览器打开:{path}", LogType.Normal);
            }
            catch
            {
                // 如果解包、DDS解析或窗口初始化失败，窗口尚未接管临时目录，
                // 因此在这里尽力清理，随后保留原异常交给统一预览错误处理。
                try
                {
                    if (Directory.Exists(tempRoot))
                        Directory.Delete(tempRoot, true);
                }
                catch
                {
                }
                throw;
            }
        }

        #endregion

        #region PAC 属性

        /// <summary>
        /// 显示当前 PAC 文件的基本属性
        /// <para>如：文件名、文件数量、文件大小、修改时间
        /// 仅在解包模式且已打开有效 PAC 时可用，否则静默返回</para>
        /// </summary>
        private void ShowPacProperties()
        {
            // 前置校验：非解包模式、未打开 PAC 或清单未加载时，无属性可显示。
            if (!areUnpackMode || string.IsNullOrEmpty(currentPACFile) || pacUnPackList == null)
                return;

            // 取磁盘上 PAC 文件元信息（名称、长度、修改时间）。
            FileInfo pacInfo = new FileInfo(currentPACFile);
            // 组装属性文本：文件名、文件数量、文件大小（人性化单位）、修改时间。
            string message = string.Format(
                "文件:{0}\n文件数量:{1}\n文件大小:{2}\n修改时间:{3}",
                pacInfo.Name,
                pacUnPackList.Count,
                PublicFunction.BytesToSize(pacInfo.Length),
                pacInfo.LastWriteTime);
            // 以模态对话框展示属性。
            MessageBox.Show(message, "PAC属性", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #endregion

        #region 界面更新

        #region PAC 文件列表

        /// <summary>
        /// 按目录载入列表框
        /// <param name="dirPath">(文本型 欲载入的路径))</param>
        /// <para>按树状模式渲染 PAC 内指定目录下的内容(WinRAR 风格)
        /// 只显示当前目录下的直接子目录和文件;非根目录时首行显示 ".." 用于返回上级</para>
        /// </summary>
        private async void LoadListViewByDirectory(string dirPath)
        {
            string targetDirPath = dirPath ?? "";
            List<Tuple<byte[], string, long, long>> sourceList = pacUnPackList;
            if (sourceList == null)
            {
                ListView_PAC.Items.Clear();
                return;
            }

            int loadVersion = ++pacListLoadVersion;
            currentDirPath = targetDirPath;

            // 先立即清空旧目录，并暂时禁止操作不完整的列表。
            // 后台只做筛选/排序/文本准备；WinForms 控件仍只在 UI 线程访问。
            ListView_PAC.Enabled = false;
            ToolStrip_UnpackSelect.Enabled = false;
            ToolStrip_UnpackAll.Enabled = false;
            MenuItem_Command_Add_UnpackSelect.Enabled = false;
            MenuItem_Command_Unpack.Enabled = false;
            ListView_PAC.Items.Clear();
            stripProgressBar.Style = ProgressBarStyle.Continuous;
            stripProgressBar.Minimum = 0;
            stripProgressBar.Maximum = 100;
            stripProgressBar.Value = 0;
            stripStatusLabel_Percent.Text = "0%";
            stripStatusLabel_Tips.Text = "正在分析目录...";

            try
            {
                IProgress<int> preparationProgress = new Progress<int>(delegate (int value)
                {
                    if (loadVersion != pacListLoadVersion || IsDisposed || Disposing)
                        return;

                    int percentage = Math.Max(0, Math.Min(45, value));
                    stripProgressBar.Value = percentage;
                    stripStatusLabel_Percent.Text = percentage + "%";

                    if (percentage < 22)
                        stripStatusLabel_Tips.Text = "正在分析目录...";
                    else if (percentage < 35)
                        stripStatusLabel_Tips.Text = "正在排序目录内容...";
                    else
                        stripStatusLabel_Tips.Text = "正在准备文件信息...";
                });

                List<PacBrowserRow> rows = await Task.Run(delegate
                {
                    return BuildPacBrowserRows(sourceList, targetDirPath, preparationProgress);
                });

                if (loadVersion != pacListLoadVersion || !areUnpackMode || IsDisposed || Disposing)
                    return;

                bool hasIcon = ListView_PAC.SmallImageList != null;
                int total = rows.Count;
                int loaded = 0;

                while (loaded < total)
                {
                    if (loadVersion != pacListLoadVersion || !areUnpackMode || IsDisposed || Disposing)
                        return;

                    int count = Math.Min(PacListLoadBatchSize, total - loaded);
                    ListViewItem[] batch = new ListViewItem[count];

                    for (int i = 0; i < count; i++)
                    {
                        PacBrowserRow row = rows[loaded + i];
                        var item = new ListViewItem(row.Text);
                        item.SubItems.Add(row.Offset ?? "");
                        item.SubItems.Add(row.Size ?? "");
                        item.SubItems.Add(row.Signature ?? "");
                        item.Tag = row.Tag;
                        if (!string.IsNullOrEmpty(row.Name))
                            item.Name = row.Name;

                        if (hasIcon)
                        {
                            if (row.IconKind == 0)
                                item.ImageIndex = 0;
                            else if (row.IconKind == 1)
                                item.ImageIndex = 1;
                            else
                                item.ImageIndex = GetFileIconIndex(row.Text);
                        }

                        batch[i] = item;
                    }

                    // 每批只触发一次 ListView 重绘，避免上万次逐项刷新；
                    // 批次之间主动把控制权还给消息循环，让窗口持续可响应。
                    ListView_PAC.BeginUpdate();
                    try
                    {
                        ListView_PAC.Items.AddRange(batch);
                    }
                    finally
                    {
                        ListView_PAC.EndUpdate();
                    }

                    loaded += count;
                    int percentage = total == 0
                        ? 100
                        : 45 + (int)((long)loaded * 55L / total);
                    percentage = Math.Max(45, Math.Min(100, percentage));
                    stripProgressBar.Value = percentage;
                    stripStatusLabel_Percent.Text = percentage + "%";
                    stripStatusLabel_Tips.Text = string.Format(
                        "正在载入文件列表... {0}/{1}", loaded, total);

                    await Task.Yield();
                }

                if (total == 0)
                {
                    stripProgressBar.Value = 100;
                    stripStatusLabel_Percent.Text = "100%";
                }

                stripStatusLabel_Tips.Text = string.Format("目录载入完成，共 {0} 项", total);
                ListView_PAC.Enabled = true;
                ToolStrip_UnpackSelect.Enabled = true;
                ToolStrip_UnpackAll.Enabled = true;
                MenuItem_Command_Add_UnpackSelect.Enabled = true;
                MenuItem_Command_Unpack.Enabled = true;

                // 短暂保留 100% 让用户看到完成状态，然后恢复到普通浏览状态。
                await Task.Delay(350);
                if (loadVersion == pacListLoadVersion && areUnpackMode && !IsDisposed && !Disposing)
                {
                    stripProgressBar.Value = 0;
                    stripStatusLabel_Percent.Text = "";
                    stripStatusLabel_Tips.Text = string.IsNullOrEmpty(currentPACFile)
                        ? "准备就绪"
                        : string.Format("已打开文件: {0}", Path.GetFileName(currentPACFile));
                }
            }
            catch (Exception ex)
            {
                if (loadVersion == pacListLoadVersion && !IsDisposed && !Disposing)
                {
                    stripProgressBar.Value = 0;
                    stripStatusLabel_Percent.Text = "";
                    stripStatusLabel_Tips.Text = "目录载入失败";
                    DrawLog(string.Format("{0} 文件浏览框载入失败:{1}",
                        PublicFunction.GetCurrentTime(), ex.Message), LogType.Error);
                    System.Media.SystemSounds.Hand.Play();
                }
            }
            finally
            {
                // 只有当前任务有权恢复列表；旧任务不能打断较新的载入状态。
                if (loadVersion == pacListLoadVersion && !IsDisposed && !Disposing)
                {
                    ListView_PAC.Enabled = true;
                    bool enableUnpackCommands = areUnpackMode;
                    ToolStrip_UnpackSelect.Enabled = enableUnpackCommands;
                    ToolStrip_UnpackAll.Enabled = enableUnpackCommands;
                    MenuItem_Command_Add_UnpackSelect.Enabled = enableUnpackCommands;
                    MenuItem_Command_Unpack.Enabled = enableUnpackCommands;
                }
            }
        }

        /// <summary>
        /// 在后台线程中筛选、排序并准备当前 PAC 目录的显示数据。
        /// 此方法不访问任何 WinForms 控件，因此可以安全地由 Task.Run 调用。
        /// </summary>
        private List<PacBrowserRow> BuildPacBrowserRows(
            List<Tuple<byte[], string, long, long>> sourceList,
            string dirPath,
            IProgress<int> progress)
        {
            var dirSet = new HashSet<string>();
            var files = new List<Tuple<byte[], string, long, long>>();
            string prefix = string.IsNullOrEmpty(dirPath) ? "" : dirPath + "/";
            int sourceCount = sourceList.Count;

            for (int i = 0; i < sourceCount; i++)
            {
                Tuple<byte[], string, long, long> entry = sourceList[i];
                if (entry != null && !string.IsNullOrEmpty(entry.Item2))
                {
                    string name = entry.Item2;
                    if (name.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        string remainder = name.Substring(prefix.Length);
                        int slashIdx = remainder.IndexOf('/');
                        if (slashIdx < 0)
                            files.Add(entry);
                        else
                            dirSet.Add(remainder.Substring(0, slashIdx));
                    }
                }

                // 不要每个文件都跨线程回报一次，避免进度通知本身成为新的性能瓶颈。
                // 即使当前条目不属于目标目录，也要推进扫描进度。
                if (progress != null && (i % 256 == 0 || i == sourceCount - 1))
                {
                    int percentage = sourceCount == 0
                        ? 20
                        : (int)((long)(i + 1) * 20L / sourceCount);
                    progress.Report(Math.Min(20, percentage));
                }
            }

            if (progress != null)
                progress.Report(22);

            var subDirs = new List<string>(dirSet);
            subDirs.Sort(delegate (string left, string right)
            {
                int result = FPACHelper.NaturalStringComparer.Compare(left, right);
                return result != 0
                    ? result
                    : string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
            });

            if (progress != null)
                progress.Report(28);

            files.Sort(delegate (Tuple<byte[], string, long, long> left,
                Tuple<byte[], string, long, long> right)
            {
                string leftName = Path.GetFileName(left.Item2.Replace('/', Path.DirectorySeparatorChar));
                string rightName = Path.GetFileName(right.Item2.Replace('/', Path.DirectorySeparatorChar));
                int result = FPACHelper.NaturalStringComparer.Compare(leftName, rightName);
                return result != 0
                    ? result
                    : string.Compare(leftName, rightName, StringComparison.OrdinalIgnoreCase);
            });

            if (progress != null)
                progress.Report(35);

            int capacity = subDirs.Count + files.Count + (string.IsNullOrEmpty(dirPath) ? 0 : 1);
            var rows = new List<PacBrowserRow>(capacity);

            if (!string.IsNullOrEmpty(dirPath))
            {
                rows.Add(new PacBrowserRow
                {
                    Text = "..",
                    Offset = "",
                    Size = "<DIR>",
                    Signature = "",
                    Tag = "up",
                    IconKind = 1
                });
            }

            foreach (string dirName in subDirs)
            {
                rows.Add(new PacBrowserRow
                {
                    Text = dirName,
                    Offset = "",
                    Size = "<DIR>",
                    Signature = "",
                    Tag = "dir",
                    IconKind = 0
                });
            }

            int fileCount = files.Count;
            for (int i = 0; i < fileCount; i++)
            {
                Tuple<byte[], string, long, long> file = files[i];
                string displayName = Path.GetFileName(
                    file.Item2.Replace('/', Path.DirectorySeparatorChar));

                rows.Add(new PacBrowserRow
                {
                    Text = displayName,
                    Offset = "0x" + file.Item3.ToString("X8"),
                    Size = PublicFunction.BytesToSize(file.Item4),
                    Signature = PublicFunction.LittleEndianToHexString(file.Item1),
                    Tag = "file",
                    Name = file.Item2,
                    IconKind = 2
                });

                if (progress != null && (i % 256 == 0 || i == fileCount - 1))
                {
                    int percentage = fileCount == 0
                        ? 45
                        : 35 + (int)((long)(i + 1) * 10L / fileCount);
                    progress.Report(Math.Min(45, percentage));
                }
            }

            if (progress != null)
                progress.Report(45);

            return rows;
        }

        /// <summary>
        /// 刷新 PAC 列表框显示
        /// <para>打包模式下按 PAC 内部路径渲染当前目录层级
        /// 只显示当前目录下的直接子目录和文件,双击目录后进入下一层</para>
        /// </summary>
        private void RefreshPackListView()
        {
            // ListView.BeginUpdate/EndUpdate 只在 UI 线程调用，避免后台线程访问控件。
            ListView_PAC.BeginUpdate();
            try
            {
                // 清空旧目录，重新渲染当前目录下的内容。
                ListView_PAC.Items.Clear();
                if (packListSum == null)
                    return;

                // SmallImageList 可能为 null，避免访问时抛出异常。
                bool hasIcon = ListView_PAC.SmallImageList != null;

                // 非根目录时,第一行插入 ".." 返回上级。
                if (!string.IsNullOrEmpty(currentDirPath))
                {
                    var upItem = new ListViewItem("..");
                    upItem.SubItems.Add("");
                    upItem.SubItems.Add("<DIR>");
                    upItem.SubItems.Add("");
                    upItem.Tag = "pack-up";
                    if (hasIcon)
                        upItem.ImageIndex = 1;
                    ListView_PAC.Items.Add(upItem);
                }

                var dirSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var files = new List<Tuple<string, string>>();
                string prefix = string.IsNullOrEmpty(currentDirPath) ? "" : currentDirPath + "/";

                foreach (Tuple<string, string> entry in packListSum)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.Item2))
                        continue;

                    string internalPath = entry.Item2.Replace('\\', '/').TrimStart('/');
                    if (!internalPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        continue;

                    string remainder = internalPath.Substring(prefix.Length);
                    if (string.IsNullOrEmpty(remainder))
                        continue;

                    int slashIndex = remainder.IndexOf('/');
                    if (slashIndex >= 0)
                    {
                        dirSet.Add(remainder.Substring(0, slashIndex));
                    }
                    else
                    {
                        files.Add(new Tuple<string, string>(entry.Item1, internalPath));
                    }
                }

                var subDirs = new List<string>(dirSet);
                subDirs.Sort(delegate (string left, string right)
                {
                    int result = FPACHelper.NaturalStringComparer.Compare(left, right);
                    return result != 0
                        ? result
                        : string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
                });

                foreach (string dirName in subDirs)
                {
                    var item = new ListViewItem(dirName);
                    item.SubItems.Add("");
                    item.SubItems.Add("<DIR>");
                    item.SubItems.Add("");
                    item.Tag = "pack-dir";
                    item.Name = string.IsNullOrEmpty(currentDirPath)
                        ? dirName
                        : currentDirPath + "/" + dirName;
                    if (hasIcon)
                        item.ImageIndex = 0;
                    ListView_PAC.Items.Add(item);
                }

                files.Sort(delegate (Tuple<string, string> left, Tuple<string, string> right)
                {
                    int result = FPACHelper.NaturalStringComparer.Compare(left.Item2, right.Item2);
                    return result != 0
                        ? result
                        : string.Compare(left.Item2, right.Item2, StringComparison.OrdinalIgnoreCase);
                });

                foreach (Tuple<string, string> entry in files)
                {
                    string internalPath = entry.Item2;
                    string sourcePath = null;
                    string sizeText = "";

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(entry.Item1))
                            sourcePath = Path.GetFullPath(entry.Item1);
                    }
                    catch
                    {
                        sourcePath = null;
                    }

                    if (sourcePath != null && File.Exists(sourcePath))
                    {
                        try
                        {
                            sizeText = PublicFunction.BytesToSize(new FileInfo(sourcePath).Length);
                        }
                        catch
                        {
                            sizeText = "";
                        }
                    }

                    string displayName = Path.GetFileName(internalPath.Replace('/', Path.DirectorySeparatorChar));
                    var item = new ListViewItem(displayName);
                    item.SubItems.Add(sourcePath ?? ""); // 所在路径
                    item.SubItems.Add(sizeText);        // 大小
                    item.SubItems.Add("");              // 签名值
                    item.Tag = "pack-file";
                    item.Name = internalPath;
                    if (hasIcon)
                        item.ImageIndex = GetFileIconIndex(displayName);
                    ListView_PAC.Items.Add(item);
                }
            }
            finally
            {
                ListView_PAC.EndUpdate();
                UpdatePackDeleteCommandState();
            }
        }

        #endregion

        #region 界面模式与控件状态

        /// <summary>
        /// 置换当前浏览模式
        /// <param name="packMode">(逻辑型 是否打包模式)</param>
        /// 根据当前浏览模式设置 PAC 列表第二列的含义。
        /// 解包模式显示 PAC 文件偏移,打包模式显示源文件所在路径。
        /// </summary>
        private void SetPacListViewMode(bool packMode)
        {
            ColumnHeader_Offset.Text = packMode ? "所在路径" : "偏移";
        }

        /// <summary>
        /// 打包模式下启用/禁用会修改待打包状态或启动打包的控件。
        /// “删除”是否可用还取决于当前是否选中了可删除项目，因此单独由 UpdatePackDeleteCommandState 计算。
        /// </summary>
        private void SetPackControlsEnabled(bool enabled)
        {
            ToolStrip_New.Enabled = enabled;
            ToolStrip_Add.Enabled = enabled;
            ToolStrip_Add_File.Enabled = enabled;
            ToolStrip_Add_Folder.Enabled = enabled;
            ToolStrip_Pack.Enabled = enabled;
            contextMenu_Pack_PACPack.Enabled = enabled;
            contextMenu_Pack_AddFile.Enabled = enabled;
            contextMenu_Pack_AddFolder.Enabled = enabled;

            UpdatePackDeleteCommandState();
        }

        /// <summary>
        /// 判断当前是否选中了打包模式下可移除的文件或目录。
        /// “..”上级目录等导航项不会被当成删除目标。
        /// </summary>
        private bool HasDeletablePackSelection()
        {
            foreach (ListViewItem item in ListView_PAC.SelectedItems)
            {
                string tag = item.Tag as string;
                if (string.Equals(tag, "pack-file", StringComparison.Ordinal) ||
                    string.Equals(tag, "pack-dir", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 同步工具栏、命令菜单和右键菜单中的删除命令状态。
        /// </summary>
        private void UpdatePackDeleteCommandState()
        {
            bool canDelete = arePackMode &&
                !packRunning &&
                packListSum != null &&
                packListSum.Count > 0 &&
                HasDeletablePackSelection();

            ToolStrip_Delete.Enabled = canDelete;
            MenuItem_Command_Delete.Enabled = canDelete;
            contextMenu_Pack_Delete.Enabled = canDelete;
        }

        #endregion

        #region 文件图标

        /// <summary>
        /// &lt;整数型&gt;取文件图标索引值
        /// <param name="fileName">(文本型 欲判断的文件名)</param>
        /// <para>根据文件名获取其在图标列表中的索引(按扩展名匹配系统关联图标)。
        /// 首次遇到某扩展名时,用 SHGetFileInfo 提取系统图标并加入 pacImageList 缓存。
        /// 提取失败或无扩展名时,返回系统默认"白纸"图标(失败则回退到文件夹图标索引 0 之外,
        /// 实际以 0 号文件夹图标兜底,避免越界)。</para>
        /// <returns>成功返回图标索引值</returns>
        /// </summary>
        private int GetFileIconIndex(string fileName)
        {
            // 前置条件:未初始化图标列表时,直接返回 0 号文件夹图标索引。
            if (pacImageList == null)
                return 0;

            string ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext))
                ext = "";
            else
                ext = ext.ToLowerInvariant();

            // 已缓存:直接返回索引
            int idx;
            if (fileIconIndex.TryGetValue(ext, out idx))
                return idx;

            // 未缓存:提取系统关联图标(未关联的扩展名系统会自动返回白纸图标)
            Image img = ToolStripIconShow.GetFileTypeIcon(ext, large: false);
            if (img == null)
            {
                // 提取失败:缓存为文件夹图标(0),避免重复尝试
                fileIconIndex[ext] = 0;
                return 0;
            }

            pacImageList.Images.Add(img);
            idx = pacImageList.Images.Count - 1;
            fileIconIndex[ext] = idx;

            // 注意:不能 Dispose img,ImageList 拥有其生命周期(同 SetuppacUnPackListViewIcons 的说明)。
            return idx;
        }

        #endregion

        #endregion

        #region 日志功能

        /// <summary>
        /// 创建统一风格的日志图标
        /// 返回的 Bitmap 由 ImageList 接管生命周期,调用方勿 Dispose。
        /// </summary>
        private static Bitmap CreateLogIcon(LogType type)
        {
            const int size = 16;
            const float center = size / 2f;
            Bitmap bmp = new Bitmap(size, size);

            Color iconColor;
            switch (type)
            {
                case LogType.Info:
                    iconColor = ColorTranslator.FromHtml("#0969DA");
                    break;
                case LogType.Normal:
                    iconColor = ColorTranslator.FromHtml("#1A7F37");
                    break;
                case LogType.Warning:
                    iconColor = ColorTranslator.FromHtml("#9A6700");
                    break;
                default:
                    iconColor = ColorTranslator.FromHtml("#CF222E");
                    break;
            }

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (var brush = new SolidBrush(iconColor))
                using (var pen = new Pen(Color.White, 1.8f))
                {
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                    switch (type)
                    {
                        case LogType.Warning:
                            // 黄色三角形
                            PointF[] triangle =
                            {
                                new PointF(center, 1.2f),
                                new PointF(14.6f, 14.2f),
                                new PointF(1.4f, 14.2f)
                            };
                            g.FillPolygon(brush, triangle);
                            g.DrawLine(pen, center, 5f, center, 9.5f);
                            g.FillEllipse(Brushes.White, center - 0.9f, 11f, 1.8f, 1.8f);
                            break;

                        default:
                            // 信息、成功、错误使用圆形底图
                            g.FillEllipse(brush, 1f, 1f, size - 2f, size - 2f);
                            if (type == LogType.Info)
                            {
                                // 信息:白色小写 i
                                g.FillEllipse(Brushes.White, center - 0.9f, 4f, 1.8f, 1.8f);
                                g.DrawLine(pen, center, 7.2f, center, 12f);
                            }
                            else if (type == LogType.Normal)
                            {
                                // 成功:白色对勾
                                g.DrawLine(pen, 4.2f, 8.2f, 7f, 11f);
                                g.DrawLine(pen, 7f, 11f, 11.8f, 5.2f);
                            }
                            else
                            {
                                // 错误:白色叉号
                                g.DrawLine(pen, 4.8f, 4.8f, 11.2f, 11.2f);
                                g.DrawLine(pen, 11.2f, 4.8f, 4.8f, 11.2f);
                            }
                            break;
                    }
                }
            }

            return bmp;
        }

        /// <summary>
        /// 绘制日志信息
        /// <param name="text">(文本型 欲绘制的日志信息, </param>
        /// <param name="type">LogType 欲绘制的日志类型)</param>
        /// <para>向日志框追加一条带颜色和图标的消息，并自动滚动到底部</para>
        /// </summary>
        private void DrawLog(string text, LogType type)
        {
            // 创建列表项
            ListViewItem item = new ListViewItem(text);

            // 根据类型设置颜色和图标索引
            // 配色参考 GitHub Primer subtle 色阶:浅 tint 背景 + 同色系深一档文字,柔和且不刺眼
            switch (type)
            {
                case LogType.Info:    // 信息 - 蓝
                    item.BackColor = ColorTranslator.FromHtml("#DDF4FF");
                    item.ForeColor = ColorTranslator.FromHtml("#0969DA");
                    item.ImageIndex = 0;
                    break;
                case LogType.Normal:  // 正常/成功 - 绿
                    item.BackColor = ColorTranslator.FromHtml("#DAFBE1");
                    item.ForeColor = ColorTranslator.FromHtml("#1A7F37");
                    item.ImageIndex = 1;
                    break;
                case LogType.Warning: // 警告 - 琥珀
                    item.BackColor = ColorTranslator.FromHtml("#FFF8C5");
                    item.ForeColor = ColorTranslator.FromHtml("#9A6700");
                    item.ImageIndex = 2;
                    break;
                case LogType.Error:   // 错误 - 红
                    item.BackColor = ColorTranslator.FromHtml("#FFEBE9");
                    item.ForeColor = ColorTranslator.FromHtml("#CF222E");
                    item.ImageIndex = 3;
                    break;
            }

            // 添加并自动滚动到底部
            ListView_Log.Items.Add(item);
            ListView_Log.EnsureVisible(item.Index);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 计算 filePath 相对于 baseDirectory 的相对路径,统一用 "/" 作为分隔符。
        /// </summary>
        private static string GetRelativeToBase(string baseDirectory, string filePath)
        {
            string baseFull = Path.GetFullPath(baseDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string fileFull = Path.GetFullPath(filePath);
            if (!fileFull.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("文件不在所选文件夹内。");
            string relative = fileFull.Substring(baseFull.Length);
            return relative.Replace('\\', '/').Replace(Path.AltDirectorySeparatorChar, '/');
        }

        /// <summary>
        /// 获取当前选中的 PAC 文件及其在 pacUnPackList 中的索引。
        /// 目录项、上级目录项和空白区域均不视为可解包文件。
        /// </summary>
        private bool TryGetSelectedPacFile(out string pacPath, out int targetIndex)
        {
            pacPath = null;
            targetIndex = -1;

            if (!areUnpackMode || pacUnPackList == null || ListView_PAC.SelectedItems.Count == 0)
                return false;

            ListViewItem selectedItem = ListView_PAC.SelectedItems[0];
            if (!string.Equals(selectedItem.Tag as string, "file", StringComparison.Ordinal))
                return false;

            string selectedPacPath = selectedItem.Name;
            if (string.IsNullOrEmpty(selectedPacPath))
                return false;

            pacPath = selectedPacPath;
            targetIndex = pacUnPackList.FindIndex(delegate (Tuple<byte[], string, long, long> entry)
            {
                return string.Equals(entry.Item2, selectedPacPath, StringComparison.Ordinal);
            });

            return targetIndex >= 0;
        }

        /// <summary>
        /// 将 PAC 内部路径安全地映射到指定目录。
        /// </summary>
        private static string CombinePacPath(string rootPath, string pacPath)
        {
            if (string.IsNullOrEmpty(rootPath) || string.IsNullOrEmpty(pacPath))
                throw new ArgumentException("PAC路径不能为空。");

            string normalizedPath = pacPath.Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
            string fullRootPath = Path.GetFullPath(rootPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string fullFilePath = Path.GetFullPath(Path.Combine(fullRootPath, normalizedPath));

            if (!fullFilePath.StartsWith(fullRootPath, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("PAC文件内部路径无效,已拒绝写出到目标目录之外。");

            return fullFilePath;
        }

        #endregion

        #region 帮助

        /// <summary>
        /// 显示“关于”窗体
        /// </summary>
        private void ShowAbout()
        {
            Form_About Form_About = new Form_About();
            Form_About.ShowDialog();
        }

        /// <summary>
        /// 显示“帮助”窗体
        /// </summary>
        private void ShowHelp()
        {
            Form_Help Form_Help = new Form_Help();
            Form_Help.Show();
        }

        /// <summary>
        /// 显示“更新日志”窗体
        /// </summary>
        private void ShowChangeLog()
        {
            Form_ChangeLog Form_ChangeLog = new Form_ChangeLog();
            Form_ChangeLog.ShowDialog();
        }

        /// <summary>
        /// 显示“许可协议”窗体
        /// </summary>
        private void ShowLicense()
        {
            Form_License Form_License = new Form_License();
            Form_License.ShowDialog();
        }

        /// <summary>
        /// 显示“更新日志”窗体（与 ShowChangeLog 重复，仅大小写不同）
        /// </summary>
        private void ShowChangeLOg()
        {
            Form_ChangeLog Form_ChangeLog = new Form_ChangeLog();
            Form_ChangeLog.ShowDialog();
        }

        #endregion

        #region 日志框右键菜单

        /// <summary>
        /// 复制选中日志响应函数
        /// </summary>
        private void contextMenu_Log_CopySelect_Click(object sender, EventArgs e)
        {
            // 复制日志框单行
            if (ListView_Log.SelectedItems.Count > 0)
            {
                Clipboard.SetText(ListView_Log.SelectedItems[0].Text);
            }
        }

        /// <summary>
        /// 复制全部日志响应函数
        /// </summary>
        private void contextMenu_Log_CopyAll_Click(object sender, EventArgs e)
        {
            //复制全部
            if (ListView_Log.Items.Count == 0)
                return;

            var sb = new System.Text.StringBuilder();
            foreach (ListViewItem item in ListView_Log.Items)
            {
                sb.AppendLine(item.Text);
            }
            Clipboard.SetText(sb.ToString());
        }

        /// <summary>
        /// 保存日志到文件响应函数
        /// </summary>
        private void contextMenu_Log_SaveAs_Click(object sender, EventArgs e)
        {
            // 设置一个文件保存为对话框，输出.log文件
            if (ListView_Log.Items.Count == 0)
                return;

            using (SaveFileDialog dlg = new SaveFileDialog
            {
                Title = "你想把操作记录保存在哪里？",
                FileName = $"FPACToolLog_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}.log",
                Filter = "日志文件 (*.log)|*.log|文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
                FilterIndex = 1,
                DefaultExt = "log",
                AddExtension = true,
                OverwritePrompt = true,
                RestoreDirectory = true
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                var sb = new System.Text.StringBuilder();
                foreach (ListViewItem item in ListView_Log.Items)
                {
                    sb.AppendLine(item.Text);
                }

                try
                {
                    System.IO.File.WriteAllText(dlg.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                    MessageBox.Show($"日志已保存至：{dlg.FileName} !", "已保存日志", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("保存日志失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

    }
}
