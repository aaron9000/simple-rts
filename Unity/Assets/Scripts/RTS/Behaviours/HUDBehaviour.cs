using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDBehaviour : MonoBehaviour
{
    public Button Lane0Button;
    public Button Lane1Button;
    public Button Lane2Button;
    public Text MoneyText;
    public Text MessageText;
    public GameObject MoneyPanel;
    public GameObject MessagePanel;
    public GameObject BlackoutPanel;

    private float _blackoutTime = 1.0f;
    private float _messageLivetime;
    private float _moneyAnimTime;

    private static HUDBehaviour _instance;

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

    // Use this for initialization
    void Start()
    {
        _instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        var money = Game.State().PlayerResources;
        var queries = Game.Queries();

        // Blackout
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

        // Fade out message
        _messageLivetime -= Time.deltaTime;
        var a = Mathf.Clamp01(_messageLivetime / 2.0f);
        MessagePanel.GetComponent<CanvasRenderer>().SetAlpha(a);
        MessageText.color = U.ChangeAlpha(MessageText.color, a);

        // Update money text
        _moneyAnimTime -= Time.deltaTime;
        MoneyText.text = "$" + money;
        MoneyText.color = money > 0 ? Color.white : Color.grey;
        var scale = _moneyAnimTime > 0f
            ? Vector3.one * (1f + _moneyAnimTime)
            : Vector3.one;
        MoneyText.transform.localScale = scale;
        MoneyPanel.GetComponent<CanvasRenderer>().transform.localScale = scale;

        // Change state of buttons
        var m0 = queries.GetLaneMetrics("0");
        var m1 = queries.GetLaneMetrics("1");
        var m2 = queries.GetLaneMetrics("2");
        _setButtonEnabled(Lane0Button, money > 0 && m0.PlayerBaseHealthPercentage > 0);
        _setButtonEnabled(Lane1Button, money > 0 && m1.PlayerBaseHealthPercentage > 0);
        _setButtonEnabled(Lane2Button, money > 0 && m2.PlayerBaseHealthPercentage > 0);
    }

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
}