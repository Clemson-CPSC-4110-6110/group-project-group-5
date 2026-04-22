using System.Collections.Generic;
using UnityEngine;

public class Heater : MonoBehaviour
{
    private float temperatureAddedPerSecond = 50;
    private List<TemperatureScript> attachedTemperatureScripts = new();
    // void OnCollisionStay(Collision collision)
    // {
    //     TemperatureScript ts = collision.gameObject.GetComponent<TemperatureScript>();
    //     if (ts == null) return;

    //     ts.AddTemperature(temperatureAddedPerSecond * Time.deltaTime);
    // }
    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.TryGetComponent<TemperatureScript>(out var ts)) return;
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
    // void OnTriggerStay(Collider other)
    // {
    //     if (!other.TryGetComponent<TemperatureScript>(out var ts)) return;

    //     ts.AddTemperature(temperatureAddedPerSecond * Time.deltaTime);
    // }
    // void OnTriggerEnter(Collider other)
    // {
    //     if (!other.gameObject.TryGetComponent<TemperatureScript>(out var ts)) return;
    //     if (attachedTemperatureScripts.Contains(ts)) return;
    //     attachedTemperatureScripts.Add(ts);
    //     Debug.Log("Adding " + ts.gameObject.name + " from list. Now there are " + attachedTemperatureScripts.Count + " items");
    // }
    // void OnTriggerExit(Collider other)
    // {
    //     if (!other.gameObject.TryGetComponent<TemperatureScript>(out var ts)) return;
    //     attachedTemperatureScripts.Remove(ts);
    //     Debug.Log("Removing " + ts.gameObject.name + " from list. Now there are " + attachedTemperatureScripts.Count + " items");
    // }
    void Update()
    {
        foreach (TemperatureScript temperatureScript in attachedTemperatureScripts)
        {
            temperatureScript.AddTemp(temperatureScript.smithingMaterial.tempLostPerSecond * 1000 * Time.deltaTime);
        }
    }
}
