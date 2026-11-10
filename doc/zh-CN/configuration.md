# 配置

[English](../manual/configuration.md) · [文档目录](../README.md)

启用本模组后，从**主菜单 → Mods → 齿轮按钮**打开游戏自动生成的配置面板。颜色以文本填写，按 Apply 应用。经营画面暂停菜单中的配置入口尚未确认。

## 配置键

| 键 | 默认值 | 范围或作用 |
| --- | --- | --- |
| `lineThickness` | `2` | 框线粗细，1–6 界面像素 |
| `cornerRadius` | `2` | 外角圆角，0–8；0 为直角 |
| `fillTint` | `true` | 直接匹配物品框内的淡底色 |
| `defaultOutlineColor` | `#5CEB6B` | 没有有效产品类型时的兜底颜色 |
| `containerOutlineColor` | `#80D8FF` | 内容物四角提示及机器储物标签页的统一颜色 |
| `typeColor_<TYPE_ID>` | 按类型预设 | 每种物品类型的独立颜色；遇到新的匹配产品类型时增加配置项 |
| `typePriority` | 见 `MarkerSettings.DefaultPriority` | 英文类型 ID，以逗号分隔，越靠前优先级越高 |
| `riskStars` | `true` | 永久星标开关，独立于需求框快捷键 |
| `stolenStarColor` | `#FFD84A` | 仅赃物的星体颜色 |
| `contrabandStarColor` | `#66ED80` | 仅违禁品的星体颜色 |
| `bothStarColor` | `#FF5656` | 两种属性同时存在的星体颜色 |
| `starOutlineColor` | `#211C30` | 像素星描边颜色 |
| `starPixelScale` | `2` | 整数倍率 1–4；原始完整掩码为 11×11 像素 |
| `starInset` | `2` | 距物品视图右上角的内边距，0–16 界面像素 |
| `filterNumbers` | `true` | 物品筛选窗口统计 |

“开关需求高亮”和“打开物品筛选”两个快捷键默认未绑定，在游戏按键设置中绑定。旧版仅对顾客卖货生效的 `highlightClientGoods` 设置由永久星标机制取代。

## 多类型和默认配色

同一物品有多个类型时，按 `typePriority` 从前向后找第一个匹配的类型，不区分大小写。没写入优先级的类型按稳定的 ID 顺序选择；没有有效产品类型则使用兜底颜色。顾客和法律属性都不参与框色选择。

`CONTRABAND`、`STOLEN`、`RESTRICTED_*` 不参与框色选择，但仍可参与顾客判断和筛选。

默认会给同一大类中的相近类型设置类似颜色：食物为桃色、工具为淡黄、水和饮料为蓝色、医疗用品为薄荷色、武器为浅红、材料为米色、机器为紫色。每个已注册类型都能单独修改，默认不同类型可能共用一种颜色。

## 颜色格式与保存

支持 `#RRGGBB` 或 `#RRGGBBAA`，开头的 `#` 也可省略。Alpha 通道控制对应框线／底色或星体的透明度；星体和描边是独立颜色。

错误颜色会记录警告并沿用本次运行中上一次有效值；如果启动时就是错误值，则使用该项默认颜色。JSON 中的错误文字不会被悄悄覆盖，修正后再次 Apply 即可。

游戏自动保存到：

`<Application.persistentDataPath>/ModConfigs/wrt_type_demand_stars.json`

手动修改时退出游戏，保留游戏生成的键值列表结构。缺少的键使用默认值。配置文件不在 Mod 安装目录内，更新 Mod 不会覆盖设置。无需 YAML 解析器或额外文件权限。
