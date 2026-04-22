using UnityEngine;

public class BillboardingUI : MonoBehaviour
{
    [SerializeField] private BillboardType billboardType;

    [Header("Lock Rotation")]
    [SerializeField] private bool lockXRotation;
    [SerializeField] private bool lockYRotation;
    [SerializeField] private bool lockZRotation;

    private Vector3 originalRotatation;

    public enum BillboardType { LookAtCamera, CameraForward };

    private void Awake()
    {
        originalRotatation = transform.rotation.eulerAngles;
    }

    void LateUpdate()
    {
        switch (billboardType)
        {
            case BillboardType.LookAtCamera:
                transform.LookAt(Camera.main.transform.position, Vector3.up);
                break;
            case BillboardType.CameraForward:
                transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
                break;
            default:
                break;
        }
        
        Vector3 rotation = transform.rotation.eulerAngles;
        if (lockXRotation) rotation.x = originalRotatation.x;
        if (lockYRotation) rotation.y = originalRotatation.y;
        if (lockZRotation) rotation.z = originalRotatation.z;
        transform.rotation = Quaternion.Euler(rotation);
    }
}
