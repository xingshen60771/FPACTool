using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace FPACTool
{
    /// <summary>
    /// 公共函数
    /// </summary>
    internal class PublicFunction
    {
        #region 公共辅助方法
        ///<summary>
        /// &lt;文本型&gt; 取软件基本信息 
        /// <param name="paramcode">(整数型 要获取的信息代码)<para>参数代码含义:</para>1：取软件名称；2:取软件版本；3:取软件开发者；4、取软件产品名称。<para></para></param>
        /// <returns><para></para>返回文本型基本信息结果，失败使用了除1-4以外的参数则返回"InvalidRequest"。</returns> 
        /// </summary>
        public static string GetAPPInformation(int paramcode)
        {
            //取软件程序集
            Assembly asm = Assembly.GetExecutingAssembly();
            //取软件标题、版本、公司、产品名称
            AssemblyTitleAttribute asmdis = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyTitleAttribute));
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            AssemblyCompanyAttribute asmcpn = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyCompanyAttribute));
            AssemblyProductAttribute Product = (AssemblyProductAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyProductAttribute));
            string ver = "Ver " + version.ToString();
            string title = asmdis.Title;
            string company = asmcpn.Company;
            string appEnglishNane = Product.Product;
            if (paramcode == 1)
            {
                return title;
            }
            else if (paramcode == 2)
            {
                return ver;
            }
            else if (paramcode == 3)
            {
                return company;
            }
            else if (paramcode == 4)
            {
                return appEnglishNane;
            }
            else
            {
                return "InvalidRequest";
            }
        }

        /// <summary>
        /// &lt;文本型&gt; 字节大小数值转换
        /// <param name="bytes">(长整型 欲转换的字节大小数组)</param>
        /// <returns><para>成功返回字节大小</para></returns>
        /// </summary>
        public static string BytesToSize(long size)
        {
            var num = 1024.00; //byte
            if (size < num)
                return size + " Byte";
            if (size < Math.Pow(num, 2))
                return (size / num).ToString("f2") + " KB";
            if (size < Math.Pow(num, 3))
                return (size / Math.Pow(num, 2)).ToString("f2") + " MB";
            if (size < Math.Pow(num, 4))
                return (size / Math.Pow(num, 3)).ToString("f2") + " GB";
            if (size < Math.Pow(num, 5))
                return (size / Math.Pow(num, 4)).ToString("f2") + " TB";
            if (size < Math.Pow(num, 6))
                return (size / Math.Pow(num, 5)).ToString("f2") + " PB";
            if (size < Math.Pow(num, 7))
                return (size / Math.Pow(num, 6)).ToString("f2") + " EB";
            if (size < Math.Pow(num, 8))
                return (size / Math.Pow(num, 7)).ToString("f2") + " ZB";
            if (size < Math.Pow(num, 9))
                return (size / Math.Pow(num, 8)).ToString("f2") + " YB";
            if (size < Math.Pow(num, 10))
                return (size / Math.Pow(num, 9)).ToString("f2") + "DB";
            return (size / Math.Pow(num, 10)).ToString("f2") + "NB";
        }

        /// <summary>
        /// 将小端存储的字节数组转换为高位在前显示的十六进制字符串。
        /// </summary>
        /// <param name="bytes">小端存储的字节数组</param>
        /// <returns>高位在前的十六进制字符串</returns>
        public static string LittleEndianToHexString(byte[] bytes)
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

        public static string GetCurrentTime()
        {
            return DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss:fff]  ");
        }
        #endregion

        /// <summary>
        /// 生成随机文件夹名称
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GenerateFolderName(string input)
        {
            // 获取当前时间精确到毫秒，格式：yyyyMMddHHmmssfff
            string timeStr = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            // 拼接输入字符和时间
            string combined = input + timeStr;

            // 计算 MD5 哈希（使用 .NET 内置类，无需外部库）
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(combined));

                // 将哈希字节转换为十六进制字符串
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); // 使用小写十六进制，也可改为 "X2" 大写
                }

                // 取前8位作为八位字符文本
                string eightChars = sb.ToString().Substring(0, 8);

                // 返回最终结果
                return "FPACTOOL$Ext" + eightChars.ToUpper();
            }
        }
    }
}
