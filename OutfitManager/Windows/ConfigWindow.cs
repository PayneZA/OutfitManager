using Dalamud.Interface.Windowing;
using ImGuiNET;
using OutfitManager.Ipc;
using OutfitManager.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OutfitManager.Windows
{
    public class ConfigWindow : Window, IDisposable
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
        private bool _ignorePersistCollection = false;
        private string _primaryCollection = "";
        private bool _showErrorPopup = false;
        private int _currentCollectionTypeIndex;
        private bool _autoCollection = false;
        private bool _autoGlamourer = false;
        private bool _customizeSupport = false;
        private bool _resetScalesToDefault = true;

        string[] collectionTypes = new string[] { "Individual", "Your Character" };

        public ConfigWindow(Plugin plugin) : base(
            "Outfit Manager Configuration Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(615, 520),
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
            _ignorePersistCollection = this._Plugin.Configuration.IgnorePersistCollection;
            _primaryCollection = this._Plugin.Configuration.PrimaryCollection;
            _currentCollectionTypeIndex = Array.IndexOf(collectionTypes, this._Plugin.Configuration.PenumbraCollectionType);
            _autoCollection = this._Plugin.Configuration.AutoCollection;
            _autoGlamourer = this._Plugin.Configuration.AutoGlamourer;
            _customizeSupport = this._Plugin.Configuration.EnableCustomizeSupport;
            _resetScalesToDefault = this._Plugin.Configuration.ResetScalesToDefault;
        }


        public void RemoteControl()
        {
            if (ImGui.Checkbox("Allow Chat Control (Via Tell)", ref _chatControl))
            {

                this._Plugin.SetChatMonitoring(_chatControl);
            }
        }
        public void CharacterName()
        {
            if (ImGui.InputTextWithHint("Character Name", "Enter your character name and press enter...", ref _characterName, 64, ImGuiInputTextFlags.EnterReturnsTrue))
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

        public void ShowErrorPopupBox()
        {
            if (_showErrorPopup)
            {
                ImGui.OpenPopup("Error");
            }
            if (ImGui.BeginPopupModal("Error", ref _showErrorPopup, ImGuiWindowFlags.AlwaysAutoResize))
            {
         
                ImGui.Text("The directory does not exist.");
                if (ImGui.Button("OK"))
                {
                    _showErrorPopup = false;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
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
                if (ImGui.InputTextWithHint("Preview Directory (Optional)", "Enter your outfit preview directory and press enter...", ref _previewDirectory, 64, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    if (Directory.Exists(this._previewDirectory))
                    {
                        this._Plugin.Configuration.PreviewDirectory = this._previewDirectory;
                        this._Plugin.Configuration.Save();
                    }
                    else
                    {
                        _showErrorPopup = true;
                    }
                }

                RemoteControl();



                if (ImGui.CollapsingHeader("Customize Settings", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    if (ImGui.Checkbox("Enable Customize Support (Requires a default-omg-scale scale to switch back to normal.", ref _customizeSupport))
                    {
                        this._Plugin.Configuration.EnableCustomizeSupport = _customizeSupport;
                        this._Plugin.Configuration.Save();
                    }

                    if (_customizeSupport)
                    {
                        if (ImGui.Checkbox("Reset scale to default if none specified ?", ref _resetScalesToDefault))
                        {
                            this._Plugin.Configuration.ResetScalesToDefault = _resetScalesToDefault;
                            this._Plugin.Configuration.Save();
                        }
                    }
                }
             
              
                PenumbraCollectionTypeSelection();

                if (ImGui.InputTextWithHint("Primary Collection (optional)", "Enter your default 'go to ' collection and press enter...", ref _primaryCollection, 64, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    this._Plugin.Configuration.PrimaryCollection = this._primaryCollection;
                    this._Plugin.Configuration.Save();
                }

                if (ImGui.CollapsingHeader("Re-Wear Settings", ImGuiTreeNodeFlags.DefaultOpen))
                {
                 //   Persist();

            
                }
                if (ImGui.CollapsingHeader("Auto-Fill Settings", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    AutoFill();

           
                }

             
            }
            ShowErrorPopupBox();
        }

        public void OutFitList()
        {
            if (ImGui.Button("Add/Edit/View Outfits"))
            {
                this._Plugin.DrawOutfitListUI();
            }
        }
        public void PenumbraCollectionTypeSelection()
        {


            if (ImGui.Combo("Penumbra Collection Type", ref _currentCollectionTypeIndex, collectionTypes, collectionTypes.Length))
            {
                this._Plugin.Configuration.PenumbraCollectionType = collectionTypes[_currentCollectionTypeIndex];
                this._Plugin.Configuration.Save();
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

                if (ImGui.Checkbox("Do not Re-Apply collection on automatic Re-Wear. (Only design will be re-applied)", ref _ignorePersistCollection))
                {

                    this._Plugin.IgnorePersistCollection = _ignorePersistCollection;
                    this._Plugin.Configuration.IgnorePersistCollection = _ignorePersistCollection;
                    this._Plugin.Configuration.Save();
                }
            }
        }

        public void AutoFill()
        {
            if (ImGui.Checkbox("Set current collection on add/update outfit if not specified.", ref _autoCollection))
            {
                this._Plugin.Configuration.AutoCollection = _autoCollection;
                this._Plugin.Configuration.Save();
            }

           if (this._Plugin.OutfitHandler.AutoGlamourerUsed || this._Plugin.Configuration.AutoGlamourer)
            {
                if (ImGui.Checkbox("Store current glamourer equipment on add/update outfit if not specified.", ref _autoGlamourer))
                {
                    this._Plugin.Configuration.AutoGlamourer = _autoGlamourer;
                    this._Plugin.Configuration.Save();
                }

            ImGui.Text("Glamourer 2 will use designs in a different way and your current auto designs will not work.");
            ImGui.Text("Please create new designs for existing auto designs.");
            ImGui.Text("You just wear the outfit, save the design and fill in the path in OutfitManager.");
            ImGui.Text("Auto designs will stop working when Glamourer 2 comes out.");
            ////    ImGui.Text("Do not use this if you are already on glamourer 2.");

            ////if (ImGui.Button("Create Glamourer Designs From AutoGlamourer (Shift)"))
            ////{
            ////    if (ImGui.IsKeyDown(ImGuiKey.ModShift))
            ////    {
            ////        this._Plugin.OutfitHandler.MigrateToGlamourer();
            ////        _autoGlamourer = false;
            ////        this._Plugin.Configuration.AutoGlamourer = false;
            ////        this._Plugin.Configuration.Save();
            ////    }
            ////}
            ////    ImGui.Text("Designs will be created in a omgoutfits folder in Glamourer, it is important to restart your game after runnning this.");
            ////    ImGui.Text("You will need to manually choose to apply customizations or not in the designs afterwards.");
            ////    ImGui.Text("Glamourer 2 will use designs in a different way and your current auto designs will not work.");
            ////    ImGui.Text("Backup your Glamourer Designs and Omg Outfits Before Doing this.");
             }
        }


    }

}
