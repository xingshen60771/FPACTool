// SPDX-License-Identifier: CC0-1.0

// 本窗体源码使用AI辅助编写，因为本项目为PAC打包解包工具，DDS浏览器不是本项目核心功能。

using System;
using System.Windows.Forms;

namespace FPACTool
{
    public partial class Form_DDSProperties : Form
    {
        // 引入 DDS 元素容器
        private DDSMetadata metadata;

        /// <summary>
        /// 实例化当前窗体
        /// </summary>
        public Form_DDSProperties()
        {
            InitializeComponent();
            InitializePropertiesView();
        }

        /// <summary>
        /// 带元数据实例化当前窗体，并填充属性内容
        /// </summary>
        internal Form_DDSProperties(DDSMetadata metadata)
            : this()
        {
            SetMetadata(metadata);
        }

        // 属性窗口是 DDS 浏览窗口的辅助浮动窗。首次显示时不抢夺
        // Owner 的激活状态，避免 DDSView 打开后还要再点一次才能继续操作。
        /// <summary>
        /// 首次显示时不抢夺 Owner 的激活状态
        /// </summary>
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        /// <summary>
        /// 初始化属性视图布局
        /// </summary>
        private void InitializePropertiesView()
        {

            this.StartPosition = FormStartPosition.Manual;  // 设置窗体启动位置为手动
            this.ShowInTaskbar = false;                     // 设置窗体不显示在任务栏

            groupBox.Dock = DockStyle.Fill;                 // 设置 groupBox 填充整个窗体
            groupBox.Padding = new Padding(6);              // 设置 groupBox 内边距为 6 像素

            // 设置 textBox 填充整个 groupBox，并设置为只读、多行、无换行、带滚动条
            textBox.Dock = DockStyle.Fill;
            textBox.ReadOnly = true;
            textBox.Multiline = true;
            textBox.WordWrap = false;
            textBox.ScrollBars = ScrollBars.Both;
            textBox.TabStop = false;
        }

        /// <summary>
        /// 设置 DDS 元数据并刷新属性内容
        /// </summary>
        internal void SetMetadata(DDSMetadata value)
        {
            // value不能为空
            if (value == null)
                throw new ArgumentNullException("value");

            // 更新元数据并刷新属性内容
            metadata = value;
            textBox.Text = metadata.ToDisplayText();
            this.Text = "DDS 属性 - " + metadata.FileName;
            textBox.SelectionStart = 0;
            textBox.SelectionLength = 0;
        }
    }
}
