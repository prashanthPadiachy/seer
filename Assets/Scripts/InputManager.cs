using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput PlayerInput;

    public static Vector2 Movement;
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;
    public static bool RunIsHeld;
    public static bool RestartWasPressed;
    public static bool NextLevelWasPressed;
    public static bool PreviousLevelWasPressed;

    private InputAction _moveAction;  
    private InputAction _jumpAction;
    private InputAction _runAction;
    private InputAction _restartAction;
    private InputAction _nextLevelAction;
    private InputAction _previousLevelAction;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _runAction = PlayerInput.actions["Run"];
        _restartAction = PlayerInput.actions["Restart"];
        _nextLevelAction = PlayerInput.actions["NextLevel"];
        _previousLevelAction = PlayerInput.actions["PreviousLevel"];
    }
    // Update is called once per frame
    void Update()
    {
        Movement = _moveAction.ReadValue<Vector2>();

        JumpWasPressed = _jumpAction.WasPressedThisFrame();
        JumpIsHeld = _jumpAction.IsPressed();
        JumpWasReleased = _jumpAction.WasReleasedThisFrame();

        RunIsHeld = _runAction.IsPressed();
        RestartWasPressed = _restartAction.WasPressedThisFrame();

        NextLevelWasPressed = _nextLevelAction.WasPressedThisFrame();
        PreviousLevelWasPressed = _previousLevelAction.WasPressedThisFrame();
    }
}
