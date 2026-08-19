using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TranslateIntoChinese.Model
{
    public class QuickActionItem : INotifyPropertyChanged
    {
        private string _id;
        private string _title;
        private string _prompt;
        private bool _enabled = true;
        private bool _isBuiltIn;
        private bool _identifierOutput;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Title
        {
            get => _title;
            set { _title = value ?? string.Empty; OnPropertyChanged(); }
        }

        public string Prompt
        {
            get => _prompt;
            set { _prompt = value ?? string.Empty; OnPropertyChanged(); }
        }

        public bool Enabled
        {
            get => _enabled;
            set { _enabled = value; OnPropertyChanged(); }
        }

        public bool IsBuiltIn
        {
            get => _isBuiltIn;
            set { _isBuiltIn = value; OnPropertyChanged(); OnPropertyChanged(nameof(KindLabel)); }
        }

        /// <summary>输出应是单个标识符（变量名优化 / 中文改英文）。</summary>
        public bool IdentifierOutput
        {
            get => _identifierOutput;
            set { _identifierOutput = value; OnPropertyChanged(); }
        }

        public string KindLabel => IsBuiltIn ? "内置" : "自定义";

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
