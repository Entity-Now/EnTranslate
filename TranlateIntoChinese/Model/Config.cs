using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TranlateIntoChinese.Utility;

namespace TranlateIntoChinese.Model
{
    public class Config
    {
        public static string Path = "_ETConfig.json";
        public static Config GlobalConfig = null;
        public static void Load()
        {
            try
            {
                GlobalConfig = JsonHelper.ReadJson<Config>(Config.Path) ?? new Config();
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
        public static void Save() 
        {
            try
            {
                JsonHelper.WriteJson(GlobalConfig, Config.Path);
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
