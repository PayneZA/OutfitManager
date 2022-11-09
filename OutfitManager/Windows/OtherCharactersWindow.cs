using Dalamud.Interface.Windowing;
using FFXIVClientStructs.Havok;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureGearsetModule;

namespace OutfitManager.Windows
{

    public class OtherCharactersWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private Character _character;
        private string _characterName = string.Empty;
        private string _selectedOutfit = string.Empty;
        private int _currentCharacter = 0;
        private int _currentOutfit = 0;
        private string[] _characters;
        private string[] _outfits;
        private string[] _filteredOutfits;
        private string _filter = string.Empty;
        private string _pastFilter = string.Empty;
        public void Dispose()
        {

        }
        public OtherCharactersWindow(Plugin plugin) : base(
           "OutfitManager Other Character Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(675, 630),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };

            this.Plugin = plugin;

            Init();
        }

        public void Init()
        {
            if (this.Plugin.Configuration.Characters != null && this.Plugin.Configuration.Characters.Count > 0)
            {
                var chars = this.Plugin.Configuration.Characters.Keys.ToList();
                _characters = chars.OrderByDescending(x => x).ToArray();
            }
            else
            {
                _characters = null;
                _outfits = null;
            }
        }
        public override void Draw()
        {
            CharacterAddition();
            CharacterList();
            OutfitList();
        }
        public void CharacterList()
        {
            if (_characters != null)
            {
                if (ImGui.ListBox("Characters", ref _currentCharacter, _characters, _characters.Count(), 5))
                {
                    _character = this.Plugin.Configuration.Characters[_characters[_currentCharacter]];
                    _filteredOutfits = _character.Outfits.Keys.ToArray();
                    _outfits = _character.Outfits.Keys.ToArray();
                }
            }
        }

        public void OutfitList()
        {
         
            if (_outfits != null)
            {
                if (ImGui.InputTextWithHint("Filter", "Filter...", ref _filter, 64))
                {

                }
                if (ImGui.ListBox("Outfits", ref _currentOutfit, FilterOutfits(_filter), _filteredOutfits.Count(), 15))
                {
                    _selectedOutfit = _filteredOutfits[_currentOutfit].ToLower();
                }

                if (ImGui.Button("Send Wear Outfit"))
                {
                    this.Plugin.SendEquipOutfit(_character.FullName,_character.Name, _selectedOutfit);
                }
            }
        }

        public string[] FilterOutfits(string filter)
        {
            if (filter != _pastFilter)
            {
                if (string.IsNullOrEmpty(filter))
                {
                    _filteredOutfits = _outfits;

                }
                else
                {
                    _filteredOutfits = _outfits.Where(x => x.Contains(_filter, StringComparison.OrdinalIgnoreCase)).ToArray();
                }
                _pastFilter = filter;
            }

            return _filteredOutfits;
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public void CharacterAddition()
        {
            if (ImGui.Button("Import Outfits From Clipboard"))
            {
                try
                {
                    string base64 = ImGui.GetClipboardText();
                    OutfitExport outfitExport = JsonConvert.DeserializeObject<OutfitExport>(Base64Decode(base64));

                    if (this.Plugin.Configuration.Characters == null)
                    {
                        this.Plugin.Configuration.Characters = new Dictionary<string, Character>();
                    }
                    Character character = outfitExport.Character;

                    foreach (string s in outfitExport.Outfits)
                    {
                        character.Outfits.Add(s,new Outfit { CharacterName = character.FullName, Name = s });
                    }

                    if (this.Plugin.Configuration.Characters.ContainsKey(outfitExport.Character.FullName))
                    {
                        this.Plugin.Configuration.Characters[outfitExport.Character.FullName] = character;
                    }
                    else
                    {
                        this.Plugin.Configuration.Characters.Add(character.FullName, character);
                    }
                    this.Plugin.Configuration.Save();

                    Init();
                }
                catch
                {

                }
            }

            if (ImGui.Button("Delete Other Character"))
            {
                if (this.Plugin.Configuration.Characters.ContainsKey(_character.FullName))
                {
                    var names = this.Plugin.Configuration.Characters.Remove(_character.FullName);
                    _currentCharacter = 0;
                    _selectedOutfit = "";
                    _currentOutfit = 0;
                    this.Plugin.Configuration.Save();

                    Init();
                }
            }
        }
    }
}
