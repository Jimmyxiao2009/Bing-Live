# Bing Live / TravelBufen

面向 **Windows 8.1 / Windows RT 8.1 和 Windows Phone 8.1** 的 C#/XAML 旅游客户端原型。项目保留了这一代 Windows 的 Hub、分组列表和详情页导航，并接入了 TripAdvisor 地点搜索的数据层。

这是两个共享代码的 8.1 应用工程，**不是 Windows 10 UWP 工程**，也不使用 WinUI 3。

## 当前功能与边界

- 首页 Hub、分组页、条目详情页，以及挂起和导航状态处理。
- 本地 `SampleData.json` 示例内容；默认界面仍含模板示例文字和图片。
- `TripAdvisorDataSource` 包含 Location Search 请求与名称、地址解析；API Key 为空或请求失败时显示本地占位地点。
- 首页探索关键字目前固定为 `Yosemite National Park`。地点详情、真实评分、点评数和照片请求尚未完成；点击探索地点目前只记录调试信息。

当前源码中的 API Key 为空，默认运行结果不能视为真实在线旅游数据。项目没有实现行程规划、预订或完整的旅游服务后端。

## 源码位置

| 路径 | 内容 |
|---|---|
| `live.travel/src/TravelBufen.sln` | 双平台解决方案 |
| `live.travel/src/TravelBufen.Shared/` | 共享入口、数据模型、示例 JSON 和导航辅助类 |
| `live.travel/src/TravelBufen.Windows/` | Windows / Windows RT 8.1 界面 |
| `live.travel/src/TravelBufen.WindowsPhone/` | Windows Phone 8.1 界面 |
| `live.travel/design/` | 设计参考资料 |

## 构建

需要 Windows、支持旧式 C#/XAML 应用的 Visual Studio/MSBuild，以及 **Windows 8.1 SDK / Windows Phone 8.1 SDK**。仅安装现代 .NET SDK 不能替代这些组件。本次源码归档使用 VS2017 MSBuild 15 完成了两个工程的 Release ARM 编译。

在仓库根目录执行；如果 VS2017 安装位置不同，修改 `$msbuild`：

```powershell
$msbuild = 'C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe'
& $msbuild 'live.travel\src\TravelBufen.WindowsPhone\TravelBufen.WindowsPhone.csproj' `
  /t:Build /p:Configuration=Release /p:Platform=ARM /p:VisualStudioVersion=15.0
& $msbuild 'live.travel\src\TravelBufen.Windows\TravelBufen.Windows.csproj' `
  /t:Build /p:Configuration=Release /p:Platform=ARM /p:VisualStudioVersion=15.0
```

编译输出位于各工程的 `bin\ARM\Release\`。部分旧工具链只生成部署布局和 `*.build.appxrecipe`，需要按该 recipe 的文件映射使用 Windows SDK `MakeAppx` 打包，再用与清单 Publisher 一致的证书签名。

Windows 工程的 ARM 包面向 Windows RT；手机应选择 **Travel Phone** 包，两者不能混用。

## 安装包

从 [GitHub Releases](https://github.com/Jimmyxiao2009/Bing-Live/releases) 获取 `TravelPhone` 或 `TravelWindows` 的 `.appx` 和安装 ZIP。开发证书需要在目标设备上受信任，私钥不随仓库或 Release 分发。

2026-11-01 的归档完成了编译、包完整性、ARM32 架构和签名检查，未在本轮重新进行手机安装或在线 API 验证。旧 SDK 可用性及外部旅游 API 的访问条件仍会影响后续构建和使用。
