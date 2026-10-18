using UnityEngine;
using UnityEngine.UI;

public class ConfirmButtonView : MonoBehaviour
{
    [SerializeField] private GameController controller;
    [SerializeField] private Button button;
    [SerializeField] private Image label;
    [SerializeField] private Sprite planningSprite;
    [SerializeField] private Sprite reportSprite;

    private void Start()
    {
        controller.Session.Changed += Refresh;
        Refresh();
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
        var session = controller.Session;
        button.interactable = session.CanConfirm;
        label.enabled = session.IsStarted;
        label.sprite = session.Phase == GamePhase.Report ? reportSprite : planningSprite;
    }
}
