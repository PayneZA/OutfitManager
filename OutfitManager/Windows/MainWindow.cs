using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.IO;
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
   
        private Plugin _Plugin;
        private string _characterName = "";
        private bool _characterExists = false;
        private string _worldName = "";
        private bool _worldExists = false;
        private bool _chatControl = false;
        private string _screenshotDirectory = "";
        private string _previewDirectory = "";
        private bool _persist = false;
        private bool _persistGearset = false;
        public MainWindow(Plugin plugin) : base(
            "OutfitManager", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(375, 330),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };

        
            this._Plugin = plugin;

            Init();
        }

        public void Dispose()
        {
           
        }

        public void Init()
        {
            _chatControl = this._Plugin.Configuration.ChatControl;
            _characterName = this._Plugin.Configuration.MyCharacter.Name;
            _worldName = this._Plugin.Configuration.MyCharacter.World;
            _characterExists = !string.IsNullOrEmpty(this._Plugin.Configuration.MyCharacter.Name);
            _worldExists = !string.IsNullOrEmpty(this._Plugin.Configuration.MyCharacter.Name);
            _previewDirectory = this._Plugin.Configuration.PreviewDirectory;
            _persist = this._Plugin.Configuration.Persist;
            _persistGearset = this._Plugin.Configuration.PersistGearset;

        }

        public void RemoteControl()
        {
            if (ImGui.Checkbox("Allow Chat Control (Via Tell)",ref _chatControl))
            {

                this._Plugin.SetChatMonitoring(_chatControl);
            }
        }
        public void CharacterName()
        {
           if( ImGui.InputTextWithHint("Character Name", "Enter your character name and press enter...", ref _characterName, 64, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                this._Plugin.Configuration.MyCharacter.Name = _characterName;
                this._Plugin.Configuration.Save();
                _characterExists = true;
            }

           if (_characterExists)
            {
                WorldName();
            }
        }

        public void WorldName()
        {
            if (ImGui.InputTextWithHint("World Name", "Enter your world name and press enter...", ref _worldName, 64, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                    this._Plugin.Configuration.MyCharacter.World = _worldName;
                    this._Plugin.Configuration.MyCharacter.FullName = $"{this._Plugin.Configuration.MyCharacter.Name}@{this._Plugin.Configuration.MyCharacter.World}";
                    this._Plugin.Configuration.Save();
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
                    this._Plugin.DrawOutfitListUI();
                }
                if (ImGui.Button("Manage allow list"))
                {
                    this._Plugin.DrawAllowedUserUI();
                }
                if (ImGui.InputTextWithHint("Preview Directory", "Enter your outfit preview directory and press enter...", ref _previewDirectory, 64, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    if (Directory.Exists(this._previewDirectory))
                    {
                        this._Plugin.Configuration.PreviewDirectory = this._previewDirectory;
                        this._Plugin.Configuration.Save();
                    }
                }

                RemoteControl();

                Persist();
            }
    
        }

        public void OutFitList()
        {
            if (ImGui.Button("Add/Edit/View Outfits"))
            {
                this._Plugin.DrawOutfitListUI();
            }
        }

        public void Persist()
        {
            if (ImGui.Checkbox("Re-wear outfit between zones. (Gearset changes will not be re-applied)", ref _persist))
            {

                this._Plugin.PersistOutfit = _persist;
                this._Plugin.Configuration.Persist = _persist;
                this._Plugin.Configuration.Save();
            }

            if (_persist)
            {
                if (ImGui.Checkbox("Re-wear outfit between gearsets. (Outfit will be re-applied on gearset change)", ref _persistGearset))
                {

                    this._Plugin.PersistGearset = _persistGearset;
                    this._Plugin.Configuration.PersistGearset = _persistGearset;

                    if (_persist)
                    {
                        this._Plugin.Configuration.Save();
                    }
                }
            }
        }
    }

}
