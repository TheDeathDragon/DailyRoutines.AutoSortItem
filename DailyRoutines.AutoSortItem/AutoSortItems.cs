global using static DailyRoutines.Helpers.NotifyHelper;
global using static OmenTools.Helpers.HelpersOm;
global using static DailyRoutines.Infos.Widgets;
global using static OmenTools.Helpers.HelpersOm;
global using static OmenTools.Infos.InfosOm;
global using static DailyRoutines.Helpers.NotifyHelper;
global using OmenTools.ImGuiOm;
global using OmenTools.Helpers;
global using OmenTools;
global using ImGuiNET;
using System.Collections.Generic;
using DailyRoutines.Abstracts;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;


namespace DailyRoutines.AutoSortItem;

public class AutoSortItems : DailyModuleBase
{
    private readonly string[] _sortOptions = ["降序", "升序"];
    private readonly string[] _tabOptions = ["分页", "不分页"];
    private readonly string[] _sortOptionsCommand = ["des", "asc"];

    private static Config _config = null!;

    public override ModuleInfo Info => new()
    {
        Author = ["那年雪落"],
        Title = "自动整理物品",
        Description = "切换地图自动整理物品，包括兵装库",
        ReportUrl = "https://github.com/TheDeathDragon/DailyRoutines.AutoSortItem.git",
        Category = ModuleCategories.General,
    };

    public override void Init()
    {
        _config = LoadConfig<Config>() ?? new();
        DService.ClientState.TerritoryChanged += OnZoneChanged;
        TaskHelper ??= new TaskHelper { TimeLimitMS = 30_000 };
    }

    private void OnZoneChanged(ushort zone)
    {
        var currentMapData = LuminaCache.GetRow<Map>(DService.ClientState.MapId);
        if (currentMapData == null || zone <= 0) return;
        var isPveMap = currentMapData.TerritoryType.Row > 0 &&
                       currentMapData.TerritoryType.Value.ContentFinderCondition.Row > 0;
        if (isPveMap) return;
        TaskHelper.Abort();
        TaskHelper.Enqueue(CheckCanSort);
    }

    private bool? CheckCanSort()
    {
        if (BetweenAreas || !IsScreenReady() || OccupiedInEvent) return false;
        if (!DService.Condition[ConditionFlag.NormalConditions] || !IsValidPveDuty())
        {
            TaskHelper.Abort();
            return true;
        }

        TaskHelper.Enqueue(SendSortCommand, "SendSortCommand", 5_000, true, 1);
        return true;
    }

    public override void Uninit()
    {
        DService.ClientState.TerritoryChanged -= OnZoneChanged;
        base.Uninit();
    }

    public override void ConfigUI()
    {
        ImGuiOm.Text("物品排序方式");
        ImGui.Spacing();
        if (ImGui.BeginTable("SortingTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("名称", ImGuiTableColumnFlags.WidthFixed, 100f * GlobalFontScale);
            ImGui.TableSetupColumn("选项", ImGuiTableColumnFlags.WidthFixed, 150f * GlobalFontScale);
            ImGui.TableSetupColumn("说明", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();

            DrawTableRow("兵装 ID", ref _config.ArmouryChestId, _sortOptions, "");
            DrawTableRow("兵装等级", ref _config.ArmouryItemLevel, _sortOptions, "");
            DrawTableRow("兵装类型", ref _config.ArmouryCategory, _sortOptions, "降序的话，生产在前，战职在后");
            DrawTableRow("物品 HQ", ref _config.InventoryHq, _sortOptions, "");
            DrawTableRow("物品 ID", ref _config.InventoryId, _sortOptions, "");
            DrawTableRow("物品等级", ref _config.InventoryItemLevel, _sortOptions, "");
            DrawTableRow("物品类型", ref _config.InventoryCategory, _sortOptions, "降序的话，生产素材会在石头之前，食物和药会在烟花幻卡之前");
            DrawTableRow("物品分页", ref _config.InventoryTab, _tabOptions, "分页的话，装备会在第一页，其他物品会在后面几页");

            ImGui.EndTable();
        }

        ImGui.Spacing();
        if (ImGuiOm.CheckboxColored("是否发送物品整理通知", ref _config.SendSortMessage))
        {
            SaveConfig(_config);
        }

        ImGui.Spacing();
        if (ImGui.Button("重置设置"))
        {
            ResetConfigToDefault();
        }
    }

    private static unsafe bool IsValidPveDuty()
    {
        HashSet<uint> invalidContentTypes = [16, 17, 18, 19, 31, 32, 34, 35];

        var isPvp = GameMain.IsInPvPArea() || GameMain.IsInPvPInstance();
        var contentData =
            LuminaCache.GetRow<ContentFinderCondition>(GameMain.Instance()->CurrentContentFinderConditionId);

        return !isPvp && (contentData == null || !invalidContentTypes.Contains(contentData.ContentType.Row));
    }

    private void DrawTableRow(string label, ref int value, string[] options, string notes)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.Text(label);

        ImGui.TableSetColumnIndex(1);
        ImGui.SetNextItemWidth(-1);

        var oldValue = value;
        if (ImGui.Combo("##" + label, ref value, options, options.Length) && value != oldValue)
        {
            SaveConfig(_config);
        }

        ImGui.TableSetColumnIndex(2);
        ImGui.SetNextItemWidth(200 * GlobalFontScale);
        ImGui.Text(notes);
    }

    private void SendSortCommand()
    {
        SendSortCondition("armourychest", "id", _config.ArmouryChestId);
        SendSortCondition("armourychest", "itemlevel", _config.ArmouryItemLevel);
        SendSortCondition("armourychest", "category", _config.ArmouryCategory);
        ChatHelper.Instance.SendMessage("/itemsort execute armourychest");

        SendSortCondition("inventory", "hq", _config.InventoryHq);
        SendSortCondition("inventory", "id", _config.InventoryId);
        SendSortCondition("inventory", "itemlevel", _config.InventoryItemLevel);
        SendSortCondition("inventory", "category", _config.InventoryCategory);

        if (_config.InventoryTab == 0)
        {
            ChatHelper.Instance.SendMessage("/itemsort condition inventory tab");
        }

        ChatHelper.Instance.SendMessage("/itemsort execute inventory");

        if (_config.SendSortMessage)
        {
            Chat(new SeStringBuilder().AddUiForeground("[AutoSortItem] 自动整理物品完成", 506).Build());
        }

        return;

        void SendSortCondition(string target, string condition, int setting)
        {
            ChatHelper.Instance.SendMessage($"/itemsort condition {target} {condition} {_sortOptionsCommand[setting]}");
        }
    }

    private void ResetConfigToDefault()
    {
        _config = new Config();
        SaveConfig(_config);
    }

    public class Config : ModuleConfiguration
    {
        public int ArmouryChestId;
        public int ArmouryItemLevel;
        public int ArmouryCategory;
        public int InventoryHq;
        public int InventoryId;
        public int InventoryItemLevel;
        public int InventoryCategory;
        public int InventoryTab;
        public bool SendSortMessage = true;
    }
}
