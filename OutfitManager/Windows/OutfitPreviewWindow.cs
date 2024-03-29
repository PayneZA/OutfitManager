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
    public class OutfitPreviewWindow : Window, IDisposable
    {
        private Plugin Plugin;
        public OutfitPreviewWindow(Plugin plugin) : base(
            "Outfit Preview Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            this.Plugin = plugin;

            if (this.Plugin.OutfitPreview != null)
            {
                var aspectRatio = (float)this.Plugin.OutfitPreview.Width / (float)this.Plugin.OutfitPreview.Height;

                this.SizeConstraints = new WindowSizeConstraints
                {
                    MinimumSize = new Vector2(600, (int)(600 / aspectRatio)),
                    MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
                };
            }
        }

        public void Dispose()
        {

        }

        public override void Draw()
        {
            if (this.Plugin.OutfitPreview != null)
            {
                var windowSize = ImGui.GetWindowSize();
                var aspectRatio = (float)this.Plugin.OutfitPreview.Width / (float)this.Plugin.OutfitPreview.Height;
                var imageSize = new Vector2(windowSize.X, windowSize.X / aspectRatio);

                if (imageSize.Y > windowSize.Y)
                {
                    imageSize = new Vector2(windowSize.Y * aspectRatio, windowSize.Y);
                }

                var imagePosition = new Vector2((windowSize.X - imageSize.X) * 0.5f, (windowSize.Y - imageSize.Y) * 0.5f);

                ImGui.SetCursorPos(imagePosition);
                ImGui.Image(this.Plugin.OutfitPreview.ImGuiHandle, imageSize);
            }
        }
    }
}
