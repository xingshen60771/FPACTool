using System;
using System.IO;
using System.Windows.Forms;

namespace FPACTool
{
    public partial class Form_Help : Form
    {
        // HTML 文档临时写出路径
        private string tmpPath;
        private string cssPath;

        #region 初始化

        /// <summary>
        /// 实例化当前窗体
        /// </summary>
        public Form_Help()
        {
            InitializeComponent();
            // 设置HTML文本临时写出路径
            tmpPath = Path.Combine(Path.GetTempPath(), "~FPACToolHelp");// 根目录
            cssPath = Path.Combine(tmpPath, "style");// CSS文件夹
            // 设置标题
            this.Text = $"{PublicFunction.GetAPPInformation(1)} 帮助文档";
            // 写出HTML
            WriteHTMLDocument();
            // 显示第一页
            webBrowser.Url = new Uri(Path.Combine(tmpPath, "index.html"));

            // 默认展开目录并显示“欢迎”页
            if (treeView.Nodes.Count > 0)
            {
                treeView.Nodes[0].Expand();
            }

            // 尝试选中“欢迎”节点，如果不存在则直接跳转到“index.htm”
            TreeNode[] indexNodes = treeView.Nodes.Find("index", true);
            if (indexNodes.Length > 0)
            {
                treeView.SelectedNode = indexNodes[0];
            }
            else
            {
                NavigateToPage("index.htm");
            }
        }

        #endregion

        #region 主要实现
        /// <summary>
        /// 写出HTML文档到临时目录。
        /// </summary>
        private void WriteHTMLDocument()
        {
            Directory.CreateDirectory(tmpPath);
            Directory.CreateDirectory(cssPath);

            File.WriteAllText( Path.Combine(cssPath, "style.css"),HelpText.style_css);

            File.WriteAllText( Path.Combine(tmpPath, "index.htm"), HelpText.index_htm);// “欢迎”节点

            File.WriteAllText(Path.Combine(tmpPath, "Pack.htm"),HelpText.pack_htm);// ”打包PAC节点“

            File.WriteAllText( Path.Combine(tmpPath, "Unpack.htm"), HelpText.unpack_htm);// ”解包PAC”节点
            
            File.WriteAllText( Path.Combine(tmpPath, "DDSView.htm"), HelpText.ddsview_htm);// ”DDS浏览”节点

            File.WriteAllText( Path.Combine(tmpPath, "FAQ.htm"),HelpText.faq_htm);// ”FAQ”节点

            File.WriteAllText( Path.Combine(tmpPath, "Chagelog.htm"),HelpText.changelog_htm);// ”更新日志”节点

            File.WriteAllText(Path.Combine(tmpPath, "About.htm"), HelpText.about_htm);// ”更新日志”节点
        }

        /// <summary>
        /// 跳转到指定的本地帮助页面。
        /// <param name="fileName">(文本型 帮助页面文件名)</param>
        /// </summary>
        private void NavigateToPage(string fileName)
        {
            string pagePath = Path.Combine(tmpPath, fileName);

            if (!File.Exists(pagePath))
            {
                MessageBox.Show(
                    "帮助页面不存在：" + fileName,
                    "帮助文档",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Uri pageUri = new Uri(pagePath);

            // 如果当前已经是目标页面，就不重复刷新。
            // 这一判断也用于避免“网页跳转 -> 同步节点 -> AfterSelect -> 再次跳转”的循环。
            if (webBrowser.Url != null &&
                webBrowser.Url.IsFile &&
                string.Equals(webBrowser.Url.LocalPath, pageUri.LocalPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            webBrowser.Url = pageUri;
        }

        #endregion

        #region 事件处理方法

        /// <summary>
        /// 目录树节点选中响应函数。
        /// </summary>
        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
            {
                return;
            }

            switch (e.Node.Name)
            {
                case "root":
                case "index":
                    NavigateToPage("index.htm");
                    break;

                case "pack":
                    NavigateToPage("Pack.htm");
                    break;

                case "unpack":
                    NavigateToPage("Unpack.htm");
                    break;

                case "ddsview":
                    NavigateToPage("DDSView.htm");
                    break;

                case "FAQ":
                    NavigateToPage("FAQ.htm");
                    break;

                case "changelog":
                    NavigateToPage("Chagelog.htm");
                    break;

                case "about":
                    NavigateToPage("About.htm");
                    break;

                case "howtouse":
                    // “使用 FPAC Tool”只是分组节点，没有单独页面。
                    // 选中时只展开其子节点，不强制切换右侧页面。
                    e.Node.Expand();
                    break;
            }
        }

        /// <summary>
        /// 当右侧 HTML 通过“主页 / 上一页 / 下一页 / 相关主题”等链接跳转后，
        /// 同步选中左侧对应的目录树节点。
        /// </summary>
        private void webBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (e.Url == null || !e.Url.IsFile)
            {
                return;
            }

            string nodeName = null;
            string fileName = Path.GetFileName(e.Url.LocalPath);

            if (string.Equals(fileName, "index.htm", StringComparison.OrdinalIgnoreCase))
            {
                nodeName = "index";
            }
            else if (string.Equals(fileName, "Pack.htm", StringComparison.OrdinalIgnoreCase))
            {
                nodeName = "pack";
            }
            else if (string.Equals(fileName, "Unpack.htm", StringComparison.OrdinalIgnoreCase))
            {
                nodeName = "unpack";
            } 
            else if (string.Equals(fileName, "DDSView.htm", StringComparison.OrdinalIgnoreCase))
            {
                nodeName = "ddsview";
            }
            else if (string.Equals(fileName, "FAQ.htm", StringComparison.OrdinalIgnoreCase))
            {
                nodeName = "FAQ";
            }
            else if (string.Equals(fileName, "Chagelog.htm", StringComparison.OrdinalIgnoreCase))
            {
                nodeName = "changelog";
            }
            else if (string.Equals(fileName, "About.htm", StringComparison.OrdinalIgnoreCase))
            {
                nodeName = "about";
            }

            if (string.IsNullOrEmpty(nodeName))
            {
                return;
            }

            TreeNode[] nodes = treeView.Nodes.Find(nodeName, true);
            if (nodes.Length > 0 && treeView.SelectedNode != nodes[0])
            {
                treeView.SelectedNode = nodes[0];
                nodes[0].EnsureVisible();
            }
        }

        private void Form_Help_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Directory.Exists(tmpPath))
            {
                Directory.Delete(tmpPath, true);
            }
        }
       
        #endregion
    }
}
