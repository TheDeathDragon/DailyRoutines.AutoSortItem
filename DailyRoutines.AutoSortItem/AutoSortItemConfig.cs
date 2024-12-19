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

    public AutoSortItemConfig(DailyModuleBase module)
    {
        var loadConfig = this.Load(module);
        ArmouryChestId = loadConfig.ArmouryChestId;
        ArmouryItemLevel = loadConfig.ArmouryItemLevel;
        ArmouryLevel = loadConfig.ArmouryLevel;
        ArmouryCategory = loadConfig.ArmouryCategory;
        InventoryHq = loadConfig.InventoryHq;
        InventoryId = loadConfig.InventoryId;
        InventoryItemLevel = loadConfig.InventoryItemLevel;
        InventoryCategory = loadConfig.InventoryCategory;
        InventoryTab = loadConfig.InventoryTab;
        SendSortMessage = loadConfig.SendSortMessage;
    }
}
