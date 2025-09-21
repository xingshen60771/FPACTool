# FPACTool

禁止商业用途！仅限个人学习研究使用，游戏资源文件之版权归相关公司所有，严禁将所获得的游戏资源文件用于其他用途，代码作者概不承担因而造成的一切后果！

## 主要功能  
提取和重建游戏“Sora no Kiseki the 1st”的PAC格式的包文件。.NET Framework 4.5框架的C#编写的Windows窗体应用程序，绿色免安装，单文件即点即用。
(需要安装.NET Framework 4.5运行库)

## 基本信息
源码名称：PCK Viewer  
源码版本：1.0.0  
源码作者：52pojie.cn  
源码语言：C#  

## 更新日志  
- 2025.09.22   
1、源码发布  

## 截图预览  
![使用效果](https://github.com/xingshen60771/FPACTool/Screenshot/Screenshot.png)  

## 如何编译  
本软件使用了Tuple多元List，因此依赖于.NET Framework V4.5运行，原则上Visual Studio 2015就可以编译，但是本人是在Visual Studio 2022中编译的，因此建议在Visual Studio 2022中编译。  

## 可能存在的bug  
打包时可能存在文件哈希值混乱，导致游戏程序读取到的是错误的哈希，造成游戏闪退。建议把解包后的文件直接放在游戏根目录下使用，确需打包的请务必备份好原文件。

## 致谢
- [coinkillerl](https://github.com/coinkillerl/FPACker/edit/master/README.md)   —— 参照了关于打包方法的文字描述。

