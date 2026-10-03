// SPDX-License-Identifier: CC0-1.0
//
// To the extent possible under law, the contributors have waived all copyright
// and related or neighboring rights to this file under CC0 1.0.
// https://creativecommons.org/publicdomain/zero/1.0/

using System;
using System.IO;

namespace FPACTool
{

    /// <summary>
    /// LZ4Helper 主要功能：
    /// 1. 无依赖的 LZ4 Frame 解码器，供 DDS 预览使用。
    /// 2. 支持标准 LZ4 Frame、独立/非独立 Block、压缩/未压缩 Block、可选 Frame 校验和。
    /// 3. 不自动识别裸 LZ4 Block：裸块不自描述，且需要可信的外部尺寸信息。
    /// </summary>
    internal static class LZ4Helper
    {
        #region LZ4 Frame 常量与标志
        private const uint Lz4FrameMagic = 0x184D2204u;              // LZ4 Frame 魔数
        private const long MaxOutputBytes = 512L * 1024L * 1024L;    // 最大解压输出 512MB，防内存爆炸
        private const int MaxHistoryBytes = 64 * 1024;               // LZ4 历史窗口 64KB
        #endregion

        #region 公有方法

        /// <summary>
        /// &lt;逻辑型&gt;判断字节数组是否为标准 LZ4 Frame
        /// <param name="data">(字节集 欲检查的数据)</param>
        /// <returns><para>是返回真，否则返回假</para></returns>
        /// </summary>
        public static bool IsLZ4Frame(byte[] data)
        {
            // 非空、至少 4 字节、前 4 字节等于 LZ4 Frame 魔数
            return data != null && data.Length >= 4 && ReadUInt32LE(data, 0) == Lz4FrameMagic;
        }

        /// <summary>
        /// &lt;字节集&gt;解码一个完整的 LZ4 Frame，返回解压后的数据
        /// <param name="data">(字节集 完整的 LZ4 Frame 数据)</param>
        /// <returns><para>返回解压后的字节数组</para></returns>
        /// </summary>
        public static byte[] DecodeFrame(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length < 7)
                throw new InvalidDataException("LZ4 Frame 数据太短。");
            if (!IsLZ4Frame(data))
                throw new InvalidDataException("数据不是标准 LZ4 Frame。");

            int position = 4;                    // 跳过 4 字节魔数后的读取位置
            int descriptorStart = position;      // 记录 Frame 描述符起始，用于 Header 校验和

            byte flg = ReadByte(data, ref position);   // FLG 字节：版本/保留位/块与校验标志
            byte bd = ReadByte(data, ref position);    // BD 字节：块最大尺寸编码

            int version = (flg >> 6) & 0x03;
            if (version != 1)
                throw new InvalidDataException("不受支持的 LZ4 Frame 版本。");
            if ((flg & 0x02) != 0)
                throw new InvalidDataException("LZ4 Frame FLG 保留位不为 0。");
            if ((bd & 0x8F) != 0)
                throw new InvalidDataException("LZ4 Frame BD 保留位不为 0。");

            // 解析 FLG 各标志位
            bool blockIndependent = (flg & 0x20) != 0;   // 块是否独立（独立块不引用前块历史）
            bool blockChecksum = (flg & 0x10) != 0;      // 是否每块带校验和
            bool hasContentSize = (flg & 0x08) != 0;     // 是否声明内容总大小
            bool contentChecksum = (flg & 0x04) != 0;    // 是否带内容校验和
            bool hasDictionaryId = (flg & 0x01) != 0;    // 是否带字典 ID

            int blockMaxSize = ResolveBlockMaxSize((bd >> 4) & 0x07);   // 解析块最大尺寸
            ulong declaredContentSize = 0;                              // 声明的内容总大小

            if (hasContentSize)
            {
                EnsureAvailable(data, position, 8);
                declaredContentSize = ReadUInt64LE(data, position);     // 读取 8 字节内容大小
                position += 8;

                if (declaredContentSize > (ulong)MaxOutputBytes)
                    throw new InvalidDataException("LZ4 解压结果超过允许的最大尺寸。");
            }

            if (hasDictionaryId)
            {
                // 字典 ID 只标识外部字典，Frame 本身不携带字典内容，静默忽略会破坏输出，
                // 故本解码器显式拒绝。
                EnsureAvailable(data, position, 4);
                position += 4;
                throw new NotSupportedException("暂不支持需要外部 Dictionary 的 LZ4 Frame。");
            }

            int descriptorLength = position - descriptorStart;          // 描述符长度
            byte expectedHeaderChecksum = ReadByte(data, ref position);
            byte actualHeaderChecksum = (byte)((XXHash32(data, descriptorStart, descriptorLength, 0) >> 8) & 0xFF);
            if (actualHeaderChecksum != expectedHeaderChecksum)
                throw new InvalidDataException("LZ4 Frame Header Checksum 校验失败。");

            // 估算初始容量：优先用声明大小，否则按源数据 2 倍（上限 16MB）预分配
            long estimatedCapacity = Math.Max((long)data.Length * 2L, 64L * 1024L);
            int initialCapacity = hasContentSize && declaredContentSize > 0 && declaredContentSize <= int.MaxValue
                ? (int)declaredContentSize
                : (int)Math.Min(estimatedCapacity, 16L * 1024L * 1024L);
            DynamicBuffer output = new DynamicBuffer(initialCapacity);

            while (true)
            {
                EnsureAvailable(data, position, 4);
                uint rawBlockSize = ReadUInt32LE(data, position);       // 块大小字段（最高位为未压缩标志）
                position += 4;

                if (rawBlockSize == 0)
                    break;                                              // 块大小为 0 表示 Frame 结束

                bool uncompressed = (rawBlockSize & 0x80000000u) != 0; // 最高位是否为 1（未压缩块）
                int storedSize = checked((int)(rawBlockSize & 0x7FFFFFFFu));  // 去掉标志位后的实际存储大小
                if (storedSize <= 0 || storedSize > blockMaxSize)
                    throw new InvalidDataException("LZ4 Frame Block 尺寸无效。");

                EnsureAvailable(data, position, storedSize);
                int blockDataPosition = position;                       // 记录块数据起始位置
                position += storedSize;

                int blockOutputStart = output.Length;                   // 本块解压前的输出长度
                if (uncompressed)
                {
                    output.Append(data, blockDataPosition, storedSize, MaxOutputBytes);   // 未压缩块直接追加
                }
                else
                {
                    // 独立块字典从本块开始，非独立块可回看最多 64KB 历史
                    int dictionaryStart = blockIndependent
                        ? blockOutputStart
                        : Math.Max(0, blockOutputStart - MaxHistoryBytes);

                    DecodeBlock(
                        data,
                        blockDataPosition,
                        storedSize,
                        output,
                        dictionaryStart,
                        blockOutputStart,
                        blockMaxSize);
                }

                if (output.Length - blockOutputStart > blockMaxSize)
                    throw new InvalidDataException("LZ4 Block 解压后尺寸超过 Frame 声明的最大 Block Size。");
                if (output.Length > MaxOutputBytes)
                    throw new InvalidDataException("LZ4 解压结果超过允许的最大尺寸。");

                if (blockChecksum)
                {
                    EnsureAvailable(data, position, 4);
                    uint expected = ReadUInt32LE(data, position);       // 期望的块校验和
                    position += 4;
                    uint actual = XXHash32(data, blockDataPosition, storedSize, 0);
                    if (actual != expected)
                        throw new InvalidDataException("LZ4 Block Checksum 校验失败。");
                }
            }

            if (hasContentSize && (ulong)output.Length != declaredContentSize)
                throw new InvalidDataException("LZ4 解压尺寸与 Frame Content Size 不一致。");

            if (contentChecksum)
            {
                EnsureAvailable(data, position, 4);
                uint expected = ReadUInt32LE(data, position);
                position += 4;
                uint actual = XXHash32(output.Buffer, 0, output.Length, 0);
                if (actual != expected)
                    throw new InvalidDataException("LZ4 Content Checksum 校验失败。");
            }

            // 单个 .dds 资源预期只包含一个完整 Frame：允许末尾零填充，但拒绝非零垃圾数据，
            // 因为非零垃圾通常意味着容器解析错误。
            for (int i = position; i < data.Length; i++)
            {
                if (data[i] != 0)
                    throw new InvalidDataException("LZ4 Frame 结束标记之后存在未知数据。");
            }

            return output.ToArray();
        }

        #endregion

        #region LZ4 块解码核心

        /// <summary>
        /// 解码单个压缩 Block（token/字面量/match 序列）
        /// <param name="source">(字节集 完整的 Frame 源数据, </param>
        /// <param name="sourceOffset">(整数型 块数据起始偏移, </param>
        /// <param name="sourceLength">整数型 块数据长度, </param>
        /// <param name="output">DynamicBuffer 输出缓冲区, </param>
        /// <param name="dictionaryStart">整数型 历史窗口起始位置, </param>
        /// <param name="blockOutputStart">整数型 本块解压前的输出长度, </param>
        /// <param name="blockMaxSize">整数型 块解压后的最大允许尺寸)</param>
        /// </summary>
        private static void DecodeBlock(
            byte[] source,
            int sourceOffset,
            int sourceLength,
            DynamicBuffer output,
            int dictionaryStart,
            int blockOutputStart,
            int blockMaxSize)
        {
            int src = sourceOffset;
            int srcEnd = checked(sourceOffset + sourceLength);

            while (src < srcEnd)
            {
                byte token = source[src++];                    // 读取 token：高 4 位字面量长度，低 4 位 match 长度

                int literalLength = token >> 4;
                if (literalLength == 15)
                    literalLength = checked(literalLength + ReadLengthExtension(source, ref src, srcEnd));   // 15 表示需扩展

                if (literalLength > srcEnd - src)
                    throw new InvalidDataException("LZ4 Literal 数据被截断。");

                if (literalLength > 0)
                {
                    output.Append(source, src, literalLength, MaxOutputBytes);   // 追加字面量
                    src += literalLength;
                }

                // 块允许在最后一段字面量之后立即结束
                if (src == srcEnd)
                    break;

                if (srcEnd - src < 2)
                    throw new InvalidDataException("LZ4 Match Offset 数据被截断。");

                int matchOffset = source[src] | (source[src + 1] << 8);   // 读取 2 字节小端 match 偏移
                src += 2;
                if (matchOffset == 0)
                    throw new InvalidDataException("LZ4 Match Offset 不能为 0。");

                int matchLength = token & 0x0F;
                if (matchLength == 15)
                    matchLength = checked(matchLength + ReadLengthExtension(source, ref src, srcEnd));
                matchLength = checked(matchLength + 4);          // match 长度基础值为 4

                int matchPosition = output.Length - matchOffset;  // match 在输出中的起始位置
                if (matchPosition < dictionaryStart || matchPosition < 0)
                    throw new InvalidDataException("LZ4 Match Offset 超出可用历史窗口。");

                if ((long)(output.Length - blockOutputStart) + matchLength > blockMaxSize)
                    throw new InvalidDataException("LZ4 Block 解压结果超过允许尺寸。");
                if ((long)output.Length + matchLength > MaxOutputBytes)
                    throw new InvalidDataException("LZ4 解压结果超过允许的最大尺寸。");

                // 逐字节拷贝是有意为之：LZ4 允许 match 与正在写入的字节重叠（如 offset=1 的重复填充）
                for (int i = 0; i < matchLength; i++)
                {
                    byte value = output.Buffer[matchPosition + i];
                    output.AppendByte(value, MaxOutputBytes);
                }
            }
        }

        /// <summary>
        /// &lt;整数型&gt;读取 LZ4 长度扩展字段
        /// <param name="source">(字节集 源数据, </param>
        /// <param name="position">引用整数型 当前读取位置, </param>
        /// <param name="end">整数型 数据结束位置)</param>
        /// <para>255 表示继续累加</para>
        /// <returns><para>返回累加的长度扩展值</para></returns>
        /// </summary>
        private static int ReadLengthExtension(byte[] source, ref int position, int end)
        {
            int total = 0;
            while (true)
            {
                if (position >= end)
                    throw new InvalidDataException("LZ4 长度扩展字段被截断。");

                int value = source[position++];
                total = checked(total + value);
                if (value != 255)
                    return total;                              // 遇到非 255 即结束
            }
        }

        /// <summary>
        /// &lt;整数型&gt;根据 BD 字节的高 4 位解析块最大尺寸
        /// <param name="code">(整数型 块尺寸编码)</param>
        /// <para>4~7 有效</para>
        /// <returns><para>返回块最大尺寸字节数</para></returns>
        /// </summary>
        private static int ResolveBlockMaxSize(int code)
        {
            switch (code)
            {
                case 4: return 64 * 1024;         // 64KB
                case 5: return 256 * 1024;        // 256KB
                case 6: return 1024 * 1024;       // 1MB
                case 7: return 4 * 1024 * 1024;   // 4MB
                default:
                    throw new InvalidDataException("LZ4 Frame Block Maximum Size 编码无效。");
            }
        }

        #endregion

        #region 数据读取工具

        /// <summary>
        /// &lt;字节型&gt;读取 1 字节并前移位置
        /// <param name="data">(字节集 源数据, </param>
        /// <param name="position">引用整数型 读取位置)</param>
        /// <returns><para>返回读取到的字节</para></returns>
        /// </summary>
        private static byte ReadByte(byte[] data, ref int position)
        {
            EnsureAvailable(data, position, 1);
            return data[position++];
        }

        /// <summary>
        /// 确保指定位置后有足够的可读字节
        /// <param name="data">(字节集 源数据, </param>
        /// <param name="position">整数型 起始位置, </param>
        /// <param name="count">整数型 需要的字节数)</param>
        /// </summary>
        private static void EnsureAvailable(byte[] data, int position, int count)
        {
            if (position < 0 || count < 0 || position > data.Length - count)
                throw new InvalidDataException("LZ4 Frame 数据被截断。");
        }

        /// <summary>
        /// &lt;无符号整数型&gt;以小端序读取 4 字节整数
        /// <param name="data">(字节集 源数据)</param>
        /// <param name="offset">(整数型 起始偏移)</param>
        /// <returns><para>返回 32 位无符号整数</para></returns>
        /// </summary>
        private static uint ReadUInt32LE(byte[] data, int offset)
        {
            EnsureAvailable(data, offset, 4);
            return (uint)(data[offset]
                | (data[offset + 1] << 8)
                | (data[offset + 2] << 16)
                | (data[offset + 3] << 24));
        }

        /// <summary>
        /// &lt;无符号长整型&gt;以小端序读取 8 字节整数
        /// <param name="data">(字节集 源数据, </param>
        /// <param name="offset">整数型 起始偏移)</param>
        /// <returns><para>返回 64 位无符号整数</para></returns>
        /// </summary>
        private static ulong ReadUInt64LE(byte[] data, int offset)
        {
            EnsureAvailable(data, offset, 8);
            uint low = ReadUInt32LE(data, offset);        // 低 4 字节
            uint high = ReadUInt32LE(data, offset + 4);   // 高 4 字节
            return low | ((ulong)high << 32);             // 合并为 64 位
        }

        #endregion

        #region XXH32

        // ---- XXH32 算法固定素数常量 ----
        private const uint Prime32_1 = 2654435761U;
        private const uint Prime32_2 = 2246822519U;
        private const uint Prime32_3 = 3266489917U;
        private const uint Prime32_4 = 668265263U;
        private const uint Prime32_5 = 374761393U;

        /// <summary>
        /// &lt;无符号整数型&gt;计算数据的 XXH32 哈希值
        /// <param name="data">(字节集 待哈希的数据, </param>
        /// <param name="offset">整数型 起始偏移, </param>
        /// <param name="length">整数型 数据长度, </param>
        /// <param name="seed">无符号整数型 哈希种子)</param>
        /// <returns><para>返回 32 位 XXH32 哈希值</para></returns>
        /// </summary>
        private static uint XXHash32(byte[] data, int offset, int length, uint seed)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (offset < 0 || length < 0 || offset > data.Length - length)
                throw new ArgumentOutOfRangeException("offset");

            unchecked
            {
                int p = offset;
                int end = offset + length;
                uint hash;

                if (length >= 16)
                {
                    // 4 个累加器初始化，进入 16 字节轮处理
                    uint v1 = seed + Prime32_1 + Prime32_2;
                    uint v2 = seed + Prime32_2;
                    uint v3 = seed;
                    uint v4 = seed - Prime32_1;
                    int limit = end - 16;

                    do
                    {
                        v1 = XXRound(v1, ReadUInt32LE(data, p)); p += 4;
                        v2 = XXRound(v2, ReadUInt32LE(data, p)); p += 4;
                        v3 = XXRound(v3, ReadUInt32LE(data, p)); p += 4;
                        v4 = XXRound(v4, ReadUInt32LE(data, p)); p += 4;
                    }
                    while (p <= limit);

                    hash = RotateLeft(v1, 1)
                        + RotateLeft(v2, 7)
                        + RotateLeft(v3, 12)
                        + RotateLeft(v4, 18);
                }
                else
                {
                    hash = seed + Prime32_5;   // 数据不足 16 字节时直接初始化
                }

                hash += (uint)length;

                // 处理剩余的 4 字节块
                while (p <= end - 4)
                {
                    hash += ReadUInt32LE(data, p) * Prime32_3;
                    hash = RotateLeft(hash, 17) * Prime32_4;
                    p += 4;
                }

                // 处理剩余单字节
                while (p < end)
                {
                    hash += data[p] * Prime32_5;
                    hash = RotateLeft(hash, 11) * Prime32_1;
                    p++;
                }

                // 最终雪崩混淆
                hash ^= hash >> 15;
                hash *= Prime32_2;
                hash ^= hash >> 13;
                hash *= Prime32_3;
                hash ^= hash >> 16;
                return hash;
            }
        }

        /// <summary>
        /// &lt;无符号整数型&gt;XXH32 单轮混合
        /// <param name="accumulator">(无符号整数型 累加器, </param>
        /// <param name="input">无符号整数型 输入数据)</param>
        /// <returns><para>返回混合后的累加器值</para></returns>
        /// </summary>
        private static uint XXRound(uint accumulator, uint input)
        {
            unchecked
            {
                accumulator += input * Prime32_2;
                accumulator = RotateLeft(accumulator, 13);
                accumulator *= Prime32_1;
                return accumulator;
            }
        }

        /// <summary>
        /// &lt;无符号整数型&gt;32 位循环左移
        /// <param name="value">(无符号整数型 待移位值, </param>
        /// <param name="count">整数型 左移位数)</param>
        /// <returns><para>返回循环左移后的值</para></returns>
        /// </summary>
        private static uint RotateLeft(uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }

        #endregion

        /// <summary>
        /// 动态增长的字节缓冲区
        /// <para>用于累积 LZ4 解压输出</para>
        /// </summary>
        private sealed class DynamicBuffer
        {
            private byte[] buffer;      // 底层存储数组
            private int length;         // 当前有效长度

            /// <summary>
            /// 创建指定初始容量的缓冲区
            /// <param name="capacity">(整数型 初始容量)</param>
            /// </summary>
            public DynamicBuffer(int capacity)
            {
                if (capacity < 1)
                    capacity = 1;        // 容量至少为 1
                buffer = new byte[capacity];
            }

            /// <summary>
            /// &lt;字节集&gt;获取底层存储数组
            /// </summary>
            public byte[] Buffer { get { return buffer; } }
            /// <summary>
            /// &lt;整数型&gt;获取当前有效长度
            /// </summary>
            public int Length { get { return length; } }

            /// <summary>
            /// 追加一段数据
            /// <param name="source">(字节集 源数据, </param>
            /// <param name="offset">整数型 源起始偏移, </param>
            /// <param name="count">整数型 字节数, </param>
            /// <param name="limit">长整型 允许的最大总长度)</param>
            /// </summary>
            public void Append(byte[] source, int offset, int count, long limit)
            {
                if (source == null)
                    throw new ArgumentNullException("source");
                if (offset < 0 || count < 0 || offset > source.Length - count)
                    throw new ArgumentOutOfRangeException("offset");
                if ((long)length + count > limit)
                    throw new InvalidDataException("LZ4 解压结果超过允许的最大尺寸。");

                EnsureCapacity(checked(length + count));
                System.Buffer.BlockCopy(source, offset, buffer, length, count);
                length += count;
            }

            /// <summary>
            /// 追加单个字节
            /// <param name="value">(字节型 要追加的字节, </param>
            /// <param name="limit">长整型 允许的最大总长度)</param>
            /// </summary>
            public void AppendByte(byte value, long limit)
            {
                if ((long)length + 1 > limit)
                    throw new InvalidDataException("LZ4 解压结果超过允许的最大尺寸。");
                EnsureCapacity(checked(length + 1));
                buffer[length++] = value;
            }

            /// <summary>
            /// &lt;字节集&gt;返回当前有效数据的一份拷贝
            /// <returns><para>返回恰好等于有效长度的字节数组</para></returns>
            /// </summary>
            public byte[] ToArray()
            {
                byte[] result = new byte[length];
                System.Buffer.BlockCopy(buffer, 0, result, 0, length);
                return result;
            }

            /// <summary>
            /// 确保底层数组容量足够
            /// <param name="required">(整数型 需要的最小容量)</param>
            /// </summary>
            private void EnsureCapacity(int required)
            {
                if (required <= buffer.Length)
                    return;                                // 容量已够，无需扩容

                int newCapacity = buffer.Length;
                while (newCapacity < required)
                {
                    // 小容量翻倍，大容量 1.5 倍增长
                    int next = newCapacity < 1024 * 1024
                        ? newCapacity * 2
                        : newCapacity + (newCapacity / 2);
                    if (next <= newCapacity || next > MaxOutputBytes)
                    {
                        // 溢出或超上限时直接取所需大小与上限的较小值
                        newCapacity = checked((int)Math.Min(MaxOutputBytes, required));
                        break;
                    }
                    newCapacity = next;
                }

                if (newCapacity < required)
                    throw new InvalidDataException("LZ4 解压结果超过允许的最大尺寸。");

                Array.Resize(ref buffer, newCapacity);
            }
        }
    }
}
