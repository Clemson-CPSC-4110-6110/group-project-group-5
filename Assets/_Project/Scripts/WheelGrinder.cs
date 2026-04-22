using UnityEngine;

public class WheelGrinder : MonoBehaviour
{
    [SerializeField] Transform rotationPivot;
    [SerializeField] float speed = 100f;

    void Update()
    {
        rotationPivot.Rotate(speed * Time.deltaTime * Vector3.right);
    }
}
