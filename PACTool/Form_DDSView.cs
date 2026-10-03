// SPDX-License-Identifier: CC0-1.0

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace FPACTool
{
    public partial class Form_DDSView : Form
    {
        private const int PropertiesWindowOverlap = 8;    // 属性窗口与主窗口的边框重叠像素（用于"零缝隙"吸附）
        private const int MaximizedPropertiesMargin = 8;  // 最大化时属性窗口距图片区边缘的留白
        private const double ZoomStep = 1.25;             // 每次滚轮缩放的倍率（放大 1.25 倍 / 缩小 0.8 倍）
        private const double MinZoomFactor = 0.01;        // 最小缩放系数（1%）
        private const double MaxZoomFactor = 16.0;        // 最大缩放系数（1600%）

        private DDSImage loadedImage;                     // 当前已加载的 DDS 图像（含位图与元数据）
        private Form_DDSProperties propertiesForm;        // 右侧属性窗口
        private string temporaryDirectory;                // 临时解包目录（预览文件存放处）
        private bool cleanupTemporaryDirectory;           // 关闭时是否需要清理临时目录
        private string currentFileName;                   // 当前 DDS 文件的完整路径
        private double zoomFactor = 1.0;                  // 当前缩放系数（1.0 = 原始大小）
        private Size propertiesPreferredSize = Size.Empty; // 属性窗口的首选尺寸
        private double propertiesAspectRatio = 1.0;       // 属性窗口宽高比
        private ToolTip imageButtonToolTip;               // 图像按钮的悬停提示
        private Image zoomInButtonImage;                  // "放大"按钮图标
        private Image zoomOutButtonImage;                 // "缩小"按钮图标
        private Image saveAsButtonImage;                  // "另存为"按钮图标
        private Image propertiesButtonImage;              // "图像属性"按钮图标

        #region 构造函数

        /// <summary>
        /// 实例化当前窗体
        /// </summary>
        public Form_DDSView()
        {
            InitializeComponent();
            InitializeDDSView();
        }

        /// <summary>
        /// 带文件名与临时目录实例化窗口并加载 DDS
        /// </summary>
        internal Form_DDSView(string fileName, string temporaryDirectory)
            : this()
        {
            // 空文件名检查
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("DDS 文件名不能为空。", "fileName");
            // 文件存在性检查
            this.temporaryDirectory = temporaryDirectory;
            cleanupTemporaryDirectory = !string.IsNullOrEmpty(temporaryDirectory);

            // 载入 DDS 文件
            LoadDDSFile(fileName);
        }
        #endregion

        #region 初始化

        /// <summary>
        /// 初始化 DDS 浏览视图（图片显示、缩放、事件绑定）
        /// </summary>
        private void InitializeDDSView()
        {
            this.StartPosition = FormStartPosition.CenterParent;

            // PictureBox 只负责显示已经解码好的 Bitmap。
            // 大小由 zoomFactor 控制，始终从图片面板左上角开始显示。
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.Location = Point.Empty;
            pictureBox.BackColor = SystemColors.AppWorkspace;
            splitContainer.Panel1.BackColor = SystemColors.AppWorkspace;

            InitializeBackColorComboBox();
            InitializeImageButtons();
            LayoutCommandPanel();
            splitContainer.Panel2.Resize += CommandPanel_Resize;

            this.Shown += DDSView_Shown;
            this.LocationChanged += DDSView_LocationChanged;
            this.SizeChanged += DDSView_SizeChanged;
            this.FormClosed += DDSView_FormClosed;

            // Ctrl + 鼠标滚轮缩放。三个入口用于覆盖焦点位于窗体、
            // 图片区或 PictureBox 上的常见 WinForms 场景。
            this.MouseWheel += DDSView_MouseWheel;
            splitContainer.Panel1.MouseWheel += DDSView_MouseWheel;
            pictureBox.MouseWheel += DDSView_MouseWheel;
            splitContainer.Panel1.MouseEnter += ImageArea_MouseEnter;
            pictureBox.MouseEnter += ImageArea_MouseEnter;
        }

        /// <summary>
        /// 初始化图像按钮（缩放图与提示）
        /// </summary>
        private void InitializeImageButtons()
        {
            // 原始 PNG 资源保持 100x100 不做修改，只在运行时生成适合 50x50
            // 图像按钮的小尺寸副本。这样不会降低资源文件本身的质量。
            zoomInButtonImage = CreateScaledButtonImage(btn_ZoomIn.Image, 34);
            zoomOutButtonImage = CreateScaledButtonImage(btn_ZoomOut.Image, 34);
            saveAsButtonImage = CreateScaledButtonImage(btn_SaveAs.Image, 34);
            propertiesButtonImage = CreateScaledButtonImage(btn_Properties.Image, 34);

            ConfigureImageButton(btn_ZoomIn, zoomInButtonImage, "放大", "放大（Ctrl + 鼠标滚轮向上）");
            ConfigureImageButton(btn_ZoomOut, zoomOutButtonImage, "缩小", "缩小（Ctrl + 鼠标滚轮向下）");
            ConfigureImageButton(btn_SaveAs, saveAsButtonImage, "另存为", "将当前 DDS 图像另存为 PNG、BMP、JPEG 或 TIFF");
            ConfigureImageButton(btn_Properties, propertiesButtonImage, "图像属性", "显示或隐藏 DDS 图像属性");

            imageButtonToolTip = new ToolTip();
            imageButtonToolTip.AutoPopDelay = 8000;
            imageButtonToolTip.InitialDelay = 350;
            imageButtonToolTip.ReshowDelay = 100;
            imageButtonToolTip.ShowAlways = true;
            imageButtonToolTip.SetToolTip(btn_ZoomIn, "放大（Ctrl + 鼠标滚轮向上）");
            imageButtonToolTip.SetToolTip(btn_ZoomOut, "缩小（Ctrl + 鼠标滚轮向下）");
            imageButtonToolTip.SetToolTip(btn_SaveAs, "另存为...");
            imageButtonToolTip.SetToolTip(btn_Properties, "图像属性");
        }

        /// <summary>
        /// 配置图像按钮外观与无障碍属性
        /// </summary>
        private static void ConfigureImageButton(
            Button button,
            Image scaledImage,
            string accessibleName,
            string accessibleDescription)
        {
            button.Size = new Size(50, 50);                        // 按钮尺寸 50×50 像素
            button.Image = scaledImage;                            // 设置按钮图标
            button.ImageAlign = ContentAlignment.MiddleCenter;     // 图标对齐：水平垂直居中
            button.Text = string.Empty;                            // 按钮不显示文字
            button.Cursor = Cursors.Hand;                          // 鼠标悬停时显示"手型"光标（可点击提示）
            button.FlatStyle = FlatStyle.Flat;                     // 扁平样式（无 3D 立体边框）

            // FlatAppearance：扁平样式的外观细节设置
            button.FlatAppearance.BorderSize = 1;                               // 边框粗细：1 像素
            button.FlatAppearance.BorderColor = SystemColors.ControlDark;       // 边框颜色：深灰
            button.FlatAppearance.MouseOverBackColor = SystemColors.ControlLight; // 鼠标悬停背景：浅灰
            button.FlatAppearance.MouseDownBackColor = SystemColors.ControlDark;  // 鼠标按下背景：深灰
            button.BackColor = SystemColors.Control;                // 常规背景：标准灰
            button.UseVisualStyleBackColor = false;                 // 不用系统视觉样式背景，用上面自定义的背景色

            // 无障碍属性：供屏幕阅读器朗读，帮助视障用户识别按钮用途
            button.AccessibleName = accessibleName;
            button.AccessibleDescription = accessibleDescription;
        }

        /// <summary>
        /// 把原始按钮图像按目标尺寸等比缩放，生成一张新的小尺寸按钮图标。
        /// 缩放使用高质量插值，避免缩小后出现锯齿；空白区域用透明色填充并居中绘制。
        /// </summary>
        private static Image CreateScaledButtonImage(Image source, int targetSize)
        {
            if (source == null)
                return null;

            // 创建一张 targetSize × targetSize 的新位图。
            // Format32bppPArgb：32 位真彩色、带 Alpha 通道（P = 预乘 Alpha），是 GDI+ 中绘图质量最好的位图格式。
            Bitmap bitmap = new Bitmap(targetSize, targetSize, PixelFormat.Format32bppPArgb);

            // FromImage：以这张位图为"画布"创建 Graphics 绘图对象，之后的所有绘制都落在 bitmap 上。
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);   // 先用透明色清空画布

                // 下面 5 项是 GDI+ 的绘图质量设置，全部设为"高质量"，
                // 目的是让缩小后的按钮图标边缘平滑、不出现锯齿：
                graphics.CompositingMode = CompositingMode.SourceOver;          // 合成模式：源像素直接叠加到目标上（标准透明混合）
                graphics.CompositingQuality = CompositingQuality.HighQuality;   // 合成质量：高质量
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic; // 插值模式：高质量双三次插值（缩放最平滑）
                graphics.SmoothingMode = SmoothingMode.HighQuality;             // 平滑模式：抗锯齿（边缘平滑）
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;         // 像素偏移模式：高质量（半像素对齐，避免锯齿）

                // 计算等比缩放比例：取宽、高两个方向中较小的缩放比，
                // 保证整张图完整放进目标尺寸、不变形。
                double scale = Math.Min(
                    targetSize / (double)source.Width,
                    targetSize / (double)source.Height);

                // 缩放后的实际宽高（至少 1 像素），并算出居中偏移量。
                int width = Math.Max(1, (int)Math.Round(source.Width * scale));
                int height = Math.Max(1, (int)Math.Round(source.Height * scale));
                int x = (targetSize - width) / 2;
                int y = (targetSize - height) / 2;

                // DrawImage：把源图按缩放尺寸绘制到目标位图的居中位置。
                // GraphicsUnit.Pixel：坐标单位为"像素"。
                graphics.DrawImage(
                    source,
                    new Rectangle(x, y, width, height),
                    new Rectangle(0, 0, source.Width, source.Height),
                    GraphicsUnit.Pixel);
            }

            return bitmap;
        }

        /// <summary>
        /// 初始化背景色下拉框：填充所有系统已知颜色，并设置为自绘（每项左侧画色块、右侧写颜色名）。
        /// </summary>
        private void InitializeBackColorComboBox()
        {
            // 参考常见 WinForms OwnerDraw ComboBox 的实现方式：
            // 每一项左侧绘制颜色块，右侧显示 KnownColor 名称。
            comboBox_BackColor.BeginUpdate();   // 开始批量更新（暂停重绘，提升性能）
            try
            {
                comboBox_BackColor.Items.Clear();

                // OwnerDrawFixed：自绘模式，每一项的显示内容由我们自己绘制（固定高度）
                comboBox_BackColor.DrawMode = DrawMode.OwnerDrawFixed;

                // DropDownList：只能从列表选择，不可手动输入文字
                comboBox_BackColor.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBox_BackColor.ItemHeight = 18;      // 每一项的高度：18 像素
                comboBox_BackColor.MaxDropDownItems = 14; // 下拉列表最多同时显示 14 项（超出则滚动）

                // KnownColor：.NET 内置的"已知颜色"枚举（如 Red、Blue、AppWorkspace 等系统色）。
                // 遍历枚举所有值，把它们逐个加入下拉框。
                Array allColors = Enum.GetValues(typeof(KnownColor));
                foreach (KnownColor knownColor in allColors)
                {
                    Color color = Color.FromKnownColor(knownColor);  // 由颜色名得到实际 Color 对象

                    // 透明色（Alpha=0）不能作为普通 WinForms 面板背景使用，跳过。
                    if (color.A == 0)
                        continue;

                    comboBox_BackColor.Items.Add(knownColor);
                }
            }
            finally
            {
                comboBox_BackColor.EndUpdate();  // 结束批量更新，恢复重绘
            }

            // 绑定两个事件：自绘每一项 + 选择改变时刷新背景色
            comboBox_BackColor.DrawItem += comboBox_BackColor_DrawItem;
            comboBox_BackColor.SelectedIndexChanged += comboBox_BackColor_SelectedIndexChanged;

            // 默认选中 AppWorkspace（应用工作区灰，与图片面板背景一致）
            int defaultIndex = comboBox_BackColor.Items.IndexOf(KnownColor.AppWorkspace);
            comboBox_BackColor.SelectedIndex = defaultIndex >= 0 ? defaultIndex : 0;
        }
        #endregion

        #region 布局

        /// <summary>
        /// 布局命令面板上的按钮与背景色下拉框
        /// </summary>
        private void LayoutCommandPanel()
        {
            Control panel = splitContainer.Panel2;
            int buttonSize = 50;
            int buttonGap = 8;
            int left = 16;
            int top = Math.Max(6, (panel.ClientSize.Height - buttonSize) / 2);

            btn_ZoomIn.Location = new Point(left, top);
            btn_ZoomOut.Location = new Point(left + (buttonSize + buttonGap), top);
            btn_SaveAs.Location = new Point(left + (buttonSize + buttonGap) * 2, top);
            btn_Properties.Location = new Point(left + (buttonSize + buttonGap) * 3, top);

            int comboX = panel.ClientSize.Width - comboBox_BackColor.Width - 16;
            int comboY = Math.Max(6, (panel.ClientSize.Height - comboBox_BackColor.Height) / 2);
            comboBox_BackColor.Location = new Point(comboX, comboY);

            int labelX = comboX - label1.Width - 8;
            int labelY = comboY + Math.Max(0, (comboBox_BackColor.Height - label1.Height) / 2);
            label1.Location = new Point(labelX, labelY);
        }
        #endregion

        #region 资源释放

        /// <summary>
        /// 释放图像按钮相关资源
        /// </summary>
        private void DisposeImageButtonResources()
        {
            // Button 仍持有运行时缩放图的引用，先断开引用再释放，
            // 避免窗体销毁过程中控件再次重绘时访问已经 Dispose 的 Image。
            btn_ZoomIn.Image = null;
            btn_ZoomOut.Image = null;
            btn_SaveAs.Image = null;
            btn_Properties.Image = null;

            if (imageButtonToolTip != null)
            {
                imageButtonToolTip.Dispose();
                imageButtonToolTip = null;
            }

            if (zoomInButtonImage != null)
            {
                zoomInButtonImage.Dispose();
                zoomInButtonImage = null;
            }
            if (zoomOutButtonImage != null)
            {
                zoomOutButtonImage.Dispose();
                zoomOutButtonImage = null;
            }
            if (saveAsButtonImage != null)
            {
                saveAsButtonImage.Dispose();
                saveAsButtonImage = null;
            }
            if (propertiesButtonImage != null)
            {
                propertiesButtonImage.Dispose();
                propertiesButtonImage = null;
            }
        }

        /// <summary>
        /// 清理临时目录
        /// </summary>
        private void CleanupTemporaryFiles()
        {
            if (!cleanupTemporaryDirectory || string.IsNullOrEmpty(temporaryDirectory))
                return;

            try
            {
                if (Directory.Exists(temporaryDirectory))
                    Directory.Delete(temporaryDirectory, true);
            }
            catch
            {
                // 这种情况不可能存在，临时解包出来的 DDS 文件放在系统 %TEMP% 目录下。清理失败绝不能让主工具崩溃；
                // 残留的临时文件可交由系统下一次临时目录清理机制处理。
            }
            finally
            {
                cleanupTemporaryDirectory = false;
            }
        }
        #endregion

        #region DDS 文件加载

        /// <summary>
        /// 载入指定 DDS 文件
        /// <param name="fileName">(文本型 欲载入的图像)</param>
        /// </summary>
        private void LoadDDSFile(string fileName)
        {
            // 取新的 DDSImage 对象
            DDSImage newImage = DDSHelper.LoadDDS(fileName);

            // 非空检查：如果解码失败，LoadDDS 会返回 null。此时不再继续执行后续操作。
            if (loadedImage != null)
            {
                pictureBox.Image = null;
                loadedImage.Dispose();
            }

            // 把新图像对象传给当前 loadedImage，更新当前文件名，显示位图，并更新窗口标题。
            loadedImage = newImage;
            currentFileName = fileName;
            pictureBox.Image = loadedImage.Bitmap;
            this.Text = Path.GetFileName(fileName) + " - DDS 浏览";

            // 根据当前缩放系数调整 PictureBox 尺寸，确保图片显示在 Panel1 内。
            FitImageToPanel();
        }
        #endregion

        #region 属性窗口管理

        /// <summary>
        /// 确保属性窗口存在并显示
        /// </summary>
        private void EnsurePropertiesWindow()
        {
            // null检查
            if (loadedImage == null)
                return;

            // 当属性窗口不存在或已被释放的时候，则创建一个新的属性窗口实例，并绑定 FormClosed 事件。
            if (propertiesForm == null || propertiesForm.IsDisposed)
            {
                propertiesForm = new Form_DDSProperties(loadedImage.Metadata);
                propertiesForm.FormClosed += PropertiesForm_FormClosed;

                // 记录设计器中属性窗口的原始尺寸。普通窗口状态下仍可与
                // DDS 主窗口等高吸附；主窗口最大化时恢复这个宽高比，
                // 避免属性窗口变成占满整屏高度的狭长窗口。
                propertiesPreferredSize = propertiesForm.Size;
                if (propertiesPreferredSize.Height > 0)
                {
                    propertiesAspectRatio =
                        propertiesPreferredSize.Width / (double)propertiesPreferredSize.Height;
                }
            }
            else
            {
                // 若属性窗口已经存在，则更新其元数据内容，确保显示的是当前加载的 DDS 文件的属性
                propertiesForm.SetMetadata(loadedImage.Metadata);
            }

            // 确保属性窗口可见，否则则调用 Show 方法显示属性窗口
            if (!propertiesForm.Visible)
                propertiesForm.Show(this);
        }

        /// <summary>
        /// 定位属性窗口（普通状态吸附）
        /// </summary>
        private void PositionPropertiesWindow()
        {
            if (propertiesForm == null || propertiesForm.IsDisposed || !propertiesForm.Visible)
                return;
            if (this.WindowState == FormWindowState.Minimized)
                return;

            Rectangle workingArea = Screen.FromControl(this).WorkingArea;

            // DDS 主窗口最大化后，屏幕右侧已经没有可用于“外部吸附”的空间。
            // 此时恢复属性窗口原始宽高比，并把它限制在图片浏览区的右上侧，
            // 从而避开底部命令区和背景颜色下拉框。
            if (this.WindowState == FormWindowState.Maximized)
            {
                PositionPropertiesWindowWhenMaximized(workingArea);
                return;
            }

            int preferredWidth = propertiesPreferredSize.Width > 0
                ? propertiesPreferredSize.Width
                : propertiesForm.Width;

            propertiesForm.Width = preferredWidth;

            int desiredHeight = Math.Max(propertiesForm.MinimumSize.Height, this.Height);
            if (desiredHeight > workingArea.Height)
                desiredHeight = workingArea.Height;
            propertiesForm.Height = desiredHeight;

            // Form.Bounds 不包含 Windows 的外部窗口阴影。直接把 Location 放在
            // this.Right 时，视觉上仍会出现明显缝隙。这里让两个窗口边框轻微
            // 重叠，遮住阴影，从视觉上形成“零缝隙”吸附。
            int x = this.Right - PropertiesWindowOverlap;
            if (x + propertiesForm.Width > workingArea.Right)
                x = this.Left - propertiesForm.Width + PropertiesWindowOverlap;

            if (x < workingArea.Left)
                x = workingArea.Left;
            if (x + propertiesForm.Width > workingArea.Right)
                x = Math.Max(workingArea.Left, workingArea.Right - propertiesForm.Width);

            int y = this.Top;
            if (y < workingArea.Top)
                y = workingArea.Top;
            if (y + propertiesForm.Height > workingArea.Bottom)
                y = Math.Max(workingArea.Top, workingArea.Bottom - propertiesForm.Height);

            propertiesForm.Location = new Point(x, y);
        }

        /// <summary>
        /// 定位最大化状态下的属性窗口
        /// </summary>
        private void PositionPropertiesWindowWhenMaximized(Rectangle workingArea)
        {
            // 最大化时属性窗口改为只覆盖“图片浏览区”，不再贴着整个客户区
            // 的右下角。这样底部命令区（尤其是背景颜色下拉框）始终可见。
            Rectangle imageArea = splitContainer.Panel1.RectangleToScreen(
                splitContainer.Panel1.ClientRectangle);

            int width = propertiesPreferredSize.Width > 0
                ? propertiesPreferredSize.Width
                : propertiesForm.Width;

            int height;
            if (propertiesAspectRatio > 0.01)
            {
                height = Math.Max(1, (int)Math.Round(width / propertiesAspectRatio));
            }
            else
            {
                height = propertiesPreferredSize.Height > 0
                    ? propertiesPreferredSize.Height
                    : propertiesForm.Height;
            }

            // 防止高 DPI 或特别小的图片浏览区放不下属性窗口。必要时仍按
            // 原比例整体缩小，且高度严格限制在 Panel1 内，不覆盖命令区。
            int maxWidth = Math.Max(1, imageArea.Width - MaximizedPropertiesMargin * 2);
            int maxHeight = Math.Max(1, imageArea.Height - MaximizedPropertiesMargin);

            if (width > maxWidth)
            {
                double scale = maxWidth / (double)width;
                width = maxWidth;
                height = Math.Max(1, (int)Math.Round(height * scale));
            }

            if (height > maxHeight)
            {
                double scale = maxHeight / (double)height;
                height = maxHeight;
                width = Math.Max(1, (int)Math.Round(width * scale));
            }

            propertiesForm.Size = new Size(width, height);

            // X 仍保持在图片区右侧；TOP 与图片浏览区顶边完全平行。
            // 属性窗因此悬浮在右上侧图片区域，不再遮挡底部背景颜色控件。
            int x = imageArea.Right
                - propertiesForm.Width
                - MaximizedPropertiesMargin;

            int y = imageArea.Bottom
                - propertiesForm.Height
                - MaximizedPropertiesMargin;

            propertiesForm.Location = new Point(x, y);
        }
        #endregion

        #region 图像缩放

        /// <summary>
        /// 按面板尺寸适配显示图像
        /// </summary>
        private void FitImageToPanel()
        {
            if (loadedImage == null || loadedImage.Bitmap == null)
                return;

            // ClientSize：图片面板内部可用尺寸（不含边框/滚动条）
            Size client = splitContainer.Panel1.ClientSize;
            if (client.Width <= 1 || client.Height <= 1)
                return;

            int imageWidth = loadedImage.Bitmap.Width;    // 图像原始宽度（像素）
            int imageHeight = loadedImage.Bitmap.Height;  // 图像原始高度（像素）

            // 分别计算宽、高两个方向的缩放比（面板尺寸 ÷ 图像尺寸）
            double scaleX = client.Width / (double)imageWidth;
            double scaleY = client.Height / (double)imageHeight;

            // 取两个方向中较小的缩放比，保证整图完整放进面板；
            // 且上限为 1.0（不放大，只缩小），避免小图被拉大产生模糊。
            double scale = Math.Min(1.0, Math.Min(scaleX, scaleY));
            if (scale <= 0)
                scale = 1.0;

            // 以面板左上角为锚点应用缩放（不保持锚点）
            SetZoom(scale, Point.Empty, false);
        }

        /// <summary>
        /// 置图像缩放比例
        /// <param name="newZoomFactor">(双精度小数型 新的缩放系数, </param>
        /// <param name="anchorPoint">Point Point放锚点, </param>
        /// <param name="preserveAnchor">逻辑型 是否保持锚点位置)</param>
        /// </summary>
        private void SetZoom(double newZoomFactor, Point anchorPoint, bool preserveAnchor)
        {
            if (loadedImage == null || loadedImage.Bitmap == null)
                return;

            // 把缩放系数限制在 [MinZoomFactor, MaxZoomFactor] 之间，防止缩得太小或太大
            newZoomFactor = Math.Max(MinZoomFactor, Math.Min(MaxZoomFactor, newZoomFactor));

            double oldZoomFactor = zoomFactor;  // 记住旧缩放系数
            if (oldZoomFactor <= 0)
                oldZoomFactor = 1.0;

            // AutoScrollPosition：面板的自动滚动位置。
            // 它的坐标是"负值"表示滚动量，所以取反得到实际的滚动偏移。
            int oldScrollX = -splitContainer.Panel1.AutoScrollPosition.X;
            int oldScrollY = -splitContainer.Panel1.AutoScrollPosition.Y;

            // 按新缩放系数算出 PictureBox 的新尺寸（图像的实际显示像素大小）
            int width = Math.Max(1, (int)Math.Round(loadedImage.Bitmap.Width * newZoomFactor));
            int height = Math.Max(1, (int)Math.Round(loadedImage.Bitmap.Height * newZoomFactor));

            // 先回到虚拟画布原点，再改变 PictureBox 尺寸。
            // 图片永远左上对齐，不做居中处理。
            splitContainer.Panel1.AutoScrollPosition = Point.Empty;
            pictureBox.Location = Point.Empty;
            pictureBox.Size = new Size(width, height);
            zoomFactor = newZoomFactor;

            // 如果要求保持锚点（缩放时鼠标指向的那一点保持不动）：
            // 按新旧缩放比重新计算滚动位置，让锚点像素在缩放后仍停留在原视觉位置。
            if (preserveAnchor)
            {
                double ratio = newZoomFactor / oldZoomFactor;   // 新旧缩放比
                int newScrollX = (int)Math.Round((oldScrollX + anchorPoint.X) * ratio - anchorPoint.X);
                int newScrollY = (int)Math.Round((oldScrollY + anchorPoint.Y) * ratio - anchorPoint.Y);

                if (newScrollX < 0)
                    newScrollX = 0;
                if (newScrollY < 0)
                    newScrollY = 0;

                splitContainer.Panel1.AutoScrollPosition = new Point(newScrollX, newScrollY);
            }
        }

        /// <summary>
        /// 按倍率缩放图像
        /// <param name="factor">(双精度小数型 缩放倍率, </param>
        /// <param name="anchorPoint">Point 缩放锚点)</param>
        /// </summary>
        private void ZoomBy(double factor, Point anchorPoint)
        {
            if (factor <= 0)
                return;

            // 新缩放系数 = 当前系数 × 倍率，并保持锚点位置不动
            SetZoom(zoomFactor * factor, anchorPoint, true);
        }

        /// <summary>
        /// 获取图片面板中心点
        /// </summary>
        private Point GetImagePanelCenter()
        {
            return new Point(
                splitContainer.Panel1.ClientSize.Width / 2,
                splitContainer.Panel1.ClientSize.Height / 2);
        }
        #endregion

        #region 事件处理方法

        /// <summary>
        /// 命令面板尺寸变化时重新布局
        /// </summary>
        private void CommandPanel_Resize(object sender, EventArgs e)
        {
            // 面板尺寸改变（窗口缩放、分隔条拖动等）时，重新计算按钮和下拉框的位置
            LayoutCommandPanel();
        }

        /// <summary>
        /// 自绘背景色下拉框的每一项
        /// </summary>
        private void comboBox_BackColor_DrawItem(object sender, DrawItemEventArgs e)
        {
            // e.Index：当前要绘制的项索引；越界（如 -1 或超出范围）时直接跳过
            if (e.Index < 0 || e.Index >= comboBox_BackColor.Items.Count)
                return;

            // 先绘制默认背景（含选中高亮效果）
            e.DrawBackground();

            // 取出当前项对应的已知颜色
            KnownColor knownColor = (KnownColor)comboBox_BackColor.Items[e.Index];
            Color color = Color.FromKnownColor(knownColor);

            // 色块区域：位于每一项左侧，左右各留 3 像素边距，宽 24 像素
            Rectangle itemRect = e.Bounds;
            Rectangle colorRect = new Rectangle(
                itemRect.X + 3,
                itemRect.Y + 3,
                24,
                Math.Max(1, itemRect.Height - 6));

            // 用该颜色的实心画刷填充色块（using 确保画刷及时释放）
            using (SolidBrush colorBrush = new SolidBrush(color))
            {
                e.Graphics.FillRectangle(colorBrush, colorRect);
            }
            // 色块四周画黑色边框，使颜色在浅色背景下也能看清
            e.Graphics.DrawRectangle(Pens.Black, colorRect);

            // 文字颜色：选中项用系统高亮文字色，否则用下拉框常规前景色
            Color textColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                ? SystemColors.HighlightText
                : comboBox_BackColor.ForeColor;

            // 文字区域：从色块右侧开始，占满剩余宽度
            Rectangle textRect = new Rectangle(
                colorRect.Right + 6,
                itemRect.Y,
                Math.Max(1, itemRect.Right - colorRect.Right - 8),
                itemRect.Height);

            // 绘制颜色名称文字：左对齐 + 垂直居中 + 超出省略号 + 不解析 & 前缀
            TextRenderer.DrawText(
                e.Graphics,
                knownColor.ToString(),
                e.Font,
                textRect,
                textColor,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix);

            // 最后绘制焦点框（键盘选中项时显示虚线框）
            e.DrawFocusRectangle();
        }

        /// <summary>
        /// 背景色选择改变时刷新图片区背景
        /// </summary>
        private void comboBox_BackColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 未选中任何项时直接返回
            if (comboBox_BackColor.SelectedItem == null)
                return;

            // 取出选中的已知颜色
            KnownColor knownColor = (KnownColor)comboBox_BackColor.SelectedItem;
            Color color = Color.FromKnownColor(knownColor);

            // 同时更新图片面板和 PictureBox 的背景色
            splitContainer.Panel1.BackColor = color;
            pictureBox.BackColor = color;
        }

        /// <summary>
        /// 窗口首次显示完成后初始化属性窗口并归还焦点
        /// </summary>
        private void DDSView_Shown(object sender, EventArgs e)
        {
            EnsurePropertiesWindow();
            PositionPropertiesWindow();
            FitImageToPanel();

            // 属性窗是 Owned Form。即使系统在 Show(owner) 过程中短暂改变了
            // 激活窗口，也在消息队列空闲后把键盘焦点明确还给 DDS 浏览窗口。
            // 使用 BeginInvoke 是为了等属性窗的首次 Show/Activate 消息处理完毕。
            BeginInvoke((MethodInvoker)delegate
            {
                if (!IsDisposed && Visible && WindowState != FormWindowState.Minimized)
                    Activate();
            });
        }

        /// <summary>
        /// 窗口位置改变时同步移动属性窗口
        /// </summary>
        private void DDSView_LocationChanged(object sender, EventArgs e)
        {
            // 主窗口移动后，属性窗口需跟随重新吸附定位
            PositionPropertiesWindow();
        }

        /// <summary>
        /// 窗口尺寸改变时重新定位属性窗口
        /// </summary>
        private void DDSView_SizeChanged(object sender, EventArgs e)
        {
            // 用户手工缩放图片后，调整窗口大小不能把缩放比例重置成“适合窗口”。
            // 这里只重新定位属性窗口，不动图像缩放。
            PositionPropertiesWindow();
        }

        /// <summary>
        /// 属性窗口关闭时清空其引用
        /// </summary>
        private void PropertiesForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 用引用比较（ReferenceEquals）确认关闭的就是当前持有的那个属性窗口，
            // 避免误把其它窗体当成属性窗清空。
            if (ReferenceEquals(sender, propertiesForm))
                propertiesForm = null;
        }

        /// <summary>
        /// 鼠标进入图片区时确保滚轮缩放可用
        /// </summary>
        private void ImageArea_MouseEnter(object sender, EventArgs e)
        {
            // 让 Ctrl + 滚轮在鼠标位于图片区时可靠地发送给 DDSView。
            // 只在本窗体本来就是活动窗体时获取焦点，不抢其它程序的前台焦点。
            if (Form.ActiveForm == this)
                this.Focus();
        }

        /// <summary>
        /// 处理鼠标滚轮：按住 Ctrl 时缩放图像
        /// </summary>
        private void DDSView_MouseWheel(object sender, MouseEventArgs e)
        {
            // 未按住 Ctrl 键时不缩放（普通滚轮走面板默认滚动行为）
            if ((Control.ModifierKeys & Keys.Control) != Keys.Control)
                return;
            // 没有加载图像时不缩放
            if (loadedImage == null || loadedImage.Bitmap == null)
                return;

            // 标记事件已处理，阻止滚轮事件继续冒泡（避免叠加默认滚动）
            HandledMouseEventArgs handledArgs = e as HandledMouseEventArgs;
            if (handledArgs != null)
                handledArgs.Handled = true;

            // 把鼠标的屏幕坐标换算成图片面板内的客户端坐标，作为缩放锚点
            Point anchor = splitContainer.Panel1.PointToClient(Control.MousePosition);
            // 若鼠标不在图片面板范围内，则改用面板中心作为锚点
            if (anchor.X < 0 || anchor.Y < 0 ||
                anchor.X > splitContainer.Panel1.ClientSize.Width ||
                anchor.Y > splitContainer.Panel1.ClientSize.Height)
            {
                anchor = GetImagePanelCenter();
            }

            // e.Delta 是滚轮滚动的带符号量，通常一格为 ±120；
            // 除以 120 得到滚动的“格数”（正=向上放大，负=向下缩小）
            double wheelSteps = e.Delta / 120.0;
            if (wheelSteps == 0)
                return;

            // 按 ZoomStep 的滚轮格数次方计算最终倍率（每格 ×1.25 或 ÷1.25）
            ZoomBy(Math.Pow(ZoomStep, wheelSteps), anchor);
        }

        /// <summary>
        /// 窗口关闭时释放图像、属性窗口与临时文件
        /// </summary>
        private void DDSView_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 属性窗口还未关闭时：先解除事件绑定，再关闭并释放它，避免二次触发本事件
            if (propertiesForm != null && !propertiesForm.IsDisposed)
            {
                propertiesForm.FormClosed -= PropertiesForm_FormClosed;
                propertiesForm.Close();
                propertiesForm.Dispose();
                propertiesForm = null;
            }

            // 断开 PictureBox 对位图的引用，然后释放加载的 DDS 图像
            pictureBox.Image = null;
            if (loadedImage != null)
            {
                loadedImage.Dispose();
                loadedImage = null;
            }

            // 释放按钮图标、工具提示等资源
            DisposeImageButtonResources();
            // 清理预览用的临时目录
            CleanupTemporaryFiles();
        }

        /// <summary>
        /// 放大按钮：以图片中心为锚点放大
        /// </summary>
        private void btn_ZoomIn_Click(object sender, EventArgs e)
        {
            // 以面板中心为锚点，放大一个 ZoomStep（1.25 倍）
            ZoomBy(ZoomStep, GetImagePanelCenter());
        }

        /// <summary>
        /// 缩小按钮：以图片中心为锚点缩小
        /// </summary>
        private void btn_ZoomOut_Click(object sender, EventArgs e)
        {
            // 以面板中心为锚点，缩小一个 ZoomStep（1 ÷ 1.25 = 0.8 倍）
            ZoomBy(1.0 / ZoomStep, GetImagePanelCenter());
        }

        /// <summary>
        /// 另存为按钮：把当前图像保存为 PNG/BMP/JPEG/TIFF
        /// </summary>
        private void btn_SaveAs_Click(object sender, EventArgs e)
        {
            // 没有加载图像时直接返回
            if (loadedImage == null || loadedImage.Bitmap == null)
                return;

            // 默认文件名用 "DDS_Image"；若知道原文件名，则用原文件的主名（不含扩展名）
            string baseName = "DDS_Image";
            if (!string.IsNullOrEmpty(currentFileName))
            {
                string candidate = Path.GetFileNameWithoutExtension(currentFileName);
                if (!string.IsNullOrEmpty(candidate))
                    baseName = candidate;
            }

            // 用 using 确保保存对话框用完后立即释放
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "保存 DDS 图像";
                // Filter：文件类型筛选，支持 4 种格式；竖线前是显示名，后是扩展名通配符
                dialog.Filter =
                    "PNG 图像 (*.png)|*.png|" +
                    "Bitmap 图像 (*.bmp)|*.bmp|" +
                    "JPEG 图像 (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                    "TIFF 图像 (*.tif;*.tiff)|*.tif;*.tiff";
                dialog.FilterIndex = 1;       // 默认选中第 1 项（PNG）
                dialog.DefaultExt = "png";   // 未写扩展名时自动补 .png
                dialog.AddExtension = true;   // 自动追加扩展名
                dialog.OverwritePrompt = true; // 覆盖已有文件前先提示
                dialog.FileName = baseName + ".png";

                // 用户取消（未点“保存”）时直接返回
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    // 根据对话框的 FilterIndex 决定要保存的图片格式
                    ImageFormat format;
                    switch (dialog.FilterIndex)
                    {
                        case 2:
                            format = ImageFormat.Bmp;
                            break;
                        case 3:
                            format = ImageFormat.Jpeg;
                            break;
                        case 4:
                            format = ImageFormat.Tiff;
                            break;
                        default:
                            format = ImageFormat.Png;
                            break;
                    }

                    // 始终保存原始解码分辨率，不保存当前缩放后的 PictureBox 尺寸。
                    loadedImage.Bitmap.Save(dialog.FileName, format);
                }
                catch (Exception ex)
                {
                    // 保存失败时弹错误框，不让异常冒泡导致崩溃
                    MessageBox.Show(
                        this,
                        "保存图像失败：\r\n" + ex.Message,
                        "保存失败",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 图像属性按钮：显示/隐藏属性窗口
        /// </summary>
        private void btn_Properties_Click(object sender, EventArgs e)
        {
            // 没有加载图像时直接返回
            if (loadedImage == null)
                return;

            // 属性窗口尚未创建或已关闭：重新创建并显示
            if (propertiesForm == null || propertiesForm.IsDisposed)
            {
                EnsurePropertiesWindow();
                PositionPropertiesWindow();
                return;
            }

            // 属性窗口已存在：根据当前可见状态切换显示/隐藏
            if (propertiesForm.Visible)
            {
                propertiesForm.Hide();  // 已显示则隐藏
            }
            else
            {
                propertiesForm.Show(this);  // 已隐藏则显示（以本窗体为 Owner）
                PositionPropertiesWindow();
                propertiesForm.Activate();  // 激活属性窗口
            }
        }

        #endregion
    }
}
