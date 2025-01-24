using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private InputManager inputManager;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            currentState = GameState.Gameplay;
            inputManager = GetComponent<InputManager>();
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
    private void InputManager_onEscaped()
    {
        HandlesChangingGameState();
    }
    #region GameState Logic
    public event Action<GameState> OnChangeState;
    private GameState currentState;
    public void HandlesChangingGameState()
    {
        currentState = currentState == GameState.Gameplay ? GameState.UI: GameState.Gameplay;
        OnChangeState?.Invoke(currentState);
    }

    #endregion
}
public enum GameState
{
    Gameplay,
    UI
}
