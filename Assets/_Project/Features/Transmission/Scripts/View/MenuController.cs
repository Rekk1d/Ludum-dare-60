using System.Collections;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    private const int ProceedOption = 0;

    [SerializeField] private GameController controller;
    [SerializeField] private TerminalView terminal;
    [SerializeField] private MenuSheet mainSheet;
    [SerializeField] private MenuSheet lostSheet;
    [SerializeField] private MenuSheet wonSheet;
    [SerializeField] private MenuSheet manualSheet;
    [SerializeField] private GameObject radarGlassOff;
    [SerializeField] private GameObject[] poweredLabels;
    [SerializeField, Min(0f)] private float endDelay = 1f;

    private void Start()
    {
        mainSheet.OptionClicked += index => OnOptionClicked(mainSheet, index, false);
        lostSheet.OptionClicked += index => OnOptionClicked(lostSheet, index, true);
        wonSheet.OptionClicked += index => OnOptionClicked(wonSheet, index, true);
        manualSheet.OptionClicked += OnManualClicked;
        terminal.EndPrinted += OnEndPrinted;
        SetPowered(false);
        lostSheet.gameObject.SetActive(false);
        wonSheet.gameObject.SetActive(false);
        manualSheet.gameObject.SetActive(false);
        mainSheet.Open(false);
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
        if (index != ProceedOption)
        {
            return;
        }

        sheet.Approve(index, () =>
        {
            SetPowered(true);
            if (!replay)
            {
                manualSheet.Open(true);
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

        manualSheet.Approve(index, () => controller.StartGame(false));
    }

    private void SetPowered(bool powered)
    {
        radarGlassOff.SetActive(!powered);
        foreach (var label in poweredLabels)
        {
            label.SetActive(powered);
        }
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
