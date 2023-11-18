using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Container for the UI components we are interested
/// in manipulating with our state machine.
/// </summary>
[AddComponentMenu("")]
public class MainMenuPanels : MonoBehaviour
{
    [Header("Main Menu")]
    public CanvasGroup mainMenuPanel;
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;
    [Header("Settings")]
    public CanvasGroup settingsPanel;
    public Slider[] sliders;
    public Button backButton;
    [Header("Play")]
    public CanvasGroup loadingPanel;
    public Text loadingText;
}
