# Type Demand & Risk Stars — 安装测试包

版本：1.0.0-wrt.2。已使用提供的真实程序集完成编译、API 签名检查及游戏自带扫描；游戏内视觉效果尚待实测。

1. 将压缩包中的 `wrt_type_demand_stars` 文件夹放入游戏根目录的 `Mods` 文件夹。
2. 停用原 Demand Highlight、Client Demand Highlight 和其他重复画框或星标的 Mod。
3. 在游戏主菜单 Mods 中启用 **Type Demand & Risk Stars**，然后重启游戏。
4. 进入存档后，需求框和属性星标默认开启，无需先绑定快捷键。

主菜单 → Mods → 本模组齿轮按钮可配置颜色、圆角、粗细、底色及星标大小。颜色填写 `#RRGGBB` 或 `#RRGGBBAA`。需求框开关与筛选窗口快捷键在游戏按键设置中绑定；关闭需求框不会关闭星标。

默认星标：仅赃物黄星，仅违禁品绿星，两种属性同时存在为一颗红星。普通和拖拽视图分别标记；容器不继承内容物的星标属性。

需要反馈时，提供游戏版本、当前 Mod 列表，以及 `Player.log` 中 `[TypeDemandStars]` 附近的内容。

## English

Copy the included `wrt_type_demand_stars` folder into the game's `Mods` directory. Disable the original Demand Highlight and other overlapping marker mods. Enable **Type Demand & Risk Stars** in the main-menu Mods list, then restart.

Outlines and permanent risk stars are enabled by default. Configure colors and geometry through the mod's gear button. Both optional hotkeys start unbound. The outline hotkey does not turn off stars.

Yellow means stolen only, green contraband only, red both. Native drag previews are supported. The container's star describes the container itself.

Real-assembly compilation, API checks and the game's scanner passed; actual in-game appearance and behavior still require player testing. No game assemblies or test stand-ins are included.
