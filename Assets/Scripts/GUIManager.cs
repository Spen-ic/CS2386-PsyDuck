using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GUIManager : MonoBehaviour
{
    private GroupBox _timerGb;
    private Label _time;

    private GroupBox _reportGb;
    private Label _titleLabel;
    private Label _line1Label;
    private Label _line2Label;
    private Button _returnButton;
    
    // ReSharper disable once IdentifierTypo
    private LevelManager _levelmanager;
    
    // Start is called before the first frame update
    void Start()
    {
        _levelmanager = GameObject.Find("LevelManager").GetComponent<LevelManager>();

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        
        _timerGb = root.Q<GroupBox>("timer");
        _time = root.Q<Label>("time");
        
        _reportGb = root.Q<GroupBox>("report");
        _titleLabel = root.Q<Label>("title");
        _line1Label = root.Q<Label>("line1");
        _line2Label = root.Q<Label>("line2");
        _returnButton = root.Q<Button>("return");
        
        _returnButton.clicked += ReturnButtonPressed;

        SetDisplay(_reportGb, false);
        SetDisplay(_timerGb, true);
    }

    // Update is called once per frame
    void Update()
    {
        if (GetDisplay(_timerGb))
        {
            _time.text = (_levelmanager.endTime - _levelmanager.currentTime).ToString("0");
        }
    }

    public void ReportToPlayer(string title, string line1, float visibleTime)
    {
        Debug.Log("ReportToPlayer called");
        if (GetDisplay(_reportGb))
        {
            CancelInvoke(nameof(HideReport));
        }
        SetDisplay(_timerGb, false);
        SetDisplay(_reportGb, true);

        _titleLabel.text = title;
        _line1Label.text = line1;
        _line2Label.text = "";
        _titleLabel.visible = true;
        _line1Label.visible = true;
        _line2Label.visible = false;
        _returnButton.visible = true;
        _returnButton.text = "Return to Start";
        
        Invoke(nameof(HideReport), visibleTime);
    }
    
    public void ReportToPlayer(string title, string line1, string line2)
    {
        if (GetDisplay(_reportGb))
        {
            CancelInvoke(nameof(HideReport));
        }
        SetDisplay(_timerGb, false);
        SetDisplay(_reportGb, true);

        _titleLabel.text = title;
        _line1Label.text = line1;
        _line2Label.text = line2;
        _titleLabel.visible = true;
        _line1Label.visible = true;
        _line2Label.visible = true;
        _returnButton.visible = true;
        _returnButton.text = "Return to Start";
    }

    private void HideReport()
    {
        SetDisplay(_timerGb, true);
        SetDisplay(_reportGb, false);
    }

    private void ReturnButtonPressed()
    {
        SceneManager.LoadScene("Scenes/Start");
    }

    private void SetDisplay(GroupBox gb, bool displaying)
    {
        gb.style.display = displaying ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private bool GetDisplay(GroupBox gb)
    {
        if (gb.style.display == DisplayStyle.Flex)
        {
            return true;
        }

        return false;
    }
}
