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
        hpLamps.SetState(session.MaxHp, session.Hp);
        roundText.text = (session.RoundIndex + 1).ToString("00");
        decryptLamps.SetState(session.DecryptTotal, session.DecryptsLeft);
        orderLamps.SetState(session.OrdersTotal, session.OrdersLeft);
    }
}
