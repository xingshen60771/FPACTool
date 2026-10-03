// 〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓
//
// 提示：本C#类使用AI辅助生成，因为DDS浏览功能不是本项目的核
// 心功能，不是本项目讨论的重点，只为实现DDS图像的显示。
// 
// 〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓

// SPDX-License-Identifier: CC0-1.0
//
// To the extent possible under law, the contributors have waived all copyright
// and related or neighboring rights to this file under CC0 1.0.
// https://creativecommons.org/publicdomain/zero/1.0/
//
// BC7 is implemented from the published BPTC/BC7 bit layout. The fixed BC7
// partition/anchor tables are format constants. The public-domain alternative
// of Richard Geldreich's bc7 decoder was used as an interoperability reference.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// DDSHelper主要功能：
/// 1. LoadDDS / GetDDSInfo 读取文件字节。
/// 2. 先判断是否为标准 DDS；否则尝试按 LZ4 Frame 解压，解压后必须仍是 DDS。
/// 3. ParseMetadata 解析 DDS 头、DX10 头、像素格式、mip、array/cubemap、payload 大小。
/// 4. 当前支持：DX10 BC7_UNORM / BC7_UNORM_SRGB、Legacy DXT1/BC1、Legacy 32-bit RGB/RGBA。
/// 5. LoadDDS 只解码顶层 mip 的第一个 surface，生成 GDI+ Bitmap 供界面显示。
/// 6. 其余 mip、array、cubemap 面只参与 metadata 统计和 payload 校验，不逐面显示。
/// </summary>
namespace FPACTool
{
    #region DDS 基础辅助类
    /// <summary>
    /// 内部解码路径类别标记
    /// </summary>
    internal enum DDSCompressionKind
    {
        BC1,            // Legacy DXT1/BC1 块压缩：4x4 像素一块，每块 8 字节
        BC7,            // DX10 BC7 块压缩：4x4 像素一块，每块 16 字节
        Uncompressed32  // 未压缩 32-bit RGB/RGBA：每像素 4 字节，按通道掩码取色
    }

    /// <summary>
    /// DDS 元数据容器
    /// <para>保存解析头部得到的全部信息，供信息面板显示与解码前校验使用。不负责解码，只描述“这个 DDS 是什么”</para>。
    /// </summary>
    internal sealed class DDSMetadata
    {
        public string FileName { get; internal set; }           // 源文件名（仅文件名，不含路径）
        public long OriginalFileSize { get; internal set; }     // 原始文件大小
        public string OuterContainer { get; internal set; }     // 外层容器描述，如 "DDS" 或 "LZ4 Frame → DDS"
        public long DDSSize { get; internal set; }              // DDS真实大小
        public int Width { get; internal set; }                 // 顶层宽度（像素）
        public int Height { get; internal set; }                // 顶层高度（像素）
        public int Depth { get; internal set; }                 // 深度，3D 纹理用；2D 通常为 0
        public int MipCount { get; internal set; }              // mip 层数，DDS 头为 0 时按 1 处理
        public string FourCC { get; internal set; }             // DDS PixelFormat 的 FourCC 字符串（无则为空）
        public bool HasDX10Header { get; internal set; }        // 是否有 DX10 扩展头
        public uint DXGIFormat { get; internal set; }           // DX10 扩展头的 DXGI_FORMAT 值（无则为 0）
        public string FormatName { get; internal set; }         // 格式名称，如 "BC7_UNORM"、"BC1 / DXT1"、"BGRA8888" 等
        public uint PixelFormatFlags { get; internal set; }     // DDS_PIXELFORMAT.dwFlags
        public uint RGBBitCount { get; internal set; }          // 每像素位数，未压缩路径使用
        public uint RBitMask { get; internal set; }             // 红色通道掩码
        public uint GBitMask { get; internal set; }             // 绿色通道掩码
        public uint BBitMask { get; internal set; }             // 蓝色通道掩码
        public uint ABitMask { get; internal set; }             // alpha 通道掩码，0 表示无 alpha
        internal DDSCompressionKind CompressionKind { get; set; }// 内部解码分类：BC1 / BC7 / Uncompressed32
        public uint ResourceDimension { get; internal set; }     // DX10 资源维度：2=1D, 3=2D, 4=3D
        public uint MiscFlag { get; internal set; }              // DX10 miscFlag，cubemap 位在其中
        public uint ArraySize { get; internal set; }             // DX10 数组大小
        public uint MiscFlags2 { get; internal set; }            // DX10 miscFlags2，当前仅记录
        public bool IsCubeMap { get; internal set; }             // 是否 cubemap
        public int SurfaceCount { get; internal set; }           // 总 surface 数：array 数 × cubemap 面数
        public int DataOffset { get; internal set; }             // 像素数据起始偏移：128 或 148
        public long ActualPayloadSize { get; internal set; }     // 文件里实际可用的 payload 字节数
        public long ExpectedPayloadSize { get; internal set; }   // 按宽高/mip/格式计算出的应有字节数
        public bool WasLZ4 { get; internal set; }                // 源文件是否为 LZ4 Frame 容器

        /// <summary>
        /// 生成信息面板显示用的多行文本。
        /// <para>依次输出：文件信息 → 尺寸/mip → 资源类型 → 格式 → payload 校验。</para>
        /// </summary>
        public string ToDisplayText()
        {
            // 信息面板文本构建器
            StringBuilder text = new StringBuilder();

            // 文件级信息：文件名、原始大小、外层容器、LZ4 解压后大小
            text.AppendLine("文件名: " + FileName);
            text.AppendLine("原始大小: " + FormatBytes(OriginalFileSize));
            text.AppendLine("外层容器: " + OuterContainer);
            if (WasLZ4)
                text.AppendLine("解压后 DDS: " + FormatBytes(DDSSize));
            text.AppendLine();

            // 尺寸与资源结构：宽高、mip、资源维度、array、cubemap、surface 数
            text.AppendLine("宽度: " + Width);
            text.AppendLine("高度: " + Height);
            text.AppendLine("Mip 数量: " + MipCount);
            text.AppendLine("资源类型: " + GetResourceDimensionName(ResourceDimension));
            text.AppendLine("ArraySize: " + ArraySize);
            text.AppendLine("Cubemap: " + (IsCubeMap ? "是" : "否"));
            text.AppendLine("Surface 数量: " + SurfaceCount);
            text.AppendLine();

            // 格式信息：FourCC、是否 DX10 头、DXGI_FORMAT
            text.AppendLine("FourCC: " + (string.IsNullOrEmpty(FourCC) ? "无" : FourCC));
            text.AppendLine("DX10 Header: " + (HasDX10Header ? "是" : "否"));
            if (HasDX10Header)
                text.AppendLine("DXGI_FORMAT: " + DXGIFormat);

            // 按解码路径分两种展示
            if (CompressionKind == DDSCompressionKind.Uncompressed32)
            {
                text.AppendLine("纹理压缩: 无");
                text.AppendLine("像素格式: " + FormatName);
                text.AppendLine("RGBBitCount: " + RGBBitCount);
                text.AppendLine("R Mask: 0x" + RBitMask.ToString("X8"));
                text.AppendLine("G Mask: 0x" + GBitMask.ToString("X8"));
                text.AppendLine("B Mask: 0x" + BBitMask.ToString("X8"));
                text.AppendLine("A Mask: 0x" + ABitMask.ToString("X8"));
            }
            else
            {
                // 压缩格式（BC1 / BC7）：只显示格式名
                text.AppendLine("纹理压缩: " + FormatName);
            }

            // payload 信息：数据偏移、实际大小、计算大小、校验结果
            text.AppendLine("DDS 数据偏移: " + DataOffset + " bytes");
            text.AppendLine("实际 Payload: " + FormatBytes(ActualPayloadSize));
            text.AppendLine("计算 Payload: " + FormatBytes(ExpectedPayloadSize));
            text.AppendLine("Payload 校验: " + (ActualPayloadSize >= ExpectedPayloadSize ? "正常" : "数据不足"));

            // 先去掉末尾多余换行后再返回
            return text.ToString().TrimEnd();
        }


        /// <summary>
        /// &lt;文本型&gt;将 DX10 资源维度数值转为可读名称
        /// <param name="value">(无符号整数型 欲转换的数值)</param>
        /// <returns><para>返回可读的名称</para></returns>
        /// </summary>
        private static string GetResourceDimensionName(uint value)
        {
            switch (value)
            {
                case 2: return "Texture1D";   // 1D 纹理
                case 3: return "Texture2D";   // 2D 纹理
                case 4: return "Texture3D";   // 3D 纹理
                default: return value == 0 ? "Legacy / 未指定" : "未知(" + value + ")"; // 0 为旧版/未指定，其他为未知
            }
        }

        /// <summary>
        /// &lt;文本型&gt;将字节数格式化为可读的容量字符串
        /// <param name="size">(长整型 欲格式化的字节数)</param>
        /// <returns><para>返回格式化后的字符串，如 "1.50 KB (1536 bytes)"</para></returns>
        /// </summary>
        private static string FormatBytes(long size)
        {
            // 字节级
            if (size < 1024)
                return size + " bytes";
            // KB级
            if (size < 1024L * 1024L)
                return string.Format("{0:N2} KB ({1:N0} bytes)", size / 1024.0, size);
            // MB级
            if (size < 1024L * 1024L * 1024L)
                return string.Format("{0:N2} MB ({1:N0} bytes)", size / 1024.0 / 1024.0, size);            
            // 否则就是GB级
            return string.Format("{0:N2} GB ({1:N0} bytes)", size / 1024.0 / 1024.0 / 1024.0, size);
        }
    }

    /// <summary>
    /// DDS 加载结果
    /// <para>包含解码后的位图与解析出的元数据，并实现 IDisposable 以释放位图资源。</para>
    /// </summary>
    internal sealed class DDSImage : IDisposable
    {
        /// <summary>
        /// &lt;位图型&gt;获取解码后的顶层 Surface 位图。
        /// </summary>
        public Bitmap Bitmap { get; private set; }
        
        /// <summary>
        /// &lt;元数据&gt;获取该 DDS 的解析元数据。
        /// </summary>
        public DDSMetadata Metadata { get; private set; }

        /// <summary>
        /// 创建 DDSImage 实例
        /// <param name="bitmap">(位图型 解码后的位图, </param>
        /// <param name="metadata">元数据 DDS 元数据)</param>
        /// </summary>
        public DDSImage(Bitmap bitmap, DDSMetadata metadata)
        {
            // 无论bitmap还是metadata都不能为null，否则抛出异常
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            // 将传入的bitmap和metadata赋值给属性
            Bitmap = bitmap;
            Metadata = metadata;
        }

        /// <summary>
        /// 释放内部持有的位图资源，并将 Bitmap 置为 null。
        /// </summary>
        public void Dispose()
        {
            if (Bitmap != null)
            {
                Bitmap.Dispose();
                Bitmap = null;
            }
        }
    }
    #endregion

    #region DDS解析与解码辅助类
    internal static class DDSHelper
    {

        #region DDS 常量与标志

        private const uint DDSMagic = 0x20534444u;              // "DDS " 文件魔数
        private const uint FourCCDX10 = 0x30315844u;            // "DX10" DX10 扩展头 FourCC
        private const uint FourCCDXT1 = 0x31545844u;            // "DXT1" Legacy DXT1/BC1 FourCC
        private const uint DDPFAlphaPixels = 0x00000001u;       // 像素格式含 Alpha 通道
        private const uint DDPFFourCC = 0x00000004u;            // 像素格式由 FourCC 描述
        private const uint DDPFRGB = 0x00000040u;               // 未压缩 RGB/RGBA 像素格式
        private const uint DXGIFormatBC7Unorm = 98u;            // DXGI_FORMAT_BC7_UNORM
        private const uint DXGIFormatBC7UnormSrgb = 99u;        // DXGI_FORMAT_BC7_UNORM_SRGB
        private const long MaxPixelCount = 128L * 1024L * 1024L; // 最大允许像素数 128M，防内存爆炸

        #endregion

        #region DDSHelper 公有方法

        /// <summary>
        /// &lt;DDSImage 型&gt;载入 DDS 文件
        /// <param name="fileName">(文本型 DDS 文件路径)</param>
        /// <para>支持自动识别标准 DDS 或 LZ4 Frame(DDS)，解析元数据并解码顶层 Surface 为位图</para>
        /// <returns><para>返回包含解码位图与元数据的 DDSImage 实例</para></returns>
        /// </summary>
        public static DDSImage LoadDDS(string fileName)
        {
            // 文件名空白检查
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("DDS 文件名不能为空。", "fileName");
            // 文件存在性检查
            if (!File.Exists(fileName))
                throw new FileNotFoundException("DDS 文件不存在。", fileName);

            byte[] source = File.ReadAllBytes(fileName);     // 一次性读入整个文件
            bool wasLz4 = false;
            byte[] dds;

            if (IsDDS(source))
            {
                dds = source;                               // 源文件已是标准 DDS，直接使用
            }
            else if (LZ4Helper.IsLZ4Frame(source))
            {
                dds = LZ4Helper.DecodeFrame(source);        // 发现此为 LZ4 Frame 容器，先做解压处理
                wasLz4 = true;
                
                // 异常处置：LZ4已解开，但不是标准 DDS
                if (!IsDDS(dds))
                    throw new InvalidDataException("LZ4 解压成功，但内容不是 DDS。");
            }
            else
            {
                // 异常处置：既不是标准 DDS，也不是可识别的 LZ4 Frame DDS
                throw new InvalidDataException("文件既不是标准 DDS，也不是可识别的 LZ4 Frame DDS。");
            }

            // 开始解析 DDS 元数据，获取宽高、mip、格式等信息
            DDSMetadata metadata = ParseMetadata(
                dds,
                Path.GetFileName(fileName),     // 只取文件名，不含路径
                source.LongLength,              // 原始文件大小
                wasLz4);                        // 是否经过 LZ4 解压

            Bitmap bitmap = DecodeTopLevelSurface(dds, metadata);   // 只解码顶层 mip 的第一个 surface
            return new DDSImage(bitmap, metadata);                  // 封装返回
        }

        /// <summary>
        /// &lt;DDSMetadata&gt;取 DDS 文件信息
        /// <param name="fileName">(文本型 DDS 文件路径)</param>
        /// <returns><para>返回解析出的 DDSMetadata 元数据实例</para></returns>
        /// </summary>
        public static DDSMetadata GetDDSInfo(string fileName)
        {
            // 文件名空白检查
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("DDS 文件名不能为空。", "fileName");
           
            // 文件存在性检查
            if (!File.Exists(fileName))
                throw new FileNotFoundException("DDS 文件不存在。", fileName);

            byte[] source = File.ReadAllBytes(fileName);        // 一次性读入整个文件
            bool wasLz4 = false;
            byte[] dds;

            if (IsDDS(source))
                dds = source;                                   // 源文件已是标准 DDS，直接使用
            else if (LZ4Helper.IsLZ4Frame(source))
            {
                dds = LZ4Helper.DecodeFrame(source);            // 发现此为 LZ4 Frame 容器，先做解压处理
                wasLz4 = true;

                // 异常处置：LZ4已解开，但不是标准 DDS
                if (!IsDDS(dds))
                    throw new InvalidDataException("LZ4 解压成功，但内容不是 DDS。");
            }
            else
                // 异常处置：既不是标准 DDS，也不是可识别的 LZ4 Frame DDS
                throw new InvalidDataException("文件既不是标准 DDS，也不是可识别的 LZ4 Frame DDS。");

            // 只解析元数据，不解码位图
            return ParseMetadata(dds, Path.GetFileName(fileName), source.LongLength, wasLz4);
        }

        #endregion

        #region DDS 内部辅助方法

        /// <summary>
        /// &lt;逻辑型&gt;判断当前字节数组是否以 DDS 魔数开头
        /// <param name="data">(字节集 欲检查的数据)</param>
        /// <returns><para>是返回真，否则返回假</para></returns>
        /// </summary>
        private static bool IsDDS(byte[] data)
        {
            // 非空、至少 4 字节、前 4 字节等于 "DDS "
            return data != null && data.Length >= 4 && ReadUInt32LE(data, 0) == DDSMagic;
        }

        /// <summary>
        /// &lt;DDSMetadata&gt;解析 DDS 字节数组的头部信息
        /// <param name="dds">(字节集 完整的 DDS 数据,</param>
        /// <param name="fileName">文本型 用于显示的文件名,</param>
        /// <param name="originalLength">长整型 原始文件字节数,</param>
        /// <param name="wasLz4">逻辑型 是否经过 LZ4 解压)</param>
        /// <para>主要用途：识别格式，计算 payload 大小并校验，返回元数据实例。</para>
        /// <returns><para>返回解析完成的 DDSMetadata 实例</para></returns>
        /// </summary>
        private static DDSMetadata ParseMetadata(byte[] dds, string fileName, long originalLength, bool wasLz4)
        {
            // 基础长度与魔数校验
            if (dds == null || dds.Length < 128)                                        // 标准 DDS 头至少 128 字节
                throw new InvalidDataException("DDS 文件不足 128 字节。");
            if (!IsDDS(dds))
                throw new InvalidDataException("DDS Magic 错误。");                     // 前 4 字节必须是 "DDS "
            if (ReadUInt32LE(dds, 4) != 124)
                throw new InvalidDataException("DDS_HEADER size 不是 124。");           // DDS_HEADER 固定为 124
            if (ReadUInt32LE(dds, 76) != 32)
                throw new InvalidDataException("DDS_PIXELFORMAT size 不是 32。");       // DDS_PIXELFORMAT 固定为 32

            // 读取宽高并限制最大像素数
            int height = CheckedPositiveInt(ReadUInt32LE(dds, 12), "DDS 高度无效。");    // 偏移 12 为高度
            int width = CheckedPositiveInt(ReadUInt32LE(dds, 16), "DDS 宽度无效。");     // 偏移 16 为宽度
            long pixelCount = checked((long)width * height);                             // 计算总像素数
            if (pixelCount > MaxPixelCount)
                throw new InvalidDataException("DDS 图片尺寸过大，已拒绝分配位图内存。");// 超过 128M 像素直接拒绝

            // 读取 Depth 与 MipCount
            uint rawDepth = ReadUInt32LE(dds, 24);                           // 偏移 24 为深度
            if (rawDepth > int.MaxValue)
                throw new InvalidDataException("DDS Depth 数值无效。");
            int depth = (int)rawDepth;

            uint rawMipCount = ReadUInt32LE(dds, 28);                        // 偏移 28 为 mip 层数
            if (rawMipCount > 32)                                            // 超过 32 层视为异常
                throw new InvalidDataException("DDS Mip 数量无效。");        // 0 表示只有顶层，按 1 处理
            int mipCount = rawMipCount == 0 ? 1 : (int)rawMipCount;

            // 读取像素格式字段
            uint pixelFormatFlags = ReadUInt32LE(dds, 80);   // DDS_PIXELFORMAT.dwFlags
            uint fourCCValue = ReadUInt32LE(dds, 84);        // DDS_PIXELFORMAT.dwFourCC
            uint rgbBitCount = ReadUInt32LE(dds, 88);        // 每像素位数
            uint rBitMask = ReadUInt32LE(dds, 92);           // 红色通道掩码
            uint gBitMask = ReadUInt32LE(dds, 96);           // 绿色通道掩码
            uint bBitMask = ReadUInt32LE(dds, 100);          // 蓝色通道掩码
            uint aBitMask = ReadUInt32LE(dds, 104);          // alpha 通道掩码
            string fourCC = FourCCToString(fourCCValue);     // FourCC 转可读字符串
            bool hasDx10 = (pixelFormatFlags & DDPFFourCC) != 0 && fourCCValue == FourCCDX10; // 是否 DX10 扩展头

            // 声明后续分支所需变量
            uint dxgiFormat = 0;
            uint resourceDimension = 0;
            uint miscFlag = 0;
            uint arraySize = 1;
            uint miscFlags2 = 0;
            bool isCube = false;
            int surfaceCount = 1;
            int dataOffset;
            int blockBytes = 0;
            int bytesPerPixel = 0;
            string formatName;
            DDSCompressionKind compressionKind;

            // 分支一：DX10 扩展头
            if (hasDx10)
            {
                if (dds.Length < 148)
                    throw new InvalidDataException("DX10 DDS Header 被截断。");     // DX10 头额外 20 字节

                dxgiFormat = ReadUInt32LE(dds, 128);          // DXGI_FORMAT
                resourceDimension = ReadUInt32LE(dds, 132);   // 资源维度
                miscFlag = ReadUInt32LE(dds, 136);            // miscFlag
                arraySize = ReadUInt32LE(dds, 140);           // 数组大小
                miscFlags2 = ReadUInt32LE(dds, 144);          // miscFlags2

                if (arraySize == 0)
                    throw new InvalidDataException("DX10 DDS ArraySize 不能为 0。");
                if (resourceDimension != 3)
                    throw new NotSupportedException("DDS 浏览器目前仅支持 Texture2D。");     // 只支持 2D 纹理

                // 只适配 BC7_UNORM 和 BC7_UNORM_SRGB
                if (dxgiFormat == DXGIFormatBC7Unorm)
                    formatName = "BC7_UNORM";
                else if (dxgiFormat == DXGIFormatBC7UnormSrgb)
                    formatName = "BC7_UNORM_SRGB";
                else
                    throw new NotSupportedException(
                        "当前 DX10 DDS 格式尚未适配，DXGI_FORMAT=" + dxgiFormat + "。");

                compressionKind = DDSCompressionKind.BC7;                   // 解码路径为 BC7
                blockBytes = 16;                                            // BC7 每块 16 字节
                dataOffset = 148;                                           // 像素数据从 148 开始

                isCube = (miscFlag & 0x4u) != 0;                            // 判断是否 cubemap
                long surfaceCountLong = isCube
                    ? checked((long)arraySize * 6L)                         // cubemap 有 6 个面
                    : arraySize;
                if (surfaceCountLong <= 0 || surfaceCountLong > int.MaxValue)
                    throw new InvalidDataException("DDS Surface 数量无效。");
                surfaceCount = (int)surfaceCountLong;
            }
            // 分支二：Legacy DXT1 / BC1
            else if ((pixelFormatFlags & DDPFFourCC) != 0 && fourCCValue == FourCCDXT1)
            {
                // Legacy DDS: DXT1 是 BC1 块压缩的旧称
                compressionKind = DDSCompressionKind.BC1;
                formatName = "BC1 / DXT1";
                blockBytes = 8;            // BC1 每块 8 字节
                dataOffset = 128;          // 像素数据从 128 开始

                GetLegacySurfaceInfo(dds, "Legacy DXT1", out isCube, out surfaceCount);     // 处理 cubemap/volume
                resourceDimension = 3;
                arraySize = 1;
            }
            // 分支三：Legacy 未压缩 32-bit RGB/RGBA
            else if ((pixelFormatFlags & DDPFRGB) != 0 && rgbBitCount == 32)
            {
                // Legacy 未压缩 RGB/RGBA DDS。FourCC 通常为 0。
                // 通道顺序由 bit mask 描述，因此 RGBA8888/BGRA8888/BGRX8888 可共用一套解码器。
                ValidateLegacy32BitMasks(rBitMask, gBitMask, bBitMask, aBitMask);       // 校验掩码合法性

                compressionKind = DDSCompressionKind.Uncompressed32;
                formatName = GetLegacy32BitFormatName(rBitMask, gBitMask, bBitMask, aBitMask);
                bytesPerPixel = 4;          // 每像素 4 字节
                dataOffset = 128;

                GetLegacySurfaceInfo(dds, "Legacy 32-bit RGB/RGBA", out isCube, out surfaceCount);
                resourceDimension = 3;
                arraySize = 1;
            }
            // 其他格式：暂不支持
            else
            {
                string fourCCDescription = string.IsNullOrEmpty(fourCC) ? "无" : fourCC;
                throw new NotSupportedException(
                    "DDS 格式尚未适配。FourCC=" + fourCCDescription +
                    "，PixelFormatFlags=0x" + pixelFormatFlags.ToString("X8") +
                    "，RGBBitCount=" + rgbBitCount +
                    "。当前支持 DX10 BC7、Legacy DXT1/BC1 与 Legacy 32-bit RGB/RGBA。");
            }

            //逐 mip 计算单个 surface 的字节数
            long perSurfaceBytes = 0;
            for (int mip = 0; mip < mipCount; mip++)
            {
                int mipWidth = Math.Max(1, width >> mip);     // 每级 mip 宽度减半，最小为 1
                int mipHeight = Math.Max(1, height >> mip);   // 每级 mip 高度减半，最小为 1
                long mipBytes = compressionKind == DDSCompressionKind.Uncompressed32
                    ? GetLinearMipSize(mipWidth, mipHeight, bytesPerPixel)   // 未压缩：宽 × 高 × 每像素字节数
                    : GetBCMipSize(mipWidth, mipHeight, blockBytes);         // 压缩：按 4x4 块计算
                perSurfaceBytes = checked(perSurfaceBytes + mipBytes);
            }

            // 计算并校验总 payload
            long expectedPayload = checked(perSurfaceBytes * surfaceCount);     // 期望总字节数
            long actualPayload = dds.LongLength - dataOffset;                   // 实际可用字节数
            if (actualPayload < expectedPayload)
                throw new InvalidDataException(
                    "DDS " + formatName + " Payload 被截断。");                 // 数据不足，抛出异常

            // 构造并返回元数据
            return new DDSMetadata
            {
                // 文件级信息
                FileName = fileName,                                   // 源文件名（仅文件名，不含路径）
                OriginalFileSize = originalLength,                     // 原始文件字节数（LZ4 时为压缩前大小）
                OuterContainer = wasLz4 ? "LZ4 Frame → DDS" : "DDS",   // 外层容器描述：标准 DDS 或 LZ4 解压而来
                DDSSize = dds.LongLength,                              // 真正的 DDS 字节数（LZ4 解压后的大小）

                //尺寸与 mip
                Width = width,                                         // 顶层宽度（像素）
                Height = height,                                       // 顶层高度（像素）
                Depth = depth,                                         // 深度，3D 纹理用；2D 通常为 0
                MipCount = mipCount,                                   // mip 层数，头中为 0 时已按 1 处理

                // 格式标识
                FourCC = fourCC,                                       // FourCC 字符串，无则为空
                HasDX10Header = hasDx10,                               // 是否存在 DX10 扩展头
                DXGIFormat = dxgiFormat,                               // DX10 头中的 DXGI_FORMAT 值，非 DX10 时为 0
                FormatName = formatName,                               // 可读格式名，如 "BC7_UNORM"、"BC1 / DXT1"

                // 像素格式字段（Legacy 路径使用）
                PixelFormatFlags = pixelFormatFlags,                   // DDS_PIXELFORMAT.dwFlags
                RGBBitCount = rgbBitCount,                             // 每像素位数，未压缩路径使用
                RBitMask = rBitMask,                                   // 红色通道掩码
                GBitMask = gBitMask,                                   // 绿色通道掩码
                BBitMask = bBitMask,                                   // 蓝色通道掩码
                ABitMask = aBitMask,                                   // alpha 通道掩码，0 表示无 alpha

                // 内部解码分类
                CompressionKind = compressionKind,                     // 解码路径：BC1 / BC7 / Uncompressed32

                // DX10 资源结构字段
                ResourceDimension = resourceDimension,                 // DX10 资源维度：2=1D, 3=2D, 4=3D
                MiscFlag = miscFlag,                                   // DX10 miscFlag，cubemap 位在其中
                ArraySize = arraySize,                                 // DX10 数组大小
                MiscFlags2 = miscFlags2,                               // DX10 miscFlags2，当前仅记录
                IsCubeMap = isCube,                                    // 是否 cubemap
                SurfaceCount = surfaceCount,                           // 总 surface 数：array 数 × cubemap 面数

                // 数据布局与校验
                DataOffset = dataOffset,                               // 像素数据起始偏移：128 或 148
                ActualPayloadSize = actualPayload,                     // 文件里实际可用的 payload 字节数
                ExpectedPayloadSize = expectedPayload,                 // 按宽高/mip/格式计算出的应有字节数
                WasLZ4 = wasLz4                                        // 源文件是否为 LZ4 Frame 容器
            };
        }
        #endregion

        #region 各种Surface 解码器
        /// <summary>
        /// &lt;位图型&gt;根据元数据中的压缩类型分派到对应的顶层 Surface 解码器
        /// <param name="dds">(字节集 完整的 DDS 数据,</param>
        /// <param name="metadata">DDSMetadata 已解析的元数据)</param>
        /// <returns><para>返回解码后的顶层 Surface 位图</para></returns>
        /// </summary>
        private static Bitmap DecodeTopLevelSurface(byte[] dds, DDSMetadata metadata)
        {
            // 根据解析阶段确定的解码路径分派
            if (metadata.CompressionKind == DDSCompressionKind.BC1)
                return DecodeBC1TopLevelSurface(dds, metadata);          // BC1 / DXT1 块压缩解码

            if (metadata.CompressionKind == DDSCompressionKind.BC7)
                return DecodeBC7TopLevelSurface(dds, metadata);          // BC7 块压缩解码

            if (metadata.CompressionKind == DDSCompressionKind.Uncompressed32)
                return DecodeUncompressed32TopLevelSurface(dds, metadata); // 未压缩 32-bit RGB/RGBA 解码

            throw new NotSupportedException("DDS 纹理格式尚未实现。");   // 理论上不会到达，ParseMetadata 已过滤
        }

        /// <summary>
        /// &lt;位图型&gt;解码 Legacy 未压缩 32-bit RGB/RGBA 的顶层 Surface
        /// <param name="dds">(字节集 完整的 DDS 数据，</param>
        /// <param name="metadata">DDSMetadata 已解析的元数据)</param>
        /// <returns><para>返回解码后的顶层 Surface 位图</para></returns>
        /// </summary>
        private static Bitmap DecodeUncompressed32TopLevelSurface(byte[] dds, DDSMetadata metadata)
        {
            int width = metadata.Width;                                  // 顶层宽度
            int height = metadata.Height;                                // 顶层高度

            // 计算顶层未压缩数据大小并校验是否越界
            long required = GetLinearMipSize(width, height, 4);          // 未压缩 32-bit：宽 × 高 × 4 字节
            if ((long)metadata.DataOffset + required > dds.LongLength)
                throw new InvalidDataException("DDS 顶层 32-bit RGB/RGBA 数据被截断。"); // 数据不足，拒绝解码

            // 创建 GDI+ 32bppArgb 位图
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bits = null;

            try
            {
                // 锁定内存，准备直接写入像素
                bits = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                int rowBytes = checked(width * 4);                       // 每行字节数：宽 × 4
                byte[] row = new byte[rowBytes];                         // 复用的行缓冲
                int sourceOffset = metadata.DataOffset;                  // 源数据起始偏移

                // 逐行逐像素解码
                for (int y = 0; y < height; y++)
                {
                    int rowSource = sourceOffset + checked(y * rowBytes); // 当前行在源数据中的起始位置

                    for (int x = 0; x < width; x++)
                    {
                        uint packed = ReadUInt32LE(dds, rowSource + x * 4); // 读取 packed 32-bit 像素
                        int dest = x * 4;

                        // 按通道掩码提取各通道。GDI+ Format32bppArgb 内存顺序为 B,G,R,A
                        row[dest + 0] = ExtractMaskedChannel(packed, metadata.BBitMask, 0);    // 蓝色通道
                        row[dest + 1] = ExtractMaskedChannel(packed, metadata.GBitMask, 0);    // 绿色通道
                        row[dest + 2] = ExtractMaskedChannel(packed, metadata.RBitMask, 0);    // 红色通道
                        row[dest + 3] = ExtractMaskedChannel(packed, metadata.ABitMask, 255);  // alpha 通道，无掩码时默认 255
                    }

                    // 将当前行拷贝到 Bitmap 对应行
                    IntPtr destination = IntPtr.Add(bits.Scan0, checked(y * bits.Stride));     // 目标行地址
                    Marshal.Copy(row, 0, destination, rowBytes);                              // 整行拷贝
                }
            }
            catch
            {
                bitmap.Dispose();                                        // 解码失败时释放位图，避免泄漏
                throw;
            }
            finally
            {
                if (bits != null)
                    bitmap.UnlockBits(bits);                             // 确保解锁
            }

            return bitmap;
        }

        /// <summary>
        /// &lt;位图型&gt;解码 Legacy BC1/DXT1 的顶层 Surface
        /// <param name="dds">(字节集 完整的 DDS 数据,</param>
        /// <param name="metadata">(DDSMetadata 已解析的元数据)</param>
        /// <returns><para>返回解码后的顶层 Surface 位图</para></returns>
        /// </summary>
        private static Bitmap DecodeBC1TopLevelSurface(byte[] dds, DDSMetadata metadata)
        {
            int width = metadata.Width;                              // 顶层宽度
            int height = metadata.Height;                            // 顶层高度

            // ---- 计算块数与所需字节数 ----
            int blocksX = Math.Max(1, (width + 3) / 4);              // 水平方向 4x4 块数，向上取整
            int blocksY = Math.Max(1, (height + 3) / 4);             // 垂直方向 4x4 块数，向上取整
            long required = checked((long)blocksX * blocksY * 8L);   // BC1 每块 8 字节
            if ((long)metadata.DataOffset + required > dds.LongLength)
                throw new InvalidDataException("DDS 顶层 BC1/DXT1 数据被截断。");   // 数据不足，拒绝解码

            // ---- 创建 GDI+ 32bppArgb 位图 ----
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bits = null;

            try
            {
                // ---- 锁定内存，准备直接写入像素 ----
                bits = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                int rowBytes = checked(width * 4);                          // 每行字节数
                byte[] fourRows = new byte[checked(rowBytes * 4)];          // 一次攒够 4 行的缓冲
                Pixel[] blockPixels = new Pixel[16];                        // 复用的块像素缓冲，每块 16 像素
                int blockOffset = metadata.DataOffset;                      // 源数据游标

                // ---- 逐块行解码 ----
                for (int blockY = 0; blockY < blocksY; blockY++)
                {
                    Array.Clear(fourRows, 0, fourRows.Length);              // 每轮清空 4 行缓冲

                    // ---- 逐块列解码 ----
                    for (int blockX = 0; blockX < blocksX; blockX++)
                    {
                        DecodeBC1Block(dds, blockOffset, blockPixels);      // 解码一个 4x4 BC1 块
                        blockOffset += 8;                                   // 游标前进 8 字节

                        // ---- 把块内 16 像素写入 4 行缓冲 ----
                        for (int py = 0; py < 4; py++)
                        {
                            int y = blockY * 4 + py;
                            if (y >= height)
                                break;                                      // 超出实际高度，跳过

                            int rowBase = py * rowBytes;
                            for (int px = 0; px < 4; px++)
                            {
                                int x = blockX * 4 + px;
                                if (x >= width)
                                    break;                                  // 超出实际宽度，跳过

                                Pixel pixel = blockPixels[py * 4 + px];
                                int dest = rowBase + x * 4;
                                fourRows[dest + 0] = pixel.B;               // GDI+ 内存顺序 B,G,R,A
                                fourRows[dest + 1] = pixel.G;
                                fourRows[dest + 2] = pixel.R;
                                fourRows[dest + 3] = pixel.A;
                            }
                        }
                    }

                    // ---- 一次性把 4 行缓冲拷贝到 Bitmap ----
                    CopyFourRowsToBitmap(bits, fourRows, rowBytes, blockY, height);
                }
            }
            catch
            {
                bitmap.Dispose();                                           // 解码失败时释放位图，避免泄漏
                throw;
            }
            finally
            {
                if (bits != null)
                    bitmap.UnlockBits(bits);                                // 确保解锁
            }

            return bitmap;
        }

        /// <summary>
        /// &lt;位图型&gt;解码 DX10 BC7 的顶层 Surface
        /// <param name="dds">(字节集 完整的 DDS 数据,</param>
        /// <param name="metadata">DDSMetadata 型 已解析的元数据)</param>
        /// <returns><para>返回解码后的顶层 Surface 位图</para></returns>
        /// </summary>
        private static Bitmap DecodeBC7TopLevelSurface(byte[] dds, DDSMetadata metadata)
        {
            int width = metadata.Width;                              // 顶层宽度
            int height = metadata.Height;                            // 顶层高度

            // ---- 计算块数与所需字节数 ----
            int blocksX = Math.Max(1, (width + 3) / 4);              // 水平方向 4x4 块数，向上取整
            int blocksY = Math.Max(1, (height + 3) / 4);             // 垂直方向 4x4 块数，向上取整
            long required = checked((long)blocksX * blocksY * 16L);  // BC7 每块 16 字节
            if ((long)metadata.DataOffset + required > dds.LongLength)
                throw new InvalidDataException("DDS 顶层 BC7 数据被截断。");   // 数据不足，拒绝解码

            // ---- 创建 GDI+ 32bppArgb 位图 ----
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bits = null;

            try
            {
                // ---- 锁定内存，准备直接写入像素 ----
                bits = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                int rowBytes = checked(width * 4);                          // 每行字节数
                byte[] fourRows = new byte[checked(rowBytes * 4)];          // 一次攒够 4 行的缓冲
                Bc7Workspace workspace = new Bc7Workspace();                // 复用的 BC7 解码工作区
                int blockOffset = metadata.DataOffset;                      // 源数据游标

                // ---- 逐块行解码 ----
                for (int blockY = 0; blockY < blocksY; blockY++)
                {
                    Array.Clear(fourRows, 0, fourRows.Length);              // 每轮清空 4 行缓冲

                    // ---- 逐块列解码 ----
                    for (int blockX = 0; blockX < blocksX; blockX++)
                    {
                        // 解码一个 4x4 BC7 块；mode 无效或保留时直接抛异常
                        if (!DecodeBC7Block(dds, blockOffset, workspace))
                            throw new InvalidDataException(
                                "BC7 Block 包含无效或保留的 Mode。数据偏移: " + blockOffset + "。");
                        blockOffset += 16;                                  // 游标前进 16 字节

                        // ---- 把块内 16 像素写入 4 行缓冲 ----
                        for (int py = 0; py < 4; py++)
                        {
                            int y = blockY * 4 + py;
                            if (y >= height)
                                break;                                      // 超出实际高度，跳过

                            int rowBase = py * rowBytes;
                            for (int px = 0; px < 4; px++)
                            {
                                int x = blockX * 4 + px;
                                if (x >= width)
                                    break;                                  // 超出实际宽度，跳过

                                Pixel pixel = workspace.Pixels[py * 4 + px];
                                int dest = rowBase + x * 4;
                                fourRows[dest + 0] = pixel.B;               // GDI+ 内存顺序 B,G,R,A
                                fourRows[dest + 1] = pixel.G;
                                fourRows[dest + 2] = pixel.R;
                                fourRows[dest + 3] = pixel.A;
                            }
                        }
                    }

                    // ---- 一次性把 4 行缓冲拷贝到 Bitmap ----
                    CopyFourRowsToBitmap(bits, fourRows, rowBytes, blockY, height);
                }
            }
            catch
            {
                bitmap.Dispose();                                           // 解码失败时释放位图，避免泄漏
                throw;
            }
            finally
            {
                if (bits != null)
                    bitmap.UnlockBits(bits);                                // 确保解锁
            }

            return bitmap;
        }

        #endregion

        #region BC1 块解码辅助

        /// <summary>
        /// 将攒好的 4 行像素缓冲一次性拷贝到 Bitmap 对应行
        /// <param name="bits">(BitmapData 已锁定的位图内存, </param>
        /// <param name="fourRows">字节集 攒满 4 行的像素缓冲, </param>
        /// <param name="rowBytes">整数型 每行字节数, </param>
        /// <param name="blockY">整数型 当前块行号, </param>
        /// <param name="height">整数型 图像总高度) </param>
        /// </summary>
        private static void CopyFourRowsToBitmap(
            BitmapData bits,
            byte[] fourRows,
            int rowBytes,
            int blockY,
            int height)
        {
            for (int py = 0; py < 4; py++)                                     // 逐行拷贝，最多 4 行
            {
                int y = blockY * 4 + py;                                       // 实际像素行号
                if (y >= height)
                    break;                                                     // 超出图像高度，结束

                IntPtr destination = IntPtr.Add(bits.Scan0, checked(y * bits.Stride));  // 目标行起始地址
                Marshal.Copy(fourRows, py * rowBytes, destination, rowBytes);             // 整行拷贝
            }
        }

        /// <summary>
        /// 解码一个 4x4 的 Legacy BC1/DXT1 块，填充 16 个像素
        /// <param name="data">(字节集 完整的 DDS 数据, </param>
        /// <param name="offset">整数型 块数据起始偏移, </param>
        /// <param name="output">(Pixel 数组 至少 16 个像素的输出缓冲)</param>
        /// </summary>
        private static void DecodeBC1Block(byte[] data, int offset, Pixel[] output)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (output == null || output.Length < 16)
                throw new ArgumentException("BC1 输出缓冲区至少需要 16 个像素。", "output");
            if (offset < 0 || offset > data.Length - 8)
                throw new InvalidDataException("BC1 Block 数据被截断。");

            ushort color0 = ReadUInt16LE(data, offset);           // 端点颜色 0（RGB565 编码）
            ushort color1 = ReadUInt16LE(data, offset + 2);       // 端点颜色 1（RGB565 编码）
            uint indices = ReadUInt32LE(data, offset + 4);        // 16 个像素的 2-bit 索引，共 32 位

            Pixel palette0 = DecodeRgb565(color0);                // 调色板颜色 0
            Pixel palette1 = DecodeRgb565(color1);                // 调色板颜色 1
            Pixel palette2;
            Pixel palette3;

            if (color0 > color1)
            {
                // 端点 0 > 1：4 色模式，palette2/3 由两端点 2:1、1:2 插值，不透明
                palette2 = InterpolateBC1(palette0, palette1, 2, 1, 3, 255);
                palette3 = InterpolateBC1(palette0, palette1, 1, 2, 3, 255);
            }
            else
            {
                // 端点 0 <= 1：3 色模式，palette2 为两端点 1:1 插值，palette3 为透明黑
                palette2 = InterpolateBC1(palette0, palette1, 1, 1, 2, 255);
                palette3 = new Pixel(0, 0, 0, 0);
            }

            for (int pixel = 0; pixel < 16; pixel++)
            {
                int paletteIndex = (int)((indices >> (pixel * 2)) & 0x3u);   // 取出第 pixel 个像素的 2-bit 索引
                switch (paletteIndex)
                {
                    case 0:
                        output[pixel] = palette0;
                        break;
                    case 1:
                        output[pixel] = palette1;
                        break;
                    case 2:
                        output[pixel] = palette2;
                        break;
                    default:
                        output[pixel] = palette3;
                        break;
                }
            }
        }

        /// <summary>
        /// &lt;像素型&gt;将 RGB565 值解码为 8-bit 的 RGBA 不透明像素
        /// <param name="value">(无符号短整型 RGB565 编码的颜色值)</param>
        /// <returns><para>返回解码后的不透明像素</para></returns>
        /// </summary>
        private static Pixel DecodeRgb565(ushort value)
        {
            int r5 = (value >> 11) & 0x1F;                        // 高 5 位：红色
            int g6 = (value >> 5) & 0x3F;                         // 中 6 位：绿色
            int b5 = value & 0x1F;                                // 低 5 位：蓝色

            int r = (r5 << 3) | (r5 >> 2);                        // 5-bit 扩 8-bit：左移 3 位 + 高 3 位填充
            int g = (g6 << 2) | (g6 >> 4);                        // 6-bit 扩 8-bit：左移 2 位 + 高 2 位填充
            int b = (b5 << 3) | (b5 >> 2);                        // 5-bit 扩 8-bit：左移 3 位 + 高 3 位填充

            return new Pixel(r, g, b, 255);                       // 返回不透明像素
        }

        /// <summary>
        /// &lt;像素型&gt;按权重插值两个 BC1 端点颜色
        /// <param name="first">(像素型 第一个端点颜色)</param>
        /// <param name="second">(像素型 第二个端点颜色)</param>
        /// <param name="firstWeight">(整数型 第一个端点的权重)</param>
        /// <param name="secondWeight">(整数型 第二个端点的权重)</param>
        /// <param name="divisor">(整数型 权重分母)</param>
        /// <param name="alpha">(整数型 插值结果的 alpha 值)</param>
        /// <returns><para>返回加权平均后的像素</para></returns>
        /// </summary>
        private static Pixel InterpolateBC1(
            Pixel first,
            Pixel second,
            int firstWeight,
            int secondWeight,
            int divisor,
            int alpha)
        {
            return new Pixel(
                (first.R * firstWeight + second.R * secondWeight) / divisor,
                (first.G * firstWeight + second.G * secondWeight) / divisor,
                (first.B * firstWeight + second.B * secondWeight) / divisor,
                alpha);
        }

        #endregion

        #region Legacy 通道与掩码工具

        /// <summary>
        /// 解析 Legacy DDS 头的 caps2 字段
        /// <param name="dds">(字节集 完整的 DDS 数据, </param>
        /// <param name="formatDescription">文本型 格式描述，用于异常提示, </param>
        /// <param name="isCube">逻辑型 输出 是否为 cubemap, </param>
        /// <param name="surfaceCount">整数型 输出 surface 总数)</param>
        /// </summary>
        private static void GetLegacySurfaceInfo(
            byte[] dds,
            string formatDescription,
            out bool isCube,
            out int surfaceCount)
        {
            uint caps2 = ReadUInt32LE(dds, 112);                  // DDS 头偏移 112 处的 dwCaps2
            const uint DDSCAPS2_CUBEMAP = 0x00000200u;            // cubemap 总标志
            const uint DDSCAPS2_CUBEMAP_POSITIVEX = 0x00000400u;  // +X 面
            const uint DDSCAPS2_CUBEMAP_NEGATIVEX = 0x00000800u;  // -X 面
            const uint DDSCAPS2_CUBEMAP_POSITIVEY = 0x00001000u;  // +Y 面
            const uint DDSCAPS2_CUBEMAP_NEGATIVEY = 0x00002000u;  // -Y 面
            const uint DDSCAPS2_CUBEMAP_POSITIVEZ = 0x00004000u;  // +Z 面
            const uint DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x00008000u;  // -Z 面
            const uint DDSCAPS2_VOLUME = 0x00200000u;             // volume 纹理标志

            if ((caps2 & DDSCAPS2_VOLUME) != 0)
                throw new NotSupportedException(formatDescription + " Volume Texture 尚未适配。");

            isCube = (caps2 & DDSCAPS2_CUBEMAP) != 0;
            surfaceCount = 1;
            if (!isCube)
                return;

            // 统计实际设置的面标志位
            uint faceMask = caps2 & (
                DDSCAPS2_CUBEMAP_POSITIVEX |
                DDSCAPS2_CUBEMAP_NEGATIVEX |
                DDSCAPS2_CUBEMAP_POSITIVEY |
                DDSCAPS2_CUBEMAP_NEGATIVEY |
                DDSCAPS2_CUBEMAP_POSITIVEZ |
                DDSCAPS2_CUBEMAP_NEGATIVEZ);

            int faces = CountBits(faceMask);
            // 部分 DDS 写入器只设置 CUBEMAP 总标志而不细分面，此时按完整 6 面处理。
            surfaceCount = faces == 0 ? 6 : faces;
        }

        /// <summary>
        /// 校验 Legacy 32-bit 的 RGBA 通道掩码是否合法
        /// <param name="rMask">(无符号整数型 红色通道掩码, </param>
        /// <param name="gMask">无符号整数型 绿色通道掩码, </param>
        /// <param name="bMask">无符号整数型 蓝色通道掩码, </param>
        /// <param name="aMask">无符号整数型 alpha 通道掩码)</param>
        /// </summary>
        private static void ValidateLegacy32BitMasks(
            uint rMask,
            uint gMask,
            uint bMask,
            uint aMask)
        {
            if (rMask == 0 || gMask == 0 || bMask == 0)
                throw new NotSupportedException("Legacy 32-bit RGB DDS 的 RGB Channel Mask 不能为 0。");

            // RGB 三通道之间不能重叠
            uint rgbOverlap = (rMask & gMask) | (rMask & bMask) | (gMask & bMask);
            if (rgbOverlap != 0 || (aMask != 0 && ((aMask & rMask) != 0 || (aMask & gMask) != 0 || (aMask & bMask) != 0)))
                throw new NotSupportedException("Legacy 32-bit RGB DDS 的 Channel Mask 发生重叠，暂不支持。");

            // 每个通道掩码必须是连续的一段位
            if (!IsContiguousMask(rMask) || !IsContiguousMask(gMask) ||
                !IsContiguousMask(bMask) || (aMask != 0 && !IsContiguousMask(aMask)))
                throw new NotSupportedException("Legacy 32-bit RGB DDS 使用了非连续 Channel Mask，暂不支持。");
        }

        /// <summary>
        /// &lt;逻辑型&gt;判断一个位掩码是否为连续的一段 1
        /// <param name="mask">(无符号整数型 待判断的掩码)</param>
        /// <returns><para>掩码为连续段则返回 true，否则返回 false</para></returns>
        /// </summary>
        private static bool IsContiguousMask(uint mask)
        {
            if (mask == 0)
                return false;                                       // 空掩码视为不合法

            while ((mask & 1u) == 0)
                mask >>= 1;                                         // 跳过末尾的 0

            while ((mask & 1u) != 0)
                mask >>= 1;                                         // 跳过连续的 1

            return mask == 0;                                       // 全部处理完说明是连续段
        }

        /// <summary>
        /// &lt;文本型&gt;根据通道掩码识别 Legacy 32-bit 的像素格式名称
        /// <param name="rMask">(无符号整数型 红色通道掩码, </param>
        /// <param name="gMask">无符号整数型 绿色通道掩码, </param>
        /// <param name="bMask">无符号整数型 蓝色通道掩码, </param>
        /// <param name="aMask">无符号整数型 alpha 通道掩码)</param>
        /// <returns><para>返回格式名称字符串</para></returns>
        /// </summary>
        private static string GetLegacy32BitFormatName(
            uint rMask,
            uint gMask,
            uint bMask,
            uint aMask)
        {
            if (rMask == 0x000000FFu && gMask == 0x0000FF00u &&
                bMask == 0x00FF0000u && aMask == 0xFF000000u)
                return "RGBA8888 (Legacy 32-bit)";

            if (rMask == 0x00FF0000u && gMask == 0x0000FF00u &&
                bMask == 0x000000FFu && aMask == 0xFF000000u)
                return "BGRA8888 (Legacy 32-bit)";

            if (rMask == 0x00FF0000u && gMask == 0x0000FF00u &&
                bMask == 0x000000FFu && aMask == 0)
                return "BGRX8888 (Legacy 32-bit)";

            return aMask == 0
                ? "RGB 32-bit (Channel Mask)"
                : "RGBA 32-bit (Channel Mask)";
        }

        /// <summary>
        /// &lt;字节型&gt;从打包像素中按掩码提取单个通道并扩展到 8-bit
        /// <param name="packed">(无符号整数型 打包的像素值, </param>
        /// <param name="mask">无符号整数型 通道掩码, </param>
        /// <param name="defaultValue">字节型 掩码为 0 时的默认值)</param>
        /// <returns><para>返回扩展后的 8-bit 通道值</para></returns>
        /// </summary>
        private static byte ExtractMaskedChannel(uint packed, uint mask, byte defaultValue)
        {
            if (mask == 0)
                return defaultValue;                                // 无该通道，返回默认值

            // 计算掩码最低位的位移量
            int shift = 0;
            uint shiftedMask = mask;
            while ((shiftedMask & 1u) == 0)
            {
                shiftedMask >>= 1;
                shift++;
            }

            // 统计掩码连续 1 的位数
            int bits = 0;
            uint valueMask = shiftedMask;
            while ((valueMask & 1u) != 0)
            {
                bits++;
                valueMask >>= 1;
            }

            uint raw = (packed & mask) >> shift;                    // 取出通道原始值并右移到低位
            if (bits >= 8)
            {
                if (bits == 8)
                    return (byte)raw;                               // 正好 8 位，直接返回

                ulong maximum = bits == 32 ? uint.MaxValue : ((1UL << bits) - 1UL);
                return (byte)((raw * 255UL + maximum / 2UL) / maximum);   // 按比例缩放到 8-bit
            }

            uint max = (1u << bits) - 1u;
            return (byte)((raw * 255u + max / 2u) / max);           // 低位宽按比例缩放到 8-bit
        }

        #endregion

        #region 尺寸计算与数据读取工具

        /// <summary>
        /// &lt;长整型&gt;计算线性（未压缩）格式单层 mip 的字节数
        /// <param name="width">(整数型 该层宽度, </param>
        /// <param name="height">整数型 该层高度, </param>
        /// <param name="bytesPerPixel">整数型 每像素字节数)</param>
        /// <returns><para>返回该层 mip 的字节数</para></returns>
        /// </summary>
        private static long GetLinearMipSize(int width, int height, int bytesPerPixel)
        {
            return checked((long)width * height * bytesPerPixel);   // checked 防止乘法溢出
        }

        /// <summary>
        /// &lt;长整型&gt;计算块压缩（BC1/BC7）格式单层 mip 的字节数
        /// <param name="width">(整数型 该层宽度, </param>
        /// <param name="height">整数型 该层高度, </param>
        /// <param name="blockBytes">整数型 每块字节数)</param>
        /// <returns><para>返回该层 mip 的字节数</para></returns>
        /// </summary>
        private static long GetBCMipSize(int width, int height, int blockBytes)
        {
            long blocksX = Math.Max(1L, (width + 3L) / 4L);        // 水平 4x4 块数，向上取整，至少 1 块
            long blocksY = Math.Max(1L, (height + 3L) / 4L);       // 垂直 4x4 块数，向上取整，至少 1 块
            return checked(blocksX * blocksY * blockBytes);        // checked 防止乘法溢出
        }

        /// <summary>
        /// &lt;无符号短整型&gt;以小端序读取 2 字节整数
        /// <param name="data">(字节集 数据源, </param>
        /// <param name="offset">整数型 起始偏移)</param>
        /// <returns><para>返回读取到的 16 位无符号整数</para></returns>
        /// </summary>
        private static ushort ReadUInt16LE(byte[] data, int offset)
        {
            if (data == null || offset < 0 || offset > data.Length - 2)
                throw new InvalidDataException("DDS 数据被截断。");

            return (ushort)(data[offset] | (data[offset + 1] << 8));   // 低字节在前，高字节在后
        }

        /// <summary>
        /// &lt;整数型&gt;统计一个无符号整数中二进制 1 的个数
        /// <param name="value">(无符号整数型 待统计的值)</param>
        /// <returns><para>返回 1 的个数</para></returns>
        /// </summary>
        private static int CountBits(uint value)
        {
            int count = 0;
            while (value != 0)
            {
                value &= value - 1;      // 每次消除最低位的 1
                count++;
            }
            return count;
        }

        /// <summary>
        /// &lt;整数型&gt;将无符号整数安全转换为正整数，越界则抛异常
        /// <param name="value">(无符号整数型 待转换的值, </param>
        /// <param name="message">文本型 越界时的异常提示)</param>
        /// <returns><para>返回转换后的 int 值</para></returns>
        /// </summary>
        private static int CheckedPositiveInt(uint value, string message)
        {
            if (value == 0 || value > int.MaxValue)
                throw new InvalidDataException(message);   // 0 或超过 int 上限都视为非法
            return (int)value;
        }

        /// <summary>
        /// &lt;无符号整数型&gt;以小端序读取 4 字节整数
        /// <param name="data">(字节集 数据源, </param>
        /// <param name="offset">整数型 起始偏移)</param>
        /// <returns><para>返回读取到的 32 位无符号整数</para></returns>
        /// </summary>
        private static uint ReadUInt32LE(byte[] data, int offset)
        {
            if (data == null || offset < 0 || offset > data.Length - 4)
                throw new InvalidDataException("DDS 数据被截断。");
            return (uint)(data[offset]
                | (data[offset + 1] << 8)
                | (data[offset + 2] << 16)
                | (data[offset + 3] << 24));               // 低字节在前，逐字节左移
        }

        /// <summary>
        /// &lt;文本型&gt;将 FourCC 数值转为 4 字符可读字符串
        /// <param name="value">(无符号整数型 FourCC 值)</param>
        /// <returns><para>返回可读字符串，0 则返回空串</para></returns>
        /// </summary>
        private static string FourCCToString(uint value)
        {
            if (value == 0)
                return string.Empty;

            char[] chars = new char[4];
            chars[0] = ToPrintableFourCCChar((byte)(value & 0xFF));          // 第 1 个字符
            chars[1] = ToPrintableFourCCChar((byte)((value >> 8) & 0xFF));   // 第 2 个字符
            chars[2] = ToPrintableFourCCChar((byte)((value >> 16) & 0xFF));  // 第 3 个字符
            chars[3] = ToPrintableFourCCChar((byte)((value >> 24) & 0xFF));  // 第 4 个字符
            return new string(chars);
        }

        /// <summary>
        /// &lt;字符型&gt;将字节转为可打印字符
        /// <param name="value">(字节型 待转换的字节)</param>
        /// <para>非打印字符用 '?' 代替</para>
        /// <returns><para>返回可打印字符</para></returns>
        /// </summary>
        private static char ToPrintableFourCCChar(byte value)
        {
            return value >= 0x20 && value <= 0x7E ? (char)value : '?';   // 只接受可见 ASCII
        }

        #endregion

        #region BC7 decoder

        // ---- 以下为 BC7 格式固定常量表，源自公开 BPTC/BC7 位布局规范 ----

        // 权重表：2/3/4-bit 索引对应的插值权重（0~64）
        private static readonly int[] Weights2 = { 0, 21, 43, 64 };
        private static readonly int[] Weights3 = { 0, 9, 18, 27, 37, 46, 55, 64 };
        private static readonly int[] Weights4 = { 0, 4, 9, 13, 17, 21, 26, 30, 34, 38, 43, 47, 51, 55, 60, 64 };

        // 分区表 Partition2：2 子集模式的 64 种分区，每项 16 个像素的归属子集编号
        private static readonly byte[] Partition2 =
        {
            0,0,1,1,0,0,1,1,0,0,1,1,0,0,1,1, 0,0,0,1,0,0,0,1,0,0,0,1,0,0,0,1, 0,1,1,1,0,1,1,1,0,1,1,1,0,1,1,1, 0,0,0,1,0,0,1,1,0,0,1,1,0,1,1,1, 0,0,0,0,0,0,0,1,0,0,0,1,0,0,1,1, 0,0,1,1,0,1,1,1,0,1,1,1,1,1,1,1, 0,0,0,1,0,0,1,1,0,1,1,1,1,1,1,1, 0,0,0,0,0,0,0,1,0,0,1,1,0,1,1,1,
            0,0,0,0,0,0,0,0,0,0,0,1,0,0,1,1, 0,0,1,1,0,1,1,1,1,1,1,1,1,1,1,1, 0,0,0,0,0,0,0,1,0,1,1,1,1,1,1,1, 0,0,0,0,0,0,0,0,0,0,0,1,0,1,1,1, 0,0,0,1,0,1,1,1,1,1,1,1,1,1,1,1, 0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1, 0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1, 0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,
            0,0,0,0,1,0,0,0,1,1,1,0,1,1,1,1, 0,1,1,1,0,0,0,1,0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0,1,0,0,0,1,1,1,0, 0,1,1,1,0,0,1,1,0,0,0,1,0,0,0,0, 0,0,1,1,0,0,0,1,0,0,0,0,0,0,0,0, 0,0,0,0,1,0,0,0,1,1,0,0,1,1,1,0, 0,0,0,0,0,0,0,0,1,0,0,0,1,1,0,0, 0,1,1,1,0,0,1,1,0,0,1,1,0,0,0,1,
            0,0,1,1,0,0,0,1,0,0,0,1,0,0,0,0, 0,0,0,0,1,0,0,0,1,0,0,0,1,1,0,0, 0,1,1,0,0,1,1,0,0,1,1,0,0,1,1,0, 0,0,1,1,0,1,1,0,0,1,1,0,1,1,0,0, 0,0,0,1,0,1,1,1,1,1,1,0,1,0,0,0, 0,0,0,0,1,1,1,1,1,1,1,1,0,0,0,0, 0,1,1,1,0,0,0,1,1,0,0,0,1,1,1,0, 0,0,1,1,1,0,0,1,1,0,0,1,1,1,0,0,
            0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1, 0,0,0,0,1,1,1,1,0,0,0,0,1,1,1,1, 0,1,0,1,1,0,1,0,0,1,0,1,1,0,1,0, 0,0,1,1,0,0,1,1,1,1,0,0,1,1,0,0, 0,0,1,1,1,1,0,0,0,0,1,1,1,1,0,0, 0,1,0,1,0,1,0,1,1,0,1,0,1,0,1,0, 0,1,1,0,1,0,0,1,0,1,1,0,1,0,0,1, 0,1,0,1,1,0,1,0,1,0,1,0,0,1,0,1,
            0,1,1,1,0,0,1,1,1,1,0,0,1,1,1,0, 0,0,0,1,0,0,1,1,1,1,0,0,1,0,0,0, 0,0,1,1,0,0,1,0,0,1,0,0,1,1,0,0, 0,0,1,1,1,0,1,1,1,1,0,1,1,1,0,0, 0,1,1,0,1,0,0,1,1,0,0,1,0,1,1,0, 0,0,1,1,1,1,0,0,1,1,0,0,0,0,1,1, 0,1,1,0,0,1,1,0,1,0,0,1,1,0,0,1, 0,0,0,0,0,1,1,0,0,1,1,0,0,0,0,0,
            0,1,0,0,1,1,1,0,0,1,0,0,0,0,0,0, 0,0,1,0,0,1,1,1,0,0,1,0,0,0,0,0, 0,0,0,0,0,0,1,0,0,1,1,1,0,0,1,0, 0,0,0,0,0,1,0,0,1,1,1,0,0,1,0,0, 0,1,1,0,1,1,0,0,1,0,0,1,0,0,1,1, 0,0,1,1,0,1,1,0,1,1,0,0,1,0,0,1, 0,1,1,0,0,0,1,1,1,0,0,1,1,1,0,0, 0,0,1,1,1,0,0,1,1,1,0,0,0,1,1,0,
            0,1,1,0,1,1,0,0,1,1,0,0,1,0,0,1, 0,1,1,0,0,0,1,1,0,0,1,1,1,0,0,1, 0,1,1,1,1,1,1,0,1,0,0,0,0,0,0,1, 0,0,0,1,1,0,0,0,1,1,1,0,0,1,1,1, 0,0,0,0,1,1,1,1,0,0,1,1,0,0,1,1, 0,0,1,1,0,0,1,1,1,1,1,1,0,0,0,0, 0,0,1,0,0,0,1,0,1,1,1,0,1,1,1,0, 0,1,0,0,0,1,0,0,0,1,1,1,0,1,1,1
        };

        // 分区表 Partition3：3 子集模式的 64 种分区，每项 16 个像素的归属子集编号
        private static readonly byte[] Partition3 =
        {
            0,0,1,1,0,0,1,1,0,2,2,1,2,2,2,2, 0,0,0,1,0,0,1,1,2,2,1,1,2,2,2,1, 0,0,0,0,2,0,0,1,2,2,1,1,2,2,1,1, 0,2,2,2,0,0,2,2,0,0,1,1,0,1,1,1, 0,0,0,0,0,0,0,0,1,1,2,2,1,1,2,2, 0,0,1,1,0,0,1,1,0,0,2,2,0,0,2,2, 0,0,2,2,0,0,2,2,1,1,1,1,1,1,1,1, 0,0,1,1,0,0,1,1,2,2,1,1,2,2,1,1,
            0,0,0,0,0,0,0,0,1,1,1,1,2,2,2,2, 0,0,0,0,1,1,1,1,1,1,1,1,2,2,2,2, 0,0,0,0,1,1,1,1,2,2,2,2,2,2,2,2, 0,0,1,2,0,0,1,2,0,0,1,2,0,0,1,2, 0,1,1,2,0,1,1,2,0,1,1,2,0,1,1,2, 0,1,2,2,0,1,2,2,0,1,2,2,0,1,2,2, 0,0,1,1,0,1,1,2,1,1,2,2,1,2,2,2, 0,0,1,1,2,0,0,1,2,2,0,0,2,2,2,0,
            0,0,0,1,0,0,1,1,0,1,1,2,1,1,2,2, 0,1,1,1,0,0,1,1,2,0,0,1,2,2,0,0, 0,0,0,0,1,1,2,2,1,1,2,2,1,1,2,2, 0,0,2,2,0,0,2,2,0,0,2,2,1,1,1,1, 0,1,1,1,0,1,1,1,0,2,2,2,0,2,2,2, 0,0,0,1,0,0,0,1,2,2,2,1,2,2,2,1, 0,0,0,0,0,0,1,1,0,1,2,2,0,1,2,2, 0,0,0,0,1,1,0,0,2,2,1,0,2,2,1,0,
            0,1,2,2,0,1,2,2,0,0,1,1,0,0,0,0, 0,0,1,2,0,0,1,2,1,1,2,2,2,2,2,2, 0,1,1,0,1,2,2,1,1,2,2,1,0,1,1,0, 0,0,0,0,0,1,1,0,1,2,2,1,1,2,2,1, 0,0,2,2,1,1,0,2,1,1,0,2,0,0,2,2, 0,1,1,0,0,1,1,0,2,0,0,2,2,2,2,2, 0,0,1,1,0,1,2,2,0,1,2,2,0,0,1,1, 0,0,0,0,2,0,0,0,2,2,1,1,2,2,2,1,
            0,0,0,0,0,0,0,2,1,1,2,2,1,2,2,2, 0,2,2,2,0,0,2,2,0,0,1,2,0,0,1,1, 0,0,1,1,0,0,1,2,0,0,2,2,0,2,2,2, 0,1,2,0,0,1,2,0,0,1,2,0,0,1,2,0, 0,0,0,0,1,1,1,1,2,2,2,2,0,0,0,0, 0,1,2,0,1,2,0,1,2,0,1,2,0,1,2,0, 0,1,2,0,2,0,1,2,1,2,0,1,0,1,2,0, 0,0,1,1,2,2,0,0,1,1,2,2,0,0,1,1,
            0,0,1,1,1,1,2,2,2,2,0,0,0,0,1,1, 0,1,0,1,0,1,0,1,2,2,2,2,2,2,2,2, 0,0,0,0,0,0,0,0,2,1,2,1,2,1,2,1, 0,0,2,2,1,1,2,2,0,0,2,2,1,1,2,2, 0,0,2,2,0,0,1,1,0,0,2,2,0,0,1,1, 0,2,2,0,1,2,2,1,0,2,2,0,1,2,2,1, 0,1,0,1,2,2,2,2,2,2,2,2,0,1,0,1, 0,0,0,0,2,1,2,1,2,1,2,1,2,1,2,1,
            0,1,0,1,0,1,0,1,0,1,0,1,2,2,2,2, 0,2,2,2,0,1,1,1,0,2,2,2,0,1,1,1, 0,0,0,2,1,1,1,2,0,0,0,2,1,1,1,2, 0,0,0,0,2,1,1,2,2,1,1,2,2,1,1,2, 0,2,2,2,0,1,1,1,0,1,1,1,0,2,2,2, 0,0,0,2,1,1,1,2,1,1,1,2,0,0,0,2, 0,1,1,0,0,1,1,0,0,1,1,0,2,2,2,2, 0,0,0,0,0,0,0,0,2,1,1,2,2,1,1,2,
            0,1,1,0,0,1,1,0,2,2,2,2,2,2,2,2, 0,0,2,2,0,0,1,1,0,0,1,1,0,0,2,2, 0,0,2,2,1,1,2,2,1,1,2,2,0,0,2,2, 0,0,0,0,0,0,0,0,0,0,0,0,2,1,1,2, 0,0,0,2,0,0,0,1,0,0,0,2,0,0,0,1, 0,2,2,2,1,2,2,2,0,2,2,2,1,2,2,2, 0,1,0,1,2,2,2,2,2,2,2,2,2,2,2,2, 0,1,1,1,2,0,1,1,2,2,0,1,2,2,2,0
        };

        // 锚点表 Anchor2：2 子集模式下各分区的锚点索引（权重少 1 位）
        private static readonly byte[] Anchor2 =
        {
            15,15,15,15,15,15,15,15, 15,15,15,15,15,15,15,15,
            15,2,8,2,2,8,8,15, 2,8,2,2,8,8,2,2,
            15,15,6,8,2,8,15,15, 2,8,2,2,2,15,15,6,
            6,2,6,8,15,15,2,2, 15,15,15,15,15,2,2,15
        };

        // 锚点表 Anchor31：3 子集模式下第 1 子集的锚点索引
        private static readonly byte[] Anchor31 =
        {
            3,3,15,15,8,3,15,15, 8,8,6,6,6,5,3,3,
            3,3,8,15,3,3,6,10, 5,8,8,6,8,5,15,15,
            8,15,3,5,6,10,8,15, 15,3,15,5,15,15,15,15,
            3,15,5,5,5,8,5,10, 5,10,8,13,15,12,3,3
        };

        // 锚点表 Anchor32：3 子集模式下第 2 子集的锚点索引
        private static readonly byte[] Anchor32 =
        {
            15,8,8,3,15,15,3,8, 15,15,15,15,15,15,15,8,
            15,8,15,3,15,8,15,8, 3,15,6,10,15,15,10,8,
            15,3,15,10,10,8,9,10, 6,15,8,15,3,6,6,8,
            15,3,15,15,15,15,15,15, 15,15,15,15,3,15,15,8
        };

        /// <summary>
        /// &lt;逻辑型&gt;解码一个 4x4 BC7 块
        /// <param name="data">(字节集 完整的 DDS 数据, </param>
        /// <param name="offset">整数型 块数据起始偏移, </param>
        /// <param name="ws">Bc7Workspace 复用的解码工作区)</param>
        /// <para>按 mode 分派到对应解码函数</para>
        /// <returns><para>解码成功返回 true，模式无效或数据越界返回 false</para></returns>
        /// </summary>
        private static bool DecodeBC7Block(byte[] data, int offset, Bc7Workspace ws)
        {
            if (offset < 0 || offset > data.Length - 16)
                return false;                                       // 块数据不足，返回失败

            int firstByte = data[offset];
            int mode = -1;
            for (int i = 0; i < 8; i++)
            {
                if ((firstByte & (1 << i)) != 0)                    // 模式编码：最低位第一个 1 的位号
                {
                    mode = i;
                    break;
                }
            }
            if (mode < 0)
                return false;                                       // 无有效模式位，返回失败

            switch (mode)
            {
                case 0:
                case 2:
                    return DecodeMode0Or2(data, offset, mode, ws);
                case 1:
                case 3:
                case 7:
                    return DecodeMode1Or3Or7(data, offset, mode, ws);
                case 4:
                case 5:
                    return DecodeMode4Or5(data, offset, mode, ws);
                case 6:
                    return DecodeMode6(data, offset, ws);
                default:
                    return false;
            }
        }

        /// <summary>
        /// &lt;逻辑型&gt;解码 BC7 Mode 0 或 Mode 2
        /// <param name="data">(字节集 完整的 DDS 数据, </param>
        /// <param name="offset">整数型 块数据起始偏移, </param>
        /// <param name="mode">整数型 模式号，0 或 2, </param>
        /// <param name="ws">Bc7Workspace 复用工作区)</param>
        /// <para>3 子集模式</para>
        /// <returns><para>解码成功返回 true，位流不合法返回 false</para></returns>
        /// </summary>
        private static bool DecodeMode0Or2(byte[] data, int offset, int mode, Bc7Workspace ws)
        {
            BitReader reader = new BitReader(data, offset);
            if (reader.ReadBits(mode + 1) != (1 << mode))
                return false;                                       // 模式位校验失败

            int partition = reader.ReadBits(mode == 0 ? 4 : 6);     // 分区索引，mode0=4 位，mode2=6 位
            int weightBits = mode == 0 ? 3 : 2;                     // 权重位数
            int endpointBits = mode == 0 ? 4 : 5;                   // 端点位数
            int pbitCount = mode == 0 ? 6 : 0;                      // p-bit 个数，mode2 无 p-bit
            int weightValues = 1 << weightBits;                     // 权重取值数

            Array.Clear(ws.Endpoints, 0, ws.Endpoints.Length);
            for (int component = 0; component < 3; component++)     // 3 个颜色分量，6 个端点
                for (int endpoint = 0; endpoint < 6; endpoint++)
                    ws.Endpoints[endpoint * 4 + component] = reader.ReadBits(endpointBits);

            Array.Clear(ws.PBits, 0, ws.PBits.Length);
            for (int p = 0; p < pbitCount; p++)
                ws.PBits[p] = reader.ReadBits(1);                   // 读取 p-bit

            for (int i = 0; i < 16; i++)
            {
                bool anchor = i == 0 || i == Anchor31[partition] || i == Anchor32[partition];
                ws.Weights[i] = reader.ReadBits(anchor ? weightBits - 1 : weightBits);   // 锚点权重少 1 位
            }

            if (reader.BitOffset != 128)
                return false;                                       // 位流未读完 128 位，格式错

            for (int endpoint = 0; endpoint < 6; endpoint++)
            {
                for (int component = 0; component < 3; component++)
                {
                    int raw = ws.Endpoints[endpoint * 4 + component];
                    ws.Endpoints[endpoint * 4 + component] = pbitCount != 0
                        ? Dequant(raw, ws.PBits[endpoint], endpointBits)   // 带 p-bit 反量化
                        : Dequant(raw, endpointBits);                     // 无 p-bit 反量化
                }
                ws.Endpoints[endpoint * 4 + 3] = 255;               // 本模式无 alpha，置不透明
            }

            for (int subset = 0; subset < 3; subset++)              // 3 个子集各自建立调色板
            {
                int e0 = subset * 2;
                int e1 = e0 + 1;
                for (int w = 0; w < weightValues; w++)
                {
                    int paletteIndex = subset * 16 + w;
                    ws.Palette[paletteIndex] = new Pixel(
                        Interpolate(ws.Endpoints[e0 * 4 + 0], ws.Endpoints[e1 * 4 + 0], w, weightBits),
                        Interpolate(ws.Endpoints[e0 * 4 + 1], ws.Endpoints[e1 * 4 + 1], w, weightBits),
                        Interpolate(ws.Endpoints[e0 * 4 + 2], ws.Endpoints[e1 * 4 + 2], w, weightBits),
                        255);
                }
            }

            for (int i = 0; i < 16; i++)
            {
                int subset = Partition3[partition * 16 + i];        // 查分区表取子集
                ws.Pixels[i] = ws.Palette[subset * 16 + ws.Weights[i]];
            }
            return true;
        }

        /// <summary>
        /// &lt;逻辑型&gt;解码 BC7 Mode 
        /// <param name="data">(字节集 完整的 DDS 数据, </param>
        /// <param name="offset">整数型 块数据起始偏移, </param>
        /// <param name="mode">整数型 模式号, </param>
        /// <param name="ws">(Bc7Workspace 复用工作区)</param>
        /// <para>1 / 3 / 7,2 子集模式</para>
        /// <returns><para>解码成功返回 true，位流不合法返回 false</para></returns>
        /// </summary>
        private static bool DecodeMode1Or3Or7(byte[] data, int offset, int mode, Bc7Workspace ws)
        {
            BitReader reader = new BitReader(data, offset);
            if (reader.ReadBits(mode + 1) != (1 << mode))
                return false;                                       // 模式位校验失败

            int partition = reader.ReadBits(6);                     // 分区索引，6 位
            int components = mode == 7 ? 4 : 3;                     // mode7 含 alpha，其余 3 分量
            int weightBits = mode == 1 ? 3 : 2;                     // 权重位数
            int endpointBits = mode == 7 ? 5 : (mode == 1 ? 6 : 7); // 端点位数
            int pbitCount = mode == 1 ? 2 : 4;                      // p-bit 个数
            bool sharedPBits = mode == 1;                           // mode1 的 p-bit 为共享
            int weightValues = 1 << weightBits;

            Array.Clear(ws.Endpoints, 0, ws.Endpoints.Length);
            for (int component = 0; component < components; component++)
                for (int endpoint = 0; endpoint < 4; endpoint++)
                    ws.Endpoints[endpoint * 4 + component] = reader.ReadBits(endpointBits);

            Array.Clear(ws.PBits, 0, ws.PBits.Length);
            for (int p = 0; p < pbitCount; p++)
                ws.PBits[p] = reader.ReadBits(1);                   // 读取 p-bit

            for (int i = 0; i < 16; i++)
            {
                bool anchor = i == 0 || i == Anchor2[partition];    // 锚点权重少 1 位
                ws.Weights[i] = reader.ReadBits(anchor ? weightBits - 1 : weightBits);
            }

            if (reader.BitOffset != 128)
                return false;                                       // 位流未读完 128 位，格式错

            for (int endpoint = 0; endpoint < 4; endpoint++)
            {
                int pbit = ws.PBits[sharedPBits ? endpoint >> 1 : endpoint];  // 共享 p-bit 时两两共用
                for (int component = 0; component < components; component++)
                {
                    int raw = ws.Endpoints[endpoint * 4 + component];
                    ws.Endpoints[endpoint * 4 + component] = Dequant(raw, pbit, endpointBits);
                }
                if (components == 3)
                    ws.Endpoints[endpoint * 4 + 3] = 255;           // 无 alpha 分量时置不透明
            }

            for (int subset = 0; subset < 2; subset++)              // 2 个子集各自建立调色板
            {
                int e0 = subset * 2;
                int e1 = e0 + 1;
                for (int w = 0; w < weightValues; w++)
                {
                    int paletteIndex = subset * 16 + w;
                    ws.Palette[paletteIndex] = new Pixel(
                        Interpolate(ws.Endpoints[e0 * 4 + 0], ws.Endpoints[e1 * 4 + 0], w, weightBits),
                        Interpolate(ws.Endpoints[e0 * 4 + 1], ws.Endpoints[e1 * 4 + 1], w, weightBits),
                        Interpolate(ws.Endpoints[e0 * 4 + 2], ws.Endpoints[e1 * 4 + 2], w, weightBits),
                        components == 4
                            ? Interpolate(ws.Endpoints[e0 * 4 + 3], ws.Endpoints[e1 * 4 + 3], w, weightBits)
                            : 255);
                }
            }

            for (int i = 0; i < 16; i++)
            {
                int subset = Partition2[partition * 16 + i];        // 查分区表取子集
                ws.Pixels[i] = ws.Palette[subset * 16 + ws.Weights[i]];
            }
            return true;
        }

        /// <summary>
        /// &lt;逻辑型&gt;解码 BC7 Mode 4 或 Mode 5
        /// <param name="data">(字节集 完整的 DDS 数据, </param>
        /// <param name="offset">整数型 块数据起始偏移, </param>
        /// <param name="mode">整数型 模式号，4 或 5, </param>
        /// <param name="ws">Bc7Workspace 复用工作区)</param>
        /// <para>含旋转的独立分量模式</para>
        /// <returns><para>解码成功返回 true，位流不合法返回 false</para></returns>
        /// </summary>
        private static bool DecodeMode4Or5(byte[] data, int offset, int mode, Bc7Workspace ws)
        {
            BitReader reader = new BitReader(data, offset);
            if (reader.ReadBits(mode + 1) != (1 << mode))
                return false;                                       // 模式位校验失败

            int rotation = reader.ReadBits(2);                      // 旋转模式 0~3
            int indexMode = mode == 4 ? reader.ReadBits(1) : 0;     // mode4 有索引交换位，mode5 无
            int rgbIndexBits = 2;
            int alphaIndexBits = mode == 4 ? 3 : 2;
            int rgbEndpointBits = mode == 4 ? 5 : 7;
            int alphaEndpointBits = mode == 4 ? 6 : 8;

            Array.Clear(ws.Endpoints, 0, ws.Endpoints.Length);
            for (int component = 0; component < 4; component++)
            {
                int bits = component == 3 ? alphaEndpointBits : rgbEndpointBits;   // alpha 用独立位宽
                for (int endpoint = 0; endpoint < 2; endpoint++)
                    ws.Endpoints[endpoint * 4 + component] = reader.ReadBits(bits);
            }

            // 索引交换时，RGB 与 alpha 的索引位数互换
            int colorBits = indexMode != 0 ? alphaIndexBits : rgbIndexBits;
            int alphaBits = indexMode != 0 ? rgbIndexBits : alphaIndexBits;

            int[] firstTarget = indexMode != 0 ? ws.AlphaWeights : ws.Weights;
            int firstBits = indexMode != 0 ? alphaBits : colorBits;
            for (int i = 0; i < 16; i++)
                firstTarget[i] = reader.ReadBits(i == 0 ? firstBits - 1 : firstBits);   // 首像素少 1 位

            int[] secondTarget = indexMode != 0 ? ws.Weights : ws.AlphaWeights;
            int secondBits = indexMode != 0 ? colorBits : alphaBits;
            for (int i = 0; i < 16; i++)
                secondTarget[i] = reader.ReadBits(i == 0 ? secondBits - 1 : secondBits);

            if (reader.BitOffset != 128)
                return false;                                       // 位流未读完 128 位，格式错

            for (int endpoint = 0; endpoint < 2; endpoint++)
            {
                for (int component = 0; component < 3; component++)
                    ws.Endpoints[endpoint * 4 + component] = Dequant(ws.Endpoints[endpoint * 4 + component], rgbEndpointBits);
                ws.Endpoints[endpoint * 4 + 3] = Dequant(ws.Endpoints[endpoint * 4 + 3], alphaEndpointBits);
            }

            for (int i = 0; i < 16; i++)
            {
                Pixel p = new Pixel(
                    Interpolate(ws.Endpoints[0], ws.Endpoints[4], ws.Weights[i], colorBits),
                    Interpolate(ws.Endpoints[1], ws.Endpoints[5], ws.Weights[i], colorBits),
                    Interpolate(ws.Endpoints[2], ws.Endpoints[6], ws.Weights[i], colorBits),
                    Interpolate(ws.Endpoints[3], ws.Endpoints[7], ws.AlphaWeights[i], alphaBits));

                if (rotation == 1)                                  // 旋转 1：交换 R 与 A
                {
                    byte temp = p.A; p.A = p.R; p.R = temp;
                }
                else if (rotation == 2)                             // 旋转 2：交换 G 与 A
                {
                    byte temp = p.A; p.A = p.G; p.G = temp;
                }
                else if (rotation == 3)                             // 旋转 3：交换 B 与 A
                {
                    byte temp = p.A; p.A = p.B; p.B = temp;
                }

                ws.Pixels[i] = p;
            }
            return true;
        }

        /// <summary>
        /// &lt;逻辑型&gt;解码 BC7 Mode 6
        /// <param name="data">(字节集 完整的 DDS 数据, </param>
        /// <param name="offset">(整数型 块数据起始偏移, </param>
        /// <param name="ws">Bc7Workspace 复用工作区)</param>
        /// <para>单分区带 alpha 模式</para>
        /// <returns><para>解码成功返回 true，位流不合法返回 false</para></returns>
        /// </summary>
        private static bool DecodeMode6(byte[] data, int offset, Bc7Workspace ws)
        {
            BitReader reader = new BitReader(data, offset);
            if (reader.ReadBits(7) != 64)
                return false;                                       // 模式位校验失败（7 位全 1）

            int r0 = reader.ReadBits(7);                            // 端点颜色分量，各 7 位
            int r1 = reader.ReadBits(7);
            int g0 = reader.ReadBits(7);
            int g1 = reader.ReadBits(7);
            int b0 = reader.ReadBits(7);
            int b1 = reader.ReadBits(7);
            int a0 = reader.ReadBits(7);
            int a1 = reader.ReadBits(7);
            int p0 = reader.ReadBits(1);                            // 共享 p-bit
            int p1 = reader.ReadBits(1);

            // 将 p-bit 拼入各端点的每个分量，扩展为 8 位
            r0 = (r0 << 1) | p0;
            g0 = (g0 << 1) | p0;
            b0 = (b0 << 1) | p0;
            a0 = (a0 << 1) | p0;
            r1 = (r1 << 1) | p1;
            g1 = (g1 << 1) | p1;
            b1 = (b1 << 1) | p1;
            a1 = (a1 << 1) | p1;

            for (int i = 0; i < 16; i++)
            {
                int index = reader.ReadBits(i == 0 ? 3 : 4);        // 首像素 3 位，其余 4 位
                int weight = Weights4[index];                       // 查 4-bit 权重表
                int inverse = 64 - weight;                          // 反权重
                ws.Pixels[i] = new Pixel(
                    (r0 * inverse + r1 * weight + 32) >> 6,          // 加权平均 + 四舍五入
                    (g0 * inverse + g1 * weight + 32) >> 6,
                    (b0 * inverse + b1 * weight + 32) >> 6,
                    (a0 * inverse + a1 * weight + 32) >> 6);
            }

            return reader.BitOffset == 128;                         // 位流必须正好读完
        }

        /// <summary>
        /// &lt;整数型&gt;带 p-bit 的反量化
        /// <param name="value">(整数型 原始端点值, </param>
        /// <param name="pbit">整数型 附加的 p-bit, </param>
        /// <param name="valueBits">整数型 原始端点位数)</param>
        /// <para>将低位宽数值扩展到 8-bit</para>
        /// <returns><para>返回扩展到 8-bit 的数值</para></returns>
        /// </summary>
        private static int Dequant(int value, int pbit, int valueBits)
        {
            int totalBits = valueBits + 1;
            value = (value << 1) | pbit;          // 拼入 p-bit 作为最低位
            value <<= 8 - totalBits;              // 左移填充到 8 位
            value |= value >> totalBits;          // 用高位移填补低位
            return value & 0xFF;
        }

        /// <summary>
        /// &lt;整数型&gt;无 p-bit 的反量化：
        /// <param name="value">(整数型 原始端点值, </param>
        /// <param name="valueBits">整数型 原始端点位数)</param>
        /// <para>将低位宽数值扩展到 8-bit</para>
        /// <returns><para>返回扩展到 8-bit 的数值</para></returns>
        /// </summary>
        private static int Dequant(int value, int valueBits)
        {
            value <<= 8 - valueBits;              // 左移填充到 8 位
            value |= value >> valueBits;          // 用高位移填补低位
            return value & 0xFF;
        }

        /// <summary>
        /// &lt;整数型&gt;按索引位宽在两个端点间做加权插值
        /// <param name="low">(整数型 低端点值, </param>
        /// <param name="high">整数型 高端点值, </param>
        /// <param name="index">整数型 插值索引, </param>
        /// <param name="indexBits">整数型 索引位宽)</param>
        /// <para>索引位宽，2/3/4</para>
        /// <returns><para>返回插值结果</para></returns>
        /// </summary>
        private static int Interpolate(int low, int high, int index, int indexBits)
        {
            int weight;
            if (indexBits == 2)
                weight = Weights2[index];
            else if (indexBits == 3)
                weight = Weights3[index];
            else if (indexBits == 4)
                weight = Weights4[index];
            else
                throw new InvalidDataException("BC7 Index 位数无效。");

            return (low * (64 - weight) + high * weight + 32) >> 6;   // 加权平均 + 四舍五入
        }

        /// <summary>
        /// BC7 解码用的 RGBA 像素结构
        /// </summary>
        private struct Pixel
        {
            public byte R;
            public byte G;
            public byte B;
            public byte A;

            public Pixel(int r, int g, int b, int a)
            {
                R = (byte)r;      // 构造时截断为字节
                G = (byte)g;
                B = (byte)b;
                A = (byte)a;
            }
        }

        /// <summary>
        /// BC7 解码的复用工作区，避免每块重复分配内存
        /// </summary>
        private sealed class Bc7Workspace
        {
            public readonly int[] Endpoints = new int[24];     // 端点值：6 端点 × 4 分量
            public readonly int[] PBits = new int[6];          // p-bit 数组
            public readonly int[] Weights = new int[16];       // RGB 权重索引
            public readonly int[] AlphaWeights = new int[16];  // alpha 权重索引（mode4/5 用）
            public readonly Pixel[] Palette = new Pixel[48];   // 调色板：3 子集 × 16 项
            public readonly Pixel[] Pixels = new Pixel[16];    // 解码出的 16 个像素
        }

        /// <summary>
        /// BC7 位流读取器：支持从任意偏移按位读取，最多 128 位并做越界检查
        /// </summary>
        private struct BitReader
        {
            private readonly byte[] data;        // 源数据
            private readonly int baseOffset;     // 块的起始偏移
            private int bitOffset;               // 当前读取到的位偏移

            public BitReader(byte[] data, int baseOffset)
            {
                this.data = data;
                this.baseOffset = baseOffset;
                bitOffset = 0;
            }

            public int BitOffset { get { return bitOffset; } }   // 只读暴露当前位偏移

            public int ReadBits(int count)
            {
                if (count < 0 || count > 24 || bitOffset + count > 128)
                    throw new InvalidDataException("BC7 Block 位流越界。");
                if (count == 0)
                    return 0;

                int result = 0;
                int written = 0;
                while (written < count)
                {
                    int byteIndex = baseOffset + (bitOffset >> 3);    // 当前字节位置
                    int bitInByte = bitOffset & 7;                    // 字节内位偏移
                    int take = Math.Min(count - written, 8 - bitInByte);   // 本次可读位数
                    int mask = (1 << take) - 1;
                    int value = (data[byteIndex] >> bitInByte) & mask;
                    result |= value << written;                       // 拼入结果
                    bitOffset += take;
                    written += take;
                }
                return result;
            }
        }

        #endregion
    }
    #endregion
}
