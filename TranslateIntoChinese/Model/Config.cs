using MoqDictionary.Model.Enum;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using TranslateIntoChinese.Model.Enums;

namespace TranslateIntoChinese.Model
{
    public class Config : INotifyPropertyChanged
    {
        // --- 属性字段 ---
        private bool _isDark = false;
        public bool IsDark
        {
            get => _isDark;
            set { _isDark = value; OnPropertyChanged(); }
        }

        private bool _translateDescribe = false;
        public bool TranslateDescribe
        {
            get => _translateDescribe;
            set { _translateDescribe = value; OnPropertyChanged(); }
        }

        private SoundType _sound = SoundType.Default;
        public SoundType Sound
        {
            get => _sound;
            set { _sound = value; OnPropertyChanged(); }
        }

        private string _soundName = string.Empty; // 建议默认为空字符串而非 null
        public string SoundName
        {
            get => _soundName;
            set { _soundName = value; OnPropertyChanged(); }
        }

        private bool _isRemoteTranslate = false;
        public bool IsRemoteTranslate
        {
            get => _isRemoteTranslate;
            set { _isRemoteTranslate = value; OnPropertyChanged(); }
        }

        private TranslateType _translateType = TranslateType.Bing;
        public TranslateType TranslateType
        {
            get => _translateType;
            set { _translateType = value; OnPropertyChanged(); }
        }

        // --- 事件处理 ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // --- 静态方法：加载与保存 ---

        /// <summary>
        /// 从本地文件加载配置
        /// </summary>
        public static Config Load()
        {
            try
            {
                // 确保目录存在
                if (!Directory.Exists(Constants.StoragePath)) Directory.CreateDirectory(Constants.StoragePath);
                if (!Directory.Exists(Constants.AudioPath)) Directory.CreateDirectory(Constants.AudioPath);

                if (!File.Exists(Constants.SettingsPath))
                {
                    return new Config();
                }

                string settingsJson = File.ReadAllText(Constants.SettingsPath);

                // 增加容错：如果文件为空或解析失败，返回默认对象
                var settings = JsonSerializer.Deserialize<Config>(settingsJson);
                return settings ?? new Config();
            }
            catch (Exception)
            {
                // 记录错误或简单返回默认配置，防止插件奔溃
                return new Config();
            }
        }

        /// <summary>
        /// 保存当前全局配置对象
        /// </summary>
        public static void Save()
        {
            try
            {
                // 确保目录存在（防止手动删除了目录）
                string dir = Path.GetDirectoryName(Constants.SettingsPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                // 使用带有格式化的 JSON，方便用户手动查看/修改配置
                var options = new JsonSerializerOptions { WriteIndented = true };
                string settingsJson = JsonSerializer.Serialize(Constants.Config, options);

                File.WriteAllText(Constants.SettingsPath, settingsJson);
            }
            catch (Exception ex)
            {
                // VSIX 插件中建议记录到输出窗口或日志文件
                System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
            }
        }
    }
}