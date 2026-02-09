using UnityEngine;
using uLipSync;

namespace DigitalHuman.Core
{
    [RequireComponent(typeof(uLipSync.uLipSync))]
    public class AvatarLipSyncController : MonoBehaviour
    {
        private uLipSync.uLipSync _lipSync;
        private uLipSyncBlendShape _lipSyncBlendShape;
        private UniVRM10.Vrm10Instance _vrmInstance; // For VRM 1.0
        // Or using a more generic approach if VRM version is uncertain

        void Awake()
        {
            _lipSync = GetComponent<uLipSync.uLipSync>();
            _lipSyncBlendShape = GetComponent<uLipSyncBlendShape>();
            _vrmInstance = GetComponent<UniVRM10.Vrm10Instance>();

            if (_lipSync == null)
            {
                Debug.LogError("[AvatarLipSyncController] uLipSync component missing on " + gameObject.name);
            }
        }

        public void SetExpression(string emotion)
        {
            if (_vrmInstance == null) _vrmInstance = GetComponent<UniVRM10.Vrm10Instance>();
            if (_vrmInstance == null) return;

            UniVRM10.ExpressionPreset preset = emotion.ToLower() switch
            {
                "happy" => UniVRM10.ExpressionPreset.happy,
                "angry" => UniVRM10.ExpressionPreset.angry,
                "sad" => UniVRM10.ExpressionPreset.sad,
                "surprised" => UniVRM10.ExpressionPreset.surprised,
                _ => UniVRM10.ExpressionPreset.neutral
            };

            UniVRM10.ExpressionKey key = new UniVRM10.ExpressionKey(preset);
            _vrmInstance.Runtime.Expression.SetWeight(key, 1.0f);
            
            // Reset other major emotions to 0 for clarity (simplified logic)
            ResetOtherExpressions(key);
        }

        private void ResetOtherExpressions(UniVRM10.ExpressionKey currentKey)
        {
            var presets = new[] { 
                UniVRM10.ExpressionPreset.happy, 
                UniVRM10.ExpressionPreset.angry, 
                UniVRM10.ExpressionPreset.sad, 
                UniVRM10.ExpressionPreset.surprised,
                UniVRM10.ExpressionPreset.neutral 
            };

            foreach (var p in presets)
            {
                var k = new UniVRM10.ExpressionKey(p);
                if (!k.Equals(currentKey)) _vrmInstance.Runtime.Expression.SetWeight(k, 0f);
            }
        }

        public void ConfigureForAudioSource(AudioSource source)
        {
            if (_lipSync == null) return;
            
            // uLipSync automatically analyzes the AudioSource on the same GameObject 
            // via OnAudioFilterRead if it's there. 
            // If the source is different, we need to ensure uLipSync is aware.
            // In most standard setups, we put uLipSync on the same object as AudioSource.
        }

        public void SetLipSyncProfile(Profile profile)
        {
            if (_lipSync != null)
            {
                _lipSync.profile = profile;
            }
        }

        public void OnConversationStart()
        {
            // Can be used to trigger "Listening" or "Thinking" expressions
            Debug.Log("[AvatarLipSyncController] Character started responding...");
        }
    }
}
