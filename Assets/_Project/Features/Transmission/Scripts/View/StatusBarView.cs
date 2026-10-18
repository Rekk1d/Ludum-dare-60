using TMPro;
using UnityEngine;

public class StatusBarView : MonoBehaviour
{
    [SerializeField] private GameController controller;
    [SerializeField] private LampStrip hpLamps;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private LampStrip decryptLamps;
    [SerializeField] private LampStrip orderLamps;

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
        if (!session.IsStarted)
        {
            hpLamps.SetState(0, 0);
            roundText.text = string.Empty;
            decryptLamps.SetState(0, 0);
            orderLamps.SetState(0, 0);
            return;
        }

        hpLamps.SetState(session.MaxHp, session.Hp);
        roundText.text = (session.RoundIndex + 1).ToString("00");
        decryptLamps.SetState(session.DecryptTotal, session.DecryptsLeft);
        orderLamps.SetState(session.OrdersTotal, session.OrdersLeft);
    }
}
