using Dalamud.Interface.Windowing;
using ImGuiNET;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
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
        private string _tags = string.Empty;

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

            if (this.Plugin.Configuration.MyCharacter != null && this.Plugin.Configuration.MyCharacter.FullName != "")
            {

                OutfitAddition();
                OutfitList();
                ExportToClipboard();
            }

        }

        public void Init()
        {
            if (!string.IsNullOrEmpty(this.Plugin.Configuration.MyCharacter.Name))
            {
                _outfitList = this.Plugin.Configuration.MyCharacter.Outfits.Values.OrderBy(x => x.DisplayName).ToList();
                _characterName = this.Plugin.Configuration.MyCharacter.Name;
                _outfits = _outfitList.Select(f => f.DisplayName).ToArray();
            }
        }
        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public void ExportToClipboard()
        {


            if (ImGui.Button("Export list to clipboard") && this._outfits.Count() > 0)
            {
                OutfitExport outfitExport = new OutfitExport();

                Character character = new Character { FullName = this.Plugin.Configuration.MyCharacter.FullName, isUserCharacter = false, Name = this.Plugin.Configuration.MyCharacter.Name, World = this.Plugin.Configuration.MyCharacter.World };
                character.Outfits = new Dictionary<string, Outfit>();

                outfitExport.Outfits = this._outfits;
                outfitExport.Character = character;

                string json = JsonConvert.SerializeObject(outfitExport);

                ImGui.SetClipboardText(Base64Encode(json));
            }
        }
        public void OutfitList()
        {
        

            if (ImGui.ListBox("Outfits", ref _currentItem, _outfits, _outfits.Count(), 10))
            {
                _outfit = this.Plugin.Configuration.MyCharacter.Outfits[_outfits[_currentItem].ToLower()];

                _newOutfitName = _outfit.DisplayName;
                _penumbraCollection = _outfit.CollectionName;
                if (_outfit.GearSet == null)
                {
                    _outfit.GearSet = "";
                }
                _gearset = _outfit.GearSet;
                _glamourerDesign = _outfit.DesignPath;
                _notes = _outfit.Notes;
                _tags = String.Join(",", _outfit.Tags);
            }
        }

       

        public void OutfitAddition()
        {
            ImGui.InputTextWithHint("Outfit Name", "Enter outfit name...", ref _newOutfitName, 64, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.InputTextWithHint("Penumbra Collection", "Enter penumbra collection name (e.g. character name)", ref _penumbraCollection, 64, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.InputTextWithHint("Glamourer Design", "Glamourer Design Path (E.g. /collections/outfit1) (optional)", ref _glamourerDesign, 64, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.InputTextWithHint("Gearset", "Enter gearset number (e.g. 1 10 for gearset 1 glam plate 10)(optional)", ref _gearset, 64, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.InputTextWithHint("Tags", "Enter tags for your gear comma separated (optional)", ref _tags, 64, ImGuiInputTextFlags.EnterReturnsTrue);
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
                    Notes = _notes,
                    Tags = _tags.ToLower().Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                };

                if (!this.Plugin.Configuration.MyCharacter.Outfits.ContainsKey(_outfit.Name))
                {
                    this.Plugin.Configuration.MyCharacter.Outfits.Add(_outfit.Name, _outfit);
                }
                else
                {
                    this.Plugin.Configuration.MyCharacter.Outfits[_outfit.Name] = _outfit;
                }

                _outfitList = this.Plugin.Configuration.MyCharacter.Outfits.Values.OrderBy(x => x.DisplayName).ToList();

                _outfits = _outfitList.Select(f => f.DisplayName).ToArray();

                if (!string.IsNullOrEmpty(_characterName))
                {
                    this.Plugin.Configuration.MyCharacter.Name = _characterName;
                }
                this.Plugin.Configuration.Save();

                
                Init();
            }

            if (ImGui.Button("Delete Outfit"))
            {
                if (this.Plugin.Configuration.MyCharacter.Outfits.ContainsKey(_outfit.Name))
                {
                    this.Plugin.Configuration.MyCharacter.Outfits.Remove(_outfit.Name);

                    this.Plugin.Configuration.Save();
                    _newOutfitName = "";
                    _penumbraCollection = "";
                    _glamourerDesign = "";
                    _gearset = "";
                    _notes = "";
                    _tags = "";
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
                _tags = "";
                this.Plugin.EquipOutfit(_outfit.Name);
            }
        }
    }
}
