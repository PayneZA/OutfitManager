using System;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Ipc;

namespace OutfitManager.Ipc
{
    public class HeelsPluginIpc
    {
        private static HeelsPluginIpc? _instance;
        public static HeelsPluginIpc Instance => _instance ??= new HeelsPluginIpc();

        private readonly ICallGateSubscriber<string> ApiVersion;
        private readonly ICallGateSubscriber<float> GetOffset;
        private readonly ICallGateSubscriber<float, object?> OffsetChanged;
        private readonly ICallGateSubscriber<GameObject, float, object?> RegisterPlayer;
        private readonly ICallGateSubscriber<GameObject, object?> UnregisterPlayer;

        private HeelsPluginIpc()
        {
            ApiVersion = DalamudService.PluginInterface.GetIpcSubscriber<string>("HeelsPlugin.ApiVersion");
            GetOffset = DalamudService.PluginInterface.GetIpcSubscriber<float>("HeelsPlugin.GetOffset");
            OffsetChanged = DalamudService.PluginInterface.GetIpcSubscriber<float, object?>("HeelsPlugin.OffsetChanged");
            RegisterPlayer = DalamudService.PluginInterface.GetIpcSubscriber<GameObject, float, object?>("HeelsPlugin.RegisterPlayer");
            UnregisterPlayer = DalamudService.PluginInterface.GetIpcSubscriber<GameObject, object?>("HeelsPlugin.UnregisterPlayer");
        }

        public string ApiVersionIpc() => ApiVersion.InvokeFunc();

        public float GetOffsetIpc() => GetOffset.InvokeFunc();

        public void OffsetChangedIpc(float offset) => OffsetChanged.InvokeAction(offset);

        public void RegisterPlayerIpc(GameObject gameObject, float offset) => RegisterPlayer.InvokeAction(gameObject, offset);

        public void UnregisterPlayerIpc(GameObject gameObject) => UnregisterPlayer.InvokeAction(gameObject);
    }
}
