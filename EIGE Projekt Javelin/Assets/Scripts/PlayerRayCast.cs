using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRayCast : MonoBehaviour
{
    private CurrentAction doing;
    public Camera thirdPersonCam;
    public GameObject player;

    public CurrentAction getCurrentAction()
    {
        return doing;
    }
    
  void Awake()
    {
        doing = CurrentAction.FREERUNNING;
    }
    
    void Update()
    {
        this.getCurrentAction();
        if (doing == CurrentAction.AIMING && Input.GetMouseButtonDown(0))
        {

            Shoot();

        }
    }

    void Shoot()
    {
        RaycastHit hit;

        if (Physics.Raycast(thirdPersonCam.transform.position, thirdPersonCam.transform.forward, out hit))
        {
            Debug.Log(hit.transform.name);
        }
    }
}
