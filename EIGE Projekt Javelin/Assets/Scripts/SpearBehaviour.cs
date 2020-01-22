using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearBehaviour : MonoBehaviour
{
    
    void Update()
    {
         void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.transform.name + "wurde getroffen");
        }


        
    }

   /* void freezePosition()
    {
        this.isKinematic = true;
        // spear.GetComponent<Rigidbody>().isKinematic = true;
    }
    */
}
