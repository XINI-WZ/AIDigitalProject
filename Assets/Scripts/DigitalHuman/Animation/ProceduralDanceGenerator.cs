using UnityEngine;
using System.Collections.Generic;

namespace DigitalHuman.Animation
{
    /// <summary>
    /// 程序化舞蹈生成器 - 实时通过数学函数和算法生成舞蹈动作
    /// 不依赖预设动画，完全由代码驱动骨骼
    /// </summary>
    public class ProceduralDanceGenerator : MonoBehaviour
    {
        [Header("骨骼引用")]
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _hips;
        [SerializeField] private Transform _spine;
        [SerializeField] private Transform _chest;
        [SerializeField] private Transform _neck;
        [SerializeField] private Transform _head;

        [SerializeField] private Transform _leftShoulder;
        [SerializeField] private Transform _leftUpperArm;
        [SerializeField] private Transform _leftLowerArm;
        [SerializeField] private Transform _leftHand;

        [SerializeField] private Transform _rightShoulder;
        [SerializeField] private Transform _rightUpperArm;
        [SerializeField] private Transform _rightLowerArm;
        [SerializeField] private Transform _rightHand;

        [SerializeField] private Transform _leftUpperLeg;
        [SerializeField] private Transform _leftLowerLeg;
        [SerializeField] private Transform _leftFoot;

        [SerializeField] private Transform _rightUpperLeg;
        [SerializeField] private Transform _rightLowerLeg;
        [SerializeField] private Transform _rightFoot;

        [Header("舞蹈风格")]
        [SerializeField] private DanceStyle _currentStyle = DanceStyle.HipHop;
        [SerializeField] private float _danceSpeed = 1f;
        [SerializeField] private float _danceIntensity = 1f;

        [Header("节奏设置")]
        [SerializeField] private float _bpm = 120f;
        // [SerializeField] private bool _useAudioRhythm = false;
        // [SerializeField] private AudioSource _audioSource;

        [Header("IK 设置")]
        [SerializeField] private bool _enableIK = true;
        // [SerializeField] private float _ikWeight = 0.5f;
        // [SerializeField] private Vector3 _ikTargetOffset = new Vector3(0, 0, 0.5f);

        [Header("调试")]
        // [SerializeField] private bool _showBones = true;
        [SerializeField] private bool _showGizmos = true;

        private bool _isDancing = false;
        private float _danceTime = 0f;
        private float _beatTime = 0f;
        private int _currentBeat = 0;

        // 缓存初始变换
        private Dictionary<Transform, TransformData> _initialTransforms = new Dictionary<Transform, TransformData>();

        // 当前舞蹈动作
        private DanceMove _currentMove;

        void Awake()
        {
            if (_animator == null) _animator = GetComponent<Animator>();
            CacheInitialTransforms();
            InitializeDanceMoves();
        }

        void Start()
        {
            // 禁用默认动画
            if (_animator != null)
            {
                _animator.enabled = false;
            }
        }

        void Update()
        {
            if (!_isDancing) return;

            _danceTime += Time.deltaTime * _danceSpeed;
            _beatTime += Time.deltaTime * (_bpm / 60f);

            // 检测节拍
            int newBeat = Mathf.FloorToInt(_beatTime);
            if (newBeat > _currentBeat)
            {
                _currentBeat = newBeat;
                OnBeat(newBeat);
            }

            // 生成舞蹈动作
            GenerateDanceMove();

            // 应用到骨骼
            ApplyDanceMove();

            // 应用 IK
            if (_enableIK)
            {
                ApplyIK();
            }
        }

        /// <summary>
        /// 开始跳舞
        /// </summary>
        public void StartDancing(DanceStyle style = DanceStyle.HipHop)
        {
            _currentStyle = style;
            _isDancing = true;
            _danceTime = 0f;
            _beatTime = 0f;
            _currentBeat = 0;

            Debug.Log($"[ProceduralDanceGenerator] 开始程序化舞蹈: {style}");
        }

        /// <summary>
        /// 停止跳舞
        /// </summary>
        public void StopDancing()
        {
            _isDancing = false;

            // 恢复初始姿态
            ResetToInitialPose();

            Debug.Log("[ProceduralDanceGenerator] 停止舞蹈");
        }

        /// <summary>
        /// 设置舞蹈风格
        /// </summary>
        public void SetDanceStyle(DanceStyle style)
        {
            _currentStyle = style;
            Debug.Log($"[ProceduralDanceGenerator] 切换舞蹈风格: {style}");
        }

        /// <summary>
        /// 设置节奏（BPM）
        /// </summary>
        public void SetBPM(float bpm)
        {
            _bpm = Mathf.Clamp(bpm, 60f, 200f);
            Debug.Log($"[ProceduralDanceGenerator] 设置 BPM: {_bpm}");
        }

        /// <summary>
        /// 获取当前节奏（BPM）
        /// </summary>
        public float GetBPM()
        {
            return _bpm;
        }

        /// <summary>
        /// 设置舞蹈强度
        /// </summary>
        public void SetDanceIntensity(float intensity)
        {
            _danceIntensity = Mathf.Clamp01(intensity);
        }

        // ==================== 骨骼控制 ====================

        /// <summary>
        /// 缓存初始变换
        /// </summary>
        private void CacheInitialTransforms()
        {
            _initialTransforms.Clear();

            var bones = new Transform[]
            {
                _hips, _spine, _chest, _neck, _head,
                _leftShoulder, _leftUpperArm, _leftLowerArm, _leftHand,
                _rightShoulder, _rightUpperArm, _rightLowerArm, _rightHand,
                _leftUpperLeg, _leftLowerLeg, _leftFoot,
                _rightUpperLeg, _rightLowerLeg, _rightFoot
            };

            foreach (var bone in bones)
            {
                if (bone != null)
                {
                    _initialTransforms[bone] = new TransformData
                    {
                        localPosition = bone.localPosition,
                        localRotation = bone.localRotation,
                        localScale = bone.localScale
                    };
                }
            }
        }

        /// <summary>
        /// 恢复初始姿态
        /// </summary>
        private void ResetToInitialPose()
        {
            foreach (var kvp in _initialTransforms)
            {
                Transform bone = kvp.Key;
                TransformData data = kvp.Value;

                bone.localPosition = data.localPosition;
                bone.localRotation = data.localRotation;
                bone.localScale = data.localScale;
            }
        }

        /// <summary>
        /// 节拍事件
        /// </summary>
        private void OnBeat(int beat)
        {
            // 在节拍时触发特殊效果
            // 例如：重拍、高潮
        }

        /// <summary>
        /// 生成舞蹈动作
        /// </summary>
        private void GenerateDanceMove()
        {
            _currentMove = new DanceMove();

            float beatProgress = _beatTime % 1f; // 0-1 之间的节拍进度

            switch (_currentStyle)
            {
                case DanceStyle.HipHop:
                    GenerateHipHopMove(beatProgress);
                    break;
                case DanceStyle.Pop:
                    GeneratePopMove(beatProgress);
                    break;
                case DanceStyle.Ballet:
                    GenerateBalletMove(beatProgress);
                    break;
                case DanceStyle.Robot:
                    GenerateRobotMove(beatProgress);
                    break;
                case DanceStyle.Wave:
                    GenerateWaveMove(beatProgress);
                    break;
            }
        }

        /// <summary>
        /// 应用舞蹈动作到骨骼
        /// </summary>
        private void ApplyDanceMove()
        {
            if (_hips != null)
            {
                _hips.localPosition = _initialTransforms[_hips].localPosition + _currentMove.hipsPosition;
                _hips.localRotation = _initialTransforms[_hips].localRotation * Quaternion.Euler(_currentMove.hipsRotation);
            }

            if (_spine != null)
            {
                _spine.localRotation = _initialTransforms[_spine].localRotation * Quaternion.Euler(_currentMove.spineRotation);
            }

            if (_chest != null)
            {
                _chest.localRotation = _initialTransforms[_chest].localRotation * Quaternion.Euler(_currentMove.chestRotation);
            }

            if (_head != null)
            {
                _head.localRotation = _initialTransforms[_head].localRotation * Quaternion.Euler(_currentMove.headRotation);
            }

            // 手臂
            if (_leftUpperArm != null)
            {
                _leftUpperArm.localRotation = _initialTransforms[_leftUpperArm].localRotation * Quaternion.Euler(_currentMove.leftArmRotation);
            }

            if (_rightUpperArm != null)
            {
                _rightUpperArm.localRotation = _initialTransforms[_rightUpperArm].localRotation * Quaternion.Euler(_currentMove.rightArmRotation);
            }

            if (_leftLowerArm != null)
            {
                _leftLowerArm.localRotation = _initialTransforms[_leftLowerArm].localRotation * Quaternion.Euler(_currentMove.leftForearmRotation);
            }

            if (_rightLowerArm != null)
            {
                _rightLowerArm.localRotation = _initialTransforms[_rightLowerArm].localRotation * Quaternion.Euler(_currentMove.rightForearmRotation);
            }

            // 腿部
            if (_leftUpperLeg != null)
            {
                _leftUpperLeg.localRotation = _initialTransforms[_leftUpperLeg].localRotation * Quaternion.Euler(_currentMove.leftLegRotation);
            }

            if (_rightUpperLeg != null)
            {
                _rightUpperLeg.localRotation = _initialTransforms[_rightUpperLeg].localRotation * Quaternion.Euler(_currentMove.rightLegRotation);
            }

            if (_leftLowerLeg != null)
            {
                _leftLowerLeg.localRotation = _initialTransforms[_leftLowerLeg].localRotation * Quaternion.Euler(_currentMove.leftCalfRotation);
            }

            if (_rightLowerLeg != null)
            {
                _rightLowerLeg.localRotation = _initialTransforms[_rightLowerLeg].localRotation * Quaternion.Euler(_currentMove.rightCalfRotation);
            }
        }

        // ==================== 舞蹈风格生成 ====================

        /// <summary>
        /// 生成 HipHop 风格舞蹈
        /// </summary>
        private void GenerateHipHopMove(float beatProgress)
        {
            // 身体上下律动
            _currentMove.hipsPosition.y = Mathf.Sin(beatProgress * Mathf.PI * 2) * 0.05f * _danceIntensity;
            _currentMove.hipsPosition.x = Mathf.Cos(beatProgress * Mathf.PI * 2) * 0.02f * _danceIntensity;

            // 躯干旋转
            _currentMove.spineRotation = new Vector3(
                Mathf.Sin(beatProgress * Mathf.PI * 2) * 5f,
                Mathf.Sin(beatProgress * Mathf.PI * 4) * 3f,
                Mathf.Cos(beatProgress * Mathf.PI * 2) * 2f
            ) * _danceIntensity;

            // 胸部扭动
            _currentMove.chestRotation = new Vector3(
                Mathf.Sin(beatProgress * Mathf.PI * 2 + Mathf.PI / 4) * 8f,
                0,
                Mathf.Cos(beatProgress * Mathf.PI * 2 + Mathf.PI / 4) * 5f
            ) * _danceIntensity;

            // 头部跟随节奏点头
            _currentMove.headRotation = new Vector3(
                Mathf.Sin(beatProgress * Mathf.PI * 2) * 10f,
                0,
                Mathf.Cos(beatProgress * Mathf.PI * 2) * 5f
            ) * _danceIntensity;

            // 手臂摆动
            _currentMove.leftArmRotation = new Vector3(
                -20f + Mathf.Sin(beatProgress * Mathf.PI * 2) * 30f,
                Mathf.Cos(beatProgress * Mathf.PI * 2) * 20f,
                0
            ) * _danceIntensity;

            _currentMove.rightArmRotation = new Vector3(
                -20f + Mathf.Cos(beatProgress * Mathf.PI * 2) * 30f,
                Mathf.Sin(beatProgress * Mathf.PI * 2) * 20f,
                0
            ) * _danceIntensity;

            // 前臂
            _currentMove.leftForearmRotation = new Vector3(
                -30f + Mathf.Sin(beatProgress * Mathf.PI * 2 + Mathf.PI / 2) * 40f,
                0,
                0
            ) * _danceIntensity;

            _currentMove.rightForearmRotation = new Vector3(
                -30f + Mathf.Cos(beatProgress * Mathf.PI * 2 + Mathf.PI / 2) * 40f,
                0,
                0
            ) * _danceIntensity;

            // 腿部律动
            _currentMove.leftLegRotation = new Vector3(
                Mathf.Sin(beatProgress * Mathf.PI * 2) * 15f,
                0,
                -5f
            ) * _danceIntensity;

            _currentMove.rightLegRotation = new Vector3(
                Mathf.Cos(beatProgress * Mathf.PI * 2) * 15f,
                0,
                5f
            ) * _danceIntensity;

            // 小腿
            _currentMove.leftCalfRotation = new Vector3(
                10f + Mathf.Sin(beatProgress * Mathf.PI * 2) * 10f,
                0,
                0
            ) * _danceIntensity;

            _currentMove.rightCalfRotation = new Vector3(
                10f + Mathf.Cos(beatProgress * Mathf.PI * 2) * 10f,
                0,
                0
            ) * _danceIntensity;
        }

        /// <summary>
        /// 生成 Pop 风格舞蹈
        /// </summary>
        private void GeneratePopMove(float beatProgress)
        {
            // 更轻快的节奏
            _currentMove.hipsPosition.y = Mathf.Sin(beatProgress * Mathf.PI * 2) * 0.03f * _danceIntensity;

            // 躯干挺直
            _currentMove.spineRotation = new Vector3(
                Mathf.Sin(beatProgress * Mathf.PI * 2) * 3f,
                0,
                0
            ) * _danceIntensity;

            // 胸部轻微扭动
            _currentMove.chestRotation = new Vector3(
                0,
                Mathf.Sin(beatProgress * Mathf.PI * 2) * 5f,
                0
            ) * _danceIntensity;

            // 头部轻微晃动
            _currentMove.headRotation = new Vector3(
                0,
                Mathf.Cos(beatProgress * Mathf.PI * 2) * 5f,
                Mathf.Sin(beatProgress * Mathf.PI * 2) * 3f
            ) * _danceIntensity;

            // 手臂优雅摆动
            _currentMove.leftArmRotation = new Vector3(
                -10f + Mathf.Sin(beatProgress * Mathf.PI * 2) * 20f,
                10f + Mathf.Cos(beatProgress * Mathf.PI * 2) * 15f,
                5f
            ) * _danceIntensity;

            _currentMove.rightArmRotation = new Vector3(
                -10f + Mathf.Cos(beatProgress * Mathf.PI * 2) * 20f,
                -10f + Mathf.Sin(beatProgress * Mathf.PI * 2) * 15f,
                -5f
            ) * _danceIntensity;

            // 前臂
            _currentMove.leftForearmRotation = new Vector3(
                -20f + Mathf.Sin(beatProgress * Mathf.PI * 2) * 30f,
                0,
                0
            ) * _danceIntensity;

            _currentMove.rightForearmRotation = new Vector3(
                -20f + Mathf.Cos(beatProgress * Mathf.PI * 2) * 30f,
                0,
                0
            ) * _danceIntensity;

            // 腿部轻盈移动
            _currentMove.leftLegRotation = new Vector3(
                Mathf.Sin(beatProgress * Mathf.PI * 2) * 10f,
                0,
                -3f
            ) * _danceIntensity;

            _currentMove.rightLegRotation = new Vector3(
                Mathf.Cos(beatProgress * Mathf.PI * 2) * 10f,
                0,
                3f
            ) * _danceIntensity;

            // 小腿
            _currentMove.leftCalfRotation = new Vector3(
                5f + Mathf.Sin(beatProgress * Mathf.PI * 2) * 5f,
                0,
                0
            ) * _danceIntensity;

            _currentMove.rightCalfRotation = new Vector3(
                5f + Mathf.Cos(beatProgress * Mathf.PI * 2) * 5f,
                0,
                0
            ) * _danceIntensity;
        }

        /// <summary>
        /// 生成 Ballet 风格舞蹈
        /// </summary>
        private void GenerateBalletMove(float beatProgress)
        {
            // 优雅的身体移动
            _currentMove.hipsPosition.y = Mathf.Sin(beatProgress * Mathf.PI * 2) * 0.04f * _danceIntensity;

            // 躯干挺直
            _currentMove.spineRotation = new Vector3(
                -2f + Mathf.Sin(beatProgress * Mathf.PI * 2) * 2f,
                0,
                0
            ) * _danceIntensity;

            // 胸部优雅
            _currentMove.chestRotation = new Vector3(
                2f + Mathf.Sin(beatProgress * Mathf.PI * 2) * 3f,
                0,
                0
            ) * _danceIntensity;

            // 头部优雅抬起
            _currentMove.headRotation = new Vector3(
                -5f + Mathf.Cos(beatProgress * Mathf.PI * 2) * 3f,
                0,
                Mathf.Sin(beatProgress * Mathf.PI * 2) * 2f
            ) * _danceIntensity;

            // 手臂优雅伸展
            _currentMove.leftArmRotation = new Vector3(
                -30f + Mathf.Sin(beatProgress * Mathf.PI * 2) * 15f,
                30f + Mathf.Cos(beatProgress * Mathf.PI * 2) * 20f,
                10f
            ) * _danceIntensity;

            _currentMove.rightArmRotation = new Vector3(
                -30f + Mathf.Cos(beatProgress * Mathf.PI * 2) * 15f,
                -30f + Mathf.Sin(beatProgress * Mathf.PI * 2) * 20f,
                -10f
            ) * _danceIntensity;

            // 前臂优雅
            _currentMove.leftForearmRotation = new Vector3(
                -40f + Mathf.Sin(beatProgress * Mathf.PI * 2) * 20f,
                0,
                0
            ) * _danceIntensity;

            _currentMove.rightForearmRotation = new Vector3(
                -40f + Mathf.Cos(beatProgress * Mathf.PI * 2) * 20f,
                0,
                0
            ) * _danceIntensity;

            // 腿部优雅移动
            _currentMove.leftLegRotation = new Vector3(
                5f + Mathf.Sin(beatProgress * Mathf.PI * 2) * 10f,
                0,
                -5f
            ) * _danceIntensity;

            _currentMove.rightLegRotation = new Vector3(
                5f + Mathf.Cos(beatProgress * Mathf.PI * 2) * 10f,
                0,
                5f
            ) * _danceIntensity;

            // 小腿
            _currentMove.leftCalfRotation = new Vector3(
                15f + Mathf.Sin(beatProgress * Mathf.PI * 2) * 5f,
                0,
                0
            ) * _danceIntensity;

            _currentMove.rightCalfRotation = new Vector3(
                15f + Mathf.Cos(beatProgress * Mathf.PI * 2) * 5f,
                0,
                0
            ) * _danceIntensity;
        }

        /// <summary>
        /// 生成 Robot 风格舞蹈
        /// </summary>
        private void GenerateRobotMove(float beatProgress)
        {
            // 机械式身体移动
            _currentMove.hipsPosition.y = Mathf.FloorToInt(Mathf.Sin(beatProgress * Mathf.PI * 2) * 2) * 0.03f * _danceIntensity;

            // 躯干僵硬
            _currentMove.spineRotation = new Vector3(
                Mathf.Floor(Mathf.Sin(beatProgress * Mathf.PI * 2)) * 5f,
                Mathf.Floor(Mathf.Cos(beatProgress * Mathf.PI * 2)) * 5f,
                0
            ) * _danceIntensity;

            // 胸部机械
            _currentMove.chestRotation = new Vector3(
                Mathf.Floor(Mathf.Cos(beatProgress * Mathf.PI * 2)) * 8f,
                0,
                0
            ) * _danceIntensity;

            // 头部机械
            _currentMove.headRotation = new Vector3(
                Mathf.Floor(Mathf.Sin(beatProgress * Mathf.PI * 2)) * 10f,
                0,
                0
            ) * _danceIntensity;

            // 手臂机械
            _currentMove.leftArmRotation = new Vector3(
                Mathf.Floor(Mathf.Sin(beatProgress * Mathf.PI * 2)) * 30f,
                0,
                0
            ) * _danceIntensity;

            _currentMove.rightArmRotation = new Vector3(
                Mathf.Floor(Mathf.Cos(beatProgress * Mathf.PI * 2)) * 30f,
                0,
                0
            ) * _danceIntensity;

            // 前臂机械
            _currentMove.leftForearmRotation = new Vector3(
                Mathf.Floor(Mathf.Cos(beatProgress * Mathf.PI * 2)) * 40f,
                0,
                0
            ) * _danceIntensity;

            _currentMove.rightForearmRotation = new Vector3(
                Mathf.Floor(Mathf.Sin(beatProgress * Mathf.PI * 2)) * 40f,
                0,
                0
            ) * _danceIntensity;

            // 腿部机械
            _currentMove.leftLegRotation = new Vector3(
                Mathf.Floor(Mathf.Sin(beatProgress * Mathf.PI * 2)) * 15f,
                0,
                -5f
            ) * _danceIntensity;

            _currentMove.rightLegRotation = new Vector3(
                Mathf.Floor(Mathf.Cos(beatProgress * Mathf.PI * 2)) * 15f,
                0,
                5f
            ) * _danceIntensity;

            // 小腿
            _currentMove.leftCalfRotation = new Vector3(
                Mathf.Floor(Mathf.Sin(beatProgress * Mathf.PI * 2)) * 20f,
                0,
                0
            ) * _danceIntensity;

            _currentMove.rightCalfRotation = new Vector3(
                Mathf.Floor(Mathf.Cos(beatProgress * Mathf.PI * 2)) * 20f,
                0,
                0
            ) * _danceIntensity;
        }

        /// <summary>
        /// 生成 Wave 风格舞蹈
        /// </summary>
        private void GenerateWaveMove(float beatProgress)
        {
            // 身体波浪式移动
            _currentMove.hipsPosition.y = Mathf.Sin(beatProgress * Mathf.PI * 4) * 0.04f * _danceIntensity;
            _currentMove.hipsPosition.x = Mathf.Sin(beatProgress * Mathf.PI * 2) * 0.03f * _danceIntensity;

            // 躯干波浪
            _currentMove.spineRotation = new Vector3(
                Mathf.Sin(beatProgress * Mathf.PI * 2 + 0.5f) * 5f,
                Mathf.Cos(beatProgress * Mathf.PI * 2) * 5f,
                0
            ) * _danceIntensity;

            // 胸部波浪
            _currentMove.chestRotation = new Vector3(
                Mathf.Sin(beatProgress * Mathf.PI * 2 + 1f) * 8f,
                0,
                Mathf.Cos(beatProgress * Mathf.PI * 2) * 5f
            ) * _danceIntensity;

            // 头部波浪
            _currentMove.headRotation = new Vector3(
                Mathf.Sin(beatProgress * Mathf.PI * 2 + 1.5f) * 10f,
                0,
                0
            ) * _danceIntensity;

            // 手臂波浪
            _currentMove.leftArmRotation = new Vector3(
                -15f + Mathf.Sin(beatProgress * Mathf.PI * 4) * 25f,
                Mathf.Cos(beatProgress * Mathf.PI * 4) * 20f,
                0
            ) * _danceIntensity;

            _currentMove.rightArmRotation = new Vector3(
                -15f + Mathf.Cos(beatProgress * Mathf.PI * 4) * 25f,
                Mathf.Sin(beatProgress * Mathf.PI * 4) * 20f,
                0
            ) * _danceIntensity;

            // 前臂波浪
            _currentMove.leftForearmRotation = new Vector3(
                -25f + Mathf.Cos(beatProgress * Mathf.PI * 4 + 1f) * 35f,
                0,
                0
            ) * _danceIntensity;

            _currentMove.rightForearmRotation = new Vector3(
                -25f + Mathf.Sin(beatProgress * Mathf.PI * 4 + 1f) * 35f,
                0,
                0
            ) * _danceIntensity;

            // 腿部波浪
            _currentMove.leftLegRotation = new Vector3(
                Mathf.Sin(beatProgress * Mathf.PI * 2) * 12f,
                0,
                -4f
            ) * _danceIntensity;

            _currentMove.rightLegRotation = new Vector3(
                Mathf.Cos(beatProgress * Mathf.PI * 2) * 12f,
                0,
                4f
            ) * _danceIntensity;

            // 小腿
            _currentMove.leftCalfRotation = new Vector3(
                8f + Mathf.Sin(beatProgress * Mathf.PI * 2 + 0.5f) * 8f,
                0,
                0
            ) * _danceIntensity;

            _currentMove.rightCalfRotation = new Vector3(
                8f + Mathf.Cos(beatProgress * Mathf.PI * 2 + 0.5f) * 8f,
                0,
                0
            ) * _danceIntensity;
        }

        /// <summary>
        /// 应用 IK（反向运动学）
        /// </summary>
        private void ApplyIK()
        {
            // 基础 IK 实现
            // 可以使用 Unity 的 IK 系统
            if (_animator != null)
            {
                // 设置 IK 目标
                // _animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget);
                // _animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTargetRotation);
                // _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, _ikWeight);
            }
        }

        /// <summary>
        /// 初始化舞蹈动作
        /// </summary>
        private void InitializeDanceMoves()
        {
            _currentMove = new DanceMove();
        }

        // ==================== 调试 ====================

        void OnDrawGizmos()
        {
            if (!_showGizmos) return;

            // 绘制骨骼连线
            if (_hips != null && _spine != null)
            {
                Gizmos.DrawLine(_hips.position, _spine.position);
            }
            if (_spine != null && _chest != null)
            {
                Gizmos.DrawLine(_spine.position, _chest.position);
            }
            if (_chest != null && _neck != null)
            {
                Gizmos.DrawLine(_chest.position, _neck.position);
            }
            if (_neck != null && _head != null)
            {
                Gizmos.DrawLine(_neck.position, _head.position);
            }
        }

        void OnGUI()
        {
            if (!Application.isEditor) return;

            GUILayout.BeginArea(new Rect(10, 10, 280, 350));
            GUILayout.Label("=== 程序化舞蹈系统 ===");
            GUILayout.Label($"舞蹈状态: {(_isDancing ? "跳舞中" : "停止")}");
            GUILayout.Label($"舞蹈风格: {_currentStyle}");
            GUILayout.Label($"BPM: {_bpm}");
            GUILayout.Label($"舞蹈强度: {_danceIntensity:F2}");

            GUILayout.Space(10);

            // 舞蹈风格
            GUILayout.Label("舞蹈风格:");
            if (GUILayout.Button("Hip Hop"))
                SetDanceStyle(DanceStyle.HipHop);
            if (GUILayout.Button("Pop"))
                SetDanceStyle(DanceStyle.Pop);
            if (GUILayout.Button("Ballet"))
                SetDanceStyle(DanceStyle.Ballet);
            if (GUILayout.Button("Robot"))
                SetDanceStyle(DanceStyle.Robot);
            if (GUILayout.Button("Wave"))
                SetDanceStyle(DanceStyle.Wave);

            GUILayout.Space(10);

            // 控制按钮
            if (GUILayout.Button(_isDancing ? "停止跳舞" : "开始跳舞"))
            {
                if (_isDancing)
                    StopDancing();
                else
                    StartDancing(_currentStyle);
            }

            GUILayout.Space(10);

            // BPM 滑块
            GUILayout.Label($"BPM: {_bpm:F0}");
            _bpm = GUILayout.HorizontalSlider(_bpm, 60f, 200f);

            // 强度滑块
            GUILayout.Label($"强度: {_danceIntensity:F2}");
            _danceIntensity = GUILayout.HorizontalSlider(_danceIntensity, 0f, 1f);

            GUILayout.EndArea();
        }
    }

    // ==================== 数据结构 ====================

    /// <summary>
    /// 舞蹈动作数据
    /// </summary>
    [System.Serializable]
    public struct DanceMove
    {
        // 身体
        public Vector3 hipsPosition;
        public Vector3 hipsRotation;

        // 躯干
        public Vector3 spineRotation;
        public Vector3 chestRotation;

        // 头部
        public Vector3 headRotation;

        // 手臂
        public Vector3 leftArmRotation;
        public Vector3 rightArmRotation;
        public Vector3 leftForearmRotation;
        public Vector3 rightForearmRotation;

        // 腿部
        public Vector3 leftLegRotation;
        public Vector3 rightLegRotation;
        public Vector3 leftCalfRotation;
        public Vector3 rightCalfRotation;
    }

    /// <summary>
    /// 变换数据
    /// </summary>
    [System.Serializable]
    public struct TransformData
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
    }

    /// <summary>
    /// 舞蹈风格
    /// </summary>
    public enum DanceStyle
    {
        HipHop,     // 嘻哈
        Pop,        // 流行
        Ballet,     // 芭蕾
        Robot,      // 机器人
        Wave        // 波浪
    }
}
