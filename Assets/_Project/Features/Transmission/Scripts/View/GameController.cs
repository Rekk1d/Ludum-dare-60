using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private Button confirmButton;

    public GameSession Session { get; private set; }

    private void Awake()
    {
        Session = new GameSession(config, new System.Random());
        confirmButton.onClick.AddListener(OnConfirm);
    }

    private IEnumerator Start()
    {
        yield return null;
        Session.StartGame();
    }

    private void OnConfirm()
    {
        switch (Session.Phase)
        {
            case GamePhase.Planning:
                Session.ConfirmOrders();
                break;
            case GamePhase.Report:
                Session.NextRound();
                break;
            default:
                Session.StartGame();
                break;
        }
    }
}
