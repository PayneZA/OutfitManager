using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using OutfitManager.Models;

namespace OutfitManager
{
    public class OutfitStorage
    {
        public Dictionary<string, OmgOutfit> Outfits { get; set; }

        private readonly string _outfitFilePath;

        public OutfitStorage(string outfitFilePath)
        {
            _outfitFilePath = outfitFilePath;
            Load();
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(Outfits, Formatting.Indented);
            File.WriteAllText(_outfitFilePath, json);
        }

        private void Load()
        {
            if (File.Exists(_outfitFilePath))
            {
                var json = File.ReadAllText(_outfitFilePath);
                Outfits = JsonConvert.DeserializeObject<Dictionary<string, OmgOutfit>>(json);
            }
            else
            {
                Outfits = new Dictionary<string, OmgOutfit>();
            }
        }
    }
}
