# Type Demand & Risk Stars — 安装测试包

版本：1.0.0-wrt.4。已使用提供的真实程序集完成编译、API 签名检查及游戏自带扫描；游戏内视觉效果尚待实测。

1. 将压缩包中的 `wrt_type_demand_stars` 文件夹放入游戏根目录的 `Mods` 文件夹。
2. 停用原 Demand Highlight、Client Demand Highlight 和其他重复画框或星标的 Mod。
3. 在游戏主菜单 Mods 中启用 **Type Demand & Risk Stars**，然后重启游戏。
4. 进入存档后，需求框和属性星标默认开启，无需先绑定快捷键。

主菜单 → Mods → 本模组齿轮按钮可配置颜色、圆角、粗细、底色及星标大小。颜色填写 `#RRGGBB` 或 `#RRGGBBAA`。需求框开关与筛选窗口快捷键在游戏按键设置中绑定；关闭需求框不会关闭星标。

从旧版更新时直接替换安装文件夹，保留现有配置即可。星标大小的旧设置会自动采用新尺寸，无需恢复默认；默认等级 2 约为三分之一格，固定在右上角。

默认星标：仅赃物黄星，仅违禁品绿星，两种属性同时存在为一颗红星。普通和拖拽视图分别标记；容器不继承内容物的星标属性。

主库存左下侧、垃圾桶下方新增常驻筛选区，可滚动勾选类型、赃物和违禁品。“清空”恢复顾客需求高亮；“更多”打开原高级筛选窗口。两处共用筛选条件，避免重复高亮。垃圾桶内的匹配商品及垃圾桶本体现在也会标记。

需要反馈时，提供游戏版本、当前 Mod 列表，以及 `Player.log` 中 `[TypeDemandStars]` 附近的内容。

## English

Copy the included `wrt_type_demand_stars` folder into the game's `Mods` directory. Disable the original Demand Highlight and other overlapping marker mods. Enable **Type Demand & Risk Stars** in the main-menu Mods list, then restart.

Outlines and permanent risk stars are enabled by default. Configure colors and geometry through the mod's gear button. Both optional hotkeys start unbound. The outline hotkey does not turn off stars.

Yellow means stolen only, green contraband only, red both. Native drag previews are supported. The container's star describes the container itself.

Real-assembly compilation, API checks and the game's scanner passed; actual in-game appearance and behavior still require player testing. No game assemblies or test stand-ins are included.

Updating from an earlier WRT version: replace the mod folder and keep your configuration. Existing size levels become compact automatically. Default level 2 is about one third of a single cell, independent of the item footprint.

The embedded checkbox dock sits below the trashcan, left of the main inventory. Scroll through types and risk statuses. Clear restores customer demand; More opens the shared advanced filter. Matching trashcan contents and the bin itself are now included.
