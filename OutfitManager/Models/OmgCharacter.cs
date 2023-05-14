using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutfitManager.Models
{
    public class OmgCharacter
    {
        public string Name { get; set; }
        public string World { get; set; }

        public string FullName { get; set; }

        public Dictionary<string, OmgOutfit> Outfits { get; set; }
        public bool isUserCharacter { get; set; }

        public bool canOutfitLock { get; set; }
        public OmgCharacter()
        {
            Name = "";
            World = "";
            FullName = "";
            Outfits = new Dictionary<string, OmgOutfit>();
            isUserCharacter = false;
            canOutfitLock = false;
        }
    }
}
