using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SolvableObject : MonoBehaviour
{
    public List<GameObject> neededObjects;

    protected virtual void FixedUpdate() { }

    public abstract bool CheckNeededObjects();
}
