using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.Havok;
using ImGuiNET;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using OutfitManager.Handlers;
using OutfitManager.Ipc;
using OutfitManager.Models;
using OutfitManager.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace OutfitManager.Windows
{
    public class OutfitListWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private string _newOutfitName = string.Empty;
        private string _penumbraCollection = string.Empty;
        private string _glamourerDesign = string.Empty;
        private string _gearset = string.Empty;
        private OmgOutfit _outfit;
        private int _currentItem = 0;
        private List<OmgOutfit> _outfitList;
        private string _characterName = string.Empty;
        private string _notes = string.Empty;
        private string _tags = string.Empty;
        private string _filter = string.Empty;
        private bool _favourite = false;
        private bool _showFavourites = false;
        private string[] _outfits;
        private string[] _filteredOutfits;
        private string _pastFilter = string.Empty;
        private string _previewDirectory = string.Empty;
        string _favouritesText = "Show Favourites";
        private bool _showPreview = false;
        private bool _showErrorPopup = false;
        private bool _firstDraw = true;
        private string _errorText = "";
        private int _itemsToShow = 15;
        public override void OnClose()
        {
            Dispose();
        }

        public void Dispose()
        {
            this.Plugin.ShowOrHideWindow("Outfit Preview Window", false);
            this.Plugin.OutfitPreview = null;
        }
        public OutfitListWindow(Plugin plugin) : base(
           "OutfitManager Outfit List Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {

            DalamudService.PluginInterface.UiBuilder.DisableGposeUiHide = true;

            // Remove the minimum size constraint
            this.SizeConstraints = new WindowSizeConstraints
            {   MinimumSize = new Vector2(385, 290),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };

            this.Plugin = plugin;

            Init();

        }
        public override void Draw()
        {
            if (_firstDraw)
            {
                if (this.Plugin.Configuration.MyCharacter != null && this.Plugin.OutfitHandler.Outfits != null)
                {
                    if (this.Plugin.OutfitHandler.Outfits.Count == 0)
                    {
                    
                            this.Size = new Vector2(775, 790); // Replace with your desired initial size
                            _firstDraw = false;
                        
                    }
                }
            }
            //   List<String> outfitList = this.Plugin.CustomConfig.Outfits.Keys.ToList();
            // Wrap the content in ImGui.BeginChild() and ImGui.EndChild()
         

       //     if (ImGui.BeginTabBar("OutfitTabBar"))
         //   {
          //      if (ImGui.BeginTabItem("View All"))
           //     {
                    ImGui.BeginChild("OutfitListWindowContent", new Vector2(-1, -1), false, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
                    _itemsToShow = 15;
                    if (this.Plugin.Configuration.MyCharacter != null && this.Plugin.Configuration.MyCharacter.FullName != "")
                    {
                        if (ImGui.CollapsingHeader("Add / Manage",ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            OutfitAddition();
                       AddOutfitButton();

                ImGui.SameLine();
                        }

                      
                            ButtonRow();
                          if (ImGui.CollapsingHeader("Outfit Listing", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            DrawCurrentOutfitName();
                            if (!string.IsNullOrEmpty(_previewDirectory))
                            {
                                PreviewImage();
                            }
                            OutfitList();
                            ExportToClipboard();
                        }

                    }
                    ImGui.EndChild();
                    ImGui.EndTabItem();
         //       }
    
                //if (ImGui.BeginTabItem("Outfit List"))
                //{
               
                //    _itemsToShow = 25;
                //    ButtonRow(false);

                //    DrawCurrentOutfitName();
                //    if (!string.IsNullOrEmpty(_previewDirectory))
                //    {
                //        PreviewImage();
                //    }
                //    ImGui.BeginChild("OutfitListWindowContent", new Vector2(-1, -1), false, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
                //    OutfitList();
                //    ImGui.EndChild();
                //    ImGui.EndTabItem();
                //}
            

          //  }
        }

        public void ShowErrorPopupBox()
        {
            if (_showErrorPopup)
            {
                ImGui.OpenPopup("Error");
            }
            if (ImGui.BeginPopupModal("Error", ref _showErrorPopup, ImGuiWindowFlags.AlwaysAutoResize))
            {
                _errorText = "The Penumbra collection does not exist.";
                ImGui.Text(_errorText);
                if (ImGui.Button("OK"))
                {
                    _showErrorPopup = false;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }
        }

        public void Init()
        {
            if (!string.IsNullOrEmpty(this.Plugin.Configuration.MyCharacter.Name))
            {
                _outfitList = this.Plugin.OutfitHandler.Outfits.Values.OrderBy(x => x.DisplayName).ToList();
                _characterName = this.Plugin.Configuration.MyCharacter.Name;
                _outfits = _outfitList.Select(f => f.DisplayName).ToArray();
                _filteredOutfits = _outfits;
                _filter = "";

                if (!string.IsNullOrEmpty(this.Plugin.Configuration.PreviewDirectory) && Directory.Exists(this.Plugin.Configuration.PreviewDirectory))
                {
                    _previewDirectory = this.Plugin.Configuration.PreviewDirectory;
                }
                _showPreview = this.Plugin.Configuration.ShowPreview;
            }
        }

        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public void ExportToClipboard()
        {


            if (ImGui.Button("Export all outfits to clipboard") && this._outfits.Count() > 0)
            {
                OutfitExport outfitExport = new OutfitExport();

                OmgCharacter character = new OmgCharacter { FullName = this.Plugin.Configuration.MyCharacter.FullName, isUserCharacter = false, Name = this.Plugin.Configuration.MyCharacter.Name, World = this.Plugin.Configuration.MyCharacter.World };
                character.Outfits = new Dictionary<string, OmgOutfit>();

                outfitExport.Outfits = this._outfits;
                outfitExport.Character = character;

                string json = JsonConvert.SerializeObject(outfitExport);

                ImGui.SetClipboardText(Base64Encode(json));
            }
        }

        public void PreviewImage()
        {
          
                if (ImGui.Checkbox("Show Preview Window.", ref _showPreview))
                {

                    if (this.Plugin.OutfitPreview != null && _showPreview)
                    {
                        this.Plugin.ShowOrHideWindow("Outfit Preview Window", true);
                    }
                    else
                    {
                        this.Plugin.ShowOrHideWindow("Outfit Preview Window", false);
                    }

                    if (this.Plugin.Configuration.ShowPreview != this._showPreview)
                    {
                        this.Plugin.Configuration.ShowPreview = _showPreview;
                        this.Plugin.Configuration.Save();
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
        public void DrawCurrentOutfitName()
        {
            if (this.Plugin.Configuration.OutfitName != null && this.Plugin.OutfitHandler.Outfits.ContainsKey(this.Plugin.Configuration.OutfitName))
            {
                ImGui.Separator();
                OmgOutfit currentOutfit = this.Plugin.OutfitHandler.Outfits[this.Plugin.Configuration.OutfitName];

                if (ImGui.Selectable($"Currently Equipped Outfit: {currentOutfit.DisplayName}"))
                {
                    SelectOutfit(currentOutfit);
                }
            }
        }

        private void SelectOutfit(OmgOutfit outfit)
        {
            _outfit = outfit;
            _newOutfitName = _outfit.DisplayName;
            _penumbraCollection = _outfit.CollectionName;
            _glamourerDesign = _outfit.DesignPath;
            _gearset = _outfit.GearSet;
            _notes = _outfit.Notes;
            _tags = String.Join(",", _outfit.Tags);
            _favourite = _outfit.IsFavourite;

            if (_showPreview)
            {
                this.Plugin.SetImagePreview(_outfit.Name);
            }
        }


        public void OutfitList()
        {

             if( ImGui.InputTextWithHint("Filter", "Filter...", ref _filter, 64))
            {
              
            }
            if (ImGui.ListBox("Outfits", ref _currentItem, FilterOutfits(_filter), _filteredOutfits.Count(), _itemsToShow))
            {
                _outfit = this.Plugin.OutfitHandler.Outfits[_filteredOutfits[_currentItem].ToLower()];
                SelectOutfit(_outfit);
            }
        }

       

        public void OutfitAddition()
        {
            ImGui.InputTextWithHint("Outfit Name", "Enter outfit name...", ref _newOutfitName, 64, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.InputTextWithHint("Penumbra Collection", "Enter penumbra collection name (e.g. character name)", ref _penumbraCollection, 64, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.InputTextWithHint("Glamourer Design", "Glamourer Design Path (E.g. /collections/outfit1) (optional)", ref _glamourerDesign, 64, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.InputTextWithHint("Gearset", "Enter gearset number (e.g. 1 10 for gearset 1 glam plate 10)(optional)", ref _gearset, 64, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.InputTextWithHint("Tags", "Enter tags for your gear comma separated (optional)", ref _tags, 64, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.Checkbox("Favourite ?", ref _favourite);
            ImGui.InputTextMultiline("Notes", ref _notes,512, new Vector2(440, 60));



            ShowErrorPopupBox();
        }

        public void ButtonRow()
        {

                if (ImGui.Button("Delete Outfit (shift)"))
                {
                    if (ImGui.IsKeyDown(ImGuiKey.ModShift))
                    {
                        if (this.Plugin.OutfitHandler.Outfits.ContainsKey(_outfit.Name))
                        {
                            this.Plugin.OutfitHandler.Outfits.Remove(_outfit.Name);

                            this.Plugin.Configuration.Save();
                            _newOutfitName = "";
                            _penumbraCollection = "";
                            _glamourerDesign = "";
                            _gearset = "";
                            _notes = "";
                            _tags = "";
                            _favourite = false;
                            Init();

                        }
                    }
                }
            
            if (ImGui.GetWindowSize().X < 485)
            {
                float currentY = ImGui.GetCursorPosY();
                float spacing = 1.0f; // Adjust the value to change the spacing between the buttons
                ImGui.SetCursorPosY(currentY + spacing);
            }
            else
            {
               
                    ImGui.SameLine();
                
            }
            if (ImGui.Button(_favouritesText))
            {
                if (!_showFavourites)
                {
                    _favouritesText = "Show All";
                    _showFavourites = true;
                    _filteredOutfits = _outfitList.Where(x => x.IsFavourite).Select(f => f.DisplayName).ToArray();
                }
                else
                {
                    _favouritesText = "Show Favourites";
                    _showFavourites = false;
                    _filteredOutfits = _outfitList.Select(f => f.DisplayName).ToArray();
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("Wear Outfit"))
            {
                _newOutfitName = "";
                _penumbraCollection = "";
                _glamourerDesign = "";
                _gearset = "";
                _notes = "";
                _tags = "";
                _favourite = false;
                this.Plugin.OutfitHandler.EquipOutfit(_outfit.Name);
            }
        }

        public void AddOutfitButton()
        {
            if (ImGui.Button("Add / Update Outfit") && (!string.IsNullOrEmpty(_newOutfitName)))
            {
                if (!string.IsNullOrEmpty(_penumbraCollection))
                {
                    if (!this.Plugin.GetAvailableCollections().Contains(_penumbraCollection, StringComparer.OrdinalIgnoreCase))
                    {
                        _showErrorPopup = true;
                        return;
                    }
                }
                _outfit = new OmgOutfit
                {
                    CollectionName = _penumbraCollection,
                    DesignPath = _glamourerDesign,
                    DisplayName = _newOutfitName,
                    GearSet = _gearset,
                    Name = _newOutfitName.ToLower().Replace(":", ""),
                    Notes = _notes,
                    Tags = _tags.ToLower().Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
                    IsFavourite = _favourite
                };




                if (!this.Plugin.OutfitHandler.Outfits.ContainsKey(_outfit.Name))
                {
                    this.Plugin.OutfitHandler.Outfits.Add(_outfit.Name, _outfit);
                }
                else
                {
                    this.Plugin.OutfitHandler.Outfits[_outfit.Name] = _outfit;
                }

                _outfitList = this.Plugin.OutfitHandler.Outfits.Values.OrderBy(x => x.DisplayName).ToList();

                _outfits = _outfitList.Select(f => f.DisplayName).ToArray();
                _filteredOutfits = _outfits;
                _filter = "";
                if (!string.IsNullOrEmpty(_characterName))
                {
                    this.Plugin.Configuration.MyCharacter.Name = _characterName;
                }
                this.Plugin.Configuration.Save();
                this.Plugin.OutfitHandler.SaveOutfits(this.Plugin.OutfitHandler.Outfits);

                Init();
            }
        }

        //Potential future feature
        public void AddSnapshotButton()
        {

            if (ImGui.Button("Add / Update Snapshot") && (!string.IsNullOrEmpty(_newOutfitName)))
            {
                if (!string.IsNullOrEmpty(_penumbraCollection))
                {
                    if (!this.Plugin.GetAvailableCollections().Contains(_penumbraCollection, StringComparer.OrdinalIgnoreCase))
                    {
                        _showErrorPopup = true;
                        return;
                    }
                }
                else
                {
                  _penumbraCollection = this.Plugin.GetCurrentCollection();
                }
                _outfit = new OmgOutfit
                {
                    CollectionName = _penumbraCollection,
                    DesignPath = _glamourerDesign,
                    DisplayName = _newOutfitName,
                    GearSet = _gearset,
                    Name = _newOutfitName.ToLower().Replace(":", ""),
                    Notes = _notes,
                    Tags = _tags.ToLower().Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
                    IsFavourite = _favourite,
                    IsSnapshot = true
                };

                try
                {
                   string glamourerData = GlamourerIpc.Instance.GetAllCustomizationFromCharacterIpc(DalamudService.ClientState.LocalPlayer);

                    _outfit.GlamourerData = glamourerData;

                
                }
                catch
                {
                    _outfit.IsSnapshot = false;
                    _errorText = "Problem contacting glamourer";
                    _showErrorPopup = true;
                    return;
                  
                }

                if (!this.Plugin.OutfitHandler.Outfits.ContainsKey(_outfit.Name))
                {
                    this.Plugin.OutfitHandler.Outfits.Add(_outfit.Name, _outfit);
                }
                else
                {
                    this.Plugin.OutfitHandler.Outfits[_outfit.Name] = _outfit;
                }

                _outfitList = this.Plugin.OutfitHandler.Outfits.Values.OrderBy(x => x.DisplayName).ToList();

                _outfits = _outfitList.Select(f => f.DisplayName).ToArray();
                _filteredOutfits = _outfits;
                _filter = "";
                if (!string.IsNullOrEmpty(_characterName))
                {
                    this.Plugin.Configuration.MyCharacter.Name = _characterName;
                }
                this.Plugin.Configuration.Save();
                this.Plugin.OutfitHandler.SaveOutfits(this.Plugin.OutfitHandler.Outfits);

                Init();
            }
        }
    }
}
