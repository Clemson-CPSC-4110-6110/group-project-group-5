using System.Collections.Generic;
using UnityEngine;

public class QuenchingScript : MonoBehaviour
{
    Dictionary<GameObject, TemperatureScript> temperatureScripts = new();
    float temperatureLostPerSecond = 20;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("anvilSocketable")) return;

        GameObject objectWithScript = other.gameObject;
        if (!other.TryGetComponent<TemperatureScript>(out var temperatureScript))
        {
            objectWithScript = objectWithScript.transform.parent.gameObject;
            temperatureScript = other.GetComponentInParent<TemperatureScript>();
        }

        temperatureScripts[objectWithScript] = temperatureScript;
    }

    void OnTriggerExit(Collider other)
    {
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
