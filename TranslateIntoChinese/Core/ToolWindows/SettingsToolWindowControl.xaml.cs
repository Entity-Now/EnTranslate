using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TranslateIntoChinese.Model;
using TranslateIntoChinese.Model.Enums;
using Edge_tts_sharp;
using System.Speech.Synthesis;
using MoqDictionary.Model.Enum;

namespace TranslateIntoChinese.Core
{
    public partial class SettingsToolWindowControl : UserControl, INotifyPropertyChanged
    {
        private bool _capturingSecondChord;
        private string _firstChord;
        private DateTime _chordUntil;

        public SettingsToolWindowControl()
        {
            InitializeComponent();
            LoadSelect(Config.Sound);
            ReloadQuickActions();
            this.DataContext = this;
            Loaded += SettingsToolWindowControl_Loaded;
        }

        private void SettingsToolWindowControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (ApiKeyBox != null && Config != null)
                ApiKeyBox.Password = Config.AiApiKey ?? string.Empty;
        }

        ObservableCollection<QuickActionItem> _quickActionItems = new ObservableCollection<QuickActionItem>();
        public ObservableCollection<QuickActionItem> QuickActionItems
        {
            get => _quickActionItems;
            set
            {
                _quickActionItems = value;
                OnPropertyChanged(nameof(QuickActionItems));
            }
        }

        private void ReloadQuickActions()
        {
            QuickActionItems = new ObservableCollection<QuickActionItem>(
                BuiltInQuickActions.Merge(Config?.QuickActions));
            if (Config != null)
                Config.QuickActions = QuickActionItems.ToList();
        }

        private void SyncQuickActions()
        {
            if (Config != null)
                Config.QuickActions = QuickActionItems.ToList();
        }

        public Config Config { get => Constants.Config; set { } }

        public ObservableCollection<SelectOption<TranslateType>> _translateItems = new ObservableCollection<SelectOption<TranslateType>>
        {
            new SelectOption<TranslateType>{ Name = "Bing", Value = TranslateType.Bing },
            new SelectOption<TranslateType>{ Name = "Google",Value = TranslateType.Google },
            new SelectOption<TranslateType>{ Name = "Deep",Value = TranslateType.Deep },
            new SelectOption<TranslateType>{ Name = "Yandex",Value = TranslateType.Yandex },
        };
        public ObservableCollection<SelectOption<TranslateType>> TranslateItems
        {
            get => _translateItems;
            set
            {
                _translateItems = value;
                OnPropertyChanged(nameof(TranslateItems));
            }
        }

        public ObservableCollection<SelectOption<AiProviderType>> _aiProviderItems = new ObservableCollection<SelectOption<AiProviderType>>
        {
            new SelectOption<AiProviderType>{ Name = "OpenAI-Compatible", Value = AiProviderType.OpenAICompatible },
            new SelectOption<AiProviderType>{ Name = "LM Studio", Value = AiProviderType.LmStudio },
            new SelectOption<AiProviderType>{ Name = "Ollama", Value = AiProviderType.Ollama },
            new SelectOption<AiProviderType>{ Name = "Anthropic-compatible", Value = AiProviderType.Anthropic },
        };
        public ObservableCollection<SelectOption<AiProviderType>> AiProviderItems
        {
            get => _aiProviderItems;
            set
            {
                _aiProviderItems = value;
                OnPropertyChanged(nameof(AiProviderItems));
            }
        }

        public ObservableCollection<SelectOption<SoundType>> _soundItems = new ObservableCollection<SelectOption<SoundType>>
        {
            new SelectOption<SoundType>{ Name = "默认", Value = SoundType.Default },
            new SelectOption<SoundType>{ Name = "Edge语音转文字",Value = SoundType.Edge },
            new SelectOption<SoundType>{ Name = "有道翻译接口",Value = SoundType.YouDao }
        };
        public ObservableCollection<SelectOption<SoundType>> SoundItems
        {
            get => _soundItems;
            set
            {
                _soundItems = value;
                OnPropertyChanged(nameof(SoundItems));
            }
        }
        ObservableCollection<string> soundList = new ObservableCollection<string>();
        public ObservableCollection<string> SoundList
        {
            get => soundList;
            set
            {
                soundList = value;
                OnPropertyChanged(nameof(SoundList));
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _ = ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    if (ApiKeyBox != null)
                        Config.AiApiKey = ApiKeyBox.Password ?? string.Empty;
                    SyncQuickActions();
                    Config.Save();
                    await HotkeyService.ApplyAsync(Config.TranslateHotkey);
                    await VS.StatusBar.ShowMessageAsync("保存成功！");
                }
                catch (Exception ex)
                {
                    ex.Log();
                }
            });
        }

        private void sound_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var combobox = sender as ComboBox;
            if (combobox?.SelectedValue is SoundType soundType)
            {
                Config.Sound = soundType;
                LoadSelect(Config.Sound);
            }
        }

        private void AiProvider_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Config == null) return;
            var provider = Config.AiProvider;
            if (AiProviderDefaults.IsKnownBaseUrl(Config.AiBaseUrl))
                Config.AiBaseUrl = AiProviderDefaults.GetBaseUrl(provider);
            if (AiProviderDefaults.IsKnownModel(Config.AiModel))
                Config.AiModel = AiProviderDefaults.GetModel(provider);
        }

        private void ApiKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (Config != null && sender is PasswordBox box)
                Config.AiApiKey = box.Password ?? string.Empty;
        }

        private void ResetPrompt_Click(object sender, RoutedEventArgs e)
        {
            Config.AiPrompt = AiProviderDefaults.DefaultPrompt;
        }

        private void TestAi_Click(object sender, RoutedEventArgs e)
        {
            if (ApiKeyBox != null)
                Config.AiApiKey = ApiKeyBox.Password ?? string.Empty;

            AiTestResult.Text = "正在测试连接…";
            _ = ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20)))
                    {
                        var result = await AiTranslateService.TranslateOneStrictAsync("hello", Config, cts.Token);
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        if (string.IsNullOrWhiteSpace(result))
                            AiTestResult.Text = "已发出请求，但未解析到译文。请检查模型名称、Base URL 和协议类型。";
                        else
                            AiTestResult.Text = "连接成功：" + result;
                    }
                }
                catch (Exception ex)
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    AiTestResult.Text = "连接失败：" + ex.Message;
                }
            });
        }

        private void AddQuickAction_Click(object sender, RoutedEventArgs e)
        {
            var title = (NewActionTitle?.Text ?? string.Empty).Trim();
            var prompt = (NewActionPrompt?.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                _ = VS.StatusBar.ShowMessageAsync("请填写自定义指令名称");
                return;
            }
            if (string.IsNullOrWhiteSpace(prompt))
            {
                _ = VS.StatusBar.ShowMessageAsync("请填写自定义指令内容");
                return;
            }
            if (QuickActionItems.Count >= 16)
            {
                _ = VS.StatusBar.ShowMessageAsync("指令数量已达上限（16）");
                return;
            }

            QuickActionItems.Add(new QuickActionItem
            {
                Id = "custom." + Guid.NewGuid().ToString("N"),
                Title = title,
                Prompt = prompt,
                Enabled = true,
                IsBuiltIn = false
            });
            if (NewActionTitle != null) NewActionTitle.Text = string.Empty;
            if (NewActionPrompt != null) NewActionPrompt.Text = string.Empty;
            SyncQuickActions();
        }

        private void DeleteQuickAction_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is QuickActionItem item && !item.IsBuiltIn)
            {
                QuickActionItems.Remove(item);
                SyncQuickActions();
            }
        }

        private void ResetActionPrompt_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is QuickActionItem item && item.IsBuiltIn)
            {
                BuiltInQuickActions.ResetPrompt(item);
                SyncQuickActions();
            }
        }

        private void HotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.IsRepeat) return;

            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key == Key.Escape)
            {
                _capturingSecondChord = false;
                return;
            }
            if (key == Key.Back || key == Key.Delete)
            {
                _capturingSecondChord = false;
                Config.TranslateHotkey = "Ctrl+Alt+T";
                return;
            }

            var chord = HotkeyService.Capture(e);
            if (string.IsNullOrEmpty(chord)) return;

            if (_capturingSecondChord && DateTime.UtcNow <= _chordUntil)
            {
                Config.TranslateHotkey = _firstChord + ", " + chord;
                _capturingSecondChord = false;
                return;
            }

            _firstChord = chord;
            _capturingSecondChord = true;
            _chordUntil = DateTime.UtcNow.AddMilliseconds(900);
            Config.TranslateHotkey = chord;
        }

        void LoadSelect(SoundType soundType)
        {
            SoundList.Clear();
            if (soundType == SoundItems[0].Value)
            {
                try
                {
                    using (var synth = new SpeechSynthesizer())
                    {
                        foreach (var item in synth.GetInstalledVoices())
                        {
                            SoundList.Add(item.VoiceInfo.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Log();
                    SoundList.Add("未检测到语音引擎/语音包");
                }
            }
            else if (soundType == SoundItems[1].Value)
            {
                foreach (var item in Edge_tts.GetVoice())
                {
                    SoundList.Add(item.Name);
                }
            }
            else if (soundType == SoundItems[2].Value)
            {
                SoundList.Add("1");
                SoundList.Add("2");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
