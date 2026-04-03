using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Checklist : MonoBehaviour
{
    public GameObject taskPrefab; // Drag your TaskItem prefab here
    public Transform listContainer; // Drag the Panel with the Vertical Layout Group here

    public void AddTask(string taskDescription)
    {
        GameObject newTemplate = Instantiate(taskPrefab, listContainer);
        // Find the Text component and change it
        newTemplate.GetComponentInChildren<TextMeshProUGUI>().text = taskDescription;
        
        // Find the Toggle component to listen for completion
        Toggle toggle = newTemplate.GetComponentInChildren<Toggle>();
        toggle.onValueChanged.AddListener(delegate {
            OnTaskChanged(taskDescription, toggle.isOn);
        });
    }

    void OnTaskChanged(string name, bool isDone)
    {
        Debug.Log($"Task '{name}' is now {(isDone ? "Complete" : "Incomplete")}");
        // Add logic here (e.g., play a "ding" sound)
    }
}