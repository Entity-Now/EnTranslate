using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EnTranslate.utility
{
    public static class utlis
    {
        /// <summary>
        /// 获取程序集的嵌入资源
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string ReadEmbeddedResource(string resourceName)
        {
            // 获取当前程序集
            Assembly assembly = Assembly.GetExecutingAssembly();

            // 获取资源流
            using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                {
                    throw new ArgumentException($"Resource '{resourceName}' not found.");
                }

                // 使用资源流进行操作
                using (StreamReader reader = new StreamReader(resourceStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// 防抖函数，传入延迟时间，返回一个函数，该函数接受一个回调函数，当调用该函数时，会在延迟时间后执行回调函数
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static Action<Action> Debounce(int delay)
        {
            Timer timing = null;
            return (callback) =>
            {
                timing?.Dispose();
                timing = new Timer((_) =>
                {
                    // do something
                    callback();
                }, null, delay, Timeout.Infinite);
            };
        }
    }
}
