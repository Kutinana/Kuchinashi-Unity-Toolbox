using UnityEngine;

namespace Kuchinashi.Utils
{
    /// <summary>
    /// 在本地 XY 平面内对摄像机（或任意 Transform）施加有界的轻微随机晃动。
    /// 使用柏林噪声，运动平滑且幅度不超过 <see cref="maxOffset"/>。
    /// </summary>
    public class CameraMicroSway2D : MonoBehaviour
    {
        [Tooltip("相对启用时本地位置，X/Y 方向最大偏移（单位：米，世界/本地由 useLocalSpace 决定）。")]
        [SerializeField] private Vector2 maxOffset = new Vector2(0.03f, 0.03f);

        [Tooltip("数值越大，晃动变化越快。")]
        [SerializeField] private float frequency = 0.35f;

        [SerializeField] private bool useLocalSpace = true;

        [Tooltip("使用不受 timeScale 影响的时间（例如暂停时仍晃动）。")]
        [SerializeField] private bool useUnscaledTime;

        private Vector3 _basePosition;
        private float _noiseSeedX;
        private float _noiseSeedY;

        private void Awake()
        {
            _noiseSeedX = Random.Range(0f, 256f);
            _noiseSeedY = Random.Range(0f, 256f);
        }

        private void OnEnable()
        {
            _basePosition = useLocalSpace ? transform.localPosition : transform.position;
        }

        private void OnDisable()
        {
            if (useLocalSpace)
                transform.localPosition = _basePosition;
            else
                transform.position = _basePosition;
        }

        private void LateUpdate()
        {
            float t = (useUnscaledTime ? Time.unscaledTime : Time.time) * frequency;
            float nx = Mathf.PerlinNoise(_noiseSeedX + t, _noiseSeedY) * 2f - 1f;
            float ny = Mathf.PerlinNoise(_noiseSeedX, _noiseSeedY + t) * 2f - 1f;
            var delta = new Vector3(nx * maxOffset.x, ny * maxOffset.y, 0f);

            if (useLocalSpace)
                transform.localPosition = _basePosition + delta;
            else
                transform.position = _basePosition + delta;
        }
    }
}
