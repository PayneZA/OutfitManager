using Dalamud.Logging;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using System;
using Dalamud.Game.ClientState.Objects.Types;
using OutfitManager.Services;

namespace OutfitManager.Ipc
{
    //public class CustomizeIPC
    //{
    //    private static CustomizeIPC? _instance;
    //    public static CustomizeIPC Instance => _instance ??= new CustomizeIPC();

    //    private readonly ICallGateSubscriber<string, string?> GetBodyScale;
    //    private readonly ICallGateSubscriber<Character?, string?> GetBodyScaleFromCharacter;
    //    private readonly ICallGateSubscriber<string, string, object> SetBodyScale;
    //    private readonly ICallGateSubscriber<string, Character?, object> SetBodyScaleToCharacter;
    //    private readonly ICallGateSubscriber<string, object> Revert;
    //    private readonly ICallGateSubscriber<Character?, object> RevertCharacter;
    //    private readonly ICallGateSubscriber<string> ApiVersion;

    //    private CustomizeIPC()
    //    {
    //        GetBodyScale = DalamudService.PluginInterface.GetIpcSubscriber<string, string?>("CustomizePlus.GetBodyScale");
    //        GetBodyScaleFromCharacter = DalamudService.PluginInterface.GetIpcSubscriber<Character?, string?>("CustomizePlus.GetBodyScaleFromCharacter");
    //        SetBodyScale = DalamudService.PluginInterface.GetIpcSubscriber<string, string, object>("CustomizePlus.SetBodyScale");
    //        SetBodyScaleToCharacter = DalamudService.PluginInterface.GetIpcSubscriber<string, Character?, object>("CustomizePlus.SetBodyScaleToCharacter");
    //        Revert = DalamudService.PluginInterface.GetIpcSubscriber<string, object>("CustomizePlus.Revert");
    //        RevertCharacter = DalamudService.PluginInterface.GetIpcSubscriber<Character?, object>("CustomizePlus.RevertCharacter");
    //        ApiVersion = DalamudService.PluginInterface.GetIpcSubscriber<string>("CustomizePlus.GetApiVersion");
    //    }

    //    public string? GetBodyScaleIpc(string characterName) => GetBodyScale.InvokeFunc(characterName);

    //    public string? GetBodyScaleFromCharacterIpc(Character? character) => GetBodyScaleFromCharacter.InvokeFunc(character);

    //    public void SetBodyScaleIpc(string bodyScale, string characterName) => SetBodyScale.InvokeAction(bodyScale, characterName);

    //    public void SetBodyScaleToCharacterIpc(string bodyScale, Character? character) => SetBodyScaleToCharacter.InvokeAction(bodyScale, character);

    //    public void RevertIpc(string characterName) => Revert.InvokeAction(characterName);

    //    public void RevertCharacterIpc(Character? character) => RevertCharacter.InvokeAction(character);

    //    public string ApiVersionIpc() => ApiVersion.InvokeFunc();

    //}
}
