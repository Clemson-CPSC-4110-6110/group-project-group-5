using System.Collections.Generic;
using UnityEngine;

public class QuenchingScript : MonoBehaviour
{
    [SerializeField] AudioClip audioClip;
    [SerializeField] float volume;
    Dictionary<GameObject, TemperatureScript> temperatureScripts = new();
    float temperatureLostPerSecond = 20;
    float audioPerSecond = 2;
    float timer = 0f;
    public float interval = 0.1f;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name + " Entered Trigger");
        if (!other.CompareTag("anvilSocketable")) return;
        if (!other.TryGetComponent<TemperatureScript>(out var temperatureScript))
        {
            temperatureScript = other.GetComponentInParent<TemperatureScript>();
        }
        if (temperatureScript == null)
        {
            Debug.Log("No temperature script found");
        }
        GameObject objectWithScript = temperatureScript.gameObject;
        Debug.Log("Adding objectWithScript: " + objectWithScript.name);
        temperatureScripts[objectWithScript] = temperatureScript;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<TemperatureScript>(out var temperatureScript))
        {
            temperatureScript = other.GetComponentInParent<TemperatureScript>();
        }
        if (temperatureScript == null) return;
        GameObject objectWithScript = temperatureScript.gameObject;
        temperatureScripts.Remove(objectWithScript);
        Debug.Log("Removing: " + objectWithScript.name);
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
            if (temperatureScripts[obj].GetPercentMaxTemp() == 0)
            {
                Debug.Log("Removing " + obj.name + " because it cooled off");
                temperatureScripts.Remove(obj);
            }
        }

        if (temperatureScripts.Keys.Count == 0) return;
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;

            SoundFXManager.Instance.PlaySoundFXClip(audioClip, transform, volume);
        }
    }
}
