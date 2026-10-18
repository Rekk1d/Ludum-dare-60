using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TerminalView terminal;

    public GameSession Session { get; private set; }

    private void Awake()
    {
        Session = new GameSession(config, new System.Random());
        confirmButton.onClick.AddListener(OnConfirm);
        terminal.IntroPrinted += OnIntroPrinted;
    }

    private IEnumerator Start()
    {
        yield return null;
        Session.StartGame();
    }

    private void Update()
    {
        Session.Tick(Time.deltaTime);
    }

    private void OnDestroy()
    {
        if (terminal != null)
        {
            terminal.IntroPrinted -= OnIntroPrinted;
        }
    }

    private void OnIntroPrinted()
    {
        Session.StartClock();
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
