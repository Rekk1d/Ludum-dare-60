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

    private readonly List<Image> blips = new List<Image>();
    private readonly List<float> blipAngles = new List<float>();
    private readonly List<bool> blipDetected = new List<bool>();
    private Texture2D texture;
    private Color32[] pixels;
    private float[] pixelAngles;
    private float angle;

    public float SweepAngle => angle;

    private void Awake()
    {
        texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        pixels = new Color32[size * size];
        pixelAngles = new float[size * size];
        var center = (size - 1) / 2f;
        var radius = size / 2f;
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = x - center;
                var dy = y - center;
                pixelAngles[y * size + x] = dx * dx + dy * dy <= radius * radius
                    ? (Mathf.Atan2(dx, dy) * Mathf.Rad2Deg + 360f) % 360f
                    : -1f;
            }
        }

        sweepImage.texture = texture;
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
        var previous = angle;
        angle = (angle + sweepSpeed * Time.deltaTime) % 360f;
        DrawSweep();
        UpdateBlips(previous);
    }

    private void DrawSweep()
    {
        var bright = (Color32)sweepColor;
        for (var i = 0; i < pixels.Length; i++)
        {
            var pixelAngle = pixelAngles[i];
            if (pixelAngle < 0f)
            {
                pixels[i] = default;
                continue;
            }

            var behind = (angle - pixelAngle + 360f) % 360f;
            if (behind > trailDegrees)
            {
                pixels[i] = default;
                continue;
            }

            var step = Mathf.Min(2, (int)(behind / trailDegrees * 3f));
            var alpha = step == 0 ? 200 : step == 1 ? 110 : 50;
            pixels[i] = new Color32(bright.r, bright.g, bright.b, (byte)alpha);
        }

        texture.SetPixels32(pixels);
        texture.Apply(false);
    }

    private void OnRoundStarted()
    {
        foreach (var blip in blips)
        {
            Destroy(blip.gameObject);
        }

        blips.Clear();
        blipAngles.Clear();
        blipDetected.Clear();

        var radius = size / 2f;
        foreach (var attack in controller.Session.Attacks)
        {
            var blipAngle = (SectorAngle(attack.Sector) + Random.Range(-blipAngleSpread, blipAngleSpread) + 360f) % 360f;
            var distance = Random.Range(blipRadiusMin, blipRadiusMax) * radius;
            var radians = blipAngle * Mathf.Deg2Rad;
            var blip = Instantiate(blipTemplate, blipTemplate.transform.parent);
            blip.rectTransform.anchoredPosition = new Vector2(Mathf.Round(Mathf.Sin(radians) * distance), Mathf.Round(Mathf.Cos(radians) * distance));
            blip.gameObject.SetActive(false);
            blips.Add(blip);
            blipAngles.Add(blipAngle);
            blipDetected.Add(false);
        }
    }

    private void UpdateBlips(float previous)
    {
        var session = controller.Session;
        var frame = (int)(Time.time / blinkInterval) % 2 == 0 ? blipFrame1 : blipFrame2;
        for (var i = 0; i < blips.Count && i < session.Attacks.Count; i++)
        {
            if (!blipDetected[i] && Passed(previous, angle, blipAngles[i]))
            {
                blipDetected[i] = true;
            }

            var attack = session.Attacks[i];
            var showFake = !attack.IsReal && (attack.RealityRevealed || session.Phase != GamePhase.Planning);
            blips[i].gameObject.SetActive(blipDetected[i]);
            blips[i].sprite = frame;
            blips[i].color = showFake ? fakeColor : Color.white;
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
