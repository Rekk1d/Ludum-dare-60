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
    [SerializeField] private Sprite bloomFrame;
    [SerializeField] private Sprite lockFrame1;
    [SerializeField] private Sprite lockFrame2;
    [SerializeField] private Sprite[] flashFrames;
    [SerializeField, Min(0.01f)] private float flashFrameTime = 0.06f;
    [SerializeField, Min(8)] private int size = 125;
    [SerializeField] private float trailDegrees = 45f;
    [SerializeField, Range(0f, 10f)] private float edgeDegrees = 2f;
    [SerializeField, Range(0, 255)] private int edgeAlpha = 255;
    [SerializeField, Range(0, 255)] private int headAlpha = 210;
    [SerializeField, Range(0, 255)] private int tailAlphaNear = 70;
    [SerializeField, Range(0, 255)] private int tailAlphaMid = 45;
    [SerializeField, Range(0, 255)] private int tailAlphaFar = 25;
    [SerializeField] private Color sweepColor = new Color(0.557f, 0.941f, 0.541f, 1f);
    [SerializeField] private float blinkInterval = 0.4f;
    [SerializeField] private Color fakeColor = new Color(1f, 1f, 1f, 0.35f);

    private readonly List<Image> _blips = new List<Image>();
    private readonly List<Image> _frames = new List<Image>();
    private readonly List<Image> _flashes = new List<Image>();
    private readonly List<bool> _detected = new List<bool>();
    private readonly List<float> _flashStart = new List<float>();
    private Texture2D _texture;
    private Color32[] _pixels;
    private float[] _pixelAngles;
    private float _previousAngle;

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
        controller.Session.TargetsLocked += OnTargetsLocked;
    }

    private void OnDestroy()
    {
        if (controller != null && controller.Session != null)
        {
            controller.Session.RoundStarted -= OnRoundStarted;
            controller.Session.TargetsLocked -= OnTargetsLocked;
        }
    }

    private void Update()
    {
        var session = controller.Session;
        var angle = session.SweepAngle;
        DrawSweep(angle, session.LockWindow);
        UpdateTargets(session, _previousAngle, angle);
        _previousAngle = angle;
    }

    private void DrawSweep(float angle, float head)
    {
        var bright = (Color32)sweepColor;
        var tail = Mathf.Max(0.01f, trailDegrees - head);
        for (var i = 0; i < _pixels.Length; i++)
        {
            var pixelAngle = _pixelAngles[i];
            if (pixelAngle < 0f)
            {
                _pixels[i] = default;
                continue;
            }

            var behind = (angle - pixelAngle + 360f) % 360f;
            if (behind > trailDegrees)
            {
                _pixels[i] = default;
                continue;
            }

            int alpha;
            if (behind <= edgeDegrees)
            {
                alpha = edgeAlpha;
            }
            else if (behind <= head)
            {
                alpha = headAlpha;
            }
            else
            {
                var step = Mathf.Min(2, (int)((behind - head) / tail * 3f));
                alpha = step == 0 ? tailAlphaNear : step == 1 ? tailAlphaMid : tailAlphaFar;
            }

            _pixels[i] = new Color32(bright.r, bright.g, bright.b, (byte)alpha);
        }

        _texture.SetPixels32(_pixels);
        _texture.Apply(false);
    }

    private void OnRoundStarted()
    {
        DestroyAll(_blips);
        DestroyAll(_frames);
        DestroyAll(_flashes);
        _detected.Clear();
        _flashStart.Clear();

        foreach (var attack in controller.Session.Attacks)
        {
            _blips.Add(CreateImage(blipFrame1));
            _frames.Add(CreateImage(lockFrame1));
            _flashes.Add(CreateImage(flashFrames.Length > 0 ? flashFrames[0] : blipFrame1));
            _detected.Add(false);
            _flashStart.Add(-1f);
        }
    }

    private void OnTargetsLocked(IReadOnlyList<Attack> locked)
    {
        var attacks = controller.Session.Attacks;
        for (var i = 0; i < attacks.Count && i < _flashStart.Count; i++)
        {
            foreach (var attack in locked)
            {
                if (attacks[i] == attack)
                {
                    _flashStart[i] = Time.time;
                }
            }
        }
    }

    private void UpdateTargets(GameSession session, float previous, float current)
    {
        var radius = size / 2f;
        var blink = (int)(Time.time / blinkInterval) % 2 == 0;
        var attacks = session.Attacks;
        for (var i = 0; i < _blips.Count && i < attacks.Count; i++)
        {
            var attack = attacks[i];
            if (attack.Spawned && !_detected[i] && (attack.Locked || Passed(previous, current, attack.Angle)))
            {
                _detected[i] = true;
            }

            var visible = _detected[i] && !attack.Lost;
            var radians = attack.Angle * Mathf.Deg2Rad;
            var distance = attack.Distance * radius;
            var position = new Vector2(Mathf.Round(Mathf.Sin(radians) * distance), Mathf.Round(Mathf.Cos(radians) * distance));
            var showFake = !attack.IsReal && (attack.RealityRevealed || session.Phase != GamePhase.Planning);

            var blip = _blips[i];
            blip.gameObject.SetActive(visible);
            blip.rectTransform.anchoredPosition = position;
            SetSprite(blip, session.InLockZone(attack) ? bloomFrame : blink ? blipFrame1 : blipFrame2);
            blip.color = showFake ? fakeColor : Color.white;

            var frame = _frames[i];
            frame.gameObject.SetActive(visible && attack.Locked);
            frame.rectTransform.anchoredPosition = position;
            SetSprite(frame, blink ? lockFrame1 : lockFrame2);

            var flash = _flashes[i];
            var index = _flashStart[i] < 0f ? -1 : (int)((Time.time - _flashStart[i]) / flashFrameTime);
            var flashing = visible && index >= 0 && index < flashFrames.Length;
            flash.gameObject.SetActive(flashing);
            if (flashing)
            {
                flash.rectTransform.anchoredPosition = position;
                SetSprite(flash, flashFrames[index]);
            }
        }
    }

    private Image CreateImage(Sprite sprite)
    {
        var image = Instantiate(blipTemplate, blipTemplate.transform.parent);
        SetSprite(image, sprite);
        image.gameObject.SetActive(false);
        return image;
    }

    private static void SetSprite(Image image, Sprite sprite)
    {
        if (image.sprite == sprite)
        {
            return;
        }

        image.sprite = sprite;
        image.SetNativeSize();
    }

    private static void DestroyAll(List<Image> images)
    {
        foreach (var image in images)
        {
            Destroy(image.gameObject);
        }

        images.Clear();
    }

    private static bool Passed(float from, float to, float target)
    {
        var travelled = (to - from + 360f) % 360f;
        var toTarget = (target - from + 360f) % 360f;
        return toTarget <= travelled;
    }
}
