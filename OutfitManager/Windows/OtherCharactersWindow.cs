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

    public class OtherCharactersWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private string _characterName = string.Empty;
        private string _worldName = string.Empty;
        private Character _character = new Character();
        private int _currentItem = 0;
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
        }
        public override void Draw()
        {
            CharacterAddition();
            CharacterList();



        }
        public void CharacterList()
        {

        }
        public void CharacterAddition()
        {
            ImGui.InputTextWithHint("Character Name", "Enter character name that can dress you...", ref _characterName, 64, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.InputTextWithHint("World Name", "Enter world of character (optional but reccomended).", ref _worldName, 64, ImGuiInputTextFlags.EnterReturnsTrue);


            if (ImGui.Button("Add / Update Character") && (!string.IsNullOrEmpty(_characterName)))
            {
                _character = new Character
                {
                    Name = _characterName,
                    World = _worldName
                };

                if (!string.IsNullOrEmpty(_characterName))
                {
                    if (!this.Plugin.Configuration.SafeSenders.ContainsKey(_characterName))
                    {
                        this.Plugin.Configuration.SafeSenders.Add(_characterName + "@" + _worldName, _character);
                    }
                    else
                    {
                        this.Plugin.Configuration.SafeSenders[_characterName + "@" + _worldName] = _character;
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
