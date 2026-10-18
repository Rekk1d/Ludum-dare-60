using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadarView : MonoBehaviour
{
    [SerializeField] private GameController controller;
    [SerializeField] private RawImage sweepImage;
    [SerializeField] private Image blipTemplate;
    [SerializeField] private Sprite blipFrame1;
    [SerializeField] private Sprite blipFrame2;
    [SerializeField, Min(8)] private int size = 169;
    [SerializeField] private float sweepSpeed = 90f;
    [SerializeField] private float trailDegrees = 45f;
    [SerializeField] private Color sweepColor = new Color(0.557f, 0.941f, 0.541f, 1f);
    [SerializeField, Range(0f, 1f)] private float blipRadiusMin = 0.4f;
    [SerializeField, Range(0f, 1f)] private float blipRadiusMax = 0.8f;
    [SerializeField] private float blipAngleSpread = 25f;
    [SerializeField] private float blinkInterval = 0.4f;
    [SerializeField] private Color fakeColor = new Color(1f, 1f, 1f, 0.35f);

    private readonly List<Image> _blips = new List<Image>();
    private readonly List<float> _blipAngles = new List<float>();
    private readonly List<bool> _blipDetected = new List<bool>();
    private Texture2D _texture;
    private Color32[] _pixels;
    private float[] _pixelAngles;
    private float _angle;

    public float SweepAngle => _angle;

    private void Awake()
    {
        _texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        _pixels = new Color32[size * size];
        _pixelAngles = new float[size * size];
        var center = (size - 1) / 2f;
        var radius = size / 2f;
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = x - center;
                var dy = y - center;
                _pixelAngles[y * size + x] = dx * dx + dy * dy <= radius * radius
                    ? (Mathf.Atan2(dx, dy) * Mathf.Rad2Deg + 360f) % 360f
                    : -1f;
            }
        }

        sweepImage.texture = _texture;
        blipTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        controller.Session.RoundStarted += OnRoundStarted;
    }

    private void OnDestroy()
    {
        if (controller != null && controller.Session != null)
        {
            controller.Session.RoundStarted -= OnRoundStarted;
        }
    }

    private void Update()
    {
        var previous = _angle;
        _angle = (_angle + sweepSpeed * Time.deltaTime) % 360f;
        DrawSweep();
        UpdateBlips(previous);
    }

    private void DrawSweep()
    {
        var bright = (Color32)sweepColor;
        for (var i = 0; i < _pixels.Length; i++)
        {
            var pixelAngle = _pixelAngles[i];
            if (pixelAngle < 0f)
            {
                _pixels[i] = default;
                continue;
            }

            var behind = (_angle - pixelAngle + 360f) % 360f;
            if (behind > trailDegrees)
            {
                _pixels[i] = default;
                continue;
            }

            var step = Mathf.Min(2, (int)(behind / trailDegrees * 3f));
            var alpha = step == 0 ? 200 : step == 1 ? 110 : 50;
            _pixels[i] = new Color32(bright.r, bright.g, bright.b, (byte)alpha);
        }

        _texture.SetPixels32(_pixels);
        _texture.Apply(false);
    }

    private void OnRoundStarted()
    {
        foreach (var blip in _blips)
        {
            Destroy(blip.gameObject);
        }

        _blips.Clear();
        _blipAngles.Clear();
        _blipDetected.Clear();

        var radius = size / 2f;
        foreach (var attack in controller.Session.Attacks)
        {
            var blipAngle = (SectorAngle(attack.Sector) + Random.Range(-blipAngleSpread, blipAngleSpread) + 360f) % 360f;
            var distance = Random.Range(blipRadiusMin, blipRadiusMax) * radius;
            var radians = blipAngle * Mathf.Deg2Rad;
            var blip = Instantiate(blipTemplate, blipTemplate.transform.parent);
            blip.rectTransform.anchoredPosition = new Vector2(Mathf.Round(Mathf.Sin(radians) * distance), Mathf.Round(Mathf.Cos(radians) * distance));
            blip.gameObject.SetActive(false);
            _blips.Add(blip);
            _blipAngles.Add(blipAngle);
            _blipDetected.Add(false);
        }
    }

    private void UpdateBlips(float previous)
    {
        var session = controller.Session;
        var frame = (int)(Time.time / blinkInterval) % 2 == 0 ? blipFrame1 : blipFrame2;
        for (var i = 0; i < _blips.Count && i < session.Attacks.Count; i++)
        {
            if (!_blipDetected[i] && Passed(previous, _angle, _blipAngles[i]))
            {
                _blipDetected[i] = true;
            }

            var attack = session.Attacks[i];
            var showFake = !attack.IsReal && (attack.RealityRevealed || session.Phase != GamePhase.Planning);
            _blips[i].gameObject.SetActive(_blipDetected[i]);
            _blips[i].sprite = frame;
            _blips[i].color = showFake ? fakeColor : Color.white;
        }
    }

    private static bool Passed(float from, float to, float target)
    {
        var travelled = (to - from + 360f) % 360f;
        var toTarget = (target - from + 360f) % 360f;
        return toTarget <= travelled;
    }

    private static float SectorAngle(Sector sector)
    {
        switch (sector)
        {
            case Sector.North:
                return 0f;
            case Sector.East:
                return 90f;
            case Sector.South:
                return 180f;
            default:
                return 270f;
        }
    }
}
