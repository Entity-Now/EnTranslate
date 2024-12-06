using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TranslateIntoChinese.Model.Enums;

namespace TranslateIntoChinese.Model
{
    public class Config : INotifyPropertyChanged
    {
        bool _isDark = false;
        public bool IsDark
        {
            get => _isDark;
            set
            {
                _isDark = value;
                //Save();
                OnPropertyChanged();
            }
        }
        bool _translateDescribe = false;
        public bool TranslateDescribe
        {
            get => _translateDescribe;
            set
            {
                _translateDescribe = value;
                //Save();
                OnPropertyChanged();
            }
        }
        SoundType _sound = SoundType.Default;
        public SoundType Sound
        {
            get => _sound;
            set
            {
                _sound = value;
                //Save();
                OnPropertyChanged();
            }
        }
        string _soundName = null;
        public string SoundName
        {
            get=> _soundName;
            set
            {
                _soundName = value;
                //Save();
                OnPropertyChanged();
            }
        }
        bool _isRemoteTranslate = false;
        public bool IsRemoteTranslate
        {
            get => _isRemoteTranslate;
            set
            {
                _isRemoteTranslate = value;
                OnPropertyChanged();
            }
        }
        TranslateType _translateType = TranslateType.Bing;
        public TranslateType TranslateType
        {
            get => _translateType;
            set
            {
                _translateType = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public static Config Load()
        {
            Directory.CreateDirectory(Constants.StoragePath);
            Directory.CreateDirectory(Constants.AudioPath);
            if (!File.Exists(Constants.SettingsPath))
            {
                return new Config();
            }
            var settingsJson = File.ReadAllText(Constants.SettingsPath);
            var settings = JsonSerializer.Deserialize<Config>(settingsJson);

            return settings;
        }

        public static void Save()
        {
            var settingsJson = JsonSerializer.Serialize(Constants.Config);
            File.WriteAllText(Constants.SettingsPath, settingsJson);
        }
    }
}
