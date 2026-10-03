using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace FPACTool
{
    internal static class Program
    {
        /*
         〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓
         FAPC Tool源代码
         --------------------------------------------
         52pojie@烟99 
         
         转载务必保留此部分注释！本源码首发平台为GitHub，如果你是通过付费下
         载（充值会员、下载点等），表示你已经上当受骗！转载过的源码安全性无
         法保证，请尽快删除 ，并到本源码首发链接下载。

         本源码发布链接：https://github.com/xingshen60771/FPACTool
        
         【基本介绍】
          FPACTool 是一款用于 PAC 文件打包与解包的小工具，界面简洁、操作直观，
          既能够生成用于游戏 MOD 补丁的 PAC 包，也能够从 PAC 包中提取所需内容。
          同时支持 DDS 格式图像的预览，从而省去要先提取再用专门工具打开的麻烦。
          截至当前版本，本工具已适配以下游戏的 PAC 文件打包、提取：
           ·空之轨迹 1st（The Legend of Heroes: Trails in the Sky 1st）
           ·空之轨迹 2nd（The Legend of Heroes: Trails in the Sky 2nd）
           ·亰都幻都 樱花幻舞（KYOTO XANADU -the Blooming Phantom-）
          未来同厂商的游戏的 PAC 文件理论上适配，请自行尝试。

         【郑重声明】
          本源码（含成品）仅供技术学习与交流讨论使用！严禁任何非法用途！

          考虑到本版FPACTool工程非常庞大，所以我把一些部分机械性较强、易出
          错的业务代码交给AI辅助加工了。因为DDS浏览功能不是本项目的核心功
          能，不是本项目讨论的重点，只为实现DDS图像的显示，所以便参考了一
          些资料写的偷了个懒，把出现问题的也一并交给AI来调整了，望理解！
         
         〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓
        */


        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 全局异常日志：未捕获异常写到程序目录下的 crash.log，便于定位偶发问题。
            Application.ThreadException += (sender, e) =>
            {
                LogException(e.Exception);
                MessageBox.Show("发生未处理异常，详情已写入 crash.log：\n" + e.Exception.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                LogException(e.ExceptionObject as Exception);
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 许可协议：检查注册表，未同意则先弹 Form_License
            const string regKeyPath = @"SOFTWARE\52pojie\FPACTool\Main";
            RegistryKey key = Registry.CurrentUser.OpenSubKey(regKeyPath);
            object licValue = null;
            if (key != null)
            {
                licValue = key.GetValue("License");
                key.Close();
            }
            bool licenseAccepted = (licValue != null && Convert.ToInt32(licValue) == 0);

            if (!licenseAccepted)
            {
                using (var licForm = new Form_License())
                {
                    licForm.ShowDialog();
                    if (!licForm.LicenseAccepted)
                    {
                        // 用户在许可窗体未同意（点退出/取消勾选撤回/直接关闭）。
                        // 门禁分支内已执行 Environment.Exit；此处仅作防御性退出。
                        return;
                    }
                }
            }

            Application.Run(new MainForm());
        }

        /// <summary>
        /// 输出异常到日志
        /// <param name="ex">(Exception 异常内容)</param>
        /// </summary>
        static void LogException(Exception ex)
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");
                System.IO.File.AppendAllText(path,
                    "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " +
                    (ex == null ? "(null exception)" : ex.ToString()) + "\r\n\r\n",
                    System.Text.Encoding.UTF8);
            }
            catch { }
        }
    }
}
