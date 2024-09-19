using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Interface.Windowing;
using OutfitManager.Windows;
using Dalamud.Game.Gui;
using Dalamud.Hooking;
using Newtonsoft.Json;
using System;
using Dalamud.Game.Text;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;
using System.Threading.Tasks;
using System.Linq;
using ImGuiScene;
using System.Runtime.CompilerServices;
using Dalamud.Game.ClientState.Conditions;
using System.Text;
using Penumbra.Api.Enums;
using System.Reflection.Metadata.Ecma335;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using OutfitManager.Handlers;
using OutfitManager.Models;
using OutfitManager.Services;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Internal;
using Penumbra.Api.Api;
using Dalamud.Interface.Textures.TextureWraps;

namespace OutfitManager
{
    public sealed class Plugin : IDalamudPlugin
    {
        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
        public string Name => "Outfit Manager";
        private const string CommandName = "/omg";
        public Configuration Configuration { get; init; }
        private bool isCommandsEnabled { get; set; }

        public bool PersistOutfit { get; set; }

        public IDalamudTextureWrap OutfitPreview;


        public readonly WindowSystem WindowSystem = new("OutfitManager");
        public bool PersistGearset { get; set; }

        public bool IgnorePersistCollection { get; set; }
        private bool _transition;

        private bool _previousTransition;
        public string OutfitName { get; set; }

        public bool SnapshotActive { get; set; }

        public OutfitHandler OutfitHandler { get; }

        public CommandHandler commandHandler;

        public OutfitCaptureService CaptureService;

        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }
        public string CurrentCharacter { get; set; }

        private bool FrameWorkUpdateTaskProcessing { get; set; }

        public DateTime LastFrameworkupdate { get; set; } = DateTime.Now;

        public Plugin()
        {


            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

            PluginInterface.Create<DalamudService>();

            this.OutfitHandler = new OutfitHandler(this);
            this.commandHandler = new CommandHandler(this);


            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = $"No arguments to bring up UI (Will take you to outfits if you have added any otherwise config){Environment.NewLine}" +
                $"config = bring up configuration window{Environment.NewLine}" +
                $"menu = bring up main menu window{Environment.NewLine}" +
                $"wear OUTFITNAME = wear saved outfit name{Environment.NewLine}" +
                $"random TAGNAME = wear random outfit with tag{Environment.NewLine}" +
                $"other = bring up remote outfit control.{Environment.NewLine}{Environment.NewLine}" +
                $"The outfit preview system requires images with the same name as your outfit to exist in the preview directory you set in config. (Experimental){Environment.NewLine}{Environment.NewLine}" +
                $"persist - will re-apply your outfit whenever it would be needed zone change, login.{Environment.NewLine}" +
                $"lockoutfit SECONDS (optional) - will lock you into your last worn outfit including gearset, if seconds are specified then for that amount of seconds." +
                $"reset - will clear your last equipped outfit and if present set your primary penumbra collection you set." +
                $"setcollectiontype Individual - will set your penumbra collection type away from the default 'Your Character' to Individual, if you want to go back to 'Your Character' just put Reset instead of individual." +
                $"snapshot - will have a temporary penumbra / glamourer combination that will re-apply if re-apply is enabled and be lost on wear outfit or restart." +
                $"clearsnapshot - will manually clear the snapshot."
            });


            if (this.Configuration.MyCharacter == null || string.IsNullOrEmpty(this.Configuration.MyCharacter.Name))
            {

                if (this.Configuration.Characters.Count > 0)
                {
                    var first = this.Configuration.Characters.First();
                    string key = first.Key;
                    OmgCharacter val = first.Value;

                    this.Configuration.MyCharacter = val;
                    this.Configuration.MyCharacter.Name = first.Key;
                    this.Configuration.Save();
                }
            }



            if (string.IsNullOrEmpty(this.Configuration.MyCharacter.FullName) && !string.IsNullOrEmpty(this.Configuration.MyCharacter.Name) && !string.IsNullOrEmpty(this.Configuration.MyCharacter.World))
            {
                this.Configuration.MyCharacter.FullName = $"{this.Configuration.MyCharacter.Name}@{this.Configuration.MyCharacter.World}";
                this.Configuration.Save();
            }

            if (this.Configuration.Persist)
            {
                this.Configuration.Persist = false;
                this.Configuration.Save();
            }

            WindowSystem.AddWindow(new ConfigWindow(this));
            WindowSystem.AddWindow(new MainWindow(this));
            WindowSystem.AddWindow(new AllowedCharacterWindow(this));
            WindowSystem.AddWindow(new OutfitListWindow(this));
            WindowSystem.AddWindow(new OutfitPreviewWindow(this));

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
            PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;


            DalamudService.ClientState.Login += OnLogin;
            DalamudService.Framework.Update += OnFrameworkUpdate;

            if (this.Configuration.OutfitName == null)
            {
                this.Configuration.OutfitName = "";
            }
        }

        private void OnFrameworkUpdate(IFramework framework)
        {
            if (!FrameWorkUpdateTaskProcessing)
            {
                FrameWorkUpdateTaskProcessing = true;
                LastFrameworkupdate = DateTime.Now;
            }


            FrameWorkUpdateTaskProcessing = false;
        }

        public void OnLogin()
        {

         

        }

        public void HideAllWindows()

        {
            this.ShowOrHideWindow("OutfitManager Outfit List Window", false);
            this.ShowOrHideWindow("OutfitManager Allowed Character Window", false);
            this.ShowOrHideWindow("OutfitManager", false);
            this.ShowOrHideWindow("Outfit Preview Window", false);
            this.ShowOrHideWindow("OutfitManager Outfit List Window", false);
            this.ShowOrHideWindow("OutfitManager Notice Window", false);
        }
        public void Dispose()
        {
            DalamudService.ClientState.Login -= OnLogin;
            DalamudService.Framework.Update -= OnFrameworkUpdate;
            this.WindowSystem.RemoveAllWindows();

        }

        private void OnCommand(string command, string args)
        {

            this.commandHandler.OnCommand(command, args);
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            this.ShowOrHideWindow("OutfitManager", true);
        }
        public void DrawAllowedUserUI()
        {
            this.ShowOrHideWindow("OutfitManager Allowed Character Window", true);
        }
        public void DrawOutfitListUI()
        {
            this.ShowOrHideWindow("OutfitManager Outfit List Window", true);
        }

        //public void RelayCommand(string command, int delay = 100)
        //{
        //    CommandManager.ProcessCommand(command);
        //}

        public async void RelayCommand(string command, int delay = 100)
        {
            await Task.Delay(delay).ConfigureAwait(false);
            await RunOnMainThread(() =>
            {
                CommandManager.ProcessCommand(command);
            });
        }
        public Task RunOnMainThread(System.Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            DalamudService.Framework.RunOnTick(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }


        static async Task DelayTask(int delay)
        {
            Task task = Task.Delay(delay);
            await task;
        }

        public void SetImagePreview(string name)
        {
            var imagePath = Path.Combine(this.Configuration.PreviewDirectory, $"{name}.png");

            if (this.OutfitPreview != null)
            {
                this.OutfitPreview.Dispose();
                this.OutfitPreview = null;
            }

            if (File.Exists(imagePath))
            {
                ShowOrHideWindow("Outfit Preview Window", true);

                try
                {
                    using (var stream = File.OpenRead(imagePath))
                    {
                        OutfitPreview = DalamudService.TextureProvider.CreateFromImageAsync(stream).GetAwaiter().GetResult();
                    }
                }
                catch (Exception ex)
                {
                    this.OutfitPreview = null;
                }
            }

            else
            {
                this.OutfitPreview = null;
            }
        }

        public void ShowOrHideWindow(string name, bool visible)
        {

            this.WindowSystem.Windows.FirstOrDefault(x => x.WindowName == name).IsOpen = visible;

            if (name == "Outfit Preview Window" && !visible)
            {
                this.OutfitPreview = null;
            }
        }

        public void ToggleConfigUI() => ConfigWindow.Toggle();
        public void ToggleMainUI() => MainWindow.Toggle();
    }
}

