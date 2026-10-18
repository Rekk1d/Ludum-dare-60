using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    private const int ProceedOption = 0;
    private const int SettingsOption = 1;
    private const int BackOption = 0;
    private const int ResumeOption = 0;

    [SerializeField] private GameController controller;
    [SerializeField] private TerminalView terminal;
    [SerializeField] private MenuSheet mainSheet;
    [SerializeField] private MenuSheet lostSheet;
    [SerializeField] private MenuSheet wonSheet;
    [SerializeField] private MenuSheet manualSheet;
    [SerializeField] private MenuSheet settingsSheet;
    [SerializeField] private MenuSheet pauseSheet;
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject blocker;
    [SerializeField] private GameObject radarGlassOff;
    [SerializeField] private GameObject[] poweredLabels;
    [SerializeField, Min(0f)] private float endDelay = 1f;
    [SerializeField, Min(0f), FormerlySerializedAs("manualDelay")] private float sheetDelay = 0.5f;

    private MenuSheet _returnSheet;
    private bool _paused;

    private void Start()
    {
        mainSheet.OptionClicked += index => OnOptionClicked(mainSheet, index, false);
        lostSheet.OptionClicked += index => OnOptionClicked(lostSheet, index, true);
        wonSheet.OptionClicked += index => OnOptionClicked(wonSheet, index, true);
        manualSheet.OptionClicked += OnManualClicked;
        settingsSheet.OptionClicked += OnSettingsClicked;
        pauseSheet.OptionClicked += OnPauseClicked;
        pauseButton.onClick.AddListener(OpenPause);
        terminal.EndPrinted += OnEndPrinted;
        SetPowered(false);
        lostSheet.gameObject.SetActive(false);
        wonSheet.gameObject.SetActive(false);
        manualSheet.gameObject.SetActive(false);
        settingsSheet.gameObject.SetActive(false);
        pauseSheet.gameObject.SetActive(false);
        blocker.SetActive(false);
        mainSheet.Open(false);
    }

    private void Update()
    {
        var session = controller.Session;
        pauseButton.interactable = !_paused && session.IsStarted && (session.Phase == GamePhase.Planning || session.Phase == GamePhase.Report);
    }

    private void OnDestroy()
    {
        if (terminal != null)
        {
            terminal.EndPrinted -= OnEndPrinted;
        }
    }

    private void OnOptionClicked(MenuSheet sheet, int index, bool replay)
    {
        if (index == SettingsOption)
        {
            _returnSheet = sheet;
            sheet.Approve(index, false, () => StartCoroutine(OpenLater(settingsSheet, true)));
            return;
        }

        if (index != ProceedOption)
        {
            return;
        }

        sheet.Approve(index, true, () =>
        {
            SetPowered(true);
            if (!replay)
            {
                StartCoroutine(OpenLater(manualSheet, true));
                return;
            }

            controller.StartGame(true);
        });
    }

    private void OnManualClicked(int index)
    {
        if (index != ProceedOption)
        {
            return;
        }

        manualSheet.Approve(index, true, () => controller.StartGame(false));
    }

    private void OnSettingsClicked(int index)
    {
        if (index != BackOption)
        {
            return;
        }

        settingsSheet.Approve(index, false, () => StartCoroutine(OpenLater(_returnSheet, false)));
    }

    private void OpenPause()
    {
        _paused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        blocker.SetActive(true);
        pauseSheet.Open(true, false);
    }

    private void OnPauseClicked(int index)
    {
        if (index == SettingsOption)
        {
            _returnSheet = pauseSheet;
            pauseSheet.Approve(index, false, () => StartCoroutine(OpenLater(settingsSheet, true)));
            return;
        }

        if (index == ResumeOption)
        {
            pauseSheet.Approve(index, false, Resume);
        }
    }

    private void Resume()
    {
        _paused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        blocker.SetActive(false);
    }

    private void SetPowered(bool powered)
    {
        radarGlassOff.SetActive(!powered);
        foreach (var label in poweredLabels)
        {
            label.SetActive(powered);
        }
    }

    private IEnumerator OpenLater(MenuSheet sheet, bool typed)
    {
        yield return new WaitForSecondsRealtime(sheetDelay);
        sheet.Open(true, typed);
    }

    private void OnEndPrinted()
    {
        StartCoroutine(ShowEnd());
    }

    private IEnumerator ShowEnd()
    {
        yield return new WaitForSeconds(endDelay);
        var sheet = controller.Session.Phase == GamePhase.Won ? wonSheet : lostSheet;
        sheet.Open(true);
    }
}
