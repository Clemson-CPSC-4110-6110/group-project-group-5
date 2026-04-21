using UnityEngine;

public class MoveSwordTipToEndOfBlade : MonoBehaviour
{
    [SerializeField] Transform bladeBodyPivot;
    [SerializeField] Transform bladeTipPivot;
    [SerializeField] GameObject bladeBodyComponent;
    
    public void UpdateTipPivotPosition()
    {
        float bladeBodyLength = bladeBodyComponent.GetComponent<MeshFilter>().sharedMesh.bounds.size.z * bladeBodyComponent.transform.localScale.z;
        bladeTipPivot.localPosition = bladeBodyPivot.localPosition + new Vector3(0,bladeBodyLength,0);
    }
}
