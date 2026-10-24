using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class StartScreen : MonoBehaviour
{

    private VisualElement _root;
    private Label _callToAction;
    private Label _highScore;

    // Start is called before the first frame update
    void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _callToAction = _root.Q<Label>("CallToAction");
        _highScore = _root.Q<Label>("HighScore");
        Time.timeScale = 1;
        InvokeRepeating(nameof(BlinkCallToAction), 0.25f, 0.25f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKey)
        {
            SceneManager.LoadScene("Scenes/SampleScene");
        }

        int hs = GameState.HighScore;
        _highScore.text = "High Score: " + hs.ToString("0");
    }

    void BlinkCallToAction()
    {
        _callToAction.visible = !_callToAction.visible;
    }
}
