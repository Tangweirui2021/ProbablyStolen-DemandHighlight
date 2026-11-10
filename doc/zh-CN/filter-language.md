# 筛选语言

[完整英文规范](../manual/filter-language.md) · [文档目录](../README.md)

本 Fork 保留上游表达式语法。表达式决定哪些物品匹配；颜色在类型配色设置中独立配置，不是筛选条件。

| 写法 | 含义 |
| --- | --- |
| `stolen` | 赃物 |
| `contraband` | 违禁品 |
| `owned` / `mine` | 玩家拥有 |
| `type:FOOD` / `type:食物` | 物品类型 |
| `tag:X` / `id:X` / `condition:X` | 标签／物品 ID／成色条件 |
| `a b` / `a and b` / `a + b` | 同时满足 |
| `a or b` / `a \| b` | 任一满足 |
| `not a` / `-a` | 排除 |
| `( ... )` | 分组 |
| `type:(food or medical)` | 对组内各项应用类型键 |

优先级由高到低为 `not`、`and`、`or`。含空格的名称用引号，例如 `type:"Household Good"`；需要作为值使用的 `and`、`or`、`not` 也要加引号。

```text
food -stolen
type:(FOOD or MEDICAL) owned
contraband -owned
```

最后一条可查找顾客自带的违禁品；属性星标不会依赖是否执行了这条查询。错误位置按字符报告，窗口保留上一次有效筛选。精确语法、转义、结合性和错误类别以[完整英文规范](../manual/filter-language.md)及 `tests/LanguageTest.cs` 为准。
