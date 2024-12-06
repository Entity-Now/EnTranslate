using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using TranslateIntoChinese.Model;

namespace TranslateIntoChinese.Utility
{
    internal class SystemHelper
    {
        public static void JumpBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
        public static string GetCurrentPath()
        {
            // 获取当前执行程序的 Assembly
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // 获取当前 DLL 文件的位置
            string assemblyLocation = currentAssembly.Location;

            // 获取 DLL 所在目录
            string assemblyDirectory = System.IO.Path.GetDirectoryName(assemblyLocation);
            return assemblyDirectory;
        }
        public static string GetMyDocumentPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
        public static void PlayVoice(string text)
        {
            // 创建SpeechSynthesizer对象
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                try
                {
                    // 设置讲述人的声音
                    //synth.SelectVoiceByHints(VoiceGender.NotSet, VoiceAge.NotSet);
                    var voice = Constants.Config.SoundName;
                    if (!string.IsNullOrEmpty(voice))
                    {
                        synth.SelectVoice(voice);
                    }
                    else
                    {
                        synth.SetOutputToDefaultAudioDevice();
                    }
                    // 设置音量（0到100）
                    //synth.Volume = (int)Constrant.GlobalConfig.Sound;

                    // 设置语速（-10到10）
                    //synth.Rate = (int)Constrant.GlobalConfig.SpeechSpeed;

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
