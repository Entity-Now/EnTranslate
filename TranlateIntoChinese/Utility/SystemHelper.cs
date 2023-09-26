using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using TranlateIntoChinese.Model;

namespace TranlateIntoChinese.Utility
{
    internal class SystemHelper
    {
        public static void PlayVoice(string text)
        {
            // 创建SpeechSynthesizer对象
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                // 设置讲述人的声音
                //synth.SelectVoiceByHints(VoiceGender.NotSet, VoiceAge.NotSet);
                //synth.SetOutputToDefaultAudioDevice();
                var voice = Config.GlobalConfig.SelectedVoice;
                if (!string.IsNullOrEmpty(voice))
                {
                    synth.SelectVoice(voice);
                }
                // 设置音量（0到100）
                synth.Volume = (int)Config.GlobalConfig.Sound;

                // 设置语速（-10到10）
                synth.Rate = (int)Config.GlobalConfig.SpeechSpeed;

                // 调用Speak方法让讲述人说出文本
                synth.Speak(text);
            }
        }
        public static ReadOnlyCollection<InstalledVoice> GetInstallVoice()
        {
            // 创建SpeechSynthesizer对象
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                // 获取所有可用的语音
                return synth.GetInstalledVoices();
            }
        }
    }
}
