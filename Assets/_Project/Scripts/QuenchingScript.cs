using System.Collections.Generic;
using UnityEngine;

public class QuenchingScript : MonoBehaviour
{
    Dictionary<GameObject, TemperatureScript> temperatureScripts = new();
    float temperatureLostPerSecond = 20;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("entered object: " + other.gameObject.name);

        if (!other.CompareTag("anvilSocketable")) return;

        if (!other.TryGetComponent<TemperatureScript>(out var temperatureScript))
        {
            temperatureScript = other.GetComponentInParent<TemperatureScript>();
        }
        GameObject objectWithScript = temperatureScript.gameObject;
        Debug.Log("objectWithScript: " + objectWithScript.name);
        temperatureScripts[objectWithScript] = temperatureScript;
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Removing: " + other.gameObject.name);
        temperatureScripts.Remove(other.gameObject);
    }

    // void OnTriggerStay(Collider other)
    // {
    // if (!other.CompareTag("anvilSocketable")) return;

    // if (!other.TryGetComponent<TemperatureScript>(out var temperatureScript))
    //     temperatureScript = other.GetComponentInParent<TemperatureScript>();

    // temperatureScript.AddTemp(-50f * Time.fixedDeltaTime);

    // }

    void Update()
    {
        foreach (GameObject obj in temperatureScripts.Keys)
        {
            temperatureScripts[obj].AddTemp(-temperatureLostPerSecond * Time.deltaTime);
        }
    }
}
