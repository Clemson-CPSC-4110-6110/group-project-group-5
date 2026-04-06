using System.Collections.Generic;
using UnityEngine;

public class Heater : MonoBehaviour
{
    private float temperatureAddedPerSecond = 50;
    private List<TemperatureScript> attachedTemperatureScripts;
    void OnCollisionEnter(Collision collision)
    {
        TemperatureScript ts = collision.gameObject.GetComponent<TemperatureScript>();
        if (ts == null) return;
        if (attachedTemperatureScripts.Contains(ts)) return;
        attachedTemperatureScripts.Add(ts);
        Debug.Log("Adding " + ts.gameObject.name + " from list. Now there are " + attachedTemperatureScripts.Count + " items");
    }
    void OnCollisionExit(Collision collision)
    {
        TemperatureScript ts = collision.gameObject.GetComponent<TemperatureScript>();
        if (ts == null) return;
        attachedTemperatureScripts.Remove(ts);
        Debug.Log("Removing " + ts.gameObject.name + " from list. Now there are " + attachedTemperatureScripts.Count + " items");
    }
    void Update()
    {
        foreach (TemperatureScript temperatureScript in attachedTemperatureScripts)
        {
            temperatureScript.AddTemperature(temperatureAddedPerSecond * Time.deltaTime);
        }
    }
}
