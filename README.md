<img src="https://muhu-c-images.pages.dev/file/be403233da8505daff26c.png" align="left" height=186 />  
  
# DateTimer 木沪时间表 (已完结)  

查看、管理时间表以及设置倒计时  
  
![License](https://img.shields.io/github/license/Muhu-C/DateTimer?style=flat-square)
![Lang](https://img.shields.io/badge/Language-C%23_.NET_Framework_4.8.1-blue?style=flat-square)  
![Issues](https://img.shields.io/github/issues/Muhu-C/DateTimer?style=flat-square)
![GitHub](https://img.shields.io/github/downloads/Muhu-C/DateTimer/total?style=flat-square)  
![GitHub](https://img.shields.io/badge/Latest-1.2.0-red?style=flat-square)  
  
-------  
  
## 为什么要做这个程序？  
**作者 MC118CN 是一位学生，所以会在学生的角度下去做一款时间表，方便假期以及学期内对时间的管理**  
  
### 注意：程序已经停更，直到明年中考结束！
  
### 目前公告源  
  
 - [Gitee_Raw](https://gitee.com/zzhkjf/NoticePage/raw/main/DATETIMER.NOTICE) 公告源  
 - [Github_Raw](https://raw.githubusercontent.com/Muhu-C/NoticePage/main/DATETIMER.NOTICE) 公告源（仅海外）  
   - [Gitproxy](https://mirror.ghproxy.com/https://raw.githubusercontent.com/Muhu-C/NoticePage/main/DATETIMER.NOTICE) 公告源(更新延迟)  
   - [Gitmirror_Raw](https://raw.gitmirror.com/Muhu-C/NoticePage/main/DATETIMER.NOTICE) 公告源(更新延迟)  
  
### 用到的开源项目  
 - [HandyControl](https://github.com/ghost1372/HandyControl)  
 - [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)

### 1.2.0 版本特点 (完结)  

- 结束对 DateTimer 的更新
- 如果有缘的话，明年暑假还可以更新
- 完成待办事项显示功能
- 优化了程序大部分代码

### 1.1.2 版本特点

- 新增"待办"功能
- 新增待办事项显示功能(半成品)
- (半)修复 Growl 在显示结束时报错 Win32Exception 的问题
- 修复内存占用在程序运行时不断变大的问题
- 修复了同一时间只能显示一条消息的问题

*ps: 这 HandyControl 都停更好久了，Bug 提交和没提交没区别。*  
*圣诞快乐!!啊啊啊(拍桌子声)好快乐啊啊啊啊啊啊!!!!*

### 1.1.0 版本特点

- 新增编辑时间表中的"新建时间表","新建时间段","删除时间表"和"删除时间段"功能
- 新增"时间点提前提醒"功能
- 新增"时间点到点提醒"功能
- 新增使用 **HandyControl.Controls.Growl** 的提示
- 新增"Log"功能
- 新增提示音(Windows 11 官方提示音封装)
- 修复在"新建时间表"中按上下键导致选中时间表为空的问题
- 更改"关于"部分内容
- 将 **DT_Lib** 中的功能全部转移至 **DateTimer.Utils**
- 优化某些函数代码的占用，减小内存溢出可能
- 优化关闭程序的功能，防止退出时未响应

### 1.0.4 版本特点
 - 新增"系统报告"内容
 - 优化"错误"窗口功能
 - 新增未处理异常的处理

### 1.0.3 版本特点
 - 完善"新建时间表"功能  
 - 优化程序 Binding  
 - 通过 Json_Optimization 优化写入后的 json, 使其更加美观  
 - 代码注释优化  
 - 为防止第一个 Issue 中的问题，将"关于"文本的软件版本直接链接到 AssemblyVersion  
 - 修复了 HomePage 设置按钮在亮色主题下保持暗色的 bug
 - 更改编辑时间"周日"显示为"周七"的问题
  
### 1.0.2 版本特点  
 - 初步添加"新建时间表"功能  
 - 优化时间表 json 解析  
 - 优化主页排版  
 - 添加"按时间分类排序算法"功能
  
### 1.0.1 版本特点  

 - DateTimer 公告源支持自动检测  
 - 优化部分异步代码的执行  
 - 修正某些致命性错误的提示  
 - 统一错误提示  
 - 优化了 HandyControls 控件在 Page 类上的显示  
  
### 1.0.0 Beta 2 版本特点  
  
 - 在小窗口上显示时间表以及离目标剩余的时间  
 - 在控制台设置中更改  
   - 时间表文件位置  
   - 目标名称  
   - 目标时间  
   - 程序主题（亮/暗）  
  
#### 如果遇到了 bug ，请在Github上提出Issue或联系
  
 - muhu-c@outlook.com  
 - [Bilibili 账号](https://space.bilibili.com/1469137723/)

#### 本项目使用 MIT License，完全开源免费，禁止倒卖！
