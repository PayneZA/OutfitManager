using Dalamud.Interface.Windowing;
using ImGuiNET;
using Microsoft.Win32.SafeHandles;
using OutfitManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OutfitManager.Windows
{
    public class OutfitListWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private string _newOutfitName = string.Empty;
        private string _penumbraCollection = string.Empty;
        private string _glamourerDesign = string.Empty;
        private string _gearset = string.Empty;
        private Outfit _outfit;
        private int _currentItem = 0;
        private List<Outfit> _outfitList;
        private string _characterName = string.Empty;
        private string _notes = string.Empty;


        string[] _outfits;

     
        public void Dispose()
        {
          
        }
        public OutfitListWindow(Plugin plugin) : base(
           "OutfitManager Outfit List Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(675, 630),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };
            



            this.Plugin = plugin;

            
            Init();
       
        }
        public override void Draw()
        {
            //   List<String> outfitList = this.Plugin.CustomConfig.Outfits.Keys.ToList();

            if (this.Plugin.Configuration.Characters.ContainsKey(this.Plugin.Configuration.CharacterName))
            {

                OutfitAddition();

                OutfitList();
            }

        }

        public void Init()
        {
            if (!string.IsNullOrEmpty(this.Plugin.Configuration.CharacterName) && this.Plugin.Configuration.Characters.ContainsKey(this.Plugin.Configuration.CharacterName))
            {
                _outfitList = this.Plugin.Configuration.Characters[this.Plugin.Configuration.CharacterName].Outfits.Values.OrderBy(x => x.DisplayName).ToList();
                _characterName = this.Plugin.Configuration.CharacterName;
                _outfits = _outfitList.Select(f => f.DisplayName).ToArray();
            }
        }

        public void Notes()
        {

        }
        public void OutfitList()
        {
        

            if (ImGui.ListBox("Outfits", ref _currentItem, _outfits, _outfits.Count(), 10))
            {
                _outfit = this.Plugin.Configuration.Characters[this.Plugin.Configuration.CharacterName].Outfits[_outfits[_currentItem].ToLower()];

                _newOutfitName = _outfit.Name;
                _penumbraCollection = _outfit.CollectionName;
                if (_outfit.GearSet == null)
                {
                    _outfit.GearSet = "";
                }
                _gearset = _outfit.GearSet;
                _glamourerDesign = _outfit.DesignPath;
                _notes = _outfit.Notes;
            }
        }

       

        public void OutfitAddition()
        {
            ImGui.InputTextWithHint("Outfit Name", "Enter outfit name...", ref _newOutfitName, 64, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.InputTextWithHint("Penumbra Collection", "Enter penumbra collection name (e.g. character name)...", ref _penumbraCollection, 64, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.InputTextWithHint("Glamourer Design", "Enter Glamourer Design Path (E.g. /collections/outfit1)...", ref _glamourerDesign, 64, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.InputTextWithHint("Gearset", "Enter gearset number (optional)...", ref _gearset, 64, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.InputTextMultiline("Notes", ref _notes,512, new Vector2(440, 60));

            if (ImGui.Button("Add / Update Outfit") && (!string.IsNullOrEmpty(_newOutfitName)))
            {
                _outfit = new Outfit
                {
                    CollectionName = _penumbraCollection,
                    DesignPath = _glamourerDesign,
                    DisplayName = _newOutfitName,
                    GearSet = _gearset,
                    Name = _newOutfitName.ToLower(),
                    Notes = _notes
                };

                if (!this.Plugin.Configuration.Characters[this.Plugin.Configuration.CharacterName].Outfits.ContainsKey(_outfit.Name))
                {
                    this.Plugin.Configuration.Characters[this.Plugin.Configuration.CharacterName].Outfits.Add(_outfit.Name, _outfit);
                }
                else
                {
                    this.Plugin.Configuration.Characters[this.Plugin.Configuration.CharacterName].Outfits[_outfit.Name] = _outfit;
                }

                _outfitList = this.Plugin.Configuration.Characters[this.Plugin.Configuration.CharacterName].Outfits.Values.OrderBy(x => x.DisplayName).ToList();

                _outfits = _outfitList.Select(f => f.DisplayName).ToArray();

                if (!string.IsNullOrEmpty(_characterName))
                {
                    this.Plugin.Configuration.CharacterName = _characterName;
                }
                this.Plugin.Configuration.Save();
                Init();
            }

            if (ImGui.Button("Delete Outfit"))
            {
                if (this.Plugin.Configuration.Characters[this.Plugin.Configuration.CharacterName].Outfits.ContainsKey(_outfit.Name))
                {
                    this.Plugin.Configuration.Characters[this.Plugin.Configuration.CharacterName].Outfits.Remove(_outfit.Name);

                    this.Plugin.Configuration.Save();
                    _newOutfitName = "";
                    _penumbraCollection = "";
                    _glamourerDesign = "";
                    _gearset = "";
                    _notes = "";
                    Init();

                }
            }

            if (ImGui.Button("Wear Outfit"))
            {
                _newOutfitName = "";
                _penumbraCollection = "";
                _glamourerDesign = "";
                _gearset = "";
                _notes = "";
                this.Plugin.EquipOutfit(_outfit.Name);
            }
        }
    }
}
