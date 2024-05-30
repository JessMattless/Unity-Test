using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : GrabbableObject
{
    public GameObject TargetPedestal;
    public GameObject currentPedestal;

    private float minFloatHeight;
    private float maxFloatHeight;
    private int floatDir = 1;

    public override void Start()
    { 
        base.Start();

        ResetPosition();
        ResetRotation();

        if (currentPedestal != null)
        {
            /* If the object is attached to a pedestal, check if the pedestal has an item already
             * If it doesn't, assign this item as the pedestal's item
             * If it does, drop the current item on the ground
             */
            if (currentPedestal.GetComponent<Pedestal>().heldObject == null)
                currentPedestal.GetComponent<Pedestal>().heldObject = gameObject;
            else OnDrop();
        }

    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // If the object is on a pedestal, float atop the pedestal
        if (currentPedestal != null)
        {
            switch (floatDir)
            {
                case 1:
                    if (transform.position.y < maxFloatHeight)
                        transform.position = transform.position + new Vector3(0, 0.0015f, 0);
                    else floatDir = -1;
                    break;
                case -1:
                    if (transform.position.y > minFloatHeight)
                        transform.position = transform.position - new Vector3(0, 0.0015f, 0);
                    else floatDir = 1;
                    break;
            }
        }

    }

    // used to reset the position/rotation/movement of the object when placed on a pedestal
    public void ResetPosition()
    {
        minFloatHeight = currentPedestal.GetComponent<MeshRenderer>().bounds.extents.y
            + currentPedestal.transform.position.y
            + GetComponent<MeshRenderer>().bounds.extents.y;
        maxFloatHeight = currentPedestal.GetComponent<MeshRenderer>().bounds.extents.y
            + currentPedestal.transform.position.y
            + 0.15f
            + GetComponent<MeshRenderer>().bounds.extents.y;

        transform.position = new Vector3(currentPedestal.transform.position.x, minFloatHeight, currentPedestal.transform.position.z);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, currentPedestal.transform.eulerAngles.y, transform.eulerAngles.z);
    }

    public override void OnPickup(GameObject holder)
    {
        base.OnPickup(holder);
        RemoveFromPedestal();
    }

    public override void OnDrop()
    {
        base.OnDrop();
        RemoveFromPedestal();
    }

    public void OnPlace(GameObject pedestal)
    {
        ResetRotation();
        holder = null;
        rigidbody.useGravity = false;
        currentPedestal = pedestal;
        currentPedestal.GetComponent<Pedestal>().heldObject = gameObject;
        rigidbody.freezeRotation = true;
        ResetPosition();
        ResetSpeed();
        gameObject.GetComponent<Collider>().enabled = true;
    }

    public bool IsPlaced()
    {
        return currentPedestal != null;
    }

    private void RemoveFromPedestal()
    {
        if (currentPedestal != null)
        {
            rigidbody.freezeRotation = false;
            currentPedestal.GetComponent<Pedestal>().heldObject = null;
            currentPedestal = null;
        }
    }
}

