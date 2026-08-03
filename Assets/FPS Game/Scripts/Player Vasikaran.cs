using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
public class Playervasikaran : MonoBehaviour
{
    [SerializeField] float walkSpeed = 5f;
    Vector2 moveInput;
   Rigidbody  rb;

    private void Start()
    {
        rb= GetComponent<Rigidbody>();
    }
    private void Update()
    {
        Run();
    }
    private void Run()
    {
        Vector3 playerVelocity = new Vector3(moveInput.x*walkSpeed,rb.linearVelocity.y,moveInput.y*walkSpeed);
        rb.linearVelocity = playerVelocity;
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}
