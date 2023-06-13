using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

namespace OutfitManager.Windows
{
    public class MainWindow : Window, IDisposable
    {
        private Plugin _Plugin;
        private bool _characterExists = false;
        private bool _worldExists = false;

        public MainWindow(Plugin plugin) : base(
            "OutfitManager", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(575, 430),
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
            _characterExists = !string.IsNullOrEmpty(this._Plugin.Configuration.MyCharacter.Name);
            _worldExists = !string.IsNullOrEmpty(this._Plugin.Configuration.MyCharacter.World);
        }

        public override void Draw()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.9f, 0.9f, 0.9f, 1.0f));
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(10, 20));

            if (!string.IsNullOrEmpty(this._Plugin.Configuration.MyCharacter.Name) && !string.IsNullOrEmpty(this._Plugin.Configuration.MyCharacter.World))
            {
                // Outfit Manager
                ImGui.TextColored(new Vector4(0.26f, 0.59f, 0.98f, 1.0f), "Manage Outfits");
                if (ImGui.Button("Add/Edit/View Outfits"))
                {
                    this._Plugin.DrawOutfitListUI();
                }
                ImGui.SameLine();
                ImGui.TextDisabled("(Manage your character's outfits)");

                // Allow list
                ImGui.TextColored(new Vector4(0.26f, 0.59f, 0.98f, 1.0f), "Manage Allow List");
                if (ImGui.Button("Manage allow list"))
                {
                    this._Plugin.DrawAllowedUserUI();
                }
                ImGui.SameLine();
                ImGui.TextDisabled("(Manage who else can control your outfits)");

                // Allow list
                ImGui.TextColored(new Vector4(0.26f, 0.59f, 0.98f, 1.0f), "Configuration");
                if (ImGui.Button("Configuration"))
                {
                    this._Plugin.WindowSystem.GetWindow("Outfit Manager Configuration Window").IsOpen = true;
                }
                ImGui.SameLine();
                ImGui.TextDisabled("(Configure your settings.)");

            }
            else
            {
                // If criteria not met, the buttons will be disabled but still visible
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f);

                // Outfit Manager
                ImGui.TextColored(new Vector4(0.26f, 0.59f, 0.98f, 1.0f), "Manage Outfits");
                ImGui.Button("Add/Edit/View Outfits");
                ImGui.SameLine();
                ImGui.TextDisabled("(Manage your character's outfits)");

                // Allow list
                ImGui.TextColored(new Vector4(0.26f, 0.59f, 0.98f, 1.0f), "Manage Allow List");
                ImGui.Button("Manage allow list");
                ImGui.SameLine();
                ImGui.TextDisabled("(Manage who can control your outfits)");

                ImGui.PopStyleVar();

                ImGui.TextColored(new Vector4(1, 0, 0, 1), "Please configure your character name and world name first!");
                if (ImGui.Button("Go to Configuration"))
                {
                    this._Plugin.WindowSystem.GetWindow("Outfit Manager Configuration Window").IsOpen = true;
                }
            }

            ImGui.PopStyleColor();
            ImGui.PopStyleVar();
        }
    }
}
