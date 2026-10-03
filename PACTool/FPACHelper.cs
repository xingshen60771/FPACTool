using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FPACTool
{
    /// <summary>
    /// PAC打包解包操作类
    /// </summary>
    internal class FPACHelper
    {
        /*
         * 此为FPAC打包解包操作类，支持打包和解包功能。
         * 文件哈希值和打包方法参照其他网友的研究结果，解包方法为本代码作者
         * 独立研究。打包功能根据我的理解和AI给出的思路编写，可能存在文件名
         * 哈希值错误导致游戏闪退的bug，望包涵。建议把解包后的文件直接放在
         * 游戏根目录下使用，确需打包的请务必备份好原文件。
         * 仅向提供研究资料的网友表示感谢！
         * 参考资料：https://github.com/coinkillerl/FPACker/blob/master/README.md
         */

        #region 常量定义
        private const uint FPAC_MAGIC = 1128353862; // 0x46504143 "FPAC"
        private const int HEADER_SIZE = 16;         // PAC文件头大小
        private const int ENTRY_SIZE = 32;          // 每个文件条目大小
        #endregion

        #region 数据结构
        /// <summary>
        /// FPAC文件头结构
        /// </summary>
        private struct FpacHeader
        {
            public uint Magic;                      // 文件魔数
            public uint FileCount;                  // 文件数量
            public uint FirstFileOffset;            // 第一个文件数据的偏移地址
            public uint Unknown;                    // 未知字段，疑似预留的的加密类型标记
        }

        /// <summary>
        /// FPAC文件条目结构
        /// </summary>
        private struct FpacFileInfo
        {
            public uint FilenameCrc32;              // 文件名CRC32（非真实CRC32，须异或0xFFFFFFFF）
            public uint Unknown;                    // 未知字段，疑似预留的的加密类型标记
            public ulong FilenameOffset;            // 文件名字符串的偏移地址
            public ulong FileSize;                  // 文件实际大小
            public ulong FileOffset;                // 文件数据的偏移地址
        }

        /// <summary>
        /// 内部文件信息
        /// </summary>
        private class FpacFile
        {
            public FpacFileInfo FileInfo;           // 文件条目信息
            public string Filename;                 // 文件绝对路径
            public string RelativePath;             // 文件相对路径
            public byte[] FileContents;             // 文件内容
        }
        #endregion

        #region 文件状态回调
        /// <summary>
        /// 文件状态输出
        /// </summary>
        public class FileProgress
        {
            public int percentage { get; set; }     //打包解包进程百分比
            public string state { get; set; }       //打包解包状态
            public string error { get; set; }       //非空表示本文件提取失败的原因（异步时单文件失败不影响整体继续）
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// &lt;文本型&gt; 取相对路径
        /// <param name="basePath">(文本型 欲获取的基础路径, </param>
        /// <param name="fullPath">文本型 完整路径)</param>
        /// <returns><para>返回相对路径</para></returns>
        /// </summary>
        private static string GetRelativePath(string basePath, string fullPath)
        {
            if (string.IsNullOrEmpty(basePath))
                throw new ArgumentNullException(nameof(basePath));
            if (string.IsNullOrEmpty(fullPath))
                throw new ArgumentNullException(nameof(fullPath));

            // 确保路径以目录分隔符结尾
            if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
                !basePath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                basePath += Path.DirectorySeparatorChar;
            }

            Uri baseUri = new Uri(basePath);
            Uri fullUri = new Uri(fullPath);

            if (baseUri.Scheme != fullUri.Scheme)
            {
                return fullPath; // 不同驱动器，返回完整路径
            }

            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            // 将URL分隔符转换为系统路径分隔符
            if (Path.DirectorySeparatorChar != '/')
            {
                relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
            }

            return relativePath;
        }
        #endregion

        #region CRC32算法操作
        /// <summary>
        /// CRC32算法操作
        /// </summary>
        private static class CRC32
        {
            private static readonly uint[] CrcTable = new uint[256];

            static CRC32()
            {
                // 初始化CRC32查找表
                for (uint i = 0; i < 256; i++)
                {
                    uint crc = i;
                    for (int j = 0; j < 8; j++)
                    {
                        if ((crc & 1) != 0)
                            crc = (crc >> 1) ^ 0xEDB88320;
                        else
                            crc >>= 1;
                    }
                    CrcTable[i] = crc;
                }
            }
            /// <summary>
            /// &lt;无符号整数型&gt; 取CRC32
            /// <param name="data">(字节集 欲获取的CRC32字节集)</param>
            /// <returns><para>返回无符号整数</para></returns>
            /// </summary>
            public static uint Calculate(byte[] data)
            {
                uint crc = 0xFFFFFFFF;
                foreach (byte b in data)
                {
                    crc = CrcTable[(crc ^ b) & 0xFF] ^ (crc >> 8);
                }
                return crc ^ 0xFFFFFFFF;
            }
            /// <summary>
            /// &lt;无符号整数型&gt; 取CRC32
            /// <param name="data">(文本型 欲获取的CRC32文本)</param>
            /// <returns><para>返回无符号整数</para></returns>
            /// </summary>
            public static uint Calculate(string text)
            {
                return Calculate(Encoding.UTF8.GetBytes(text));
            }
        }
        #endregion

        #region 自然字符串比较
        /// <summary>
        /// 自然字符串比较
        /// </summary>
        internal static class NaturalStringComparer
        {
            /// <summary>
            /// &lt;整数型&gt; 比较自然字符串
            /// <param name="x">(文本型 欲比较的字符串X, </param>
            /// <param name="y">文本型 欲比较的字符串Y）</param>
            /// <returns><para>返回带符号数字</para></returns>
            /// </summary>
            public static int Compare(string x, string y)
            {
                // 处理空值情况
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                // 使用正则表达式分割字符串为数字和非数字部分
                var regex = new Regex(@"(\d+|\D+)");
                var xParts = regex.Matches(x).Cast<Match>().Select(m => m.Value).ToArray();
                var yParts = regex.Matches(y).Cast<Match>().Select(m => m.Value).ToArray();

                // 逐部分比较
                for (int i = 0; i < Math.Min(xParts.Length, yParts.Length); i++)
                {
                    var xPart = xParts[i];
                    var yPart = yParts[i];

                    // 尝试将部分转换为数字进行比较
                    int xNum, yNum;
                    if (int.TryParse(xPart, out xNum) && int.TryParse(yPart, out yNum))
                    {
                        int result = xNum.CompareTo(yNum);
                        if (result != 0) return result;
                    }
                    else
                    {
                        int result = string.Compare(xPart, yPart, StringComparison.OrdinalIgnoreCase);
                        if (result != 0) return result;
                    }
                }

                return xParts.Length.CompareTo(yParts.Length);
            }
        }
        #endregion

        #region PAC打包方法
        /// <summary>
        /// PAC打包操作类
        /// </summary>
        public class Pack
        {
            /// <summary>
            /// &lt;文本型&gt;获取带目录名前缀的 PAC 内部文件名
            /// <param name="packPath">(文本型 被打包的目录绝对路径, </param>
            /// <param name="file">文本型 目录下的某个文件绝对路径)</param>
            /// <para>对齐 FPACker 与游戏格式：目录名/相对路径</para>
            /// <para>游戏 pac 里的文件名形如 "table_sc/t_shop.tbl"，CRC32 是对这个完整字符串计算的，
            /// 若丢失目录名前缀会导致 hash 全部算错、游戏无法定位文件而闪退。</para>
            /// <returns>返回带目录名前缀的 PAC 内部文件名</returns>
            /// </summary>
            private static string GetPacRelativePath(string packPath, string file)
            {
                string rel = GetRelativePath(packPath, file);
                string dirName = Path.GetFileName(packPath.TrimEnd('\\', '/'));
                if (string.IsNullOrEmpty(dirName))
                    return rel.Replace('\\', '/');
                return (dirName + "/" + rel).Replace('\\', '/');
            }

            /// <summary>
            /// 打包PAC
            /// <param name="packPath">(文本型 欲打包的文件夹绝对路径, </param>
            /// <param name="filePath">文本型 欲保存的PAC文件路径)</param>
            /// </summary>
            public static void PackPAC(string packPath, string filePath)
            {
                // 检查目录是否存在
                if (!Directory.Exists(packPath))
                    throw new DirectoryNotFoundException($"目标目录不存在: {packPath}");

                // 使用 UTF-8 编码（与解包端一致，避免中文文件名乱码；对纯 ASCII 与游戏完全兼容）
                Encoding encoding = Encoding.UTF8;

                // 收集文件信息
                var files = new List<FpacFile>();
                var fileList = GetAllFilesInDirectory(packPath);

                foreach (string file in fileList)
                {
                    var relativePath = GetPacRelativePath(packPath, file);
                    var fileInfo = new FileInfo(file);

                    // 计算CRC32使用默认编码的字节数组
                    byte[] relativePathBytes = encoding.GetBytes(relativePath);
                    uint crc32 = CRC32.Calculate(relativePathBytes) ^ 0xFFFFFFFF;

                    var fpacFile = new FpacFile
                    {
                        Filename = file,
                        RelativePath = relativePath,
                        FileInfo = new FpacFileInfo
                        {
                            FileSize = (ulong)fileInfo.Length,
                            FilenameCrc32 = crc32,
                            Unknown = 0
                        }
                    };
                    files.Add(fpacFile);
                }

                // 按文件名自然排序计算地址;自然比较相等时用完整路径稳定排序。
                files.Sort(delegate (FpacFile left, FpacFile right)
                {
                    int result = NaturalStringComparer.Compare(left.RelativePath, right.RelativePath);
                    return result != 0
                        ? result
                        : string.Compare(left.RelativePath, right.RelativePath, StringComparison.OrdinalIgnoreCase);
                });

                ulong filepathBlockSize = 0;
                ulong entryBlockSize = (ulong)(ENTRY_SIZE * files.Count);
                ulong dataBlockSize = 0;

                // 计算文件名地址和文件路径块大小（使用默认编码计算字节数）
                for (int i = 0; i < files.Count; i++)
                {
                    files[i].FileInfo.FilenameOffset = (ulong)HEADER_SIZE + entryBlockSize + filepathBlockSize;
                    filepathBlockSize += (ulong)(encoding.GetByteCount(files[i].RelativePath) + 1);
                }

                // 计算文件数据地址和数据块大小
                ulong FirstFileOffset = (ulong)HEADER_SIZE + entryBlockSize + filepathBlockSize;
                for (int i = 0; i < files.Count; i++)
                {
                    files[i].FileInfo.FileOffset = FirstFileOffset + dataBlockSize;
                    dataBlockSize += files[i].FileInfo.FileSize;
                }

                // 写入PAC文件
                using (var writer = new BinaryWriter(File.Create(filePath)))
                {
                    // 写入文件头
                    var header = new FpacHeader
                    {
                        Magic = FPAC_MAGIC,
                        FileCount = (uint)files.Count,
                        FirstFileOffset = (uint)FirstFileOffset,
                        Unknown = 1
                    };
                    WriteHeader(writer, header);

                    // 按CRC32排序写入文件条目
                    files.Sort((a, b) => a.FileInfo.FilenameCrc32.CompareTo(b.FileInfo.FilenameCrc32));
                    foreach (var file in files)
                    {
                        WriteFileInfo(writer, file.FileInfo);
                    }

                    // 按文件名排序写入文件路径（使用默认编码）
                    files.Sort(delegate (FpacFile left, FpacFile right)
                    {
                        int result = NaturalStringComparer.Compare(left.RelativePath, right.RelativePath);
                        return result != 0
                            ? result
                            : string.Compare(left.RelativePath, right.RelativePath, StringComparison.OrdinalIgnoreCase);
                    });
                    foreach (var file in files)
                    {
                        var nameBytes = encoding.GetBytes(file.RelativePath);
                        writer.Write(nameBytes);
                        writer.Write((byte)0); // null terminator
                    }

                    // 按文件名排序写入文件数据
                    foreach (var file in files)
                    {
                        var fileData = File.ReadAllBytes(file.Filename);
                        writer.Write(fileData);
                    }
                }
            }

            /// <summary>
            /// 异步打包PAC
            /// <param name="packPath">(文本型 欲打包的文件夹绝对路径, </param>
            /// <param name="filePath">文本型 欲保存的PAC文件路径, </param>
            /// <param name="progressCallback">文件回调 进度回调)</param>
            /// </summary>
            public static async Task PackPACAsync(string packPath, string filePath, Action<FileProgress> progressCallback)
            {
                await Task.Run(() =>
                {
                    // 检查目录是否存在
                    if (!Directory.Exists(packPath))
                        throw new DirectoryNotFoundException($"目标目录不存在: {packPath}");

                    // 使用 UTF-8 编码（与解包端一致，避免中文文件名乱码；对纯 ASCII 与游戏完全兼容）
                    Encoding encoding = Encoding.UTF8;

                    progressCallback?.Invoke(new FileProgress { percentage = 0, state = "开始收集文件信息..." });

                    // 收集文件信息
                    var files = new List<FpacFile>();
                    var fileList = GetAllFilesInDirectory(packPath);

                    for (int i = 0; i < fileList.Count; i++)
                    {
                        string file = fileList[i];
                        var relativePath = GetPacRelativePath(packPath, file);
                        var fileInfo = new FileInfo(file);

                        // 计算CRC32使用默认编码的字节数组
                        byte[] relativePathBytes = encoding.GetBytes(relativePath);
                        uint crc32 = CRC32.Calculate(relativePathBytes) ^ 0xFFFFFFFF;

                        var fpacFile = new FpacFile
                        {
                            Filename = file,
                            RelativePath = relativePath,
                            FileInfo = new FpacFileInfo
                            {
                                FileSize = (ulong)fileInfo.Length,
                                FilenameCrc32 = crc32,
                                Unknown = 0
                            }
                        };
                        files.Add(fpacFile);

                        int progress = (i + 1) * 20 / fileList.Count;
                        progressCallback?.Invoke(new FileProgress
                        {
                            percentage = progress,
                            state = $"收集文件信息: {i + 1}/{fileList.Count}"
                        });
                    }

                    progressCallback?.Invoke(new FileProgress { percentage = 25, state = "计算文件地址..." });

                    // 按文件名自然排序计算地址;自然比较相等时用完整路径稳定排序。
                    files.Sort(delegate (FpacFile left, FpacFile right)
                    {
                        int result = NaturalStringComparer.Compare(left.RelativePath, right.RelativePath);
                        return result != 0
                            ? result
                            : string.Compare(left.RelativePath, right.RelativePath, StringComparison.OrdinalIgnoreCase);
                    });

                    ulong filepathBlockSize = 0;
                    ulong entryBlockSize = (ulong)(ENTRY_SIZE * files.Count);
                    ulong dataBlockSize = 0;

                    // 计算文件名地址和文件路径块大小（使用默认编码计算字节数）
                    for (int i = 0; i < files.Count; i++)
                    {
                        files[i].FileInfo.FilenameOffset = (ulong)HEADER_SIZE + entryBlockSize + filepathBlockSize;
                        filepathBlockSize += (ulong)(encoding.GetByteCount(files[i].RelativePath) + 1);
                    }

                    // 计算文件数据地址和数据块大小
                    ulong FirstFileOffset = (ulong)HEADER_SIZE + entryBlockSize + filepathBlockSize;
                    for (int i = 0; i < files.Count; i++)
                    {
                        files[i].FileInfo.FileOffset = FirstFileOffset + dataBlockSize;
                        dataBlockSize += files[i].FileInfo.FileSize;
                    }

                    progressCallback?.Invoke(new FileProgress { percentage = 30, state = "开始写入PAC文件..." });

                    // 写入PAC文件
                    using (var writer = new BinaryWriter(File.Create(filePath)))
                    {
                        // 写入文件头
                        var header = new FpacHeader
                        {
                            Magic = FPAC_MAGIC,
                            FileCount = (uint)files.Count,
                            FirstFileOffset = (uint)FirstFileOffset,
                            Unknown = 1
                        };
                        WriteHeader(writer, header);

                        progressCallback?.Invoke(new FileProgress { percentage = 35, state = "写入文件条目..." });

                        // 按CRC32排序写入文件条目
                        files.Sort((a, b) => a.FileInfo.FilenameCrc32.CompareTo(b.FileInfo.FilenameCrc32));
                        foreach (var file in files)
                        {
                            WriteFileInfo(writer, file.FileInfo);
                        }

                        progressCallback?.Invoke(new FileProgress { percentage = 45, state = "写入文件路径..." });

                        // 按文件名排序写入文件路径（使用默认编码）
                        files.Sort(delegate (FpacFile left, FpacFile right)
                        {
                            int result = NaturalStringComparer.Compare(left.RelativePath, right.RelativePath);
                            return result != 0
                                ? result
                                : string.Compare(left.RelativePath, right.RelativePath, StringComparison.OrdinalIgnoreCase);
                        });
                        foreach (var file in files)
                        {
                            var nameBytes = encoding.GetBytes(file.RelativePath);
                            writer.Write(nameBytes);
                            writer.Write((byte)0); // null terminator
                        }

                        progressCallback?.Invoke(new FileProgress { percentage = 50, state = "写入文件数据..." });

                        // 按文件名排序写入文件数据
                        for (int i = 0; i < files.Count; i++)
                        {
                            var fileData = File.ReadAllBytes(files[i].Filename);
                            writer.Write(fileData);

                            int progress = 50 + (i + 1) * 50 / files.Count;
                            progressCallback?.Invoke(new FileProgress
                            {
                                percentage = progress,
                                state = $"正在写入: {files[i].RelativePath}"
                            });
                        }
                    }
                    progressCallback?.Invoke(new FileProgress { percentage = 100, state = "重建PAC完成!" });
                });
            }

            /// <summary>
            /// 按待打包列表异步生成 PAC
            /// <param name="packList">(ListTuple 映射列表, </param>
            /// <param name="filePath">文本型 要生成的 PAC 文件路径)</param>
            /// <param name="progressCallback">进度回调</param>
            /// <para>列表项 Item1 为源文件完整路径，Item2 为 PAC 内部完整相对路径。
            /// 源文件位置与 PAC 内部目录彼此独立，因此同一个磁盘目录中的文件可映射到任意 PAC 子目录。</para>
            /// </summary>
            public static async Task PackPACAsync(
                List<Tuple<string, string>> packList,
                string filePath,
                Action<FileProgress> progressCallback)
            {
                await Task.Run(() => PackPACFromList(packList, filePath, progressCallback));
            }

            /// <summary>
            /// 将多个源文件映射写入一个 PAC 文件。
            /// </summary>
            private static void PackPACFromList(
                List<Tuple<string, string>> packList,
                string filePath,
                Action<FileProgress> progressCallback)
            {
                if (packList == null || packList.Count == 0)
                    throw new InvalidDataException("待打包列表为空，无法生成 PAC 文件。");
                if (string.IsNullOrWhiteSpace(filePath))
                    throw new ArgumentException("PAC 文件保存路径不能为空。", nameof(filePath));

                string outputPath = Path.GetFullPath(filePath);
                string outputDirectory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);

                Encoding encoding = Encoding.UTF8;
                var files = new List<FpacFile>();
                var internalPathSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                progressCallback?.Invoke(new FileProgress
                {
                    percentage = 0,
                    state = "开始检查待打包文件..."
                });

                for (int i = 0; i < packList.Count; i++)
                {
                    Tuple<string, string> mapping = packList[i];
                    if (mapping == null)
                        throw new InvalidDataException("待打包列表包含空项目。");

                    string sourceFile = Path.GetFullPath(mapping.Item1 ?? string.Empty);
                    string relativePath = NormalizePacRelativePath(mapping.Item2);

                    if (string.Equals(sourceFile, outputPath, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("PAC 输出文件不能同时作为待打包源文件。");
                    if (!internalPathSet.Add(relativePath))
                        throw new InvalidDataException($"PAC 内部路径重复: {relativePath}");

                    // 文件不存在、拒绝访问等情况：只记录并跳过该文件，不中断整体打包。
                    byte[] fileContents;
                    try
                    {
                        fileContents = File.ReadAllBytes(sourceFile);
                    }
                    catch (Exception ex) when (ex is FileNotFoundException ||
                                               ex is DirectoryNotFoundException ||
                                               ex is UnauthorizedAccessException ||
                                               ex is IOException)
                    {
                        progressCallback?.Invoke(new FileProgress
                        {
                            percentage = (i + 1) * 20 / packList.Count,
                            state = $"跳过文件: {relativePath}",
                            error = $"文件「{relativePath}」无法访问，已跳过：{ex.Message}"
                        });
                        continue;
                    }

                    byte[] relativePathBytes = encoding.GetBytes(relativePath);
                    uint crc32 = CRC32.Calculate(relativePathBytes) ^ 0xFFFFFFFF;

                    files.Add(new FpacFile
                    {
                        Filename = sourceFile,
                        RelativePath = relativePath,
                        FileContents = fileContents,
                        FileInfo = new FpacFileInfo
                        {
                            FileSize = (ulong)fileContents.LongLength,
                            FilenameCrc32 = crc32,
                            Unknown = 0
                        }
                    });

                    progressCallback?.Invoke(new FileProgress
                    {
                        percentage = (i + 1) * 20 / packList.Count,
                        state = $"检查文件: {i + 1}/{packList.Count}"
                    });
                }

                // 所有文件均无法访问时，不生成空 PAC，直接报错。
                if (files.Count == 0)
                    throw new InvalidDataException("所有待打包文件均无法访问，已全部跳过。");

                files.Sort(delegate (FpacFile left, FpacFile right)
                {
                    int result = NaturalStringComparer.Compare(left.RelativePath, right.RelativePath);
                    return result != 0
                        ? result
                        : string.Compare(left.RelativePath, right.RelativePath, StringComparison.OrdinalIgnoreCase);
                });

                ulong entryBlockSize = checked((ulong)ENTRY_SIZE * (ulong)files.Count);
                ulong filepathBlockSize = 0;
                for (int i = 0; i < files.Count; i++)
                {
                    files[i].FileInfo.FilenameOffset = checked(
                        (ulong)HEADER_SIZE + entryBlockSize + filepathBlockSize);
                    filepathBlockSize = checked(
                        filepathBlockSize + (ulong)encoding.GetByteCount(files[i].RelativePath) + 1UL);
                }

                ulong firstFileOffset = checked((ulong)HEADER_SIZE + entryBlockSize + filepathBlockSize);
                if (firstFileOffset > uint.MaxValue)
                    throw new InvalidDataException("PAC 目录区超过格式支持的大小。");

                ulong dataBlockSize = 0;
                for (int i = 0; i < files.Count; i++)
                {
                    files[i].FileInfo.FileOffset = checked(firstFileOffset + dataBlockSize);
                    dataBlockSize = checked(dataBlockSize + files[i].FileInfo.FileSize);
                }

                progressCallback?.Invoke(new FileProgress
                {
                    percentage = 25,
                    state = "目录信息检查完成，开始写入 PAC 文件..."
                });

                using (var writer = new BinaryWriter(new FileStream(
                    outputPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None)))
                {
                    var header = new FpacHeader
                    {
                        Magic = FPAC_MAGIC,
                        FileCount = (uint)files.Count,
                        FirstFileOffset = (uint)firstFileOffset,
                        Unknown = 1
                    };
                    WriteHeader(writer, header);

                    files.Sort(delegate (FpacFile left, FpacFile right)
                    {
                        int result = left.FileInfo.FilenameCrc32.CompareTo(right.FileInfo.FilenameCrc32);
                        return result != 0
                            ? result
                            : string.Compare(left.RelativePath, right.RelativePath, StringComparison.OrdinalIgnoreCase);
                    });
                    foreach (FpacFile file in files)
                        WriteFileInfo(writer, file.FileInfo);

                    progressCallback?.Invoke(new FileProgress
                    {
                        percentage = 40,
                        state = "正在写入文件路径..."
                    });

                    files.Sort(delegate (FpacFile left, FpacFile right)
                    {
                        int result = NaturalStringComparer.Compare(left.RelativePath, right.RelativePath);
                        return result != 0
                            ? result
                            : string.Compare(left.RelativePath, right.RelativePath, StringComparison.OrdinalIgnoreCase);
                    });
                    foreach (FpacFile file in files)
                    {
                        writer.Write(encoding.GetBytes(file.RelativePath));
                        writer.Write((byte)0);
                    }

                    progressCallback?.Invoke(new FileProgress
                    {
                        percentage = 50,
                        state = "正在写入文件数据..."
                    });

                    for (int i = 0; i < files.Count; i++)
                    {
                        // 数据已在收集阶段读入内存，直接写入，避免写入期 IO 失败中断整体打包。
                        writer.Write(files[i].FileContents);

                        progressCallback?.Invoke(new FileProgress
                        {
                            percentage = 50 + (i + 1) * 50 / files.Count,
                            state = $"正在写入: {files[i].RelativePath}"
                        });
                    }
                }

                progressCallback?.Invoke(new FileProgress
                {
                    percentage = 100,
                    state = "PAC 打包完成!"
                });
            }


            /// <summary>
            /// 规范化并校验 PAC 内部相对路径
            /// <param name="path">(文本型 欲处理的路径)</param>
            /// <returns><para>返回规范的 PAC 内部相对路径</para></returns>
            /// </summary>
            private static string NormalizePacRelativePath(string path)
            {
                if (string.IsNullOrWhiteSpace(path) || path.IndexOf('\0') >= 0)
                    throw new InvalidDataException("PAC 内部路径不能为空或包含无效字符。");

                string normalized = path.Replace('\\', '/');
                if (normalized.StartsWith("/", StringComparison.Ordinal) ||
                    Path.IsPathRooted(normalized.Replace('/', Path.DirectorySeparatorChar)))
                {
                    throw new InvalidDataException($"PAC 内部路径无效: {path}");
                }

                normalized = normalized.Trim('/');
                if (normalized.Length == 0)
                    throw new InvalidDataException($"PAC 内部路径无效: {path}");

                string[] segments = normalized.Split('/');
                for (int i = 0; i < segments.Length; i++)
                {
                    if (segments[i].Length == 0 || segments[i] == "." || segments[i] == ".." || segments[i].IndexOf(':') >= 0)
                        throw new InvalidDataException($"PAC 内部路径无效: {path}");
                }

                return string.Join("/", segments);
            }

            /// <summary>
            /// &lt;List&lt;string&gt;&gt;递归获取目录下所有文件
            /// <param name="directoryPath">(文本型 目录路径)</param>
            /// <returns><para>返回文件列表</para></returns>
            /// </summary>
            private static List<string> GetAllFilesInDirectory(string directoryPath)
            {
                List<string> fileList = new List<string>();
                try
                {
                    // 获取当前目录的所有文件，并添加到文件列表中
                    fileList.AddRange(Directory.GetFiles(directoryPath));

                    // 获取当前目录的所有子目录
                    string[] subDirectories = Directory.GetDirectories(directoryPath);

                    // 遍历子目录
                    foreach (string subDirectory in subDirectories)
                    {
                        // 检查子目录是否为空
                        if (Directory.GetFiles(subDirectory).Length > 0 || Directory.GetDirectories(subDirectory).Length > 0)
                        {
                            // 如果子目录不为空，则递归遍历子目录，并将子目录中的文件添加到文件列表中
                            fileList.AddRange(GetAllFilesInDirectory(subDirectory));
                        }
                        // 如果子目录为空，则跳过它（不执行任何操作）
                    }
                }
                catch (Exception ex)
                {
                    // 容错处理
                    throw new Exception("An error occurred while traversing the directory: " + ex.Message);
                }
                return fileList;
            }

            /// <summary>
            /// 写入文件头
            /// <param name="writer"></param>
            /// <param name="header"></param>
            /// </summary>
            private static void WriteHeader(BinaryWriter writer, FpacHeader header)
            {
                writer.Write(header.Magic);
                writer.Write(header.FileCount);
                writer.Write(header.FirstFileOffset);
                writer.Write(header.Unknown);
            }

            /// <summary>
            /// 写入文件条目
            /// <param name="writer"></param>
            /// <param name="fileInfo"></param>
            /// </summary>
            private static void WriteFileInfo(BinaryWriter writer, FpacFileInfo fileInfo)
            {
                writer.Write(fileInfo.FilenameCrc32);
                writer.Write(fileInfo.Unknown);
                writer.Write(fileInfo.FilenameOffset);
                writer.Write(fileInfo.FileSize);
                writer.Write(fileInfo.FileOffset);
            }
        }
        #endregion

        #region PAC解包方法
        /// <summary>
        /// PAC解包操作类
        /// </summary>
        public class UnPack
        {
            /// <summary>
            /// &lt;List&lt;Tuple&gt;&gt;取PAC内部文件列表
            /// <param name="PACfilePath">(文本型 欲获取内部文件列表的PAC文件)</param>
            /// <returns><para>成功返回PAC文件的文件名签名字节集、文件名文本、文件偏移、实际大小</para></returns>
            /// </summary>
            public static List<Tuple<byte[], string, long, long>> GetPACInformation(string PACfilePath)
            {
                // 初始化结果列表
                var result = new List<Tuple<byte[], string, long, long>>();

                // 检查文件是否存在
                if (!File.Exists(PACfilePath))
                    throw new FileNotFoundException($"PAC文件不存在: {PACfilePath}");

                try
                {
                    // 读取PAC文件
                    using (var reader = new BinaryReader(File.OpenRead(PACfilePath)))
                    {
                        long fileLen = reader.BaseStream.Length;

                        // 文件连文件头(16字节)都读不全 -> 不可能是有效 PAC
                        if (fileLen < HEADER_SIZE)
                            throw new InvalidDataException("未知的格式，读取失败！");

                        // 验证魔数
                        uint magic = reader.ReadUInt32();
                        if (magic != FPAC_MAGIC)
                            throw new InvalidDataException("未知的格式，读取失败！");

                        // 读取文件头
                        uint fileCount = reader.ReadUInt32();
                        uint firstFileOffset = reader.ReadUInt32();
                        uint unknown = reader.ReadUInt32();

                        // 防御性校验：条目数必须为正数且整体落在文件范围内，否则视为文件表损坏
                        if (fileCount == 0 || fileCount > int.MaxValue / ENTRY_SIZE)
                            throw new InvalidDataException("文件表损坏，读取失败！");

                        long entryTableEnd = (long)HEADER_SIZE + (long)fileCount * ENTRY_SIZE;
                        if (entryTableEnd > fileLen)
                            throw new InvalidDataException("文件表损坏，读取失败！");

                        // 首文件偏移应位于条目表之后且在文件以内
                        if (firstFileOffset < entryTableEnd || firstFileOffset > fileLen)
                            throw new InvalidDataException("文件表损坏，读取失败！");

                        // 读取文件条目（逐条校验偏移是否越界）
                        var fileInfos = new List<FpacFileInfo>();
                        for (int i = 0; i < fileCount; i++)
                        {
                            var fileInfo = new FpacFileInfo
                            {
                                FilenameCrc32 = reader.ReadUInt32(),
                                Unknown = reader.ReadUInt32(),
                                FilenameOffset = reader.ReadUInt64(),
                                FileSize = reader.ReadUInt64(),
                                FileOffset = reader.ReadUInt64()
                            };

                            // 偏移越界检查（文件名偏移、文件数据偏移、文件大小必须落在文件内）
                            if (fileInfo.FilenameOffset >= (ulong)fileLen)
                                throw new InvalidDataException("文件表损坏，读取失败！");
                            if (fileInfo.FileOffset > (ulong)fileLen)
                                throw new InvalidDataException("文件表损坏，读取失败！");
                            if (fileInfo.FileSize > (ulong)fileLen)
                                throw new InvalidDataException("文件表损坏，读取失败！");

                            fileInfos.Add(fileInfo);
                        }

                        // 读取文件名并构建结果
                        foreach (var fileInfo in fileInfos)
                        {
                            // 定位到文件名位置
                            reader.BaseStream.Seek((long)fileInfo.FilenameOffset, SeekOrigin.Begin);

                            // 读取 NULL 结尾的文件名；若到文件末尾仍未遇到 NULL，说明文件表损坏
                            var nameBytes = new List<byte>();
                            bool terminated = false;
                            while (reader.BaseStream.Position < fileLen)
                            {
                                byte b = reader.ReadByte();
                                if (b == 0) { terminated = true; break; }
                                nameBytes.Add(b);
                            }
                            if (!terminated)
                                throw new InvalidDataException("文件表损坏，读取失败！");

                            string filename = Encoding.UTF8.GetString(nameBytes.ToArray());

                            // 计算未异或的CRC32值
                            uint originalCrc32 = fileInfo.FilenameCrc32 ^ 0xFFFFFFFF;
                            byte[] crc32Bytes = BitConverter.GetBytes(originalCrc32);

                            result.Add(new Tuple<byte[], string, long, long>(
                                crc32Bytes,
                                filename,
                                (long)fileInfo.FileOffset,
                                (long)fileInfo.FileSize
                            ));
                        }
                    }
                }
                catch (InvalidDataException)
                {
                    // 已是我们主动抛出的语义化异常，原样向外传播（主窗体负责展示）
                    throw;
                }
                catch (Exception ex)
                {
                    // 其它未预期的读取错误（磁盘/权限/流意外结束等），既不是格式不对也不是表损坏，
                    // 不能误导性归为“文件表损坏”，如实告知为“其他未预料到的错误”并附带原异常消息。
                    throw new InvalidDataException($"发生其他未预料到的错误：{ex.Message} 读取失败！", ex);
                }
                return result;
            }

            /// <summary>
            /// 解压所有文件
            /// <param name="PACfilePath">(文本型 欲解压的PAC文件, </param>
            /// <param name="fileList">文本型 已处理好的文件列表, </param>
            /// <param name="savePath">文本型 欲保存的路径)</param>
            /// </summary>
            public static void UnpackAllFile(string PACfilePath, List<Tuple<byte[], string, long, long>> fileList, string savePath)
            {
                // 检查文件是否存在
                if (!File.Exists(PACfilePath))
                    throw new FileNotFoundException($"PAC文件不存在: {PACfilePath}");

                // 检查保存路径是否存在
                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);

                // 读取PAC文件并解压
                using (var reader = new BinaryReader(File.OpenRead(PACfilePath)))
                {
                    foreach (var fileInfo in fileList)
                    {
                        string filename = fileInfo.Item2;
                        long fileOffset = fileInfo.Item3;
                        long fileSize = fileInfo.Item4;

                        // 构建输出文件路径
                        string outputPath = Path.Combine(savePath, filename.Replace('/', Path.DirectorySeparatorChar));
                        string outputDir = Path.GetDirectoryName(outputPath);

                        if (!Directory.Exists(outputDir))
                            Directory.CreateDirectory(outputDir);

                        // 读取文件数据
                        reader.BaseStream.Seek(fileOffset, SeekOrigin.Begin);
                        byte[] fileData = reader.ReadBytes((int)fileSize);

                        // 写入文件
                        File.WriteAllBytes(outputPath, fileData);
                    }
                }
            }

            /// <summary>
            /// 异步解压所有文件
            /// <param name="PACfilePath">(文本型 欲解压的PAC文件, </param>
            /// <param name="fileList">文本型 已处理好的文件列表, </param>
            /// <param name="savePath">文本型 欲保存的路径, </param>
            /// <param name="progressCallback">文件回调 进度回调)</param>
            /// </summary>
            public static async Task UnpackAllFileAsync(string PACfilePath, List<Tuple<byte[], string, long, long>> targetFile, string savePath, Action<FileProgress> progressCallback)
            {
                await Task.Run(() =>
                {
                    // 检查文件是否存在
                    if (!File.Exists(PACfilePath))
                        throw new FileNotFoundException($"PAC文件不存在: {PACfilePath}");

                    // 检查保存路径是否存在
                    if (!Directory.Exists(savePath))
                        Directory.CreateDirectory(savePath);

                    progressCallback?.Invoke(new FileProgress { percentage = 0, state = "开始提取文件..." });

                    // 读取PAC文件并解压
                    using (var reader = new BinaryReader(File.OpenRead(PACfilePath)))
                    {
                        for (int i = 0; i < targetFile.Count; i++)
                        {
                            var fileInfo = targetFile[i];
                            string filename = fileInfo.Item2;
                            long fileOffset = fileInfo.Item3;
                            long fileSize = fileInfo.Item4;

                            string outputPath = null;
                            try
                            {
                                // 构建输出文件路径
                                outputPath = Path.Combine(savePath, filename.Replace('/', Path.DirectorySeparatorChar));
                                string outputDir = Path.GetDirectoryName(outputPath);

                                if (!Directory.Exists(outputDir))
                                    Directory.CreateDirectory(outputDir);

                                // 读取文件数据
                                reader.BaseStream.Seek(fileOffset, SeekOrigin.Begin);
                                byte[] fileData = reader.ReadBytes((int)fileSize);

                                // 写入文件
                                File.WriteAllBytes(outputPath, fileData);
                            }
                            catch (Exception ex)
                            {
                                // 单文件提取失败：记录错误并继续后续文件，不中断整体任务
                                progressCallback?.Invoke(new FileProgress
                                {
                                    percentage = (i + 1) * 100 / targetFile.Count,
                                    state = $"提取失败: {filename}",
                                    error = $"文件「{filename}」提取失败：{ex.Message}"
                                });
                                continue;
                            }

                            int progress = (i + 1) * 100 / targetFile.Count;
                            progressCallback?.Invoke(new FileProgress
                            {
                                percentage = progress,
                                state = $"正在提取: {filename}"
                            });
                        }
                    }

                    progressCallback?.Invoke(new FileProgress { percentage = 100, state = "提取完成!" });
                });
            }

            /// <summary>
            /// 解压单独文件
            /// <param name="PACfilePath">(文本型 欲单独解压的PAC文件, </param>
            /// <param name="fileList">文本型 已处理好的文件列表, </param>
            /// <param name="targetNum">文本型 欲解压的文件顺序号, </param>
            /// <param name="savePath">文本型 欲保存的路径)</param>
            /// </summary>
            public static void UnpackSingle(string PACfilePath, List<Tuple<byte[], string, long, long>> fileList, int targetNum, string savePath)
            {
                // 检查文件是否存在
                if (!File.Exists(PACfilePath))
                    throw new FileNotFoundException($"PAC文件不存在: {PACfilePath}");

                // 检查索引是否有效
                if (targetNum < 0 || targetNum >= fileList.Count)
                    throw new ArgumentOutOfRangeException(nameof(targetNum), "文件索引超出范围");

                /*if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);*/

                // 初始化必用变量
                var fileInfo = fileList[targetNum];
                string filename = fileInfo.Item2;
                long fileOffset = fileInfo.Item3;
                long fileSize = fileInfo.Item4;

                // 读取PAC文件
                using (var reader = new BinaryReader(File.OpenRead(PACfilePath)))
                {
                    // 构建输出文件路径 - 只取文件名，不保留目录结构
                    // string fileName = Path.GetFileName(filename.Replace('/', Path.DirectorySeparatorChar));
                    // string outputPath = Path.Combine(savePath, fileName);

                    // 读取文件数据
                    reader.BaseStream.Seek(fileOffset, SeekOrigin.Begin);
                    byte[] fileData = reader.ReadBytes((int)fileSize);

                    // 写入文件
                    File.WriteAllBytes(savePath, fileData);
                }
            }
        }
        #endregion
    }
}
