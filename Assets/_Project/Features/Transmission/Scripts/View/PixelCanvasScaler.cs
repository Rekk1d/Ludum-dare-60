using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class PixelCanvasScaler : MonoBehaviour
{
    [SerializeField, Min(1)] private int referenceWidth = 480;
    [SerializeField, Min(1)] private int referenceHeight = 270;

    private CanvasScaler _scaler;

    private void Awake()
    {
        _scaler = GetComponent<CanvasScaler>();
        Apply();
    }

    private void Update()
    {
        Apply();
    }

    private void Apply()
    {
        var scale = Mathf.Max(1, Mathf.Min(Screen.width / referenceWidth, Screen.height / referenceHeight));
        if (!Mathf.Approximately(_scaler.scaleFactor, scale))
        {
            _scaler.scaleFactor = scale;
        }
    }
}
