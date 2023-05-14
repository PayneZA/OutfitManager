using Dalamud.Data;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.IoC;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.DutyState;
using Dalamud.Game.Gui.Toast;
using Dalamud.Interface.Windowing;

namespace OutfitManager.Services
{
    public sealed class DalamudService
    {
        [PluginService] public static DalamudPluginInterface PluginInterface { get; set; } = null!;
        [PluginService] public static ChatGui Chat { get; set; } = null!;
        [PluginService] public static ClientState ClientState { get; set; } = null!;
        [PluginService] public static Framework Framework { get; set; } = null!;
        [PluginService] public static GameGui GameGui { get; set; } = null!;
        [PluginService] public static TargetManager TargetManager { get; set; } = null!;
        [PluginService] public static DutyState DutyState { get; set; } = null!;
        [PluginService] public static ToastGui Toast { get; set; } = null!;
        [PluginService] public static ObjectTable ObjectTable { get; set; } = null!;
        [PluginService] public static CommandManager Commands { get; private set; } = null!;
        [PluginService] public static DataManager GameData { get; private set; } = null!;
        [PluginService] public static Condition Conditions { get; private set; } = null!;
        [PluginService] public static TargetManager Targets { get; private set; } = null!;
        [PluginService] public static ObjectTable Objects { get; private set; } = null!;
        [PluginService] public static TitleScreenMenu TitleScreenMenu { get; private set; } = null!;
        [PluginService] public static KeyState KeyState { get; private set; } = null!;

    }
}
