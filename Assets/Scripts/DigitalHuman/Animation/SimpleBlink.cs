using UnityEngine;

namespace DigitalHuman.Core
{
    public class SimpleBlink : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer _faceMesh;
        [SerializeField] private int _blinkBlendShapeIndex = -1;
        [SerializeField] private float _blinkSpeed = 10f;
        [SerializeField] private float _minInterval = 2f;
        [SerializeField] private float _maxInterval = 5f;

        private float _timer;
        private bool _isBlinking;
        private float _blinkWeight;

        void Update()
        {
            if (_faceMesh == null || _blinkBlendShapeIndex < 0) return;

            if (!_isBlinking)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0)
                {
                    _isBlinking = true;
                    _blinkWeight = 0;
                }
            }
            else
            {
                _blinkWeight += Time.deltaTime * _blinkSpeed;
                float weight = Mathf.Sin(_blinkWeight) * 100f;
                
                if (weight < 0 && _blinkWeight > Mathf.PI)
                {
                    weight = 0;
                    _isBlinking = false;
                    _timer = Random.Range(_minInterval, _maxInterval);
                }
                
                _faceMesh.SetBlendShapeWeight(_blinkBlendShapeIndex, weight);
            }
        }
    }
}
