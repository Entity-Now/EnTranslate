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
    public static class Constants
    {
        // Storage paths
        public static string StoragePath => Path.Combine(SystemHelper.GetMyDocumentPath(), "TranslateIntoChinese");
        public static string SettingsPath => Path.Combine(StoragePath, "TC_Config.json");
        public static string AudioPath => Path.Combine(StoragePath , "audio");

        // Global configuration instance
        private static Lazy<Config> _globalConfig = new Lazy<Config>(() => 
        {
            return Config.Load();
        });
        public static Config Config => _globalConfig.Value;

    }

}
