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
            this.components = new System.ComponentModel.Container();
            this.MenuStrip = new System.Windows.Forms.MenuStrip();
            this.MenuStrip_File = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_File_Open = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_File_New = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_File_Close = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_File_Delimiter01 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_File_PACProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_File_Delimiter02 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_File_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_Command = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_Command_Pack = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_Command_Add = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_Command_Add_File = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_Command_Add_Folder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_Command_Delete = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_Command_Add_UnpackSelect = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_Command_Unpack = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_Help = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_Help_Main = new System.Windows.Forms.ToolStripMenuItem();
            this.更新日志LToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_Help_Delimiter01 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_Help_About = new System.Windows.Forms.ToolStripMenuItem();
            this.SplitContainer = new System.Windows.Forms.SplitContainer();
            this.ToolStrip = new System.Windows.Forms.ToolStrip();
            this.ToolStrip_New = new System.Windows.Forms.ToolStripButton();
            this.ToolStrip_Open = new System.Windows.Forms.ToolStripButton();
            this.ToolStrip_Close = new System.Windows.Forms.ToolStripButton();
            this.ToolStrip_Add = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStrip_Add_File = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStrip_Add_Folder = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStrip_Delete = new System.Windows.Forms.ToolStripButton();
            this.ToolStrip_Pack = new System.Windows.Forms.ToolStripButton();
            this.ToolStrip_UnpackSelect = new System.Windows.Forms.ToolStripButton();
            this.ToolStrip_UnpackAll = new System.Windows.Forms.ToolStripButton();
            this.ToolStrip_PACProperties = new System.Windows.Forms.ToolStripButton();
            this.ToolStrip_Help = new System.Windows.Forms.ToolStripButton();
            this.ToolStrip_About = new System.Windows.Forms.ToolStripButton();
            this.ListView_PAC = new System.Windows.Forms.ListView();
            this.ColumnHeader_FileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader_Offset = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader_Size = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader_Signature = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ListView_Log = new System.Windows.Forms.ListView();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.stripStatusLabel_Tips = new System.Windows.Forms.ToolStripStatusLabel();
            this.stripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.stripStatusLabel_Percent = new System.Windows.Forms.ToolStripStatusLabel();
            this.contextMenu_Unpack = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextMenu_Unpack_Preview = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenu_Unpack_Single = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenu_Unpack_All = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenu_Unpack_Delimiter01 = new System.Windows.Forms.ToolStripSeparator();
            this.contextMenu_Unpack_PACProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenu_Pack = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextMenu_Pack_AddFile = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenu_Pack_AddFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenu_Pack_Delimiter01 = new System.Windows.Forms.ToolStripSeparator();
            this.contextMenu_Pack_Delete = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenu_Pack_Delimiter02 = new System.Windows.Forms.ToolStripSeparator();
            this.contextMenu_Pack_PACPack = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenu_Log = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextMenu_Log_CopySelect = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenu_Log_CopyAll = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenu_Log__Delimiter01 = new System.Windows.Forms.ToolStripSeparator();
            this.contextMenu_Log_SaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
            this.SplitContainer.Panel1.SuspendLayout();
            this.SplitContainer.Panel2.SuspendLayout();
            this.SplitContainer.SuspendLayout();
            this.ToolStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.contextMenu_Unpack.SuspendLayout();
            this.contextMenu_Pack.SuspendLayout();
            this.contextMenu_Log.SuspendLayout();
            this.SuspendLayout();
            // 
            // MenuStrip
            // 
            this.MenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.MenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuStrip_File,
            this.MenuItem_Command,
            this.MenuItem_Help});
            this.MenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MenuStrip.Name = "MenuStrip";
            this.MenuStrip.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.MenuStrip.Size = new System.Drawing.Size(800, 28);
            this.MenuStrip.TabIndex = 0;
            this.MenuStrip.Text = "menuStrip1";
            // 
            // MenuStrip_File
            // 
            this.MenuStrip_File.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_File_Open,
            this.MenuItem_File_New,
            this.MenuItem_File_Close,
            this.MenuItem_File_Delimiter01,
            this.MenuItem_File_PACProperties,
            this.MenuItem_File_Delimiter02,
            this.MenuItem_File_Exit});
            this.MenuStrip_File.Name = "MenuStrip_File";
            this.MenuStrip_File.Size = new System.Drawing.Size(71, 24);
            this.MenuStrip_File.Text = "文件(&F)";
            // 
            // MenuItem_File_Open
            // 
            this.MenuItem_File_Open.Image = global::FPACTool.Properties.Resources.img_ToolStrip_Open;
            this.MenuItem_File_Open.Name = "MenuItem_File_Open";
            this.MenuItem_File_Open.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.MenuItem_File_Open.Size = new System.Drawing.Size(233, 26);
            this.MenuItem_File_Open.Text = "打开(&O)...";
            this.MenuItem_File_Open.Click += new System.EventHandler(this.MenuItem_File_Open_Click);
            // 
            // MenuItem_File_New
            // 
            this.MenuItem_File_New.Image = global::FPACTool.Properties.Resources.img_ToolStrip_New;
            this.MenuItem_File_New.Name = "MenuItem_File_New";
            this.MenuItem_File_New.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.MenuItem_File_New.Size = new System.Drawing.Size(233, 26);
            this.MenuItem_File_New.Text = "新建(&N)";
            this.MenuItem_File_New.Click += new System.EventHandler(this.MenuItem_File_New_Click);
            // 
            // MenuItem_File_Close
            // 
            this.MenuItem_File_Close.Image = global::FPACTool.Properties.Resources.img_ToolStrip_Close;
            this.MenuItem_File_Close.Name = "MenuItem_File_Close";
            this.MenuItem_File_Close.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.MenuItem_File_Close.Size = new System.Drawing.Size(233, 26);
            this.MenuItem_File_Close.Text = "关闭文件(&C)";
            this.MenuItem_File_Close.Click += new System.EventHandler(this.MenuItem_File_Close_Click);
            // 
            // MenuItem_File_Delimiter01
            // 
            this.MenuItem_File_Delimiter01.Name = "MenuItem_File_Delimiter01";
            this.MenuItem_File_Delimiter01.Size = new System.Drawing.Size(230, 6);
            // 
            // MenuItem_File_PACProperties
            // 
            this.MenuItem_File_PACProperties.Image = global::FPACTool.Properties.Resources.img_ToolStrip_PACProperties;
            this.MenuItem_File_PACProperties.Name = "MenuItem_File_PACProperties";
            this.MenuItem_File_PACProperties.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.MenuItem_File_PACProperties.Size = new System.Drawing.Size(233, 26);
            this.MenuItem_File_PACProperties.Text = "PAC属性(&P)";
            this.MenuItem_File_PACProperties.Click += new System.EventHandler(this.MenuItem_File_PACProperties_Click);
            // 
            // MenuItem_File_Delimiter02
            // 
            this.MenuItem_File_Delimiter02.Name = "MenuItem_File_Delimiter02";
            this.MenuItem_File_Delimiter02.Size = new System.Drawing.Size(230, 6);
            // 
            // MenuItem_File_Exit
            // 
            this.MenuItem_File_Exit.Name = "MenuItem_File_Exit";
            this.MenuItem_File_Exit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.MenuItem_File_Exit.Size = new System.Drawing.Size(233, 26);
            this.MenuItem_File_Exit.Text = "退出(&X)";
            this.MenuItem_File_Exit.Click += new System.EventHandler(this.MenuItem_File_Exit_Click);
            // 
            // MenuItem_Command
            // 
            this.MenuItem_Command.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_Command_Pack,
            this.MenuItem_Command_Add,
            this.MenuItem_Command_Delete,
            this.MenuItem_Command_Add_UnpackSelect,
            this.MenuItem_Command_Unpack});
            this.MenuItem_Command.Name = "MenuItem_Command";
            this.MenuItem_Command.Size = new System.Drawing.Size(73, 24);
            this.MenuItem_Command.Text = "命令(&C)";
            // 
            // MenuItem_Command_Pack
            // 
            this.MenuItem_Command_Pack.Image = global::FPACTool.Properties.Resources.img_ToolStrip_Pack;
            this.MenuItem_Command_Pack.Name = "MenuItem_Command_Pack";
            this.MenuItem_Command_Pack.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.MenuItem_Command_Pack.Size = new System.Drawing.Size(272, 26);
            this.MenuItem_Command_Pack.Text = "打包PAC(&P)";
            this.MenuItem_Command_Pack.Click += new System.EventHandler(this.MenuItem_Command_Pack_Click);
            // 
            // MenuItem_Command_Add
            // 
            this.MenuItem_Command_Add.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_Command_Add_File,
            this.MenuItem_Command_Add_Folder});
            this.MenuItem_Command_Add.Image = global::FPACTool.Properties.Resources.img_ToolStrip_Add;
            this.MenuItem_Command_Add.Name = "MenuItem_Command_Add";
            this.MenuItem_Command_Add.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.MenuItem_Command_Add.Size = new System.Drawing.Size(272, 26);
            this.MenuItem_Command_Add.Text = "添加(&A)";
            // 
            // MenuItem_Command_Add_File
            // 
            this.MenuItem_Command_Add_File.Name = "MenuItem_Command_Add_File";
            this.MenuItem_Command_Add_File.Size = new System.Drawing.Size(137, 26);
            this.MenuItem_Command_Add_File.Text = "文件";
            this.MenuItem_Command_Add_File.Click += new System.EventHandler(this.MenuItem_Command_Add_File_Click);
            // 
            // MenuItem_Command_Add_Folder
            // 
            this.MenuItem_Command_Add_Folder.Name = "MenuItem_Command_Add_Folder";
            this.MenuItem_Command_Add_Folder.Size = new System.Drawing.Size(137, 26);
            this.MenuItem_Command_Add_Folder.Text = "文件夹";
            this.MenuItem_Command_Add_Folder.Click += new System.EventHandler(this.MenuItem_Command_Add_Folder_Click);
            // 
            // MenuItem_Command_Delete
            // 
            this.MenuItem_Command_Delete.Image = global::FPACTool.Properties.Resources.img_ToolStrip_Delete;
            this.MenuItem_Command_Delete.Name = "MenuItem_Command_Delete";
            this.MenuItem_Command_Delete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.MenuItem_Command_Delete.Size = new System.Drawing.Size(272, 26);
            this.MenuItem_Command_Delete.Text = "删除(&D)";
            this.MenuItem_Command_Delete.Click += new System.EventHandler(this.PackDeleteCommand_Click);
            // 
            // MenuItem_Command_Add_UnpackSelect
            // 
            this.MenuItem_Command_Add_UnpackSelect.Image = global::FPACTool.Properties.Resources.img_ToolStrip_UnpackSelect;
            this.MenuItem_Command_Add_UnpackSelect.Name = "MenuItem_Command_Add_UnpackSelect";
            this.MenuItem_Command_Add_UnpackSelect.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.U)));
            this.MenuItem_Command_Add_UnpackSelect.Size = new System.Drawing.Size(272, 26);
            this.MenuItem_Command_Add_UnpackSelect.Text = "解包选中(&S)";
            this.MenuItem_Command_Add_UnpackSelect.Click += new System.EventHandler(this.MenuItem_Command_Add_UnpackSelect_Click);
            // 
            // MenuItem_Command_Unpack
            // 
            this.MenuItem_Command_Unpack.Image = global::FPACTool.Properties.Resources.img_ToolStrip_UnpackAll;
            this.MenuItem_Command_Unpack.Name = "MenuItem_Command_Unpack";
            this.MenuItem_Command_Unpack.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U)));
            this.MenuItem_Command_Unpack.Size = new System.Drawing.Size(272, 26);
            this.MenuItem_Command_Unpack.Text = "解包全部(&U)";
            this.MenuItem_Command_Unpack.Click += new System.EventHandler(this.MenuItem_Command_Unpack_Click);
            // 
            // MenuItem_Help
            // 
            this.MenuItem_Help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_Help_Main,
            this.更新日志LToolStripMenuItem,
            this.MenuItem_Help_Delimiter01,
            this.MenuItem_Help_About});
            this.MenuItem_Help.Name = "MenuItem_Help";
            this.MenuItem_Help.Size = new System.Drawing.Size(75, 24);
            this.MenuItem_Help.Text = "帮助(&H)";
            // 
            // MenuItem_Help_Main
            // 
            this.MenuItem_Help_Main.Image = global::FPACTool.Properties.Resources.img_ToolStrip_Help;
            this.MenuItem_Help_Main.Name = "MenuItem_Help_Main";
            this.MenuItem_Help_Main.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.MenuItem_Help_Main.Size = new System.Drawing.Size(203, 26);
            this.MenuItem_Help_Main.Text = "帮助主题(&M)";
            this.MenuItem_Help_Main.Click += new System.EventHandler(this.MenuItem_Help_Main_Click);
            // 
            // 更新日志LToolStripMenuItem
            // 
            this.更新日志LToolStripMenuItem.Name = "更新日志LToolStripMenuItem";
            this.更新日志LToolStripMenuItem.Size = new System.Drawing.Size(203, 26);
            this.更新日志LToolStripMenuItem.Text = "更新日志(&L)";
            this.更新日志LToolStripMenuItem.Click += new System.EventHandler(this.更新日志LToolStripMenuItem_Click);
            // 
            // MenuItem_Help_Delimiter01
            // 
            this.MenuItem_Help_Delimiter01.Name = "MenuItem_Help_Delimiter01";
            this.MenuItem_Help_Delimiter01.Size = new System.Drawing.Size(200, 6);
            // 
            // MenuItem_Help_About
            // 
            this.MenuItem_Help_About.Image = global::FPACTool.Properties.Resources.img_ToolStrip_About;
            this.MenuItem_Help_About.Name = "MenuItem_Help_About";
            this.MenuItem_Help_About.Size = new System.Drawing.Size(203, 26);
            this.MenuItem_Help_About.Text = "关于(&A)";
            this.MenuItem_Help_About.Click += new System.EventHandler(this.MenuItem_Help_About_Click);
            // 
            // SplitContainer
            // 
            this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitContainer.Location = new System.Drawing.Point(0, 28);
            this.SplitContainer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.SplitContainer.Name = "SplitContainer";
            this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // SplitContainer.Panel1
            // 
            this.SplitContainer.Panel1.Controls.Add(this.ToolStrip);
            this.SplitContainer.Panel1.Controls.Add(this.ListView_PAC);
            // 
            // SplitContainer.Panel2
            // 
            this.SplitContainer.Panel2.Controls.Add(this.ListView_Log);
            this.SplitContainer.Size = new System.Drawing.Size(800, 386);
            this.SplitContainer.SplitterDistance = 250;
            this.SplitContainer.TabIndex = 2;
            // 
            // ToolStrip
            // 
            this.ToolStrip.AutoSize = false;
            this.ToolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStrip_New,
            this.ToolStrip_Open,
            this.ToolStrip_Close,
            this.ToolStrip_Add,
            this.ToolStrip_Delete,
            this.ToolStrip_Pack,
            this.ToolStrip_UnpackSelect,
            this.ToolStrip_UnpackAll,
            this.ToolStrip_PACProperties,
            this.ToolStrip_Help,
            this.ToolStrip_About});
            this.ToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ToolStrip.Name = "ToolStrip";
            this.ToolStrip.Size = new System.Drawing.Size(800, 99);
            this.ToolStrip.TabIndex = 1;
            this.ToolStrip.Text = "toolStrip";
            // 
            // ToolStrip_New
            // 
            this.ToolStrip_New.AutoSize = false;
            this.ToolStrip_New.Image = global::FPACTool.Properties.Resources.img_ToolStrip_New;
            this.ToolStrip_New.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolStrip_New.Name = "ToolStrip_New";
            this.ToolStrip_New.Size = new System.Drawing.Size(65, 65);
            this.ToolStrip_New.Text = "新建";
            this.ToolStrip_New.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.ToolStrip_New.Click += new System.EventHandler(this.ToolStrip_New_Click);
            // 
            // ToolStrip_Open
            // 
            this.ToolStrip_Open.AutoSize = false;
            this.ToolStrip_Open.Image = global::FPACTool.Properties.Resources.img_ToolStrip_Open;
            this.ToolStrip_Open.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolStrip_Open.Name = "ToolStrip_Open";
            this.ToolStrip_Open.Size = new System.Drawing.Size(65, 65);
            this.ToolStrip_Open.Text = "打开";
            this.ToolStrip_Open.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.ToolStrip_Open.ToolTipText = "打开";
            this.ToolStrip_Open.Click += new System.EventHandler(this.ToolStrip_Open_Click);
            // 
            // ToolStrip_Close
            // 
            this.ToolStrip_Close.AutoSize = false;
            this.ToolStrip_Close.Image = global::FPACTool.Properties.Resources.img_ToolStrip_Close;
            this.ToolStrip_Close.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolStrip_Close.Name = "ToolStrip_Close";
            this.ToolStrip_Close.Size = new System.Drawing.Size(65, 65);
            this.ToolStrip_Close.Text = "关闭";
            this.ToolStrip_Close.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.ToolStrip_Close.Click += new System.EventHandler(this.ToolStrip_Close_Click);
            // 
            // ToolStrip_Add
            // 
            this.ToolStrip_Add.AutoSize = false;
            this.ToolStrip_Add.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStrip_Add_File,
            this.ToolStrip_Add_Folder});
            this.ToolStrip_Add.Image = global::FPACTool.Properties.Resources.img_ToolStrip_Add;
            this.ToolStrip_Add.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolStrip_Add.Name = "ToolStrip_Add";
            this.ToolStrip_Add.Size = new System.Drawing.Size(65, 65);
            this.ToolStrip_Add.Text = "添加";
            this.ToolStrip_Add.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // ToolStrip_Add_File
            // 
            this.ToolStrip_Add_File.Name = "ToolStrip_Add_File";
            this.ToolStrip_Add_File.Size = new System.Drawing.Size(137, 26);
            this.ToolStrip_Add_File.Text = "文件";
            this.ToolStrip_Add_File.Click += new System.EventHandler(this.ToolStrip_Add_File_Click);
            // 
            // ToolStrip_Add_Folder
            // 
            this.ToolStrip_Add_Folder.Name = "ToolStrip_Add_Folder";
            this.ToolStrip_Add_Folder.Size = new System.Drawing.Size(137, 26);
            this.ToolStrip_Add_Folder.Text = "文件夹";
            this.ToolStrip_Add_Folder.Click += new System.EventHandler(this.ToolStrip_Add_Folder_Click);
            // 
            // ToolStrip_Delete
            // 
            this.ToolStrip_Delete.AutoSize = false;
            this.ToolStrip_Delete.Image = global::FPACTool.Properties.Resources.img_ToolStrip_Delete;
            this.ToolStrip_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolStrip_Delete.Name = "ToolStrip_Delete";
            this.ToolStrip_Delete.Size = new System.Drawing.Size(65, 65);
            this.ToolStrip_Delete.Text = "删除";
            this.ToolStrip_Delete.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.ToolStrip_Delete.Click += new System.EventHandler(this.PackDeleteCommand_Click);
            // 
            // ToolStrip_Pack
            // 
            this.ToolStrip_Pack.AutoSize = false;
            this.ToolStrip_Pack.Image = global::FPACTool.Properties.Resources.img_ToolStrip_Pack;
            this.ToolStrip_Pack.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolStrip_Pack.Name = "ToolStrip_Pack";
            this.ToolStrip_Pack.Size = new System.Drawing.Size(65, 65);
            this.ToolStrip_Pack.Text = "打包";
            this.ToolStrip_Pack.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.ToolStrip_Pack.Click += new System.EventHandler(this.ToolStrip_Pack_Click);
            // 
            // ToolStrip_UnpackSelect
            // 
            this.ToolStrip_UnpackSelect.AutoSize = false;
            this.ToolStrip_UnpackSelect.Image = global::FPACTool.Properties.Resources.img_ToolStrip_UnpackSelect;
            this.ToolStrip_UnpackSelect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolStrip_UnpackSelect.Name = "ToolStrip_UnpackSelect";
            this.ToolStrip_UnpackSelect.Size = new System.Drawing.Size(65, 65);
            this.ToolStrip_UnpackSelect.Text = "提取选中";
            this.ToolStrip_UnpackSelect.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.ToolStrip_UnpackSelect.Click += new System.EventHandler(this.ToolStrip_UnpackSelect_Click);
            // 
            // ToolStrip_UnpackAll
            // 
            this.ToolStrip_UnpackAll.AutoSize = false;
            this.ToolStrip_UnpackAll.Image = global::FPACTool.Properties.Resources.img_ToolStrip_UnpackAll;
            this.ToolStrip_UnpackAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolStrip_UnpackAll.Name = "ToolStrip_UnpackAll";
            this.ToolStrip_UnpackAll.Size = new System.Drawing.Size(65, 65);
            this.ToolStrip_UnpackAll.Text = "提取全部";
            this.ToolStrip_UnpackAll.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.ToolStrip_UnpackAll.Click += new System.EventHandler(this.ToolStrip_UnpackAll_Click);
            // 
            // ToolStrip_PACProperties
            // 
            this.ToolStrip_PACProperties.AutoSize = false;
            this.ToolStrip_PACProperties.Image = global::FPACTool.Properties.Resources.img_ToolStrip_PACProperties;
            this.ToolStrip_PACProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolStrip_PACProperties.Name = "ToolStrip_PACProperties";
            this.ToolStrip_PACProperties.Size = new System.Drawing.Size(65, 65);
            this.ToolStrip_PACProperties.Text = "PAC属性";
            this.ToolStrip_PACProperties.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.ToolStrip_PACProperties.Click += new System.EventHandler(this.ToolStrip_PACProperties_Click);
            // 
            // ToolStrip_Help
            // 
            this.ToolStrip_Help.AutoSize = false;
            this.ToolStrip_Help.Image = global::FPACTool.Properties.Resources.img_ToolStrip_Help;
            this.ToolStrip_Help.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolStrip_Help.Name = "ToolStrip_Help";
            this.ToolStrip_Help.Size = new System.Drawing.Size(65, 65);
            this.ToolStrip_Help.Text = "帮助";
            this.ToolStrip_Help.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.ToolStrip_Help.Click += new System.EventHandler(this.ToolStrip_Help_Click);
            // 
            // ToolStrip_About
            // 
            this.ToolStrip_About.AutoSize = false;
            this.ToolStrip_About.Image = global::FPACTool.Properties.Resources.img_ToolStrip_About;
            this.ToolStrip_About.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolStrip_About.Name = "ToolStrip_About";
            this.ToolStrip_About.Size = new System.Drawing.Size(65, 65);
            this.ToolStrip_About.Text = "关于";
            this.ToolStrip_About.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.ToolStrip_About.Click += new System.EventHandler(this.ToolStrip_About_Click);
            // 
            // ListView_PAC
            // 
            this.ListView_PAC.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader_FileName,
            this.ColumnHeader_Offset,
            this.ColumnHeader_Size,
            this.ColumnHeader_Signature});
            this.ListView_PAC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListView_PAC.HideSelection = false;
            this.ListView_PAC.Location = new System.Drawing.Point(0, 0);
            this.ListView_PAC.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ListView_PAC.MultiSelect = false;
            this.ListView_PAC.Name = "ListView_PAC";
            this.ListView_PAC.Size = new System.Drawing.Size(800, 250);
            this.ListView_PAC.TabIndex = 0;
            this.ListView_PAC.UseCompatibleStateImageBehavior = false;
            this.ListView_PAC.View = System.Windows.Forms.View.Details;
            this.ListView_PAC.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ListView_PAC_MouseUp);
            // 
            // ColumnHeader_FileName
            // 
            this.ColumnHeader_FileName.Text = "名称";
            this.ColumnHeader_FileName.Width = 300;
            // 
            // ColumnHeader_Offset
            // 
            this.ColumnHeader_Offset.DisplayIndex = 2;
            this.ColumnHeader_Offset.Text = "偏移";
            this.ColumnHeader_Offset.Width = 100;
            // 
            // ColumnHeader_Size
            // 
            this.ColumnHeader_Size.DisplayIndex = 1;
            this.ColumnHeader_Size.Text = "大小";
            this.ColumnHeader_Size.Width = 100;
            // 
            // ColumnHeader_Signature
            // 
            this.ColumnHeader_Signature.Text = "签名值";
            this.ColumnHeader_Signature.Width = 100;
            // 
            // ListView_Log
            // 
            this.ListView_Log.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListView_Log.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ListView_Log.HideSelection = false;
            this.ListView_Log.Location = new System.Drawing.Point(0, 0);
            this.ListView_Log.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ListView_Log.Name = "ListView_Log";
            this.ListView_Log.OwnerDraw = true;
            this.ListView_Log.Size = new System.Drawing.Size(800, 132);
            this.ListView_Log.TabIndex = 0;
            this.ListView_Log.UseCompatibleStateImageBehavior = false;
            this.ListView_Log.View = System.Windows.Forms.View.List;
            this.ListView_Log.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.ListView_Log_DrawItem);
            this.ListView_Log.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ListView_Log_MouseUp);
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stripStatusLabel_Tips,
            this.stripProgressBar,
            this.stripStatusLabel_Percent});
            this.statusStrip.Location = new System.Drawing.Point(0, 414);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 13, 0);
            this.statusStrip.Size = new System.Drawing.Size(800, 36);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // stripStatusLabel_Tips
            // 
            this.stripStatusLabel_Tips.AutoSize = false;
            this.stripStatusLabel_Tips.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.stripStatusLabel_Tips.Name = "stripStatusLabel_Tips";
            this.stripStatusLabel_Tips.Size = new System.Drawing.Size(200, 30);
            this.stripStatusLabel_Tips.Text = "toolStripStatusLabel1";
            this.stripStatusLabel_Tips.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // stripProgressBar
            // 
            this.stripProgressBar.AutoSize = false;
            this.stripProgressBar.Name = "stripProgressBar";
            this.stripProgressBar.Size = new System.Drawing.Size(300, 28);
            this.stripProgressBar.Click += new System.EventHandler(this.stripProgressBar_Click);
            // 
            // stripStatusLabel_Percent
            // 
            this.stripStatusLabel_Percent.AutoSize = false;
            this.stripStatusLabel_Percent.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.stripStatusLabel_Percent.Name = "stripStatusLabel_Percent";
            this.stripStatusLabel_Percent.Size = new System.Drawing.Size(171, 30);
            this.stripStatusLabel_Percent.Text = "toolStripStatusLabel1";
            this.stripStatusLabel_Percent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.stripStatusLabel_Percent.Visible = false;
            // 
            // contextMenu_Unpack
            // 
            this.contextMenu_Unpack.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenu_Unpack.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextMenu_Unpack_Preview,
            this.contextMenu_Unpack_Single,
            this.contextMenu_Unpack_All,
            this.contextMenu_Unpack_Delimiter01,
            this.contextMenu_Unpack_PACProperties});
            this.contextMenu_Unpack.Name = "contextMenu_Unpack";
            this.contextMenu_Unpack.Size = new System.Drawing.Size(139, 106);
            // 
            // contextMenu_Unpack_Preview
            // 
            this.contextMenu_Unpack_Preview.Name = "contextMenu_Unpack_Preview";
            this.contextMenu_Unpack_Preview.Size = new System.Drawing.Size(138, 24);
            this.contextMenu_Unpack_Preview.Text = "预览文件";
            this.contextMenu_Unpack_Preview.Click += new System.EventHandler(this.contextMenu_Unpack_Preview_Click);
            // 
            // contextMenu_Unpack_Single
            // 
            this.contextMenu_Unpack_Single.Name = "contextMenu_Unpack_Single";
            this.contextMenu_Unpack_Single.Size = new System.Drawing.Size(138, 24);
            this.contextMenu_Unpack_Single.Text = "提取选中";
            this.contextMenu_Unpack_Single.Click += new System.EventHandler(this.contextMenu_Unpack_Single_Click);
            // 
            // contextMenu_Unpack_All
            // 
            this.contextMenu_Unpack_All.Name = "contextMenu_Unpack_All";
            this.contextMenu_Unpack_All.Size = new System.Drawing.Size(138, 24);
            this.contextMenu_Unpack_All.Text = "全部提取";
            this.contextMenu_Unpack_All.Click += new System.EventHandler(this.contextMenu_Unpack_All_Click);
            // 
            // contextMenu_Unpack_Delimiter01
            // 
            this.contextMenu_Unpack_Delimiter01.Name = "contextMenu_Unpack_Delimiter01";
            this.contextMenu_Unpack_Delimiter01.Size = new System.Drawing.Size(135, 6);
            // 
            // contextMenu_Unpack_PACProperties
            // 
            this.contextMenu_Unpack_PACProperties.Name = "contextMenu_Unpack_PACProperties";
            this.contextMenu_Unpack_PACProperties.Size = new System.Drawing.Size(138, 24);
            this.contextMenu_Unpack_PACProperties.Text = "PAC属性";
            this.contextMenu_Unpack_PACProperties.Click += new System.EventHandler(this.contextMenu_Unpack_PACProperties_Click);
            // 
            // contextMenu_Pack
            // 
            this.contextMenu_Pack.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenu_Pack.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextMenu_Pack_AddFile,
            this.contextMenu_Pack_AddFolder,
            this.contextMenu_Pack_Delimiter01,
            this.contextMenu_Pack_Delete,
            this.contextMenu_Pack_Delimiter02,
            this.contextMenu_Pack_PACPack});
            this.contextMenu_Pack.Name = "contextMenu_Pack";
            this.contextMenu_Pack.Size = new System.Drawing.Size(154, 112);
            // 
            // contextMenu_Pack_AddFile
            // 
            this.contextMenu_Pack_AddFile.Name = "contextMenu_Pack_AddFile";
            this.contextMenu_Pack_AddFile.Size = new System.Drawing.Size(153, 24);
            this.contextMenu_Pack_AddFile.Text = "添加文件";
            this.contextMenu_Pack_AddFile.Click += new System.EventHandler(this.contextMenu_Pack_AddFile_Click);
            // 
            // contextMenu_Pack_AddFolder
            // 
            this.contextMenu_Pack_AddFolder.CheckOnClick = true;
            this.contextMenu_Pack_AddFolder.Name = "contextMenu_Pack_AddFolder";
            this.contextMenu_Pack_AddFolder.Size = new System.Drawing.Size(153, 24);
            this.contextMenu_Pack_AddFolder.Text = "添加文件夹";
            this.contextMenu_Pack_AddFolder.Click += new System.EventHandler(this.contextMenu_Pack_AddFolder_Click);
            // 
            // contextMenu_Pack_Delimiter01
            // 
            this.contextMenu_Pack_Delimiter01.Name = "contextMenu_Pack_Delimiter01";
            this.contextMenu_Pack_Delimiter01.Size = new System.Drawing.Size(150, 6);
            // 
            // contextMenu_Pack_Delete
            // 
            this.contextMenu_Pack_Delete.Name = "contextMenu_Pack_Delete";
            this.contextMenu_Pack_Delete.Size = new System.Drawing.Size(153, 24);
            this.contextMenu_Pack_Delete.Text = "删除选中项";
            this.contextMenu_Pack_Delete.Click += new System.EventHandler(this.PackDeleteCommand_Click);
            // 
            // contextMenu_Pack_Delimiter02
            // 
            this.contextMenu_Pack_Delimiter02.Name = "contextMenu_Pack_Delimiter02";
            this.contextMenu_Pack_Delimiter02.Size = new System.Drawing.Size(150, 6);
            // 
            // contextMenu_Pack_PACPack
            // 
            this.contextMenu_Pack_PACPack.Name = "contextMenu_Pack_PACPack";
            this.contextMenu_Pack_PACPack.Size = new System.Drawing.Size(153, 24);
            this.contextMenu_Pack_PACPack.Text = "打包PAC";
            this.contextMenu_Pack_PACPack.Click += new System.EventHandler(this.contextMenu_Pack_PACPack_Click);
            // 
            // contextMenu_Log
            // 
            this.contextMenu_Log.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenu_Log.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextMenu_Log_CopySelect,
            this.contextMenu_Log_CopyAll,
            this.contextMenu_Log__Delimiter01,
            this.contextMenu_Log_SaveAs});
            this.contextMenu_Log.Name = "contextMenu_Log";
            this.contextMenu_Log.Size = new System.Drawing.Size(184, 82);
            // 
            // contextMenu_Log_CopySelect
            // 
            this.contextMenu_Log_CopySelect.Name = "contextMenu_Log_CopySelect";
            this.contextMenu_Log_CopySelect.Size = new System.Drawing.Size(183, 24);
            this.contextMenu_Log_CopySelect.Text = "复制选中行";
            this.contextMenu_Log_CopySelect.Click += new System.EventHandler(this.contextMenu_Log_CopySelect_Click);
            // 
            // contextMenu_Log_CopyAll
            // 
            this.contextMenu_Log_CopyAll.Name = "contextMenu_Log_CopyAll";
            this.contextMenu_Log_CopyAll.Size = new System.Drawing.Size(183, 24);
            this.contextMenu_Log_CopyAll.Text = "复制全部行";
            this.contextMenu_Log_CopyAll.Click += new System.EventHandler(this.contextMenu_Log_CopyAll_Click);
            // 
            // contextMenu_Log__Delimiter01
            // 
            this.contextMenu_Log__Delimiter01.Name = "contextMenu_Log__Delimiter01";
            this.contextMenu_Log__Delimiter01.Size = new System.Drawing.Size(180, 6);
            // 
            // contextMenu_Log_SaveAs
            // 
            this.contextMenu_Log_SaveAs.Name = "contextMenu_Log_SaveAs";
            this.contextMenu_Log_SaveAs.Size = new System.Drawing.Size(183, 24);
            this.contextMenu_Log_SaveAs.Text = "操作记录另存为";
            this.contextMenu_Log_SaveAs.Click += new System.EventHandler(this.contextMenu_Log_SaveAs_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.SplitContainer);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.MenuStrip);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainForm";
            this.MenuStrip.ResumeLayout(false);
            this.MenuStrip.PerformLayout();
            this.SplitContainer.Panel1.ResumeLayout(false);
            this.SplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
            this.SplitContainer.ResumeLayout(false);
            this.ToolStrip.ResumeLayout(false);
            this.ToolStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.contextMenu_Unpack.ResumeLayout(false);
            this.contextMenu_Pack.ResumeLayout(false);
            this.contextMenu_Log.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip MenuStrip;
        private System.Windows.Forms.ToolStripMenuItem MenuStrip_File;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_File_New;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_File_Open;
        private System.Windows.Forms.ToolStripSeparator MenuItem_File_Delimiter01;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_File_PACProperties;
        private System.Windows.Forms.ToolStripSeparator MenuItem_File_Delimiter02;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_File_Exit;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_Command;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_Help;
        private System.Windows.Forms.SplitContainer SplitContainer;
        private System.Windows.Forms.ListView ListView_PAC;
        private System.Windows.Forms.ListView ListView_Log;
        private System.Windows.Forms.ToolStrip ToolStrip;
        private System.Windows.Forms.ToolStripButton ToolStrip_New;
        private System.Windows.Forms.ColumnHeader ColumnHeader_FileName;
        private System.Windows.Forms.ColumnHeader ColumnHeader_Offset;
        private System.Windows.Forms.ColumnHeader ColumnHeader_Size;
        private System.Windows.Forms.ColumnHeader ColumnHeader_Signature;
        private System.Windows.Forms.ToolStripButton ToolStrip_Open;
        private System.Windows.Forms.ToolStripButton ToolStrip_UnpackSelect;
        private System.Windows.Forms.ToolStripButton ToolStrip_UnpackAll;
        private System.Windows.Forms.ToolStripButton ToolStrip_Delete;
        private System.Windows.Forms.ToolStripButton ToolStrip_PACProperties;
        private System.Windows.Forms.ToolStripButton ToolStrip_Help;
        private System.Windows.Forms.ToolStripButton ToolStrip_About;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel stripStatusLabel_Tips;
        private System.Windows.Forms.ToolStripProgressBar stripProgressBar;
        private System.Windows.Forms.ToolStripStatusLabel stripStatusLabel_Percent;
        private System.Windows.Forms.ContextMenuStrip contextMenu_Unpack;
        private System.Windows.Forms.ToolStripMenuItem contextMenu_Unpack_Preview;
        private System.Windows.Forms.ToolStripMenuItem contextMenu_Unpack_Single;
        private System.Windows.Forms.ToolStripMenuItem contextMenu_Unpack_All;
        private System.Windows.Forms.ToolStripMenuItem contextMenu_Unpack_PACProperties;
        private System.Windows.Forms.ToolStripSeparator contextMenu_Unpack_Delimiter01;
        private System.Windows.Forms.ContextMenuStrip contextMenu_Pack;
        private System.Windows.Forms.ToolStripMenuItem contextMenu_Pack_AddFile;
        private System.Windows.Forms.ToolStripMenuItem contextMenu_Pack_AddFolder;
        private System.Windows.Forms.ToolStripSeparator contextMenu_Pack_Delimiter01;
        private System.Windows.Forms.ToolStripMenuItem contextMenu_Pack_Delete;
        private System.Windows.Forms.ToolStripSeparator contextMenu_Pack_Delimiter02;
        private System.Windows.Forms.ContextMenuStrip contextMenu_Log;
        private System.Windows.Forms.ToolStripMenuItem contextMenu_Log_CopySelect;
        private System.Windows.Forms.ToolStripMenuItem contextMenu_Log_CopyAll;
        private System.Windows.Forms.ToolStripSeparator contextMenu_Log__Delimiter01;
        private System.Windows.Forms.ToolStripMenuItem contextMenu_Log_SaveAs;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripDropDownButton ToolStrip_Add;
        private System.Windows.Forms.ToolStripMenuItem ToolStrip_Add_File;
        private System.Windows.Forms.ToolStripMenuItem ToolStrip_Add_Folder;
        private System.Windows.Forms.ToolStripButton ToolStrip_Pack;
        private System.Windows.Forms.ToolStripButton ToolStrip_Close;
        private System.Windows.Forms.ToolStripMenuItem contextMenu_Pack_PACPack;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_Command_Add;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_Command_Add_File;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_Command_Add_Folder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_Command_Pack;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_Command_Delete;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_Command_Unpack;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_Command_Add_UnpackSelect;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_Help_Main;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_Help_About;
        private System.Windows.Forms.ToolStripSeparator MenuItem_Help_Delimiter01;
        private System.Windows.Forms.ToolStripMenuItem 更新日志LToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_File_Close;
    }
}

