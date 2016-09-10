using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDBehaviour : MonoBehaviour
{
    // Blackout
    public GameObject BlackoutPanel;

    // Top panel
    public GameObject TopPanel;
    public GameObject MoneyPanel;
    public GameObject MoneyIcon;
    public Text MoneyText;
    public Text RateText;

    // Messages
    public GameObject MessagePanel;
    public Text MessageText;

    // Bottom panel
    public GameObject BottomPanel;
    public Button Lane0Button;
    public Button Lane1Button;
    public Button Lane2Button;

    // Timer variables
    private float _blackoutTime = 1.0f;
    private float _messageLivetime;
    private float _moneyAnimTime;

    // Singleton instance
    private static HUDBehaviour _instance;

    #region Event Handlers

    public void Click0()
    {
        _spawnInLane(0);
    }

    public void Click1()
    {
        _spawnInLane(1);
    }

    public void Click2()
    {
        _spawnInLane(2);
    }

    public void ClickQuit()
    {
        Application.LoadLevel(Scene.Menu);
    }

    #endregion

    #region Private Helpers

    private void _spawnInLane(int laneIndex)
    {
        EventSystem.current.SetSelectedGameObject(null);
        var e = new Events.PurchaseEvent(Side.Player, laneIndex);
        Game.PushEvent(e);
    }

    private void _setButtonEnabled(Button btn, bool enabled)
    {
        btn.interactable = enabled;
    }

    private void _showMessage(string message, float livetime, Color color)
    {
        MessageText.color = color;
        MessageText.text = message;
        _messageLivetime = livetime;
    }

    private void _updateBlackout()
    {
        _blackoutTime -= Time.deltaTime;
        var b = BlackoutPanel.GetComponent<CanvasRenderer>();
        if (_blackoutTime > 0)
        {
            b.SetAlpha(Mathf.Clamp01(_blackoutTime));
        }
        else
        {
            BlackoutPanel.SetActive(false);
        }
    }

    private void _updateMessage()
    {
        _messageLivetime -= Time.deltaTime;
        var a = Mathf.Clamp01(_messageLivetime / 2.0f);
        MessagePanel.GetComponent<CanvasRenderer>().SetAlpha(a);
        MessageText.color = U.ChangeAlpha(MessageText.color, a);
    }

    private void _updateMoney()
    {
        var money = Game.State.PlayerResources;
        var income = Game.Queries.GetGameMetrics().PlayerIncome;
        _moneyAnimTime -= Time.deltaTime;
        MoneyText.text = money.ToString();
        RateText.text = "+" + income;
        var scale = _moneyAnimTime > 0f
            ? Vector3.one * (1f + _moneyAnimTime)
            : Vector3.one;
        MoneyIcon.transform.localScale = scale;
    }

    private void _updateBuyButtons()
    {
        var money = Game.State.PlayerResources;
        var queries = Game.Queries;
        var m0 = queries.GetLaneMetrics("0");
        var m1 = queries.GetLaneMetrics("1");
        var m2 = queries.GetLaneMetrics("2");
        _setButtonEnabled(Lane0Button, money > 0 && m0.PlayerBaseHealthPercentage > 0);
        _setButtonEnabled(Lane1Button, money > 0 && m1.PlayerBaseHealthPercentage > 0);
        _setButtonEnabled(Lane2Button, money > 0 && m2.PlayerBaseHealthPercentage > 0);
    }

    #endregion

    #region Unity Lifecycle

    void Start()
    {
        _instance = this;
    }

    void Update()
    {
        _updateBlackout();
        _updateMessage();
        _updateMoney();
        _updateBuyButtons();
    }

    #endregion

    #region Static Methods

    public static void ShowMessage(string message)
    {
        if (_instance == null)
        {
            Debug.LogWarning("HUDBehaviour: ShowMessage: make sure a HUDBehaviour.cs script lives in the scene");
        }
        else
        {
            _instance._showMessage(message, 3.0f, Color.white);
        }
    }

    public static void ShowWinMessage(string message)
    {
        if (_instance == null)
        {
            Debug.LogWarning("HUDBehaviour: ShowWinMessage: make sure a HUDBehaviour.cs script lives in the scene");
        }
        else
        {
            _instance._showMessage(message, 1000.0f, Color.green);
        }
    }

    public static void ShowLoseMessage(string message)
    {
        if (_instance == null)
        {
            Debug.LogWarning("HUDBehaviour: ShowLoseMessage: make sure a HUDBehaviour.cs script lives in the scene");
        }
        else
        {
            _instance._showMessage(message, 1000.0f, Color.red);
        }
    }

    public static void AnimateMoneyCounter()
    {
        if (_instance == null)
        {
            Debug.LogWarning("HUDBehaviour: AnimateMoneyCounter: make sure a HUDBehaviour.cs script lives in the scene");
        }
        else
        {
            _instance._moneyAnimTime = 0.5f;
        }
    }

    #endregion
}