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
using Penumbra.Api.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static Penumbra.Api.Ipc;

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
        private bool _replaceExisting = false;
        private bool _cropToVertical = false;
        private string _scaleName = "";
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
            {   MinimumSize = new Vector2(385, 310),
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
                        this.Size = new Vector2(667, 871); // Replace with your desired initial size
                        _firstDraw = false;
                    }
                }
            }

      
            if (ImGui.BeginTabBar("OutfitTabBar"))
            {
                if (ImGui.BeginTabItem("Standard View"))
                {
                    ImGui.BeginChild("StandardViewContent", new Vector2(-1, -1), false, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
                    _itemsToShow = 15;
                    if (this.Plugin.Configuration.MyCharacter != null && this.Plugin.Configuration.MyCharacter.FullName != "")
                    {
                        if (ImGui.CollapsingHeader("Add / Manage", ImGuiTreeNodeFlags.DefaultOpen))
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
                }

                //if (ImGui.BeginTabItem("Advanced View"))
                //{
                //    ImGui.BeginChild("AdvancedViewContent", new Vector2(-1, -1), false, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
                //    _itemsToShow = 5;
                //    if (this.Plugin.Configuration.MyCharacter != null && this.Plugin.Configuration.MyCharacter.FullName != "")
                //    {
                //        if (ImGui.CollapsingHeader("Add / Manage", ImGuiTreeNodeFlags.DefaultOpen))
                //        {
                //            OutfitAddition();
                //            AddOutfitButton();

                //            ImGui.SameLine();
                //        }


                //        ButtonRow();
                //        if (ImGui.CollapsingHeader("Outfit Listing", ImGuiTreeNodeFlags.DefaultOpen))
                //        {
                //            DrawCurrentOutfitName();
                //            if (!string.IsNullOrEmpty(_previewDirectory))
                //            {
                //                PreviewImage();
                //            }
                //            OutfitList();
                //            ExportToClipboard();
                //        }

                //    }
                //    ImGui.EndChild();
                //    ImGui.EndTabItem();
                //}

                if (ImGui.BeginTabItem("List Only"))
                {
                    ImGui.BeginChild("ListOnlyViewContent", new Vector2(-1, -1), false, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
                    _itemsToShow = 25;
                    if (this.Plugin.Configuration.MyCharacter != null && this.Plugin.Configuration.MyCharacter.FullName != "")
                    {
                      


                        ButtonRow();
                      
                            DrawCurrentOutfitName();
                            if (!string.IsNullOrEmpty(_previewDirectory))
                            {
                                PreviewImage();
                            }
                            OutfitList();
                            ExportToClipboard();
                        

                    }
                    ImGui.EndChild();
                    ImGui.EndTabItem();
                }

                if (!string.IsNullOrEmpty(this.Plugin.Configuration.PreviewDirectory))
                {
                    if (ImGui.BeginTabItem("Preview Generation"))
                    {
                        ImGui.BeginChild("PreviewGenerationContent", new Vector2(-1, -1), false);

                        ImGui.Text("Outfit Preview Generation");
                        ImGui.Separator();
                        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            ImGui.Text("Welcome to the Outfit Preview Generation process.");
                            ImGui.Text("Follow these steps to create previews of your outfits:");
                            ImGui.BulletText("Step 1: Setup the camera angle for the preview. Make sure your character is in the frame.");
                            ImGui.BulletText("Step 2: Verify that you can manually change outfits. If not, please check your settings and try again.");
                            ImGui.BulletText("Step 3: Choose your options on this screen.");
                            ImGui.BulletText("Step 4: Click on 'Generate Outfit Previews' to start the process.");
                            ImGui.BulletText("Step 5: Hide the game interface by pressing the 'Scroll Lock' key on your keyboard.");
                            ImGui.BulletText("During this process, the outfits your character is wearing will change automatically.");
                            ImGui.BulletText("The process is complete when outfits stop changing for more than 5 seconds.");
                            ImGui.BulletText("You can stop at any time by clicking on 'Stop Generating Outfit Previews'. (Remember to turn ui back on)");
                            ImGui.Text("Enjoy your new outfit previews!");


                            ImGui.Separator();

                            ImGui.Text("Options:");

                            if (ImGui.Checkbox("Replace existing outfit previews", ref _replaceExisting))
                            {

                            }
                            ImGui.BulletText("If checked, the preview generator will replace existing screenshots for outfits.");

                            if (ImGui.Checkbox("Crop to vertical", ref _cropToVertical))
                            {

                            }
                            ImGui.BulletText("If checked, the preview generator will keep window height but crop the image.");

                            bool isCaptureInProgress = this.Plugin.CaptureService != null && this.Plugin.CaptureService.captureInProgress;

                            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                if (!isCaptureInProgress)
                                {
                                    if (ImGui.Button("Generate Outfit Previews"))
                                    {
                                        if (this.Plugin.CaptureService == null)
                                        {
                                            this.Plugin.CaptureService = new OutfitCaptureService(this.Plugin, this.Plugin.OutfitHandler);
                                        }
                                        // Start the preview generation process here
                                        this.Plugin.CaptureService.CaptureOutfits(_replaceExisting, _cropToVertical);
                                    }
                                    ImGui.BulletText("Click this button to start the outfit preview generation process.");
                                }
                                else
                                {
                                    if (ImGui.Button("Stop Generating Outfit Previews"))
                                    {
                                        this.Plugin.CaptureService.HaltScreenshots = true;
                                    }
                                    ImGui.BulletText("Click this button to stop the outfit preview generation process.");
                                }
                            }
                            else
                            {
                                ImGui.Text("This feature is available only on Windows.");
                            }


                            ImGui.EndChild();
                            ImGui.EndTabItem();
                        }
                        else
                        {
                            ImGui.Text("This feature is available only on Windows.");
                        }
                    }
                }
              
            }

            ImGui.EndTabBar();

            float currentWindowHeight = ImGui.GetWindowHeight();
            float currentWindoqWidth = ImGui.GetWindowWidth();
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
            string playerName = DalamudService.ClientState.LocalPlayer.Name.TextValue;
            string outfitKey = "";

            if (this.Plugin.Configuration.LastOutfits.ContainsKey(playerName))
            {
                outfitKey = this.Plugin.Configuration.LastOutfits[playerName];
            }

            ImGui.Separator();

            OmgOutfit currentOutfit = new OmgOutfit();

            if (outfitKey != "" && this.Plugin.OutfitHandler.Outfits.ContainsKey(outfitKey))
            {
                currentOutfit = this.Plugin.OutfitHandler.Outfits[outfitKey];
            }
            else
            {
                // Set the player's outfit in LastOutfits to an empty outfit
                this.Plugin.Configuration.LastOutfits[playerName] = "";  // This assumes that "" corresponds to an empty outfit
                this.Plugin.Configuration.Save();
            }

            if (ImGui.Selectable($"Currently Equipped Outfit: {currentOutfit.DisplayName}"))
            {
                SelectOutfit(currentOutfit);
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
            _scaleName = _outfit.CustomizeScaleName;
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
            if (this.Plugin.Configuration.EnableCustomizeSupport)
            {
                ImGui.InputTextWithHint("Customize Scale", "Customize Scale name (optional)", ref _scaleName, 64, ImGuiInputTextFlags.EnterReturnsTrue);
            }
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
                             this.Plugin.OutfitHandler.SaveOutfits(this.Plugin.OutfitHandler.Outfits);
                
                            _newOutfitName = "";
                            _penumbraCollection = "";
                            _scaleName = "";
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
                _scaleName = "";
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
                        _errorText = "Penumbra collection not found.";
                        _showErrorPopup = true;
                        return;
                    }
                }
                else if (this.Plugin.Configuration.AutoCollection)
                {
                    try
                    {
                        if (this.Plugin.Configuration.PenumbraCollectionType != "Your Character")
                        {
                    
                            _penumbraCollection = GetCollectionForType.Subscriber(DalamudService.PluginInterface).Invoke(ApiCollectionType.Current);
                        }
                        else
                        {
                            _penumbraCollection = GetCollectionForType.Subscriber(DalamudService.PluginInterface).Invoke(ApiCollectionType.Yourself);
                        }

                  
                    }
                    catch(Exception ex)
                    {
                        _errorText = "Problem auto fetching penumbra collection.";
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
                    IsFavourite = _favourite,
                    CustomizeScaleName = _scaleName
                };

                if (this.Plugin.Configuration.AutoCollection)
                {
                    _outfit.CollectionName = _penumbraCollection;
                }

                    if (string.IsNullOrEmpty(_glamourerDesign) && this.Plugin.Configuration.AutoGlamourer)
                {
                    try
                    {
                       _outfit.GlamourerData = GlamourerIpc.Instance.GetAllCustomizationFromCharacterIpc(DalamudService.ClientState.LocalPlayer);
                    }
                    catch(Exception ex)
                    {
                        _errorText = "Problem auto fetching glamourer data.";
                        _showErrorPopup= true;

                        return;
                    }
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
                    IsSnapshot = true,
                    CustomizeScaleName = _scaleName
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
