using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniVRM10;

namespace DigitalHuman.Core
{
    /// <summary>
    /// VRM 数字人完整动画控制器（表情 + 身体）
    /// </summary>
    public class AvatarAnimationController : MonoBehaviour
    {
        [Header("VRM 实例引用")]
        [SerializeField] private Vrm10Instance _vrmInstance;
        
        [Header("动画组件")]
        [SerializeField] private Animator _animator;
        
        [Header("自动眨眼")]
        [SerializeField] private bool _enableAutoBlink = true;
        [SerializeField] private float _blinkIntervalMin = 2f;
        [SerializeField] private float _blinkIntervalMax = 5f;
        
        [Header("身体动画参数")]
        [SerializeField] private float _idleSwayAmount = 0.02f;
        [SerializeField] private float _idleSwaySpeed = 1f;
        
        // 表情权重动画
        private Dictionary<ExpressionKey, float> _targetExpressionWeights = new Dictionary<ExpressionKey, float>();
        private Dictionary<ExpressionKey, float> _currentExpressionWeights = new Dictionary<ExpressionKey, float>();
        private const float EXPRESSION_BLEND_SPEED = 5f;
        
        // 身体动画状态
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private float _idleTime;
        
        // 眨眼
        private float _nextBlinkTime;
        private bool _isBlinking;
        private float _blinkTimer;
        
        // 当前情绪状态
        private string _currentEmotion = "Neutral";

        void Awake()
        {
            if (_vrmInstance == null) _vrmInstance = GetComponent<Vrm10Instance>();
            if (_animator == null) _animator = GetComponent<Animator>();
            
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            
            // 初始化表情权重
            InitializeExpressions();
            
            // 设置初始眨眼时间
            _nextBlinkTime = Time.time + Random.Range(_blinkIntervalMin, _blinkIntervalMax);
        }

        void Update()
        {
            // 平滑过渡表情
            UpdateExpressionBlending();
            
            // 自动眨眼
            if (_enableAutoBlink) UpdateBlinking();
            
            // 待机身体动画
            UpdateIdleAnimation();
        }

        /// <summary>
        /// 设置情绪表情（平滑过渡）
        /// </summary>
        public void SetEmotion(string emotion)
        {
            _currentEmotion = emotion;
            
            // 重置所有表情权重为 0
            ClearAllExpressions();
            
            // 根据情绪设置目标权重（使用 VRM 标准表情预设）
            switch (emotion.ToLower())
            {
                case "happy":
                case "joy":
                    SetExpressionWeight(ExpressionKey.Happy, 1f);
                    break;
                    
                case "angry":
                case "mad":
                    SetExpressionWeight(ExpressionKey.Angry, 1f);
                    break;
                    
                case "sad":
                case "sorrow":
                    SetExpressionWeight(ExpressionKey.Sad, 1f);
                    break;
                    
                case "surprised":
                case "surprise":
                    SetExpressionWeight(ExpressionKey.Surprised, 1f);
                    break;
                    
                case "neutral":
                default:
                    SetExpressionWeight(ExpressionKey.Neutral, 0.5f);
                    break;
            }
            
            Debug.Log($"[AvatarAnimationController] 切换到情绪: {emotion}");
        }

        /// <summary>
        /// 开始说话（触发动画）
        /// </summary>
        public void OnStartSpeaking()
        {
            Debug.Log("[AvatarAnimationController] 触发说话动画开始！");
            
            // 触发身体说话动画
            SafeSetAnimatorBool("IsSpeaking", true);
            SafeSetAnimatorTrigger("StartTalking");
            
            // 说话时的微表情增强（可选）
            // 可以在这里添加说话时的特殊表情
        }

        /// <summary>
        /// 停止说话
        /// </summary>
        public void OnStopSpeaking()
        {
            Debug.Log("[AvatarAnimationController] 停止说话动画");
            SafeSetAnimatorBool("IsSpeaking", false);
        }

        /// <summary>
        /// 触发特定表情动作（如点头、摇头）
        /// </summary>
        public void TriggerGesture(string gestureName)
        {
            switch (gestureName.ToLower())
            {
                case "nod":
                    StartCoroutine(PlayNodAnimation());
                    break;
                case "shake":
                    StartCoroutine(PlayShakeAnimation());
                    break;
                case "tilt":
                    StartCoroutine(PlayTiltAnimation());
                    break;
                case "wave":
                    StartCoroutine(PlayWaveAnimation());
                    break;
                case "think":
                    StartCoroutine(PlayThinkAnimation());
                    break;
            }
        }

        #region 私有方法

        private void InitializeExpressions()
        {
            var allExpressions = new[]
            {
                ExpressionKey.Neutral, ExpressionKey.Happy, ExpressionKey.Angry,
                ExpressionKey.Sad, ExpressionKey.Surprised, ExpressionKey.Blink
            };
            
            foreach (var expr in allExpressions)
            {
                _targetExpressionWeights[expr] = 0f;
                _currentExpressionWeights[expr] = 0f;
            }
        }

        private void ClearAllExpressions()
        {
            // 创建键的副本，避免在遍历时修改集合
            var keys = new List<ExpressionKey>(_targetExpressionWeights.Keys);
            foreach (var key in keys)
            {
                _targetExpressionWeights[key] = 0f;
            }
        }

        private void SetExpressionWeight(ExpressionKey key, float weight)
        {
            _targetExpressionWeights[key] = Mathf.Clamp01(weight);
        }

        private void UpdateExpressionBlending()
        {
            if (_vrmInstance == null) return;
            
            // 平滑过渡所有表情权重
            foreach (var kvp in _targetExpressionWeights)
            {
                var key = kvp.Key;
                var targetWeight = kvp.Value;
                
                _currentExpressionWeights[key] = Mathf.MoveTowards(
                    _currentExpressionWeights[key],
                    targetWeight,
                    Time.deltaTime * EXPRESSION_BLEND_SPEED
                );
                
                // 应用到 VRM
                _vrmInstance.Runtime.Expression.SetWeight(key, _currentExpressionWeights[key]);
            }
        }

        private void UpdateBlinking()
        {
            if (_isBlinking)
            {
                _blinkTimer += Time.deltaTime;
                float blinkProgress = _blinkTimer / 0.15f; // 眨眼持续 0.15 秒
                
                if (blinkProgress >= 1f)
                {
                    _isBlinking = false;
                    _nextBlinkTime = Time.time + Random.Range(_blinkIntervalMin, _blinkIntervalMax);
                    SetExpressionWeight(ExpressionKey.Blink, 0f);
                }
                else
                {
                    // 闭眼到睁眼曲线
                    float weight = Mathf.Sin(blinkProgress * Mathf.PI);
                    SetExpressionWeight(ExpressionKey.Blink, weight);
                }
            }
            else if (Time.time >= _nextBlinkTime)
            {
                _isBlinking = true;
                _blinkTimer = 0f;
            }
        }

        private void UpdateIdleAnimation()
        {
            if (_animator == null) return;
            if (_animator.runtimeAnimatorController == null) return;
            
            _idleTime += Time.deltaTime;
            
            // 待机时轻微摇摆
            float swayX = Mathf.Sin(_idleTime * _idleSwaySpeed) * _idleSwayAmount;
            float swayZ = Mathf.Cos(_idleTime * _idleSwaySpeed * 0.7f) * _idleSwayAmount * 0.5f;
            
            // 安全地设置 Animator 参数（仅在参数存在时）
            SafeSetAnimatorFloat("IdleSwayX", swayX);
            SafeSetAnimatorFloat("IdleSwayY", swayZ);
            SafeSetAnimatorFloat("IdleTime", _idleTime);
        }
        
        private void SafeSetAnimatorFloat(string paramName, float value)
        {
            if (_animator == null) return;
            
            // 检查参数是否存在
            int paramHash = Animator.StringToHash(paramName);
            for (int i = 0; i < _animator.parameterCount; i++)
            {
                if (_animator.GetParameter(i).nameHash == paramHash)
                {
                    _animator.SetFloat(paramName, value);
                    return;
                }
            }
            // 参数不存在，静默跳过
        }
        
        private void SafeSetAnimatorBool(string paramName, bool value)
        {
            if (_animator == null)
            {
                Debug.LogWarning($"[AvatarAnimationController] Animator 为空，无法设置 {paramName}");
                return;
            }
            
            if (_animator.runtimeAnimatorController == null)
            {
                Debug.LogWarning($"[AvatarAnimationController] Animator Controller 未赋值，无法设置 {paramName}");
                return;
            }
            
            int paramHash = Animator.StringToHash(paramName);
            for (int i = 0; i < _animator.parameterCount; i++)
            {
                if (_animator.GetParameter(i).nameHash == paramHash)
                {
                    _animator.SetBool(paramName, value);
                    Debug.Log($"[AvatarAnimationController] 设置 Animator 参数 {paramName} = {value}");
                    return;
                }
            }
            
            Debug.LogWarning($"[AvatarAnimationController] Animator 中不存在参数 '{paramName}'，请在 Animator Controller 中添加该参数");
        }
        
        private void SafeSetAnimatorTrigger(string paramName)
        {
            if (_animator == null) return;
            
            int paramHash = Animator.StringToHash(paramName);
            for (int i = 0; i < _animator.parameterCount; i++)
            {
                if (_animator.GetParameter(i).nameHash == paramHash)
                {
                    _animator.SetTrigger(paramName);
                    return;
                }
            }
        }

        #endregion

        #region 手势动画协程

        private IEnumerator PlayNodAnimation()
        {
            float duration = 0.5f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float angle = Mathf.Sin(t * Mathf.PI * 2) * 10f; // 点头角度
                
                transform.rotation = _initialRotation * Quaternion.Euler(angle, 0, 0);
                yield return null;
            }
            
            transform.rotation = _initialRotation;
        }

        private IEnumerator PlayShakeAnimation()
        {
            float duration = 0.6f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float angle = Mathf.Sin(t * Mathf.PI * 3) * 15f; // 摇头角度
                
                transform.rotation = _initialRotation * Quaternion.Euler(0, angle, 0);
                yield return null;
            }
            
            transform.rotation = _initialRotation;
        }

        private IEnumerator PlayTiltAnimation()
        {
            float duration = 0.4f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float angle = Mathf.Sin(t * Mathf.PI) * 20f; // 歪头角度
                
                transform.rotation = _initialRotation * Quaternion.Euler(0, 0, angle);
                yield return null;
            }
            
            transform.rotation = _initialRotation;
        }

        private IEnumerator PlayWaveAnimation()
        {
            // 挥手动画 - 通过触发 Animator Trigger
            SafeSetAnimatorTrigger("Wave");
            yield return new WaitForSeconds(1.5f);
        }

        private IEnumerator PlayThinkAnimation()
        {
            // 思考姿势 - 使用惊讶表情来模拟"思考"状态
            SetExpressionWeight(ExpressionKey.Surprised, 0.3f);
            
            float duration = 2f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // 歪头的思考姿势
                float tilt = Mathf.Sin(t * Mathf.PI) * 15f;
                transform.rotation = _initialRotation * Quaternion.Euler(tilt * 0.5f, 0, tilt);
                
                yield return null;
            }
            
            transform.rotation = _initialRotation;
            SetExpressionWeight(ExpressionKey.Surprised, 0f);
        }

        #endregion
    }
}
