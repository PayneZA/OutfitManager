using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutfitManager
{
    //public sealed class OutfitManagerConfig : IPluginConfiguration
    //{
    //    public Dictionary<string, Outfit> Outfits { get; set; } = new();
    //    public int Version { get; set; } = 1;
    //    public void Save()
    //        => Dalamud.PluginInterface.SavePluginConfig(this);

    //    public OutfitManagerConfig Load()
    //    {

    //        var configg = Dalamud.PluginInterface.GetPluginConfig();
    //        if (Dalamud.PluginInterface.GetPluginConfig() is OutfitManagerConfig config)
    //            return config;

    //        config = new OutfitManagerConfig();
    //        config.Save();
    //        return config;
    //    }
    //}

    public class OutfitManagerConfig : IPluginConfiguration
    {
        public Dictionary<string, Character> Characters { get; set; } = new();

   
        public int Version { get; set; } = 1;
        public void Save()
        {
        }

        public static OutfitManagerConfig Load()
        {

            if (Dalamud.PluginInterface.GetPluginConfig() is OutfitManagerConfig config)
                return config;

            config = new OutfitManagerConfig();
            config.Save();
            return config;
        }
    }
}
