using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace TranslateIntoChinese.Utility
{
    internal class JsonHelper
    {
        static public T JsonToObject<T>(string json) where T : class
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        static public string ObjectToJson<T>(T Source)
        {
            return JsonSerializer.Serialize<T>(Source);
        }
        static public T? ReadJson<T>(string path) where T : class
        {
            try
            {
                if (!File.Exists(path))
                {
                    return default;
                }
                var json = File.ReadAllText(path);
                return JsonToObject<T>(json);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            return default;
        }
        static public bool WriteJson<T>(T source, string path)
        {
            try
            {
                File.WriteAllText(path, ObjectToJson(source));
                return true;
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            return false;
        }
    }
}
