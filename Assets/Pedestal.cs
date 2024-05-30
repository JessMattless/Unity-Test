using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestal : MonoBehaviour
{
    public GameObject heldObject = null;

    public bool IsHoldingObject() { return heldObject != null; }
}
