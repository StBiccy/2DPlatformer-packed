using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController : MonoBehaviour
{
    private CharacterInputs input;
    private Vector2 direction;

    private void Awake()
    {
        input = new CharacterInputs();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Move.performed += OnMovement;
        input.Player.Move.canceled += OnMovementCanceled;
    }

    private void FixedUpdate()
    {
        
    }

    private void OnDisable()
    {
        input.Enable();
        input.Player.Move.performed -= OnMovement;
        input.Player.Move.canceled -= OnMovementCanceled;
    }

    private void OnMovement(InputAction.CallbackContext value)
    {
        direction = value.ReadValue<Vector2>();
    }

    private void OnMovementCanceled(InputAction.CallbackContext value)
    {
        direction = Vector2.zero;
    }
}
