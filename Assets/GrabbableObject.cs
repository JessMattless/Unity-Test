using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public class GrabbableObject : MonoBehaviour
{
    public Vector3 defaultRotation;

    protected GameObject holder;
    protected new Rigidbody rigidbody;


    // Start is called before the first frame update
    public virtual void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        holder = null;

    }

    // Update is called once per frame
    protected virtual void FixedUpdate()
    {
        // Else if the object is being held, then move/rotate with the holder
        if (holder != null)
        {
            transform.position = holder.transform.position;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, holder.transform.eulerAngles.y, transform.eulerAngles.z);
        }
    }

    // Reset the rotation back to the default
    public void ResetRotation()
    {
        transform.eulerAngles = defaultRotation;
    }

    // Reset any speed/velocity to zero, useful when picking up a moving item
    public void ResetSpeed()
    {
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }

    public virtual void OnPickup(GameObject holder)
    {
        ResetSpeed();
        this.holder = holder;
        rigidbody.useGravity = false;
        
        ResetRotation();
        gameObject.GetComponent<Collider>().enabled = false;
    }

    public virtual void OnDrop()
    {
        rigidbody.useGravity = true;
        gameObject.GetComponent<Collider>().enabled = true;

        holder = null;
    }


}