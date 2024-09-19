using Dalamud.Data;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.DutyState;
using Dalamud.Game.Gui.Toast;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;

namespace OutfitManager.Services
{
    public sealed class DalamudService
    {
        [PluginService] public static IDalamudPluginInterface PluginInterface { get; set; } = null!;
        [PluginService] public static IChatGui Chat { get; set; } = null!;
        [PluginService] public static IClientState ClientState { get; set; } = null!;
        [PluginService] public static IFramework Framework { get; set; } = null!;
        [PluginService] public static IGameGui GameGui { get; set; } = null!;
        [PluginService] public static ITargetManager TargetManager { get; set; } = null!;
        [PluginService] public static IDutyState DutyState { get; set; } = null!;
        [PluginService] public static IToastGui Toast { get; set; } = null!;
        [PluginService] public static IObjectTable ObjectTable { get; set; } = null!;
        [PluginService] public static ICommandManager Commands { get; private set; } = null!;
        [PluginService] public static IDataManager GameData { get; private set; } = null!;
        [PluginService] public static ICondition Conditions { get; private set; } = null!;
        [PluginService] public static ITitleScreenMenu TitleScreenMenu { get; private set; } = null!;
        [PluginService] public static IKeyState KeyState { get; private set; } = null!;

        [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
    }
}
