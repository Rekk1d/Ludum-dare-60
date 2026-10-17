using UnityEngine;
using UnityEngine.UI;

public class ConfirmButtonView : MonoBehaviour
{
    [SerializeField] private GameController controller;
    [SerializeField] private Button button;
    [SerializeField] private Image label;
    [SerializeField] private Sprite planningSprite;
    [SerializeField] private Sprite reportSprite;
    [SerializeField] private Sprite endSprite;

    private void Start()
    {
        controller.Session.Changed += Refresh;
    }

    private void OnDestroy()
    {
        if (controller != null && controller.Session != null)
        {
            controller.Session.Changed -= Refresh;
        }
    }

    private void Refresh()
    {
        button.interactable = controller.Session.CanConfirm;
        switch (controller.Session.Phase)
        {
            case GamePhase.Planning:
                label.sprite = planningSprite;
                break;
            case GamePhase.Report:
                label.sprite = reportSprite;
                break;
            default:
                label.sprite = endSprite;
                break;
        }
    }
}
