using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorSide { Right, Left };

public class Door : SolvableObject
{
    protected override void FixedUpdate()
    {
        if (CheckNeededObjects())
        {
            GetComponent<Animation>().Play();
        }
    }

    public override bool CheckNeededObjects()
    {
        foreach (GameObject obj in neededObjects)
        {
            // Using a switch case for adding more object types later
            switch(obj.tag)
            {
                case "Key":
                    // Check if each key for the door is in the correct pedestal
                    if (obj.GetComponent<Key>().currentPedestal != obj.GetComponent<Key>().TargetPedestal)
                        return false;
                    break;
                default:
                    break;
            }
        }

        return true;
    }
}
