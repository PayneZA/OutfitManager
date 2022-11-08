using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace OutfitManager
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public Character MyCharacter { get; set; }
        public Dictionary<string,Character> Characters { get;set; } = new Dictionary<string, Character>();
        public Dictionary<string,Character> SafeSenders { get; set; } = new Dictionary<string, Character>();
        public bool ChatControl { get; set; }

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
