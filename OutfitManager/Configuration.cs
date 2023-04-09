using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.IO;

namespace OutfitManager
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public Character MyCharacter { get; set; } = new Character();
        public Dictionary<string,Character> Characters { get;set; } = new Dictionary<string, Character>();
        public Dictionary<string,Character> SafeSenders { get; set; } = new Dictionary<string, Character>();
        public bool ChatControl { get; set; }

        public string OutfitName { get; set; }
        public bool Persist { get; set; }

        public bool PersistGearset { get; set; }
        public bool ShowPreview { get; set; }
        public string PreviewDirectory { get; set; } = "";
        public string PrimaryCollection { get; set; } = "";

        public bool IgnorePersistCollection { get; set; }
        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
        }

        public void Save()
        {
           
            if (!string.IsNullOrEmpty(this.PreviewDirectory))
            {
                if (!this.PreviewDirectory.EndsWith("\\"))
                {
                    this.PreviewDirectory = this.PreviewDirectory + "\\";
                }

                if (!Directory.Exists(this.PreviewDirectory))
                {
                    this.PreviewDirectory = "";
                }
            }
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
