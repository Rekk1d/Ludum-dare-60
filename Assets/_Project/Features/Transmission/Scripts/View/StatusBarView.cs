using TMPro;
using UnityEngine;

public class StatusBarView : MonoBehaviour
{
    [SerializeField] private GameController controller;
    [SerializeField] private LampStrip hpLamps;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private LampStrip decryptLamps;
    [SerializeField] private LampStrip orderLamps;
    [SerializeField, Min(1)] private int brokenFromWave = 3;

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
            hpLamps.SetState(hpLamps.Capacity, 0);
            roundText.text = string.Empty;
            decryptLamps.SetState(decryptLamps.Capacity, 0);
            orderLamps.SetState(orderLamps.Capacity, 0);
            return;
        }

        hpLamps.SetState(session.MaxHp, session.Hp);
        roundText.text = (session.RoundIndex + 1).ToString("00");
        var broken = session.RoundIndex + 1 >= brokenFromWave;
        decryptLamps.SetState(broken ? session.DecryptTotal : decryptLamps.Capacity, session.DecryptsLeft);
        orderLamps.SetState(broken ? session.OrdersTotal : orderLamps.Capacity, session.OrdersLeft);
    }
}
