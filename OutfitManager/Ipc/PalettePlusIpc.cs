using System;
using Dalamud.Plugin.Ipc;
using Dalamud.Game.ClientState.Objects.Types;
using OutfitManager.Services;

namespace OutfitManager.Ipc
{
    public class PalettePlusIpc
    {
        private static PalettePlusIpc? _instance;
        public static PalettePlusIpc Instance => _instance ??= new PalettePlusIpc();

        private readonly ICallGateSubscriber<string> ApiVersion;
        private readonly ICallGateSubscriber<Character, string> GetCharaPalette;
        private readonly ICallGateSubscriber<Character, string, object> SetCharaPalette;
        private readonly ICallGateSubscriber<Character, object> RemoveCharaPalette;
        private readonly ICallGateSubscriber<Character, string> BuildCharaPalette;
        private readonly ICallGateSubscriber<Character, string> BuildCharaPaletteOrEmpty;
        private readonly ICallGateSubscriber<Character, string, object> PaletteChanged;

        private PalettePlusIpc()
        {
            ApiVersion = DalamudService.PluginInterface.GetIpcSubscriber<string>("PalettePlus.ApiVersion");
            GetCharaPalette = DalamudService.PluginInterface.GetIpcSubscriber<Character, string>("PalettePlus.GetCharaPalette");
            SetCharaPalette = DalamudService.PluginInterface.GetIpcSubscriber<Character, string, object>("PalettePlus.SetCharaPalette");
            RemoveCharaPalette = DalamudService.PluginInterface.GetIpcSubscriber<Character, object>("PalettePlus.RemoveCharaPalette");
            BuildCharaPalette = DalamudService.PluginInterface.GetIpcSubscriber<Character, string>("PalettePlus.BuildCharaPalette");
            BuildCharaPaletteOrEmpty = DalamudService.PluginInterface.GetIpcSubscriber<Character, string>("PalettePlus.BuildCharaPaletteOrEmpty");
            PaletteChanged = DalamudService.PluginInterface.GetIpcSubscriber<Character, string, object>("PalettePlus.PaletteChanged");
        }

        public string ApiVersionIpc() => ApiVersion.InvokeFunc();

        public string GetCharaPaletteIpc(Character chara) => GetCharaPalette.InvokeFunc(chara);

        public void SetCharaPaletteIpc(Character chara, string json) => SetCharaPalette.InvokeAction(chara, json);

        public void RemoveCharaPaletteIpc(Character chara) => RemoveCharaPalette.InvokeAction(chara);

        public string BuildCharaPaletteIpc(Character chara) => BuildCharaPalette.InvokeFunc(chara);

        public string BuildCharaPaletteOrEmptyIpc(Character chara) => BuildCharaPaletteOrEmpty.InvokeFunc(chara);

        public void PaletteChangedIpc(Character character, string paletteJson) => PaletteChanged.InvokeAction(character, paletteJson);
    }
}
