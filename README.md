
![logo](./imgs/logo.png)

# SchTask

| 类别 | 说明 |
| ---- | --- |
| 作者 | [AnonySec](https://github.com/An0nysec) |
| 团队 | [0x727](https://github.com/0x727) 未来一段时间将陆续开源工具 |
| 定位 | 权限维持，内存加载 |
| 语言 | C# |
| 功能 | 创建隐藏的计划任务，进行权限维持 |

## 什么是 SchTask ?

利用 Windows API，工具化创建隐藏的计划任务，同时绕过安全软件的阻断，达到持久控制。

## 实现原理 ?

### Steps

1. 选择主机随机进程名作为计划任务程序文件名
2. 将计划任务程序文件复制到 `%AppData%\Microsoft\Windows\Themes\` 中
3. 创建的计划任务名取同一随机进程名
4. 计划任务触发器以分钟为单位，无限期持续
5. 更改 Index、删除 SD 的键值，隐藏计划任务对应的 XML 文件
6. ~~删除已添加的计划任务~~

### Articles

更多技术细节，请查看  [Windows计划任务的进阶](https://payloads.cn/2021/0805/advanced-windows-scheduled-tasks.html)

## 开始体验

### Git下载安装

```bash
$ git clone https://github.com/0x727/SchTask_0x727.git
```

> 注：直接下载 [Release](https://github.com/0x727/SchTask_0x727/releases/) 版本快速使用，已经将 `Microsoft.Win32.TaskScheduler.dll` 打包到 SchTask.exe 中。

## 使用方法

### SchTask.exe   (.NET Framework 2.0)

```
C:\>SchTask.exe
  ___       _____ ____ _____
 / _ \__  _|___  |___ \___  |
| | | \ \/ /  / /  __)|  / /
| |_| |>  <  / /  / __/ / /     https://github.com/0x727
 \___//_/\_\/_/  |_____/_/      Author: AnonySec

Usage: SchTask.exe <File Path> <Minutes>
   Eg: SchTask.exe C:\Windows\System32\cmd.exe 10

C:\>SchTask.exe C:\Windows\System32\cmd.exe 1
  ___       _____ ____ _____
 / _ \__  _|___  |___ \___  |
| | | \ \/ /  / /  __)|  / /
| |_| |>  <  / /  / __/ / /     https://github.com/0x727
 \___//_/\_\/_/  |_____/_/      Author: AnonySec

[*] Copy File location:
C:\Users\Administrator\AppData\Roaming\Microsoft\Windows\Themes\SearchApp.exe
[*] Hidden task xml file:
C:\Windows\System32\Tasks\Microsoft\Windows\UPnP\SearchApp
[*] RegistryKey location:
HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tree\Microsoft\Windows\UPnP\SearchApp

[+] Successfully add scheduled task !
```

### 效果图

![image-20210127170232740](./imgs/SchTask.gif)

## 为 SchTask 做贡献

SchTask 是一个免费且开源的项目，我们欢迎任何人为其开发和进步贡献力量。

- 在使用过程中出现任何问题，可以通过 issues 来反馈。
- Bug 的修复可以直接提交 Pull Request 到 dev 分支。
- 如果是增加新的功能特性，请先创建一个 issue 并做简单描述以及大致的实现方法，提议被采纳后，就可以创建一个实现新特性的 Pull Request。
- 欢迎对说明文档做出改善，帮助更多的人使用 SchTask，特别是英文文档。
- 贡献代码请提交 PR 至 dev 分支，master 分支仅用于发布稳定可用版本。
- 如果你有任何其他方面的问题或合作，欢迎发送邮件至 0x727Team@gmail.com 。

> 提醒：和项目相关的问题最好在 issues 中反馈，这样方便其他有类似问题的人可以快速查找解决方法，并且也避免了我们重复回答一些问题。
