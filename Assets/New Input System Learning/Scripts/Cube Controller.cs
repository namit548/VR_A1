using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CubeController : MonoBehaviour
{
    private NewActions  inputActions;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputActions = new NewActions();
        inputActions.Player.MoveForward.performed += OnMoveBtnClicked;
        //  inputActions.Player.jump.performed += OnJumpBtnClicked;
        inputActions.Player.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnMoveBtnClicked(InputAction.CallbackContext context)
    {
        Vector2 move = context.ReadValue<Vector2>();
        Debug.Log("Move= " + move);
        
    }
    private void OnJumpBtnClicked()
    {
        Debug.Log("Jumped");
    }
    private void OnDisable()
    {
        inputActions.Player.Disable();
    }
}
