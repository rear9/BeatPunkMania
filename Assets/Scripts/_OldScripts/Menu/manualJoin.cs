/*using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class manualJoin : MonoBehaviour
{
    public PlayerInput p1Input;
    public PlayerInput p2Input;

    void Start()
    {
        InputUser.PerformPairingWithDevice(Keyboard.current, p1Input.user);
        p1Input.SwitchCurrentControlScheme("P1", Keyboard.current);
        InputUser.PerformPairingWithDevice(Keyboard.current, p2Input.user);
        p2Input.SwitchCurrentControlScheme("P2", Keyboard.current);
    }
}*/