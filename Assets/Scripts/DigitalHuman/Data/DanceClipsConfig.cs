using UnityEngine;
using System.Collections.Generic;

namespace DigitalHuman.Data
{
    /// <summary>
    /// 舞蹈片段数据
    /// </summary>
    [CreateAssetMenu(fileName = "DanceClipsConfig", menuName = "Digital Human/Dance Clips Config")]
    public class DanceClipsConfig : ScriptableObject
    {
        [Tooltip("所有可用的舞蹈片段")]
        public List<DanceClipData> danceClips = new List<DanceClipData>();

        /// <summary>
        /// 根据情感获取舞蹈
        /// </summary>
        public DanceClipData GetDanceByEmotion(string emotion)
        {
            return danceClips.Find(d => d.emotion.ToLower() == emotion.ToLower());
        }

        /// <summary>
        /// 获取随机舞蹈
        /// </summary>
        public DanceClipData GetRandomDance()
        {
            if (danceClips.Count == 0) return null;
            return danceClips[Random.Range(0, danceClips.Count)];
        }

        /// <summary>
        /// 获取指定节奏类型的舞蹈
        /// </summary>
        public DanceClipData GetDanceByRhythm(DanceRhythm rhythm)
        {
            var matches = danceClips.FindAll(d => d.rhythm == rhythm);
            if (matches.Count == 0) return null;
            return matches[Random.Range(0, matches.Count)];
        }
    }

    /// <summary>
    /// 舞蹈片段数据
    /// </summary>
    [System.Serializable]
    public class DanceClipData
    {
        [Tooltip("舞蹈名称")]
        public string name;

        [Tooltip("Animator 中的动画状态名称")]
        public string animationStateName;

        [Tooltip("对应的情绪（用于切换表情）")]
        public string emotion;

        [Tooltip("舞蹈节奏类型")]
        public DanceRhythm rhythm;

        [Tooltip("舞蹈速度倍率")]
        [Range(0.5f, 2f)]
        public float speedMultiplier = 1f;

        [Tooltip("是否支持循环")]
        public bool isLoop = true;

        [Tooltip("舞蹈描述（供 AI 使用）")]
        [TextArea(2, 3)]
        public string description;
    }

    /// <summary>
    /// 舞蹈节奏类型
    /// </summary>
    public enum DanceRhythm
    {
        Slow,       // 慢节奏（抒情歌曲）
        Medium,     // 中节奏（流行歌曲）
        Fast,       // 快节奏（舞曲）
        Freestyle   // 自由节奏
    }
}
