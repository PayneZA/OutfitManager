using OutfitManager.Handlers;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OutfitManager.Services
{
    public class OutfitCaptureService
    {
        private const int SRCCOPY = 0x00CC0020;
        private readonly Plugin _plugin;
        private readonly OutfitHandler _outfitHandler;
        public bool replaceExisting = false;
        public bool HaltScreenshots { get; set; }

        public bool captureInProgress { get; set; } = false;

        public OutfitCaptureService(Plugin plugin, OutfitHandler outfitHandler)
        {
            this._plugin = plugin;
            this._outfitHandler = outfitHandler;
        }

        public async Task CaptureOutfits(bool replaceExisting = false, bool cropToVerical = false)
        {
         
            captureInProgress = true;
            var previewDirectory = this._plugin.Configuration.PreviewDirectory;
            var changeDelay = 5;// this._plugin.Configuration.ChangeDelay;
            var screenshotDelay = 1;// this._plugin.Configuration.ScreenshotDelay;
            var gameWindowHandle = GetForegroundWindow();

            if (!string.IsNullOrEmpty(previewDirectory) && Directory.Exists(previewDirectory))
            {
                DalamudService.Chat.Print($"Outfit preview generation beginning, it will wake {changeDelay} seconds after each outfit change and {screenshotDelay} seconds after each screenshot. Please hide your UI.");

                await Task.Delay(TimeSpan.FromSeconds(5));

                foreach (var outfit in this._outfitHandler.Outfits)
                {
                    if (HaltScreenshots)
                    {
                        DalamudService.Chat.Print($"Preview generation canceled.");
                        break;
                    }

                    try
                    {
                        if (string.IsNullOrEmpty(outfit.Value.GearSet))
                        {
                            var imagePath = Path.Combine(previewDirectory, $"{outfit.Key}.png");
                            if (!File.Exists(imagePath) || replaceExisting)
                            {
                                this._outfitHandler.EquipOutfit(outfit.Key);
                                await Task.Delay(TimeSpan.FromSeconds(changeDelay));

                                var screenshotTask = TakeScreenshotAsync(imagePath, gameWindowHandle,cropToVerical);
                                await Task.Delay(TimeSpan.FromSeconds(screenshotDelay));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DalamudService.Chat.Print($"Error generating preview for outfit {outfit.Key}: {ex.Message}");
                        continue;
                    }
                }

                if (!HaltScreenshots)
                {
                    DalamudService.Chat.Print($"Preview generation complete.");
                }

                HaltScreenshots = false;
            }
            else
            {
                DalamudService.Chat.Print($"Preview directory does not exist. Please set a valid directory in the configuration.");
            }

            this.captureInProgress = false;
        }


        public async Task TakeScreenshotAsync(string path, IntPtr gameWindowHandle, bool cropToVertical = false)
        {
            await Task.Run(() =>
            {
                try
                {
                    var gameWindowHandle = GetForegroundWindow();
                    RECT rect;

                    GetWindowRect(gameWindowHandle, out rect);
                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;

                    using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                    {
                        using (Graphics gfx = Graphics.FromImage(bmp))
                        {
                            var hdcDest = gfx.GetHdc();
                            var hdcSrc = GetDC(IntPtr.Zero);

                            BitBlt(hdcDest, 0, 0, width, height, hdcSrc, rect.Left, rect.Top, SRCCOPY);

                            gfx.ReleaseHdc(hdcDest);
                            ReleaseDC(IntPtr.Zero, hdcSrc);
                        }

                        // Crop image if cropToVertical is set
                        if (cropToVertical)
                        {
                            int newWidth = (9 * height) / 16;
                            int cropX = (width - newWidth) / 2;

                            using (Bitmap target = new Bitmap(newWidth, height))
                            {
                                using (Graphics g = Graphics.FromImage(target))
                                {
                                    g.DrawImage(bmp, new Rectangle(0, 0, target.Width, target.Height),
                                                new Rectangle(cropX, 0, newWidth, height),
                                                GraphicsUnit.Pixel);
                                    target.Save(path, ImageFormat.Png);
                                }
                            }
                        }
                        else
                        {
                            bmp.Save(path, ImageFormat.Png);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DalamudService.Chat.Print($"Error taking screenshot: {ex.Message}");
                }
            });
        }


        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSrc, int xSrc, int ySrc, uint operation);

        [DllImport("user32.dll")]
        private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
