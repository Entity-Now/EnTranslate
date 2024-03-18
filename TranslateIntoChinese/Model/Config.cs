using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TranslateIntoChinese.Utility;

namespace TranslateIntoChinese.Model
{
    public class Config
    {
        // Storage paths
        public static string StoragePath => SystemHelper.GetMyDocumentPath() + "\\TranslateIntoChinese\\";
        public static string SettingsPath => StoragePath + "TC_Config.json";
        public static string AudioPath => StoragePath + "audio";

        // Global configuration instance
        public static Config GlobalConfig { get; set; } = new Config();

        // Load configuration
        public static void Load()
        {
            try
            {
                Directory.CreateDirectory(StoragePath);
                Directory.CreateDirectory(AudioPath);

                if (!File.Exists(SettingsPath))
                    return;

                GlobalConfig = JsonHelper.ReadJson<Config>(SettingsPath);

                if (GlobalConfig.ThemeIsLight)
                    ColorHelper.LightColor();
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        // Save configuration
        public static async Task Save()
        {
            try
            {
                JsonHelper.WriteJson(GlobalConfig, SettingsPath);
                var infoBar = await VS.InfoBar.CreateAsync(new InfoBarModel($"Successfully saved to: {SettingsPath}"));
                await infoBar.TryShowInfoBarUIAsync();
                await Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    infoBar.Close();
                });
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        // Configuration properties
        public bool ThemeIsLight { get; set; } = false;
        public bool IsEdgeTTs { get; set; } = false;
        public string SelectedVoice { get; set; } = string.Empty;
        /// <summary>
        /// 声音大小，0-100
        /// </summary>
        public long Sound { get; set; } = 100;
        /// <summary>
        /// 语速，-10 to 10
        /// </summary>
        public long SpeechSpeed { get; set; } = 0;
    }

}
