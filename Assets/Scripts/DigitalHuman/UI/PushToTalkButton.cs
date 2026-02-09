using UnityEngine;
using UnityEngine.EventSystems;
using DigitalHuman.Core;

namespace DigitalHuman.UI
{
    public class PushToTalkButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private ChatManager _chatManager;
        [SerializeField] private GameObject _recordingIndicator;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_chatManager != null)
            {
                _chatManager.StartUserRecording();
                if (_recordingIndicator != null) _recordingIndicator.SetActive(true);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_chatManager != null)
            {
                _chatManager.StopUserRecordingAndProcess();
                if (_recordingIndicator != null) _recordingIndicator.SetActive(false);
            }
        }
    }
}
