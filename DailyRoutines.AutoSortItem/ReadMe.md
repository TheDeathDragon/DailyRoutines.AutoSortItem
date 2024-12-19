## AutoSortItem

每次切换地图自动整理背包物品。

### 游戏内排序指令说明

`/道具整理(整理) 子命令`

`/itemsort`

`/isort`

可以以比“自动整理”更加详细的设置来整理物品的排列顺序。

该命令中设置的条件与原有的“自动整理”条件不会发生冲突，只会对该命令有影响。

### 子命令

条件(condition) 对象 顺序 在指定的对象范围内将指定的顺序设置为排列条件。

实行(execute) 对象 在指定的对象范围内以设置的条件进行排列。

撤除(clear) 对象 撤除指定对象范围内设置的排列条件，该状态下由于排列条件已经撤除，通过命令进行排列时不会有任何效果。

### 对象指定

- 物品(Inventory)
- 雇员物品(Retainer)
- 兵装库(ArmouryChest)
- 陆行鸟鞍囊（saddlebag）
- 陆行鸟鞍囊2（rightsaddlebag）
- 主手(main)
- 副手(sub)
- 头部(head)
- 身体(body)
- 手臂(hands)
- 腿部(legs)
- 脚部(feet)
- 颈部(neck)
- 耳部(ears)
- 腕部(wrists)
- 戒指(rings)
- 灵魂水晶(soul)

### 条件指定

- 道具编号(id)
- 精炼度(spiritbond)
- 道具种类(category)
- 装备等级(level)
- 物品品级(itemlevel)
- 打包数量(stack)
- 优质道具(hq)
- 魔晶石数(materia)
- 物理基本性能(physicaldamage)
- 魔法基本性能(magicdamage)
- 攻击间隔(delay)
- 物理自动攻击(physicalautoattack)
- 格挡发动力(blockrate)
- 格挡性能(blockstrength)
- 物理防御力(defense)
- 魔法防御力(magicdefense)
- 力量(str)
- 灵巧(dex)
- 耐力(vit)
- 智力(int)
- 精神(mnd)
- 作业精度(craftsmanship)
- 加工精度(control)
- 获得力(gathering)
- 鉴别力(perception)
- 分栏(tab)

※分栏(tab)会按照每个收纳分栏进行指定分配。

### 顺序指定

- 升序(asc)
- 降序(des)

※顺序省略时会按照升序进行处理。

※条件指定设为“分栏(tab)”时不需要设置。

### 示例

`/道具整理 条件 主手 精炼度 升序`

`/道具整理 条件 主手 物品品级 降序`

`/道具整理 实行 主手`

将兵装库内主手栏下的排列条件设为精炼度升序及道具品级降序，并开始进行整理。 
