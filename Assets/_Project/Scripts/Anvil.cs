using UnityEngine;

public class StickySurface : MonoBehaviour
{
    public float stickForce = 5000f;   // How strong the stick is

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody otherRb = collision.rigidbody;

        if (otherRb != null && otherRb.gameObject.GetComponent<FixedJoint>() == null)
        {
            // Create a joint to "stick" the object to this surface
            FixedJoint joint = otherRb.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = GetComponent<Rigidbody>();

            // If surface has no rigidbody, connect to world
            if (joint.connectedBody == null)
                joint.connectedBody = null;

            joint.breakForce = stickForce;
            joint.breakTorque = stickForce;
        }
    }
}