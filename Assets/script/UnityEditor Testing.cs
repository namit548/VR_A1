using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class UnityEditorTesting : MonoBehaviour
{
    [SerializeField] private List<GameObject> objectsToEnable = new List<GameObject>();
    [SerializeField] private List<GameObject> objectsToDisable = new List<GameObject>();

    public UnityEvent newEvent;

    void Start()
    {
        // Subscribe the method to the UnityEvent
        newEvent.AddListener(OnEventTriggered);
    }

    void Update()
    {
        // Optional: You can trigger the event manually for testing
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     newEvent.Invoke();
        // }
    }

    // This method will be called when the UnityEvent is triggered
    private void OnEventTriggered()
    {
        EnableObjects();
        DisableObjects();
    }

    private void EnableObjects()
    {
        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                Debug.Log($"Enabled: {obj.name}");
            }
        }
    }

    private void DisableObjects()
    {
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Debug.Log($"Disabled: {obj.name}");
            }
        }
    }

    // Optional: Public method to manually trigger the event
    public void TriggerEvent()
    {
        newEvent.Invoke();
    }

    // Optional: Clear methods for managing lists
    public void ClearEnableList()
    {
        objectsToEnable.Clear();
    }

    public void ClearDisableList()
    {
        objectsToDisable.Clear();
    }
}
