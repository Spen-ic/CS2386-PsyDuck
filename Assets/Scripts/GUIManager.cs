using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GUIManager : MonoBehaviour
{
    private GroupBox timerGB;
    private Label time;

    private GroupBox reportGB;
    private Label titleLabel;
    private Label line1Label;
    private Label line2Label;
    private Button returnButton;

    private LevelManager levelmanager;
    
    // Start is called before the first frame update
    void Start()
    {
        levelmanager = GameObject.Find("LevelManager").GetComponent<LevelManager>();

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        
        timerGB = root.Q<GroupBox>("timer");
        time = root.Q<Label>("time");
        
        reportGB = root.Q<GroupBox>("report");
        titleLabel = root.Q<Label>("title");
        line1Label = root.Q<Label>("line1");
        line2Label = root.Q<Label>("line2");
        returnButton = root.Q<Button>("return");
        
        returnButton.clicked += returnButtonPressed;

        setDisplay(reportGB, false);
        setDisplay(timerGB, true);
    }

    // Update is called once per frame
    void Update()
    {
        if (getDisplay(timerGB))
        {
            time.text = (levelmanager.endTime - levelmanager.currentTime).ToString("0");
        }
    }

    public void ReportToPlayer(string title, string line1, float visibleTime)
    {
        Debug.Log("ReportToPlayer called");
        if (getDisplay(reportGB))
        {
            CancelInvoke("HideReport");
        }
        setDisplay(timerGB, false);
        setDisplay(reportGB, true);

        titleLabel.text = title;
        line1Label.text = line1;
        titleLabel.visible = true;
        line1Label.visible = true;
        returnButton.visible = true;
        returnButton.text = "Return to Start";
        
        Invoke("HideReport", visibleTime);
    }
    
    public void ReportToPlayer(string title, string line1, string line2)
    {
        if (getDisplay(reportGB))
        {
            CancelInvoke("HideReport");
        }
        setDisplay(timerGB, false);
        setDisplay(reportGB, true);

        titleLabel.text = title;
        line1Label.text = line1;
        line2Label.text = line2;
        titleLabel.visible = true;
        line1Label.visible = true;
        line2Label.visible = true;
        returnButton.visible = true;
        returnButton.text = "Return to Start";
    }

    private void HideReport()
    {
        setDisplay(timerGB, true);
        setDisplay(reportGB, false);
    }

    private void returnButtonPressed()
    {
        SceneManager.LoadScene("Scenes/Start");
    }

    private void setDisplay(GroupBox gb, bool displaying)
    {
        if (displaying)
        {
            gb.style.display = DisplayStyle.Flex;

        }
        else
        {
            gb.style.display = DisplayStyle.None;

        }
    }

    private bool getDisplay(GroupBox gb)
    {
        if (gb.style.display == DisplayStyle.Flex)
        {
            return true;
        }

        return false;
    }
}
