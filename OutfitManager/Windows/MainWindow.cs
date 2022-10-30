using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Dalamud.Interface.Windowing.Window;

namespace OutfitManager.Windows
{
    public class MainWindow : Window, IDisposable
    {
   
        private Plugin Plugin;
        private string _characterName = "";
        private bool _characterExists = false;
        private string _worldName = "";
        private bool _worldExists = false;
        private bool _chatControl = false;
        public MainWindow(Plugin plugin) : base(
            "OutfitManager", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(375, 330),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };

        
            this.Plugin = plugin;
            _characterName = this.Plugin.Configuration.CharacterName;

            _worldName = "";
            if (!string.IsNullOrEmpty(_characterName))
            {
                if (this.Plugin.Configuration.Characters.ContainsKey(_characterName))
                {
                    _characterExists = true;
                    if (!string.IsNullOrEmpty(this.Plugin.Configuration.Characters[_characterName].World))
                    {
                        _worldName = this.Plugin.Configuration.Characters[_characterName].World;
                        _worldExists = true;
                        _chatControl = this.Plugin.Configuration.ChatControl;
                    }
                    
                }
            }
        }

        public void Dispose()
        {
           
        }

        public void RemoteControl()
        {
            if (ImGui.Checkbox("Allow Chat Control (Via Tell)",ref _chatControl))
            {
                this.Plugin.Configuration.ChatControl = _chatControl;
                this.Plugin.Configuration.Save();
            }
        }
        public void CharacterName()
        {
           if( ImGui.InputTextWithHint("Character Name", "Enter your character name and press enter...", ref _characterName, 64, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                this.Plugin.Configuration.CharacterName = _characterName;

                if (!this.Plugin.Configuration.Characters.ContainsKey(_characterName))
                {
                    this.Plugin.Configuration.Characters.Add(_characterName, new Character());
                }
                this.Plugin.Configuration.Save();
                _characterExists = true;
            }

           if (!string.IsNullOrEmpty(this.Plugin.Configuration.CharacterName) && this.Plugin.Configuration.Characters.ContainsKey(this.Plugin.Configuration.CharacterName))
            {
                _characterExists = true;

                WorldName();
            }
        }

        public void WorldName()
        {
            if (ImGui.InputTextWithHint("World Name", "Enter your world name and press enter...", ref _worldName, 64, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                    this.Plugin.Configuration.Characters[_characterName].World = _worldName;
                    this.Plugin.Configuration.Save();
                    _worldExists = true;
            }

        }
        public override void Draw()
        {
            CharacterName();
         
            if (_characterExists && _worldExists)
            {
             
                if (ImGui.Button("Add/Edit/View Outfits"))
                {

                    this.Plugin.DrawOutfitListUI();
                }
                if (ImGui.Button("Manage allow list"))
                {
                    this.Plugin.DrawAllowedUserUI();
                }

                RemoteControl();
            }
        
        }

        public void OutFitList()
        {
            if (ImGui.Button("Add/Edit/View Outfits"))
            {

                this.Plugin.DrawOutfitListUI();
            }
        }
    }

}
