using DailyRoutines.Abstracts;
using DailyRoutines.Infos;

namespace DailyRoutines.AutoSortItem;

public class AutoSortItemConfig : ModuleConfiguration
{
    public int ArmouryChestId { get; set; }

    public int ArmouryItemLevel { get; set; }

    public int ArmouryLevel { get; set; }

    public int ArmouryCategory { get; set; }

    public int InventoryHq { get; set; }

    public int InventoryId { get; set; }

    public int InventoryItemLevel { get; set; }

    public int InventoryCategory { get; set; }

    public int InventoryTab { get; set; }

    public bool SendSortMessage { get; set; }

    public AutoSortItemConfig() { }
}
