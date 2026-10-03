using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FPACTool
{
    /// <summary>
    /// 工具条图标显示助手：从 shell32.dll 等系统图标库中提取图标，
    /// 并应用到 ToolStrip / ToolStripButton 等控件上。
    /// </summary>
    internal static class ToolStripIconShow
    {
        #region Win32 API 声明

        /// <summary>
        /// &lt;无符号整数型&gt;从指定的图标库文件中提取一个或多个图标句柄
        /// <param name="lpszFile">(文本型 图标库文件路径, </param>
        /// <param name="nIconIndex">整数型 图标在库中的索引, </param>
        /// <param name="phiconLarge">句柄数组 大图标句柄数组, </param>
        /// <param name="phiconSmall">句柄数组 小图标句柄数组, </param>
        /// <param name="nIcons">无符号整数型 要提取的图标数量)</param>
        /// <returns><para>返回成功提取的数量，失败返回 0</para></returns>
        /// </summary>
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern uint ExtractIconEx(
            string lpszFile,
            int nIconIndex,
            IntPtr[] phiconLarge,
            IntPtr[] phiconSmall,
            uint nIcons);

        /// <summary>
        /// &lt;逻辑型&gt;销毁由 ExtractIconEx 返回的图标句柄，释放系统资源
        /// <param name="hIcon">(句柄型 待销毁的图标句柄)</param>
        /// <returns><para>销毁成功返回 true，否则返回 false</para></returns>
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        /// <summary>
        /// SHFILEINFO 结构：SHGetFileInfo 返回的文件/图标信息。
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;        // 图标句柄
            public int iIcon;           // 系统图标索引
            public uint dwAttributes;   // 文件属性
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;  // 显示名称（最大 260 字符）
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;   // 类型名称（最大 80 字符）
        }

        /// <summary>
        /// &lt;句柄型&gt;获取文件/扩展名关联的系统图标（与资源管理器一致）
        /// <para>配合 SHGFI_USEFILEATTRIBUTES 时无需真实文件存在，系统按扩展名返回关联图标</para>
        /// <param name="pszPath">(文本型 文件路径或扩展名查询串, </param>
        /// <param name="dwFileAttributes">无符号整数型 文件属性, </param>
        /// <param name="psfi">SHFILEINFO 接收返回的文件/图标信息, </param>
        /// <param name="cbFileInfo">无符号整数型 SHFILEINFO 结构大小, </param>
        /// <param name="uFlags">无符号整数型 控制标志组合)</param>
        /// <returns><para>返回图标句柄，失败返回 IntPtr.Zero</para></returns>
        /// </summary>
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(
            string pszPath,
            uint dwFileAttributes,
            ref SHFILEINFO psfi,
            uint cbFileInfo,
            uint uFlags);

        // SHGetFileInfo 的 uFlags 组合
        private const uint SHGFI_ICON = 0x000000100;             // 获取图标
        private const uint SHGFI_SMALLICON = 0x000000001;         // 小图标(16x16)
        private const uint SHGFI_LARGEICON = 0x000000000;         // 大图标(32x32)
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010; // 按文件属性判断，无需真实文件

        // dwFileAttributes
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

        #endregion

        #region 常用 shell32 图标索引

        /// <summary>
        /// shell32.dll 中常见图标的索引对照表（仅供参考，不同系统版本可能略有差异）。
        /// 索引从 0 开始。
        /// </summary>
        public static class Shell32Index
        {
            public const int Unknown = 0;          // 未知/空白
            public const int Document = 1;         // 普通文档
            public const int Application = 2;      // 应用程序(.exe)
            public const int FolderClosed = 3;     // 关闭的文件夹
            public const int FolderOpen = 4;       // 打开的文件夹
            public const int Floppy525 = 5;        // 5.25 英寸软盘
            public const int Floppy35 = 6;         // 3.5 英寸软盘
            public const int RemovableDrive = 7;   // 可移动磁盘
            public const int HardDrive = 8;        // 硬盘
            public const int NetworkDrive = 9;     // 网络驱动器
            public const int NetworkDriveOffline = 10; // 断开的网络驱动器
            public const int CDRom = 11;           // 光盘驱动器
            public const int RamDrive = 12;        // 内存盘
            public const int EntireNetwork = 13;   // 整个网络
            public const int Printer = 16;         // 打印机
            public const int MyComputer = 17;      // 我的电脑
            public const int NetworkNeighborhood = 18; // 网上邻居
        }

        #endregion

        #region 核心方法

        /// <summary>
        /// &lt;文本型&gt;获取 shell32.dll 库文件的实际路径
        /// </summary>
        private static string Shell32Path
        {
            get { return Path.Combine(Environment.SystemDirectory, "shell32.dll"); }
        }

        /// <summary>
        /// &lt;图标型&gt;从 shell32.dll 中提取指定索引的图标
        /// <param name="index">(整数型 , </param>
        /// <param name="large">逻辑型 是否取大图标)</param>
        /// <para>图标索引见 <see cref="Shell32Index"/>，true 取大图标 32x32，false 取小图标 16x16</para>
        /// <returns><para>返回提取到的 Icon，失败返回 null</para></returns>
        /// </summary>
        public static Icon GetShell32Icon(int index, bool large = true)
        {
            // ExtractIconEx 返回的句柄在特定时机可能无效，Icon.FromHandle 会抛 "参数无效"(ArgumentException)。
            // 图标仅作装饰，任何失败都应静默返回 null，绝不能拖垮整个程序。
            try
            {
                IntPtr[] largeIcons = large ? new IntPtr[1] : null;
                IntPtr[] smallIcons = large ? null : new IntPtr[1];

                uint count = ExtractIconEx(Shell32Path, index, largeIcons, smallIcons, 1);
                if (count == 0)
                    return null;

                IntPtr hIcon = large ? largeIcons[0] : smallIcons[0];
                if (hIcon == IntPtr.Zero)
                    return null;

                // FromHandle 返回的 Icon 不拥有句柄，Clone 出独立副本后手动销毁原始句柄。
                Icon icon = (Icon)Icon.FromHandle(hIcon).Clone();
                DestroyIcon(hIcon);
                return icon;
            }
            catch
            {
                // 图标提取失败：静默降级，返回 null 由调用方退化为无图标显示。
                return null;
            }
        }

        /// <summary>
        /// &lt;图像型&gt;从 shell32.dll 中提取指定索引的图标并转为 Bitmap，可直接赋给 ToolStripItem.Image
        /// <param name="index">(整数型 图标索引, </param>
        /// <param name="large">逻辑型 是否取大图标)</param>
        /// <para>true 取大图标，false 取小图标</para>
        /// <returns><para>返回图标对应的 Image，失败返回 null</para></returns>
        /// </summary>
        public static Image GetShell32Image(int index, bool large = true)
        {
            try
            {
                using (Icon icon = GetShell32Icon(index, large))
                {
                    return icon == null ? null : icon.ToBitmap();
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// &lt;图像型&gt;按扩展名获取系统关联的文件类型图标
        /// <param name="extension">(文本型 扩展名, </param>
        /// <param name="large">逻辑型 是否取大图标)</param>
        /// <para>参数一：含点如 ".txt"，空或 null 表示无扩展名文件；参数而：true 取大图标 32x32，false 取小图标 16x16。</para>
        /// <para>已关联的扩展名返回对应程序图标；未关联的扩展名由系统返回默认"白纸"图标</para>
        /// <returns><para>返回图标对应的 Image，失败返回 null</para></returns>
        /// </summary>
        public static Image GetFileTypeIcon(string extension, bool large = false)
        {
            try
            {
                // USEFILEATTRIBUTES 模式下无需真实文件，用 "*" + 扩展名 让系统按扩展名查关联图标。
                // 例如 ".txt" -> "*.txt"；空扩展名 -> "*"（返回默认未知文件图标）。
                string query = string.IsNullOrEmpty(extension) ? "*" : "*" + extension;

                SHFILEINFO shfi = new SHFILEINFO();
                uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES | (large ? SHGFI_LARGEICON : SHGFI_SMALLICON);
                IntPtr result = SHGetFileInfo(query, FILE_ATTRIBUTE_NORMAL, ref shfi,
                    (uint)Marshal.SizeOf(typeof(SHFILEINFO)), flags);

                if (result == IntPtr.Zero || shfi.hIcon == IntPtr.Zero)
                    return null;

                using (Icon icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone())
                {
                    DestroyIcon(shfi.hIcon);
                    return icon.ToBitmap();
                }
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
