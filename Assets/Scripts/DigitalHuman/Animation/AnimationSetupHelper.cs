using UnityEngine;

namespace DigitalHuman.Core
{
    /// <summary>
    /// 动画配置助手 - 自动配置基础 Animator
    /// </summary>
    public class AnimationSetupHelper : MonoBehaviour
    {
        [Header("VRM 模型")]
        [SerializeField] private Animator _animator;
        
        [Header("动画片段")]
        [SerializeField] private AnimationClip _idleClip;
        [SerializeField] private AnimationClip _talkingClip;
        
        [ContextMenu("自动配置动画")]
        private void AutoSetup()
        {
            if (_animator == null)
            {
                Debug.LogError("请先赋值 Animator 组件！");
                return;
            }
            
            // 确保有 Runtime Animator Controller
            if (_animator.runtimeAnimatorController == null)
            {
                // 创建新的 Animator Override Controller
                var overrideController = new AnimatorOverrideController();
                
                // 如果没有基础 Controller，创建一个简单的
                Debug.Log("[AnimationSetupHelper] 请先在 Animator 组件中创建一个 Animator Controller");
                Debug.Log("步骤：1. 在 Project 窗口创建 Animator Controller");
                Debug.Log("      2. 拖到 VRM 模型的 Animator 组件中");
                return;
            }
            
            Debug.Log("[AnimationSetupHelper] 检测到 Animator Controller 已配置");
            Debug.Log($"当前控制器: {_animator.runtimeAnimatorController.name}");
            
            // 检查参数
            Debug.Log($"参数数量: {_animator.parameterCount}");
            for (int i = 0; i < _animator.parameterCount; i++)
            {
                var param = _animator.GetParameter(i);
                Debug.Log($"  - {param.name} ({param.type})");
            }
        }
        
        [ContextMenu("测试说话动画")]
        private void TestTalking()
        {
            if (_animator != null)
            {
                _animator.SetBool("IsSpeaking", true);
                Debug.Log("[AnimationSetupHelper] 触发说话动画");
                
                // 3秒后停止
                Invoke(nameof(StopTalking), 3f);
            }
        }
        
        private void StopTalking()
        {
            if (_animator != null)
            {
                _animator.SetBool("IsSpeaking", false);
                Debug.Log("[AnimationSetupHelper] 停止说话动画");
            }
        }
    }
}
