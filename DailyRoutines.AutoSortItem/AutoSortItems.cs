using DailyRoutines.Abstracts;
using DailyRoutines.Infos;
using ImGuiNET;
using OmenTools;
using OmenTools.Helpers;
using OmenTools.ImGuiOm;
using OmenTools.Infos;

namespace DailyRoutines.AutoSortItem;

public class AutoSortItems : DailyModuleBase
{
    private int _armouryChestId;
    private int _armouryItemLevel;
    private int _armouryCategory;
    private int _inventoryHq;
    private int _inventoryId;
    private int _inventoryItemLevel;
    private int _inventoryCategory;
    private int _inventoryTab;
    private bool _sendSortMessage;

    private readonly string[] _sortOptions = ["降序", "升序"];
    private readonly string[] _tabOptions = ["分页", "不分页"];
    private readonly string[] _sortOptionsCommand = ["des", "asc"];

    private AutoSortItemConfig _config;

    public override ModuleInfo Info => new()
    {
        Author = ["那年雪落"],
        Title = "自动整理物品",
        Description = "切换地图自动整理物品，包括兵装库",
        ReportUrl = "https://github.com/TheDeathDragon/DailyRoutines.AutoSortItem.git",
        Category = ModuleCategories.System,
    };


    public override void Init()
    {
        TaskHelper ??= new TaskHelper { TimeLimitMS = 30_000 };
        _config = LoadConfig<AutoSortItemConfig>();
        _armouryChestId = _config.ArmouryChestId;
        _armouryItemLevel = _config.ArmouryItemLevel;
        _armouryCategory = _config.ArmouryCategory;
        _inventoryHq = _config.InventoryHq;
        _inventoryId = _config.InventoryId;
        _inventoryItemLevel = _config.InventoryItemLevel;
        _inventoryCategory = _config.InventoryCategory;
        _inventoryTab = _config.InventoryTab;
        _sendSortMessage = _config.SendSortMessage;
        DService.ClientState.TerritoryChanged += OnZoneChanged;
    }

    public override void ConfigUI()
    {
        ImGuiOm.Text("物品排序方式");
        ImGui.Spacing();
        if (ImGui.BeginTable("SortingTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("名称", ImGuiTableColumnFlags.WidthFixed, 100f);
            ImGui.TableSetupColumn("选项", ImGuiTableColumnFlags.WidthFixed, 150f);
            ImGui.TableSetupColumn("说明", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();

            DrawTableRow("兵装 ID", ref _armouryChestId, _sortOptions, "");
            DrawTableRow("兵装等级", ref _armouryItemLevel, _sortOptions, "");
            DrawTableRow("兵装类型", ref _armouryCategory, _sortOptions, "降序的话，生产在前，战职在后");
            DrawTableRow("物品 HQ", ref _inventoryHq, _sortOptions, "");
            DrawTableRow("物品 ID", ref _inventoryId, _sortOptions, "");
            DrawTableRow("物品等级", ref _inventoryItemLevel, _sortOptions, "");
            DrawTableRow("物品类型", ref _inventoryCategory, _sortOptions, "降序的话，生产素材会在石头之前，食物和药会在烟花幻卡之前");
            DrawTableRow("物品分页", ref _inventoryTab, _tabOptions, "分页的话，装备会在第一页，其他物品会在后面几页");
            ImGui.EndTable();
        }

        ImGui.Spacing();
        if (ImGuiOm.CheckboxColored("是否发送物品整理通知", ref _sendSortMessage))
        {
            _config.SendSortMessage = _sendSortMessage;
            SaveConfig(_config);
        }

        ImGui.Spacing();
        if (ImGui.Button("重置设置"))
        {
            _config.ArmouryCategory = 0;
            _config.ArmouryChestId = 0;
            _config.ArmouryItemLevel = 0;
            _config.InventoryCategory = 0;
            _config.InventoryHq = 0;
            _config.InventoryId = 0;
            _config.InventoryItemLevel = 0;
            _config.InventoryTab = 0;
            _config.SendSortMessage = true;
            _armouryCategory = 0;
            _armouryChestId = 0;
            _armouryItemLevel = 0;
            _inventoryCategory = 0;
            _inventoryHq = 0;
            _inventoryId = 0;
            _inventoryItemLevel = 0;
            _inventoryTab = 0;
            _sendSortMessage = true;
            SaveConfig(_config);
        }
    }

    private void DrawTableRow(string label, ref int value, string[] options, string notes)
    {
        ImGui.TableNextRow();

        ImGui.TableSetColumnIndex(0);
        ImGui.Text(label);

        ImGui.TableSetColumnIndex(1);
        ImGui.SetNextItemWidth(-1);
        if (ImGui.Combo("##" + label, ref value, options, options.Length))
        {
            SaveConfig(_config);
        }

        ImGui.TableSetColumnIndex(2);
        ImGui.SetNextItemWidth(200);
        ImGui.Text(notes);
    }

    private void SendSortCommand()
    {
        ChatHelper.Instance.SendMessage($"/itemsort condition armourychest id {_sortOptionsCommand[_armouryChestId]}");
        ChatHelper.Instance.SendMessage(
            $"/itemsort condition armourychest itemlevel {_sortOptionsCommand[_armouryItemLevel]}");
        ChatHelper.Instance.SendMessage(
            $"/itemsort condition armourychest category {_sortOptionsCommand[_armouryCategory]}");
        ChatHelper.Instance.SendMessage("/itemsort execute armourychest");
        ChatHelper.Instance.SendMessage($"/itemsort condition inventory hq {_sortOptionsCommand[_inventoryHq]}");
        ChatHelper.Instance.SendMessage($"/itemsort condition inventory id {_sortOptionsCommand[_inventoryId]}");
        ChatHelper.Instance.SendMessage(
            $"/itemsort condition inventory itemlevel {_sortOptionsCommand[_inventoryItemLevel]}");
        ChatHelper.Instance.SendMessage(
            $"/itemsort condition inventory category {_sortOptionsCommand[_inventoryCategory]}");
        switch (_inventoryTab)
        {
            case 0:
                ChatHelper.Instance.SendMessage("/itemsort condition inventory tab");
                break;
            case 1:
                ChatHelper.Instance.SendMessage("/itemsort execute inventory");
                break;
        }

        ChatHelper.Instance.SendMessage("/itemsort execute inventory");

        if (_sendSortMessage)
        {
            ChatHelper.Instance.SendMessage("/e [AutoSortItem] 自动整理物品完成");
        }
    }

    private void OnZoneChanged(ushort zone)
    {
        if (zone <= 0) return;
        TaskHelper.Abort();
        TaskHelper.Enqueue(SortItems);
    }

    private bool? SortItems()
    {
        if (InfosOm.BetweenAreas || !HelpersOm.IsScreenReady() || InfosOm.OccupiedInEvent) return false;
        if (DService.ClientState.LocalPlayer is null) return false;
        TaskHelper.Enqueue(SendSortCommand, "SendSortCommand", 5_000, true, 1);
        return true;
    }

    public override void Uninit()
    {
        _config.ArmouryChestId = _armouryChestId;
        _config.ArmouryItemLevel = _armouryItemLevel;
        _config.ArmouryCategory = _armouryCategory;
        _config.InventoryHq = _inventoryHq;
        _config.InventoryId = _inventoryId;
        _config.InventoryItemLevel = _inventoryItemLevel;
        _config.InventoryCategory = _inventoryCategory;
        _config.InventoryTab = _inventoryTab;
        SaveConfig(_config);
        DService.ClientState.TerritoryChanged -= OnZoneChanged;
        base.Uninit();
    }
}
