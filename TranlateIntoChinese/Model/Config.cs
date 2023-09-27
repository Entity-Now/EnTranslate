using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TranlateIntoChinese.Utility;

namespace TranlateIntoChinese.Model
{
    public class Config
    {
        public static string SettingsPath { get; set; }
        public static Config GlobalConfig { get; set; } = new Config();
        public static void Load()
        {
            try
            {
                string documentPath = SystemHelper.GetMyDocumentPath() + "\\TranslateIntoChinese\\";
                if (!Directory.Exists(documentPath))
                {
                    Directory.CreateDirectory(documentPath);
                }

                SettingsPath = Path.Combine(documentPath, "TC_Config.json");

                if (!File.Exists(SettingsPath))
                {
                    return;
                }

                GlobalConfig = JsonHelper.ReadJson<Config>(SettingsPath);

                if (GlobalConfig.ThemeIsLight)
                {
                    ColorHelper.LightColor();
                }
            }
            catch (Exception ex)
            {
                ex.Log(); 
            }
        }

        public static async Task Save() 
        {
            try
            {
                JsonHelper.WriteJson(GlobalConfig, SettingsPath);
                var infoBar = await VS.InfoBar.CreateAsync(new InfoBarModel($"成功保存到:{SettingsPath}"));
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
        public bool ThemeIsLight { get; set; } = false;
        public string SelectedVoice { get; set; } = string.Empty;
        /// <summary>
        /// 声音大小，0,100
        /// </summary>
        public long Sound { get; set; } = 100;
        /// <summary>
        /// 语速， -10 - 10
        /// </summary>
        public long SpeechSpeed { get; set; } = 0;
    }
}
