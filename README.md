# WebExplorer

局域网文件传输工具 —— 电脑端运行服务，手机浏览器即可访问、浏览、上传和下载文件，无需安装任何 APP。

## 功能特性

- **仿 Windows 11 资源管理器界面**：侧边栏导航 + 面包屑 + 工具栏，响应式设计适配手机/平板/桌面
- **文件管理**：浏览目录、下载文件、上传文件、新建文件夹、删除文件/文件夹
- **快速访问**：桌面、文档、下载、图片等常用目录一键直达；自动列出所有磁盘驱动器
- **二维码扫码访问**：生成访问地址二维码，手机扫码即用
- **系统托盘**：最小化到托盘后台运行，右键菜单快速退出/重启服务
- **开机自启动**：可选开机自动启动服务
- **配置热更新**：修改端口/IP 等配置后自动生效
- **路径安全防护**：路径规范化 + 穿越段检查，防止目录遍历攻击
- **大文件支持**：流式上传/下载，无大小限制

## 技术栈

| 层 | 技术 |
|----|------|
| 后端框架 | .NET 10.0 (net10.0-windows) + ASP.NET Core |
| UI 框架 | WinForms（服务端控制台与设置窗体） |
| HTTP 服务器 | Kestrel（ASP.NET Core 内置） |
| 前端 | 纯 HTML5 + CSS3 + Vanilla JavaScript（无构建工具） |
| 二维码 | QRCoder |
| 静态资源 | EmbeddedResource 嵌入打包 |

## 快速开始

### 环境要求

- .NET 10.0 SDK
- Windows 系统（WinForms 依赖）

### 构建运行

```bash
cd WebExplorer
dotnet build
dotnet run --project WebExplorer
```

### 使用方式

1. 启动程序后，主窗口显示本机 IP 和端口
2. 点击「启动服务」按钮
3. 手机扫码或浏览器访问显示的地址
4. 在手机端即可浏览、上传、下载文件

## 项目结构

```
WebExplorer/
├── documents/                    # 项目文档
│   ├── prd.md                    # 产品需求文档
│   └── tech-architecture.md      # 技术架构文档
├── wwwroot/                      # 前端静态文件（嵌入资源）
│   ├── index.html                # 主页面
│   ├── css/style.css             # 样式（仿 Win11 资源管理器）
│   └── js/app.js                 # 应用逻辑
└── WebExplorer/                  # 后端项目
    ├── Program.cs                # 程序入口
    ├── MainForm.cs               # 主窗体（服务控制台、托盘）
    ├── FormSettings.cs           # 设置窗体（端口/IP/二维码）
    ├── Models/FileModels.cs      # 数据模型
    ├── Services/                 # 服务层
    │   ├── HttpServerService.cs  # HTTP 服务器管理
    │   ├── FileService.cs        # 文件操作
    │   ├── UploadService.cs      # 上传处理
    │   ├── DownloadService.cs    # 下载处理
    │   ├── SettingsService.cs    # 配置管理
    │   └── LogService.cs         # 日志服务
    ├── Controllers/
    │   └── FileApiController.cs  # API 控制器
    └── Utils/                    # 工具类
        ├── PathSecurity.cs       # 路径安全
        ├── MimeTypes.cs          # MIME 映射
        └── QRCodeGenerator.cs    # 二维码生成
```

## API 接口

| 路由 | 方法 | 用途 |
|------|------|------|
| `/api/files?path=` | GET | 获取目录内容 |
| `/api/download?path=` | GET | 下载文件 |
| `/api/upload` | POST | 上传文件（FormData） |
| `/api/delete` | DELETE | 删除文件/文件夹 |
| `/api/newfolder` | POST | 创建新文件夹 |
| `/api/drives` | GET | 获取驱动器列表 |
| `/api/quickaccess` | GET | 获取快速访问路径 |

## 文档

- [产品需求文档 (PRD)](documents/prd.md)
- [技术架构文档](documents/tech-architecture.md)

## 许可证

见 [LICENSE.txt](LICENSE.txt)
