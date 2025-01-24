using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private GameManager gameManager;
    private PlayerInputSystem inputSystem;
    #region MONOBEHAVIOUR CALLBACKS
    private void Awake()
    {
        gameManager = GetComponent<GameManager>();
        inputSystem = new PlayerInputSystem();
        inputSystem.Gameplay.Enable();
        inputSystem.General.Enable();
    }
    private void Start()
    {
        GameManager.instance.OnChangeState += Instance_OnChangeState;
        inputSystem.General.Escape.performed += Escape_performed;
        GameplayMapEnabled();
    }
    private void OnDisable()
    {
        GameManager.instance.OnChangeState -= Instance_OnChangeState;
        inputSystem.General.Escape.performed -= Escape_performed;
        GameplayMapDisabled();
    }
    #endregion
    #region General Input Event Logic
    public static event Action onEscaped;
    private void Escape_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }
    #endregion
    #region Gameplay Input Event Logic
    public static event Action buttonMashed;
    public void GameplayMapEnabled()
    {
        inputSystem.Gameplay.Mashing.performed += Mashing_performed;
    }
    public void GameplayMapDisabled()
    {
        inputSystem.Gameplay.Mashing.performed -= Mashing_performed;
    }
    private void Mashing_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        buttonMashed?.Invoke();
    }
    #endregion
    #region UI Input Event Logic
    #endregion
    private void Instance_OnChangeState(GameState obj)
    {
        
    }
}
