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
        public Dictionary<string, Outfit> Outfits { get; set; } = new Dictionary<string, Outfit>();
        public bool isUserCharacter { get; set; }
    }
}
