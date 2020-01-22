using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Vector3 targetPosition;
    public PlayerBehaviour jarid;

    [SerializeField]
    public Transform followedObject;

    private float smooth;
    private float heightOverPlayer;
    private float distanceBehindPlayer;

    [SerializeField]
    private float aimingOverRightShoulder;
    [SerializeField]
    private float aimingOverPlayer;
    [SerializeField]
    private float aimingBehindPlayer;
    [SerializeField]
    private int turningAmount;

    [SerializeField]
    private int smoothnessOfDoom;
    

    void Start()
    {
        heightOverPlayer = 2;
        distanceBehindPlayer = 5;
        smooth = 100;
        aimingBehindPlayer = 1.6f;
        aimingOverPlayer = 0.3f;
        aimingOverRightShoulder = 0.8f;
        turningAmount = 30;
    }

    public void transition()
    {
        this.smooth = 100;
    }

    public void transitionOver()
    {
        this.smooth = 0;
    }

    void LateUpdate()
    {
        if (jarid.getCurrentAction() == CurrentAction.SWINGING) {
            targetPosition = jarid.getReckstange().transform.position;
            targetPosition += new Vector3(followedObject.rotation.y / 10, 0, followedObject.rotation.y-90 / 10);
            transform.position = targetPosition;
            
            transform.LookAt(jarid.transform);
        }
        else if (jarid.getCurrentAction() == CurrentAction.AIMING) {
            targetPosition = followedObject.position + aimingOverPlayer * Vector3.up - aimingBehindPlayer * followedObject.forward + aimingOverRightShoulder * followedObject.right;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smooth);
            transform.LookAt(followedObject, Vector3.up * 30);
            transform.Rotate(0, turningAmount, 0, Space.World);

        } else if (jarid.getCurrentAction() == CurrentAction.FREERUNNING) {

            targetPosition = followedObject.position + heightOverPlayer * Vector3.up - distanceBehindPlayer * followedObject.forward;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smooth);
            transform.LookAt(followedObject, Vector3.up * 30);
        }
    }
}
