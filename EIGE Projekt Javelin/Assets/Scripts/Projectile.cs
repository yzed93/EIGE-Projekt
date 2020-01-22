using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour


{
    public Transform targetedObject;
    public float projectileSpeed;

    // Start is called before the first frame update
  

    // Update is called once per frame
    void Update()
    {
        float amountToMove = projectileSpeed * Time.deltaTime;
        transform.Translate(Vector3.forward * amountToMove);
    }
}
