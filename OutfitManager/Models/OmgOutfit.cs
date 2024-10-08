using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutfitManager.Models
{
    public class OmgOutfit
    {
        public string Name { get; set; }

        public string CollectionName { get; set; }

        public string DesignPath { get; set; }

        public string DisplayName { get; set; }


        public string CharacterName { get; set; }

        public string Notes { get; set; }

        public List<string> Tags { get; set; }
        public bool IsFavourite { get; set; }

    }
}
