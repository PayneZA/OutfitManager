using Dalamud.Logging;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using System;
using Dalamud.Game.ClientState.Objects.Types;
using OutfitManager.Services;

namespace OutfitManager.Ipc
{
    public class GlamourerIpc
    {
        private static GlamourerIpc? _instance;
        public static GlamourerIpc Instance => _instance ??= new GlamourerIpc();

        private readonly ICallGateSubscriber<string, string?> GetAllCustomization;
        private readonly ICallGateSubscriber<Character?, string?> GetAllCustomizationFromCharacter;
        private readonly ICallGateSubscriber<string, string, object> ApplyAll;
        private readonly ICallGateSubscriber<string, Character?, object> ApplyAllToCharacter;
        private readonly ICallGateSubscriber<string, string, object> ApplyOnlyEquipment;
        private readonly ICallGateSubscriber<string, Character?, object> ApplyOnlyEquipmentToCharacter;
        private readonly ICallGateSubscriber<string, string, object> ApplyOnlyCustomization;
        private readonly ICallGateSubscriber<string, Character?, object> ApplyOnlyCustomizationToCharacter;
        private readonly ICallGateSubscriber<string, object> Revert;
        private readonly ICallGateSubscriber<Character?, object> RevertCharacter;
        private readonly ICallGateSubscriber<int> ApiVersion;

        private GlamourerIpc()
        {
            GetAllCustomization = DalamudService.PluginInterface.GetIpcSubscriber<string, string?>("Glamourer.GetAllCustomization");
            GetAllCustomizationFromCharacter = DalamudService.PluginInterface.GetIpcSubscriber<Character?, string?>("Glamourer.GetAllCustomizationFromCharacter");
            ApplyAll = DalamudService.PluginInterface.GetIpcSubscriber<string, string, object>("Glamourer.ApplyAll");
            ApplyAllToCharacter = DalamudService.PluginInterface.GetIpcSubscriber<string, Character?, object>("Glamourer.ApplyAllToCharacter");
            ApplyOnlyEquipment = DalamudService.PluginInterface.GetIpcSubscriber<string, string, object>("Glamourer.ApplyOnlyEquipment");
            ApplyOnlyEquipmentToCharacter = DalamudService.PluginInterface.GetIpcSubscriber<string, Character?, object>("Glamourer.ApplyOnlyEquipmentToCharacter");
            ApplyOnlyCustomization = DalamudService.PluginInterface.GetIpcSubscriber<string, string, object>("Glamourer.ApplyOnlyCustomization");
            ApplyOnlyCustomizationToCharacter = DalamudService.PluginInterface.GetIpcSubscriber<string, Character?, object>("Glamourer.ApplyOnlyCustomizationToCharacter");
            Revert = DalamudService.PluginInterface.GetIpcSubscriber<string, object>("Glamourer.Revert");
            RevertCharacter = DalamudService.PluginInterface.GetIpcSubscriber<Character?, object>("Glamourer.RevertCharacter");
            ApiVersion = DalamudService.PluginInterface.GetIpcSubscriber<int>("Glamourer.ApiVersion");
        }

        public string? GetAllCustomizationIpc(string characterName) => GetAllCustomization.InvokeFunc(characterName);

        public string? GetAllCustomizationFromCharacterIpc(Character? character) => GetAllCustomizationFromCharacter.InvokeFunc(character);

        public void ApplyAllIpc(string customization, string characterName) => ApplyAll.InvokeAction(customization, characterName);

        public void ApplyAllToCharacterIpc(string customization, Character? character) => ApplyAllToCharacter.InvokeAction(customization, character);

        public void ApplyOnlyEquipmentIpc(string customization, string characterName) => ApplyOnlyEquipment.InvokeAction(customization, characterName);

        public void ApplyOnlyEquipmentToCharacterIpc(string customization, Character? character) => ApplyOnlyEquipmentToCharacter.InvokeAction(customization, character);

        public void ApplyOnlyCustomizationIpc(string customization, string characterName) => ApplyOnlyCustomization.InvokeAction(customization, characterName);

        public void ApplyOnlyCustomizationToCharacterIpc(string customization, Character? character) => ApplyOnlyCustomizationToCharacter.InvokeAction(customization, character);

        public void RevertIpc(string characterName) => Revert.InvokeAction(characterName);

        public void RevertCharacterIpc(Character? character) => RevertCharacter.InvokeAction(character);

        public int ApiVersionIpc() => ApiVersion.InvokeFunc();

    }
}
