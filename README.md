# FPAC Tool

禁止商业用途！仅限个人学习研究使用，游戏资源文件之版权归相关公司所有，严禁将所获得的游戏资源文件用于其他用途，代码作者概不承担因而造成的一切后果！

## 主要功能  
提取和重建部分游戏的PAC格式的包文件。
截至当前版本，本工具已适配以下游戏的 PAC 文件打包、提取：    
- 空之轨迹 1st（The Legend of Heroes: Trails in the Sky 1st）     
- 空之轨迹 2nd（The Legend of Heroes: Trails in the Sky 2nd）     
- 亰都幻都 樱花幻舞（KYOTO XANADU -the Blooming Phantom-）     

未来同厂商的游戏的 PAC 文件理论上适配，请自行尝试。


此为基于.NET Framework 4.5框架的C#编写的Windows窗体应用程序，绿色免安装，单文件即点即用。
(需要安装[.NET Framework 4.5](https://dotnet.microsoft.com/zh-cn/download/dotnet-framework/net45)运行库)

## 基本信息
源码名称：FPAC Tool  
源码版本：2.1.0  
遵循协议：CC0-1.0    
源码语言：C#  

## 更新日志  

- 2026.10.04——V2.1    
  1. 【细节优化】    
       1) 移除了打开 PAC 文件时又取消打开所弹出的未选择文件提示框，此为开发期间为调试程序所遗留，故做移除处理。    
       2) 生成 PAC 的时候，由先前的先检查后写入改为边检查边写入，提升处理效率。    
       3) 打包/提取完成后将询问是否打开输出目录，省去还要到文件浏览器一通乱翻的麻烦。     
  2. 【文案调整】调整了部分文案、修正错别字。     

- 2026.09.18——V2.0    
  1. 【全新界面】工具界面全面更新！现在可以像使用压缩文件管理器一样浏览、打包和提取文件。    
  2. 【新增功能】新增 DDS 图像浏览功能。打开 PAC 文件并找到 DDS 文件后，双击即可预览，同时支持保存为 JPG、PNG 等常规图片格式。    
  3. 【Bug 修复】修复因打包生成的 PAC 文件路径哈希值错误而导致游戏闪退的问题。    

- 2025.09.22   
  1. 源码发布   

## 截图预览  
![使用效果](https://github.com/xingshen60771/FPACTool/blob/master/Screenshot/Screenshot01.png)  

![使用效果](https://github.com/xingshen60771/FPACTool/blob/master/Screenshot/Screenshot02.png)  

![使用效果](https://github.com/xingshen60771/FPACTool/blob/master/Screenshot/Screenshot03.png)  

## 如何编译  
本软件使用了Tuple多元List，因此依赖于.NET Framework V4.5运行，原则上Visual Studio 2015就可以编译，但是本人是在Visual Studio 2022中编译的，因此建议在Visual Studio 2022中编译。  

## 其它补充  
考虑到本版FPACTool工程非常庞大，所以我把一些部分机械性较强、易出错的业务代码交给AI辅助加工了。因为DDS浏览功能不是本项目的核心功能，不是本项目讨论的重点，只为实现DDS图像的显示，所以便参考了一些资料写的偷了个懒，把出现问题的也一并交给AI来调整了，望理解！

## 致谢
- [coinkillerl](https://github.com/coinkillerl/FPACker/edit/master/README.md)   —— 参照了关于打包方法的文字描述。

