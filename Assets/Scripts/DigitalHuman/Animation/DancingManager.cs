using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DigitalHuman.Core;

namespace DigitalHuman.Animation
{
    /// <summary>
    /// 舞蹈管理系统 - 控制数字人的舞蹈播放、停止和切换
    /// </summary>
    public class DancingManager : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private Animator _animator;
        [SerializeField] private AvatarAnimationController _animController;
        [SerializeField] private AvatarLipSyncController _lipSyncController;

        [Header("舞蹈动画")]
        [SerializeField] private List<DanceClip> _danceClips = new List<DanceClip>();

        [Header("设置")]
        [SerializeField] private float _transitionDuration = 0.3f;
        // [SerializeField] private bool _loopDance = true;
        [SerializeField] private bool _danceWithSinging = true;

        [Header("调试")]
        [SerializeField] private bool _showDebugUI = true;

        private int _currentDanceIndex = -1;
        private bool _isDancing = false;
        private bool _isSinging = false;

        public bool IsDancing => _isDancing;
        public bool IsSinging => _isSinging;

        void Awake()
        {
            if (_animator == null) _animator = GetComponent<Animator>();
            if (_animController == null) _animController = GetComponent<AvatarAnimationController>();
            if (_lipSyncController == null) _lipSyncController = GetComponent<AvatarLipSyncController>();
        }

        void OnGUI()
        {
            if (!_showDebugUI || !Application.isEditor) return;

            GUILayout.BeginArea(new Rect(10, 120, 250, 300));
            GUILayout.Label("=== 舞舞系统 ===");
            GUILayout.Label($"当前状态: {(IsDancing ? "跳舞中" : "空闲")}");
            GUILayout.Label($"当前舞蹈: {(_currentDanceIndex >= 0 ? _danceClips[_currentDanceIndex].name : "无")}");

            GUILayout.Space(10);

            if (GUILayout.Button("开始随机舞蹈"))
            {
                StartRandomDance();
            }

            if (GUILayout.Button("下一个舞蹈"))
            {
                NextDance();
            }

            if (GUILayout.Button("停止跳舞"))
            {
                StopDancing();
            }

            GUILayout.Space(10);

            GUILayout.Label($"唱歌模式: {(_isSinging ? "开启" : "关闭")}");
            if (GUILayout.Button("切换唱歌模式"))
            {
                ToggleSingingMode();
            }

            GUILayout.EndArea();
        }

        /// <summary>
        /// 开始随机舞蹈
        /// </summary>
        public void StartRandomDance()
        {
            if (_danceClips.Count == 0)
            {
                Debug.LogWarning("[DancingManager] 没有可用的舞蹈动画！");
                return;
            }

            int randomIndex = Random.Range(0, _danceClips.Count);
            StartDance(randomIndex);
        }

        /// <summary>
        /// 开始指定索引的舞蹈
        /// </summary>
        public void StartDance(int index)
        {
            if (index < 0 || index >= _danceClips.Count)
            {
                Debug.LogError($"[DancingManager] 舞蹈索引 {index} 超出范围！");
                return;
            }

            _currentDanceIndex = index;
            var dance = _danceClips[index];

            Debug.Log($"[DancingManager] 开始舞蹈: {dance.name}");

            // 播放舞蹈动画
            _animator.SetBool("IsDancing", true);
            _animator.CrossFadeInFixedTime(dance.animationName, _transitionDuration);

            // 设置舞蹈情绪
            if (_animController != null)
            {
                _animController.SetEmotion(dance.emotion);
            }

            _isDancing = true;
        }

        /// <summary>
        /// 停止舞蹈
        /// </summary>
        public void StopDancing()
        {
            if (!_isDancing) return;

            Debug.Log("[DancingManager] 停止舞蹈");

            _animator.SetBool("IsDancing", false);
            _animator.CrossFadeInFixedTime("Idle", _transitionDuration);

            if (_animController != null)
            {
                _animController.SetEmotion("Neutral");
            }

            _isDancing = false;
            _currentDanceIndex = -1;
        }

        /// <summary>
        /// 切换到下一个舞蹈
        /// </summary>
        public void NextDance()
        {
            if (_danceClips.Count == 0) return;

            int nextIndex = (_currentDanceIndex + 1) % _danceClips.Count;
            StartDance(nextIndex);
        }

        /// <summary>
        /// 切换唱歌模式
        /// </summary>
        public void ToggleSingingMode()
        {
            _isSinging = !_isSinging;
            Debug.Log($"[DancingManager] 唱歌模式: {_isSinging}");

            if (_isSinging && _danceWithSinging && !_isDancing)
            {
                // 唱歌模式下自动开始跳舞
                StartRandomDance();
            }
        }

        /// <summary>
        /// 开始唱歌（配合聊天系统）
        /// </summary>
        public void StartSinging(AudioClip songClip, string emotion = "Happy")
        {
            Debug.Log($"[DancingManager] 开始唱歌: {songClip.name}");

            _isSinging = true;

            // 设置情绪
            if (_animController != null)
            {
                _animController.SetEmotion(emotion);
            }

            // 如果开启了跳舞模式，开始跳舞
            if (_danceWithSinging && !_isDancing)
            {
                StartRandomDance();
            }

            // 配置口型同步
            if (_lipSyncController != null)
            {
                // TODO: 播放歌曲并配置口型
                Debug.Log("[DancingManager] 配置口型同步");
            }
        }

        /// <summary>
        /// 停止唱歌
        /// </summary>
        public void StopSinging()
        {
            Debug.Log("[DancingManager] 停止唱歌");
            _isSinging = false;

            // 如果之前是唱歌模式触发的跳舞，现在停止
            if (_danceWithSinging && _isDancing)
            {
                StopDancing();
            }

            if (_animController != null)
            {
                _animController.SetEmotion("Neutral");
            }
        }

        /// <summary>
        /// 根据情感切换舞蹈风格
        /// </summary>
        public void ChangeDanceByEmotion(string emotion)
        {
            if (!_isDancing) return;

            // 找到匹配情感的舞蹈
            int matchingIndex = _danceClips.FindIndex(d => d.emotion.ToLower() == emotion.ToLower());

            if (matchingIndex >= 0 && matchingIndex != _currentDanceIndex)
            {
                StartDance(matchingIndex);
                Debug.Log($"[DancingManager] 根据情感切换舞蹈: {emotion}");
            }
        }

        /// <summary>
        /// 获取当前舞蹈信息
        /// </summary>
        public DanceClip GetCurrentDance()
        {
            if (_currentDanceIndex >= 0 && _currentDanceIndex < _danceClips.Count)
            {
                return _danceClips[_currentDanceIndex];
            }
            return null;
        }
    }

    /// <summary>
    /// 舞蹈片段数据
    /// </summary>
    [System.Serializable]
    public class DanceClip
    {
        [Tooltip("舞蹈名称")]
        public string name;

        [Tooltip("Animator 中的动画状态名称")]
        public string animationName;

        [Tooltip("对应的情绪（用于切换表情）")]
        public string emotion;

        [Tooltip("舞蹈节奏类型（用于音乐同步）")]
        public DanceRhythm rhythm;

        [Tooltip("舞蹈速度倍率")]
        [Range(0.5f, 2f)]
        public float speedMultiplier = 1f;

        [Tooltip("是否支持循环")]
        public bool isLoop = true;
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
