using UnityEngine;

namespace DigitalHuman.Data
{
    /// <summary>
    /// 唱歌情感配置
    /// </summary>
    [CreateAssetMenu(fileName = "SingingEmotionsConfig", menuName = "Digital Human/Singing Emotions Config")]
    public class SingingEmotionsConfig : ScriptableObject
    {
        [Tooltip("所有情感配置")]
        public SingingEmotionData[] emotions;

        /// <summary>
        /// 根据情感名称获取配置
        /// </summary>
        public SingingEmotionData GetEmotion(string emotionName)
        {
            if (emotions == null || emotions.Length == 0) return null;

            foreach (var config in emotions)
            {
                if (config.emotionName.ToLower() == emotionName.ToLower())
                {
                    return config;
                }
            }

            // 未找到，返回第一个作为默认
            return emotions.Length > 0 ? emotions[0] : null;
        }
    }

    /// <summary>
    /// 单个情感配置
    /// </summary>
    [System.Serializable]
    public class SingingEmotionData
    {
        [Tooltip("情感名称")]
        public string emotionName;

        [Tooltip("基础表情")]
        public string baseEmotion;

        [Tooltip("辅助表情（基于歌曲进度切换）")]
        public string[] alternativeEmotions = new string[0];

        [Tooltip("可触发手势列表")]
        public string[] gestures = new string[] { "Nod", "Wave", "Tilt" };

        [Tooltip("手势触发概率倍率")]
        [Range(0.1f, 5f)]
        public float gestureProbabilityMultiplier = 1f;

        [Tooltip("表情切换间隔（秒）")]
        [Range(1f, 10f)]
        public float expressionChangeInterval = 3f;

        [Tooltip("歌曲风格描述（供 AI 使用）")]
        [TextArea(2, 3)]
        public string styleDescription;
    }
}
