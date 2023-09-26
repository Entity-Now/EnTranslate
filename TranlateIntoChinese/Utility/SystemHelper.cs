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
                try
                {
                    // 设置讲述人的声音
                    //synth.SelectVoiceByHints(VoiceGender.NotSet, VoiceAge.NotSet);
                    var voice = Config.GlobalConfig.SelectedVoice;
                    if (voice != null && !string.IsNullOrEmpty(voice))
                    {
                        synth.SelectVoice(voice);
                    }
                    else
                    {
                        synth.SetOutputToDefaultAudioDevice();
                    }
                    // 设置音量（0到100）
                    synth.Volume = (int)Config.GlobalConfig.Sound;

                    // 设置语速（-10到10）
                    synth.Rate = (int)Config.GlobalConfig.SpeechSpeed;

                    // 调用Speak方法让讲述人说出文本
                    synth.Speak(text);
                }
                catch (Exception ex)
                {
                    ex.Log();
                }
            }
        }
        public static ReadOnlyCollection<InstalledVoice> GetInstallVoice()
        {
            // 创建SpeechSynthesizer对象
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                try
                {
                    // 获取所有可用的语音
                    return synth.GetInstalledVoices();
                }
                catch (Exception ex)
                {
                    ex.Log();
                    return null;
                }
            }
        }
    }
}
