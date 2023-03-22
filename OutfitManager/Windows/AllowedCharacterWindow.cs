using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OutfitManager.Windows
{
    public class AllowedCharacterWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private string _characterName = string.Empty;
        private string _fullName = string.Empty;
        private string _worldName = string.Empty;
        private Character _character = new Character();
        private int _currentItem = 0;
        private bool _outfitLock = false;
        public void Dispose()
        {

        }
        public AllowedCharacterWindow(Plugin plugin) : base(
           "OutfitManager Allowed Character Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(675, 630),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };


            this.Plugin = plugin;
        }
        public override void Draw()
        {
            CharacterAddition();
            CharacterList();
        }
        public void CharacterList()
        {
            if (ImGui.ListBox("Allowed Characters", ref this._currentItem, this.Plugin.Configuration.SafeSenders.Keys.ToArray(), this.Plugin.Configuration.SafeSenders.Count(), 10))
            {

                var names = this.Plugin.Configuration.SafeSenders.Keys.ToArray();
                Character c = this.Plugin.Configuration.SafeSenders[names[_currentItem]];

                this._characterName = c.Name;
                this._worldName = c.World;

                if (string.IsNullOrEmpty(c.FullName))
                {
                    c.FullName = $"{_characterName}@{_worldName}";
                }
            }
        }
        public void CharacterAddition()
        {
            ImGui.InputTextWithHint("Character Name", "Enter character name that can dress you...", ref this._characterName, 64, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.InputTextWithHint("World Name", "Enter world of character (optional but reccomended).", ref this._worldName, 64, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.Checkbox("Character can lock my outfit manager.", ref this._outfitLock);
           

            if (ImGui.Button("Add / Update Character") && (!string.IsNullOrEmpty(_characterName)))
            {
                this._character = new Character
                {
                    Name = this._characterName,
                    World = this._worldName,
                    FullName = $"{_characterName}@{_worldName}",
                    canOutfitLock = this._outfitLock
                };

                if (!string.IsNullOrEmpty(_characterName))
                {
                    if (!this.Plugin.Configuration.SafeSenders.ContainsKey(_character.FullName))
                    {
                        this.Plugin.Configuration.SafeSenders.Add(_character.FullName, this._character);
                    }
                    else
                    {
                        this.Plugin.Configuration.SafeSenders[_character.FullName] = this._character;
                    }
                    this.Plugin.Configuration.Save();
                }
            }

            if (ImGui.Button("Delete Character"))
            {
                var names = this.Plugin.Configuration.SafeSenders.Keys.ToArray();
                this.Plugin.Configuration.SafeSenders.Remove(names[_currentItem]);
                this.Plugin.Configuration.Save();
            }
        }
    }
}
