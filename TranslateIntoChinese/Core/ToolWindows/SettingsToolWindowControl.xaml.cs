using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using TranslateIntoChinese.Model;
using TranslateIntoChinese.Utility;
using Edge_tts_sharp;
using System.Speech.Synthesis;
using TranslateIntoChinese.Model.Enums;
using MoqDictionary.Model.Enum;

namespace TranslateIntoChinese.Core
{
    public partial class SettingsToolWindowControl : UserControl , INotifyPropertyChanged
    {

        public SettingsToolWindowControl()
        {
            InitializeComponent();
            LoadSelect(Config.Sound);
            this.DataContext = this;
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
        public ObservableCollection<SelectOption<SoundType>> _soundItems = new ObservableCollection<SelectOption<SoundType>>
        {
            new SelectOption<SoundType>{ Name = "默认", Value = Model.Enums.SoundType.Default },
            new SelectOption<SoundType>{ Name = "Edge语音转文字",Value = Model.Enums.SoundType.Edge },
            new SelectOption<SoundType>{ Name = "有道翻译接口",Value = Model.Enums.SoundType.YouDao }
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



        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            Config.Save();
            await VS.StatusBar.ShowMessageAsync("保存成功！");
        }


        private void sound_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var combobox = (sender as ComboBox);
            Config.Sound = (SoundType)combobox.SelectedValue;

            LoadSelect(Config.Sound);
        }

        void LoadSelect(SoundType soundType)
        {
            SoundList.Clear();
            if (soundType == SoundItems[0].Value)
            {
                using (var synth = new SpeechSynthesizer())
                {
                    foreach (var item in synth.GetInstalledVoices())
                    {
                        SoundList.Add(item.VoiceInfo.Name);
                    }
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
