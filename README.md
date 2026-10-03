# FPACTool

禁止商业用途！仅限个人学习研究使用，游戏资源文件之版权归相关公司所有，严禁将所获得的游戏资源文件用于其他用途，代码作者概不承担因而造成的一切后果！    

## 主要功能  
PACTool 是一款用于 PAC 文件打包与解包的小工具，界面简洁、操作直观，既能够生成用于游戏 MOD 补丁的 PAC 包，也能够从 PAC 包中提取所需内容。同时支持 DDS 格式图像的预览，从而省去要先提取再用专门工具打开的麻烦。    
截至当前版本，本工具已适配以下游戏的 PAC 文件打包、提取：    
- 空之轨迹 1st（The Legend of Heroes: Trails in the Sky 1st）    
- 空之轨迹 2nd（The Legend of Heroes: Trails in the Sky 2nd）    
- 亰都幻都 樱花幻舞（KYOTO XANADU -the Blooming Phantom-）    
未来同厂商的游戏的 PAC 文件理论上适配，请自行尝试。    

本程序需要需要安装.NET Framework 4.5运行库。    

## 基本信息
源码名称：PCK Viewer  
源码版本：1.0.0  
源码作者：52pojie.cn  
源码语言：C#  

## 更新日志  
- 2026.10.03    
1、【细节优化】生成 PAC 的时候，由先前的先检查后写入改为边检查边写入，提示处理效率。

- 2026.09.18——V2.0    
1、【全新界面】工具界面全面更新！现在可以像使用压缩文件管理器一样浏览、打包和提取文件。    
2、【新增功能】新增 DDS 图像浏览功能。打开 PAC 文件并找到 DDS 文件后，双击即可预览，同时支持保存为 JPG、PNG 等常规图片格式。    
3、【Bug 修复】修复因打包生成的 PAC 文件路径哈希值错误而导致游戏闪退的问题。    

- 2025.09.22   
1、源码发布  

## 截图预览  
![使用效果](https://github.com/xingshen60771/FPACTool/Screenshot/Screenshot.png)  

## 如何编译  
本软件使用了Tuple多元List，因此依赖于.NET Framework V4.5运行，原则上Visual Studio 2015就可以编译，但是本人是在Visual Studio 2022中编译的，因此建议在Visual Studio 2022中编译。  

## 可能存在的bug  
打包时可能存在文件哈希值混乱，导致游戏程序读取到的是错误的哈希，造成游戏闪退。游戏支持加载已解包的文件，所以建议把解包后的文件直接放在游戏根目录下使用，确需打包的请务必备份好原文件。

## 致谢
- [coinkillerl](https://github.com/coinkillerl/FPACker/edit/master/README.md)   —— 参照了关于打包方法的文字描述。

如需EXE成品，请到右侧release中下载。
