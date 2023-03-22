using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutfitManager
{
    public class Character
    {
        public string Name { get; set; }
        public string World { get; set; }

        public string FullName { get; set; }

        public Dictionary<string, Outfit> Outfits { get; set; }
        public bool isUserCharacter { get; set; }

        public bool canOutfitLock { get; set; }
        public Character()
        {
            Name = "";
            World = "";
            FullName = "";
            Outfits = new Dictionary<string, Outfit>();
            isUserCharacter = false;
            canOutfitLock = false;
        }
    }
}
