using FSMForUnity;
using UnityEngine;

/// <summary>
/// With this sample, we control the state in <see cref="MainMenuPanels"/>
/// via the states in the machine. This menu mimics the appearance of a main menu
/// with submenus, with crossfades into these submenus. By Utilizing <see cref="ParallelFSMState"/>
/// we can formulate a fairly complex relationship between the states without the need for
/// many specific implementations.
/// </summary>
[AddComponentMenu("")]
public class MainMenuController : MonoBehaviour
{
    [SerializeField]
    private MainMenuPanels panels;

    private FSMMachine fsm;


    /// <summary>
    /// Here we initialize the panels and construct the state machine.
    /// Essentially we have 3 different states:
    /// <list type="bullet">
    /// <item>Main Menu</item>
    /// <item>Settings</item>
    /// <item>Loading Screen</item>
    /// </list>
    /// Between the transitions of these states we also do an alpha crossfade
    /// for a more animated look.
    /// Notice that with increasing complexity comes quite a bit of code to set up
    /// the machine. The creation of the state machine is a front loaded process as
    /// we need to define exactly how the entire thing functions before we can activate it.
    /// Consider creating the state machine procedurally or via more visually intuitive means
    /// as applicable for your use case.
    /// </summary>
    void Awake()
    {
        panels.mainMenuPanel.alpha = 0f;
        panels.mainMenuPanel.blocksRaycasts = false;
        panels.mainMenuPanel.interactable = false;
        panels.settingsPanel.alpha = 0f;
        panels.settingsPanel.blocksRaycasts = false;
        panels.settingsPanel.interactable = false;
        panels.loadingPanel.alpha = 0f;
        panels.loadingPanel.blocksRaycasts = false;
        panels.loadingPanel.interactable = false;

        // We could make a state for a quit behaviour, but this is also fine in most use cases.
        panels.quitButton.onClick.AddListener(() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });

        var builder = FSMMachine.Build();

        var mainToSettingsOut = new TriggeredFSMTransition();
        var mainToSettingsIn = new TriggeredFSMTransition();

        var mainToLoadingOut = new TriggeredFSMTransition();
        var mainToLoadingIn = new TriggeredFSMTransition();

        // Main Menu State, Showing and linking buttons to state transitions
        var mainMenuState = builder.AddState(
            new ParallelFSMState
            (
                new ShowPanelFSMState(panels.mainMenuPanel),
                new HookButtonFSMState(panels.settingsButton, mainToSettingsOut),
                new HookButtonFSMState(panels.playButton, mainToLoadingOut)
            ));
        // Fade out of main menu and into settings
        var mainMenuToSettingsFadeState = builder.AddState(
            new ParallelFSMState
            (
                new FadeOutFSMState(panels.mainMenuPanel, mainToSettingsOut),
                new FadeInFSMState(panels.settingsPanel, mainToSettingsIn)
            ));
        // Settings state, Showing and linking buttons to state transitions
        var settingsState = builder.AddState(
            new ParallelFSMState
            (
                new ShowPanelFSMState(panels.settingsPanel),
                new HookButtonFSMState(panels.backButton, mainToSettingsOut)
            ));
        // Fade out of settings and into main menu
        var settingsToMainMenuFadeState = builder.AddState(
            new ParallelFSMState
            (
                new FadeOutFSMState(panels.settingsPanel, mainToSettingsOut),
                new FadeInFSMState(panels.mainMenuPanel, mainToSettingsIn)
            ));
        // Fade out of main menu and into loading
        var mainMenuToLoadingFadeState = builder.AddState(
            new ParallelFSMState
            (
                new FadeOutFSMState(panels.mainMenuPanel, mainToLoadingOut),
                new FadeInFSMState(panels.loadingPanel, mainToLoadingIn),
                new LoadingTextFSMState(panels.loadingText)
            ));

        // Fade Main Menu to Settings
        builder.AddTransition(mainToSettingsOut, mainMenuState, mainMenuToSettingsFadeState);
        builder.AddTransition(new AllPassesFSMTransition(mainToSettingsOut, mainToSettingsIn), mainMenuToSettingsFadeState, settingsState);

        // Fade Settings to Main Menu
        builder.AddTransition(mainToSettingsOut, settingsState, settingsToMainMenuFadeState);
        builder.AddTransition(new AllPassesFSMTransition(mainToSettingsOut, mainToSettingsIn), settingsToMainMenuFadeState, mainMenuState);

        // Fade Main Menu to loading
        builder.AddTransition(mainToLoadingOut, mainMenuState, mainMenuToLoadingFadeState);

        fsm = builder.Complete();
    }

    void OnEnable()
    {
        fsm.Enable();
    }

    void OnDisable()
    {
        fsm.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        fsm.Update(Time.deltaTime);
    }

    void OnDestroy()
    {
        fsm.Destroy();
        fsm = null;
    }
}
