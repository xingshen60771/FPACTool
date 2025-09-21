using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FPACTool
{
    /// <summary>
    /// 公共函数
    /// </summary>
    internal class PublicFunction
    {
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


    }
}
