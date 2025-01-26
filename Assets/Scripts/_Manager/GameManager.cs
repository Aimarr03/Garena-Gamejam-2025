using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private InputManager inputManager;
    public bool paused { get; private set; }
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            HandlesChangingGameState(GameState.MainMenu);
            SwitchCamera(mainCamera);
            inputManager = GetComponent<InputManager>();
            IntroScene();
        }
        else
        {
            Destroy(this);
        }
    }
    
    private void Start()
    {
        InputManager.onEscaped += InputManager_onEscaped;   
    }
    private void OnDisable()
    {
        InputManager.onEscaped -= InputManager_onEscaped;
    }
    private void Update()
    {
        if (paused) return;
        switch(currentState)
        {
            case GameState.MainMenu:
                MainMenuStart();
                break;
        }
    }
    #region MainMenu
    [Header("Main Menu Components")]
    [Header("Text")]
    [SerializeField] private CanvasGroup Text_MainMenu;
    [SerializeField] private TextMeshProUGUI Text_Help;

    [Header("Image")]
    [SerializeField] private Image Background_Black;
    [SerializeField] private Image Image_Title;

    [Header("Cinemachine Virtual Camera")]
    [SerializeField] private CinemachineCamera mainCamera;
    [SerializeField] private CinemachineCamera lowerCamera;
    [SerializeField] private CinemachineCamera upperCamera;

    [SerializeField]private List<CinemachineCamera> ListOfCamera = new List<CinemachineCamera>();
    private async void IntroScene()
    {
        if (currentState != GameState.MainMenu) return;
        paused = true; 
        Background_Black.gameObject.SetActive(true);
        Image_Title.gameObject.SetActive(false);
        Text_Help.gameObject.SetActive(false);

        Background_Black.DOFade(0, 2.5f);
        await Task.Delay(3500);

        Image_Title.gameObject.SetActive(true);
        Text_Help.gameObject.SetActive(true);
        Image_Title.DOFade(0f,0);
        Text_Help.alpha = 0f;

        Image_Title.DOFade(1, 2f);
        Text_Help.DOFade(1, 1.3f).SetDelay(2.5f).OnComplete(() =>
        {
            paused = false;
            Text_Help.DOFade(0.35f, 1.6f).SetLoops(-1, LoopType.Yoyo).SetDelay(0.4f).SetEase(Ease.InOutElastic);
        });
        
    }
    [Header("Gameplay")]
    [SerializeField] private Transform slaveryPanel;
    [SerializeField] private Transform LevelContainer;
    [SerializeField] private RectTransform Algojo;
    
    private async void TransitionToGameplay()
    {
        paused = true;
        Text_Help.DOKill();
        Text_MainMenu.DOFade(0, 0.3f);
        await Task.Delay(800);
        SwitchCamera(lowerCamera);
        await Task.Delay(800);
        slaveryPanel.DOLocalMoveY(0, 0.4f).SetEase(Ease.OutCubic).SetDelay(0.4f);
        Algojo.DOLocalMoveX(1080, 0.8f).SetEase(Ease.OutCubic).SetDelay(0.3f);
        await Task.Delay(500);
        for (int index = 0; index < LevelContainer.childCount; index++)
        {
            Transform childElement = LevelContainer.GetChild(index);
            childElement.DOLocalMoveY(-140, 0.3f).SetEase(Ease.OutSine);
            await Task.Delay(100);
        }
        await Task.Delay(1500);
        paused = false;
        HandlesChangingGameState(GameState.Gameplay);
        
    }
    private void MainMenuStart()
    {
        if (Input.anyKey)
        {
            KeyCode firstKey = KeyCode.None;
            KeyCode secondKey = KeyCode.None;
            //Debug.Log("First Key " + firstKey);
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(key))
                {
                    if (firstKey == KeyCode.None)
                    {
                        firstKey = key;
                    }
                    else if (secondKey == KeyCode.None && key != firstKey)
                    {
                        secondKey = key;
                        break;
                    }
                }
            }
            if (firstKey != KeyCode.None && secondKey != KeyCode.None)
            {
                Debug.Log($"Keys pressed: {firstKey} and {secondKey}");
                TransitionToGameplay();
            }
        }
    }
    #endregion

    private void InputManager_onEscaped()
    {
        switch (currentState)
        {
            case GameState.Gameplay:
                HandlesChangingGameState(GameState.Paused);
                break;
            case GameState.Paused:
                HandlesChangingGameState(GameState.Gameplay);
                break;
            case GameState.MainMenu:

                break;
        }
        
    }
    #region GameState Logic
    public event Action<GameState> OnChangeState;
    public bool CheckGameState(GameState state) => state == currentState;
    private GameState currentState;
    public void HandlesChangingGameState(GameState newState)
    {
        currentState = newState;
        OnChangeState?.Invoke(currentState);
    }

    #endregion
    private void SwitchCamera(CinemachineCamera newCamera)
    {
        Debug.Log("Switch to " + newCamera.ToString());
        foreach(CinemachineCamera currentCamera in ListOfCamera)
        {
            bool check = newCamera == currentCamera;
            Debug.Log($"Checking current Camera {currentCamera} => {check}");
            currentCamera.Priority = check ? 10 : 0;
        }
    }
}
public enum GameState
{
    MainMenu,
    Gameplay,
    Paused
}
