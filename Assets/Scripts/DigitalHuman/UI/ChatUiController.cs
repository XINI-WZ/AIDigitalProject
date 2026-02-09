using UnityEngine;
using UnityEngine.UIElements;
using DigitalHuman.Core;

namespace DigitalHuman.UI
{
    public class ChatUiController : MonoBehaviour
    {
        [SerializeField] private ChatManager _chatManager;
        
        private VisualElement _root;
        private Button _pttButton;
        private Label _statusLabel;
        private VisualElement _recordingRing;

        private bool _isRecordingToggle = false;

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            _root = uiDocument.rootVisualElement;

            _pttButton = _root.Q<Button>("PttButton");
            _statusLabel = _root.Q<Label>("StatusLabel");
            _recordingRing = _root.Q<VisualElement>("RecordingRing");

            // 放弃不稳定的 PointerDown，改用最可靠的 clicked 事件实现切换模式
            _pttButton.clicked += OnPttButtonClicked;
            
            Debug.Log("[ChatUiController] 已切换为【点击录音/点击停止】模式");
        }

        private void OnDisable()
        {
            if (_pttButton != null)
                _pttButton.clicked -= OnPttButtonClicked;
        }

        private void OnPttButtonClicked()
        {
            if (!_isRecordingToggle)
            {
                Debug.Log("[ChatUiController] 按钮点击：开始录音");
                StartRecordingFlow();
            }
            else
            {
                Debug.Log("[ChatUiController] 按钮点击：停止录音");
                StopRecordingFlow();
            }
        }

        private void StartRecordingFlow()
        {
            if (_chatManager == null) return;
            
            _isRecordingToggle = true;
            _chatManager.StartUserRecording();
            
            _pttButton.text = "点击 停止";
            _pttButton.AddToClassList("recording");
            _statusLabel.text = "正在录音...";
            _recordingRing.style.display = DisplayStyle.Flex;
        }

        private void StopRecordingFlow()
        {
            if (_chatManager == null) return;

            _isRecordingToggle = false;
            _chatManager.StopUserRecordingAndProcess();

            _pttButton.text = "点击 说话";
            _pttButton.RemoveFromClassList("recording");
            _statusLabel.text = "正在识别...";
            _recordingRing.style.display = DisplayStyle.None;
        }

        public void SetStatus(string status)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = status;
            }
        }
    }
}
