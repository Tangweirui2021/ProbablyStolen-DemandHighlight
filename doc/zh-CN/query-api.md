# 查询 API

[完整英文接口说明](../manual/query-api.md) · [文档目录](../README.md)

本 Fork 保留上游契约 1.x。依赖本版时使用 Mod ID `wrt_type_demand_stars`，不要同时启用原版和本版两个提供者。

将 [`api/DemandHighlightQuery.cs`](../../api/DemandHighlightQuery.cs) 复制进自己的 Mod，无需引用本版 DLL：

```csharp
var found = new System.Collections.Generic.List<GameItem>();
var result = DemandHighlightQuery.Run("type:(FOOD or MEDICAL) -stolen", found);
if (result.Ok)
    UnityEngine.Debug.Log("Found " + result.Count + " items");
found.Clear();
```

- 查询覆盖全店库存、柜台、展示柜、隐藏及售出区、垃圾桶和内容物，包括关闭／上锁的容器；每件物品按自身属性判断。
- 容器只会因为本体匹配而进入结果，不会自动汇总内容物为 API 匹配。
- 这与“所有已注册界面物品的永久星标”范围不同。API 不返回星标注册表或颜色。
- 从游戏主线程调用；每次 Run 重新遍历店铺，不要逐帧调用。
- 物品引用属于当前场景，使用后释放；无提供者时返回 `NotInstalled`，不会强制失败。

`DemandHighlightQuery` 类名和 `demand_highlight.query.*` AppDomain 键是保留的兼容接口名，不是安装目录或本 Fork 的 Mod ID。

开发用 Query Sample 使用相同 SDK 和游戏程序集构建：

```bash
GAME='/path/to/Probably Stolen' ./tools/QuerySample/build.sh --install
```

字段、错误码、生命周期和兼容契约以[完整英文接口说明](../manual/query-api.md)为准。
