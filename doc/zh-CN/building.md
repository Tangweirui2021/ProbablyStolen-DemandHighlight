# 构建与安装

[English](../manual/building.md) · [文档目录](../README.md)

## 环境和当前状态

需要 .NET SDK 8 或更新版本，以及当前游戏的 `Probably Stolen_Data/Managed` 文件夹。游戏 Mod 编译目标是 `netstandard2.1`，测试运行目标是 .NET 8。

本仓库不分发游戏文件或可直接安装的发行 DLL。`manifest.xml` 保留上游的 `0.46D` 声明，尚未完成当前正式版验证。测试替身编译出来的程序不能当作 Mod 安装。

## 主模组

在仓库根目录执行：

```bash
GAME='/path/to/Probably Stolen' ./build.sh
GAME='/path/to/Probably Stolen' ./build.sh --install
```

Windows PowerShell：

```powershell
./build.ps1 -GamePath 'D:\SteamLibrary\steamapps\common\Probably Stolen'
./build.ps1 -GamePath 'D:\SteamLibrary\steamapps\common\Probably Stolen' -Install
```

直接调用 SDK，或只有 Managed 文件夹时：

```bash
dotnet build TypeDemandStars.csproj -c Release -p:GamePath='/path/to/Probably Stolen'
dotnet build TypeDemandStars.csproj -c Release -p:GameManagedDir='/path/to/Managed'
```

缺少 `Assembly-CSharp.dll` 会提前报错。项目引用游戏程序集，但不将它们复制进 Mod 输出；不再需要 Visual Studio Build Tools 的 `CSC` 变量。

产物在 `out/wrt_type_demand_stars/`，主 DLL 为 `TypeDemandStars.dll`，同时包含 manifest、翻译和许可证。不要安装 `tests/bin/` 中的替身测试程序。

## 启用和更新

不使用自动安装参数时，将整个产物文件夹复制到 `<游戏目录>/Mods/`。停用原版 Demand Highlight 和其他重叠高亮 Mod，启用 **Type Demand & Risk Stars**，然后重启。

本 Fork 尚无独立工坊订阅；原作者的工坊链接安装的是原版。更新只替换本版目录，游戏外部 `ModConfigs` 中保存的设置不会被覆盖。

## 测试

```bash
./tests/run.sh
python3 tools/check_docs.py
```

测试包含 12 组纯逻辑测试和使用替身 API 执行真实修改代码的集成测试，无需游戏文件。这不能代替实际游戏程序集编译、Harmony 补丁及旋转／翻转／裁切验证，见 [VALIDATION.md](../../VALIDATION.md)。

## 查询 API 示例

可选开发工具会运行查询并写入 `Player.log`，不包含在玩家版输出内：

```bash
GAME='/path/to/Probably Stolen' ./tools/QuerySample/build.sh --install
```

它也使用 .NET SDK 和当前游戏程序集。启用本 Fork 和 Query Sample 后可测试接口，见[查询 API](query-api.md)。
