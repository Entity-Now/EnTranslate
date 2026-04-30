using Edge_tts_sharp.Model;
using Edge_tts_sharp.Utils;
using Edge_tts_sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TranslateIntoChinese.Utility;
using TranslateIntoChinese.Model;
using System.IO;

namespace TranslateIntoChinese.Core
{
    public static class VoiceService
    {
        public static void Play(string text)
        {
            var config = Constants.Config;
            if (config.Sound == Model.Enums.SoundType.Edge)
            {
                string path = Path.Combine(Constants.AudioPath, $"{text}.mp3");
                if (File.Exists(path)) { Audio.PlayAudioAsync(path); return; }
                var voice = Edge_tts.GetVoice().FirstOrDefault(i => string.IsNullOrEmpty(config.SoundName) ? i.Name.Contains("zh-CN") : i.Name == config.SoundName);
                Edge_tts.PlayText(new PlayOption { Text = text, SavePath = path }, voice);
            }
            else if (config.Sound == Model.Enums.SoundType.YouDao)
            {
                Audio.PlayAudioFromUrlAsync($"https://dict.youdao.com/dictvoice?audio={HttpUtility.UrlEncode(text)}&type={config.SoundName}&le=cn");
            }
            else { SystemHelper.PlayVoice(text); }
        }
    }
}
