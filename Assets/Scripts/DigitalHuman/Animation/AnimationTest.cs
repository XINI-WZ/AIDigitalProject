using UnityEngine;

namespace DigitalHuman.Core
{
    /// <summary>
    /// 动画测试工具 - 用于快速测试表情和身体动画
    /// </summary>
    public class AnimationTest : MonoBehaviour
    {
        [SerializeField] private AvatarAnimationController _animationController;
        
        [Header("测试设置")]
        [SerializeField] private KeyCode _testHappyKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode _testSadKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode _testAngryKey = KeyCode.Alpha3;
        [SerializeField] private KeyCode _testSurprisedKey = KeyCode.Alpha4;
        [SerializeField] private KeyCode _testNodKey = KeyCode.N;
        [SerializeField] private KeyCode _testShakeKey = KeyCode.M;
        [SerializeField] private KeyCode _testWaveKey = KeyCode.W;
        [SerializeField] private KeyCode _testTalkKey = KeyCode.Space;

        void Update()
        {
            if (_animationController == null) return;

            // 测试不同情绪
            if (Input.GetKeyDown(_testHappyKey))
            {
                Debug.Log("[AnimationTest] 测试开心表情");
                _animationController.SetEmotion("Happy");
                _animationController.TriggerGesture("nod");
            }
            
            if (Input.GetKeyDown(_testSadKey))
            {
                Debug.Log("[AnimationTest] 测试难过表情");
                _animationController.SetEmotion("Sad");
                _animationController.TriggerGesture("shake");
            }
            
            if (Input.GetKeyDown(_testAngryKey))
            {
                Debug.Log("[AnimationTest] 测试生气表情");
                _animationController.SetEmotion("Angry");
            }
            
            if (Input.GetKeyDown(_testSurprisedKey))
            {
                Debug.Log("[AnimationTest] 测试惊讶表情");
                _animationController.SetEmotion("Surprised");
                _animationController.TriggerGesture("tilt");
            }

            // 测试手势
            if (Input.GetKeyDown(_testNodKey))
            {
                Debug.Log("[AnimationTest] 测试点头");
                _animationController.TriggerGesture("nod");
            }
            
            if (Input.GetKeyDown(_testShakeKey))
            {
                Debug.Log("[AnimationTest] 测试摇头");
                _animationController.TriggerGesture("shake");
            }
            
            if (Input.GetKeyDown(_testWaveKey))
            {
                Debug.Log("[AnimationTest] 测试挥手");
                _animationController.TriggerGesture("wave");
            }

            // 测试说话动画
            if (Input.GetKeyDown(_testTalkKey))
            {
                Debug.Log("[AnimationTest] 测试说话动画");
                _animationController.OnStartSpeaking();
            }
            
            if (Input.GetKeyUp(_testTalkKey))
            {
                _animationController.OnStopSpeaking();
            }
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== 动画测试 ===");
            GUILayout.Label("按数字键测试情绪:");
            GUILayout.Label($"1 - 开心 (Happy)");
            GUILayout.Label($"2 - 难过 (Sad)");
            GUILayout.Label($"3 - 生气 (Angry)");
            GUILayout.Label($"4 - 惊讶 (Surprised)");
            GUILayout.Label("");
            GUILayout.Label("按字母键测试动作:");
            GUILayout.Label($"N - 点头");
            GUILayout.Label($"M - 摇头");
            GUILayout.Label($"W - 挥手");
            GUILayout.Label($"Space - 说话 (按住)");
            GUILayout.EndArea();
        }
    }
}
