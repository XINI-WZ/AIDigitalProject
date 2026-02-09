using System;

namespace DigitalHuman.Data
{
    [Serializable]
    public class TtsRequest
    {
        public string text;
        public string voice = "zh-CN-XiaoxiaoNeural"; // 晓晓，最经典的甜美女声
        public string format = "mp3";
    }
}
