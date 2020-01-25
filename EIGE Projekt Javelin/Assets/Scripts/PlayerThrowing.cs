using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThrowing
{
    public GameObject direction;
    public Rigidbody spear;
    public float throwForce;
    public void Throw()
    {
        spear.transform.parent = null;
        spear.isKinematic = false;
        spear.AddForce(Camera.main.transform.TransformDirection(Vector3.forward) * throwForce, ForceMode.Impulse);
        spear.AddTorque(spear.transform.TransformDirection(Vector3.up) * 100, ForceMode.Impulse);
    }
}
