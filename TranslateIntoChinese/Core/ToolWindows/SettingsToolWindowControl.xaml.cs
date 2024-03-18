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

namespace TranslateIntoChinese.Core
{
    public partial class SettingsToolWindowControl : UserControl, INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsToolWindowControl()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        public bool IsLight
        {
            get
            {
                return Config.GlobalConfig.ThemeIsLight;
            }
            set
            {
                Config.GlobalConfig.ThemeIsLight = value;
                OnPropertyChanged();
            }
        }
        public long SoundValue
        {
            get
            {
                return Config.GlobalConfig.Sound;
            }
            set
            {
                Config.GlobalConfig.Sound = value;
                OnPropertyChanged();
            }
        }
        public long SpeechSpeed
        {
            get
            {
                return Config.GlobalConfig.SpeechSpeed;
            }
            set
            {
                Config.GlobalConfig.SpeechSpeed = value;
                OnPropertyChanged();
            }
        }
        public string VoiceSelected
        {
            get
            {
                return Config.GlobalConfig.SelectedVoice;
            }
            set
            {
                Config.GlobalConfig.SelectedVoice = value;
                OnPropertyChanged();
            }
        }
        public bool IsEdgeTTs
        {
            get => Config.GlobalConfig.IsEdgeTTs;
            set
            {
                Config.GlobalConfig.IsEdgeTTs = value;
                _getVoice();
                OnPropertyChanged();
            }
        }
        public ObservableCollection<string> _voiceList = new ObservableCollection<string>() {  };

        public ObservableCollection<string> VoiceList
        {
            get { return _voiceList; }
            set
            {
                _voiceList = value;
                OnPropertyChanged();
            }
        }

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            await Config.Save();
            await VS.StatusBar.ShowMessageAsync("保存成功！");
        }
       void _getVoice()
        {
            VoiceList.Clear();
            List<string> voiceList;
            if (IsEdgeTTs)
            {
                voiceList = Edge_tts_sharp.Edge_tts.GetVoice().Select(i => i.Name).ToList();
            }
            else
            {
                voiceList = SystemHelper.GetInstallVoice().Select(I => I.VoiceInfo.Name).ToList();
            }
            foreach (var item in voiceList)
            {
                VoiceList.Add(item);
            }
        }
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            _getVoice();
        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e)
        {
            //IsLoad = false;
            //VoiceList.Clear();
        }
    }
}
