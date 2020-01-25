using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAcrobatics
{
    private float maxWinkel;
    private float nullWinkel;
    private float hangWinkel;
    private bool runterwärts;
    private float rotateAmount;

    private AcrobaticSettings acrobaticSettings;
    private PlayerBehaviour playerBehaviour;
    private Rigidbody playerRigidbody;
    private GameObject playerGameObject;

    private Transform reckstange;

    public void hangOntoReck(Collider other)
    {
        playerBehaviour.changeDoing(CurrentAction.SWINGING);
        playerRigidbody.useGravity = false;
        playerRigidbody.isKinematic = true;
        reckstange = other.transform.GetChild(0);

        hangWinkel = 0;

        float winkelOfDoom, ankathete, gegenkathete;

        gegenkathete = Mathf.Abs(Mathf.Abs(reckstange.position.y) - Mathf.Abs(playerGameObject.transform.position.y));
        ankathete = Mathf.Abs(Mathf.Abs(reckstange.position.x) - Mathf.Abs(playerGameObject.transform.position.x));
        winkelOfDoom = Mathf.Rad2Deg * Mathf.Atan2(gegenkathete, ankathete);
        ankathete *= reckstange.position.x > playerGameObject.transform.position.x ? 1 : -1;

        winkelOfDoom = 180 - 90 - winkelOfDoom;

        Debug.Log("Ankathete: " + ankathete + "\tGegenkathete: " + gegenkathete + "\tWinkel: " + winkelOfDoom);

        //transform.rotation = Quaternion.LookRotation(reckstange.rotation.eulerAngles);
        Quaternion.LookRotation(reckstange.right, reckstange.forward);
        //                transform.RotateAround(transform.position, reckstange.up, 90);
        playerGameObject.transform.RotateAround(playerGameObject.transform.position, reckstange.forward, (ankathete > 0 ? winkelOfDoom : -winkelOfDoom));
        //transform.RotateAround(transform.position, reckstange.up, (ankathete > 0 ? 90 : -90));


        reckstange.Rotate(0, 0, winkelOfDoom + (ankathete > 0 ? 0 : -90));
        playerGameObject.transform.SetParent(reckstange);

        maxWinkel = winkelOfDoom < 3 ? 3 : (winkelOfDoom + (ankathete > 0 ? 0 : 90));
        nullWinkel = -winkelOfDoom;
        runterwärts = true;
    }
    public PlayerAcrobatics(GameObject playerGameObject)
    {
        this.playerGameObject = playerGameObject;
        this.playerRigidbody = playerGameObject.GetComponent<Rigidbody>();
    }

    public void ForwardSwinging(float forwardInput)
    {
        rotateAmount = 0;

        float hangWinkelAlt = hangWinkel;
        hangWinkel = reckstange.rotation.eulerAngles.z - 180;
        if (hangWinkelAlt * hangWinkel < 0) {
            runterwärts = !runterwärts;
            if (forwardInput == 0) {
                if (maxWinkel >= acrobaticSettings.chillWinkel + acrobaticSettings.deceleration) {
                    maxWinkel -= (acrobaticSettings.deceleration + maxWinkel) / 8f; 
                } else {
                    maxWinkel = acrobaticSettings.chillWinkel;
                }
            }
        } else if (Mathf.Abs(Mathf.Abs(hangWinkel) - Mathf.Abs(maxWinkel)) < 2 || Mathf.Abs(hangWinkel) - Mathf.Abs(maxWinkel) > 0) {
            runterwärts = true;
            if (forwardInput != 0)
                maxWinkel += acrobaticSettings.acceleration * (maxWinkel > 0 ? 1 : -1);

            if (maxWinkel > 130) maxWinkel = 130;
            if (maxWinkel < acrobaticSettings.chillWinkel) maxWinkel = acrobaticSettings.chillWinkel;
        }


        if (runterwärts) {

            rotateAmount = ((Mathf.Abs(maxWinkel) - Mathf.Abs(hangWinkel)) / acrobaticSettings.overallSlowness) * (hangWinkel > 0 ? -1 : 1);
            if (rotateAmount == 0) rotateAmount = (hangWinkel > 0 ? -0.1f : 0.1f);
        } else {
            rotateAmount = ((Mathf.Abs(hangWinkel) - Mathf.Abs(maxWinkel)) / acrobaticSettings.overallSlowness) * (hangWinkel > 0 ? -1 : 1);
            if (rotateAmount == 0) rotateAmount = (hangWinkel > 0 ? 0.1f : -0.1f);
        }

        //            Debug.Log(rotateAmount);
        //Debug.Log((runterwärts? "Runter" : "Hinauf") + ",\thangWinkel=" + hangWinkel + ",\tmaxWinkel=" + maxWinkel + ",\tnullWinkel=" + nullWinkel + ",\tamount=" + rotateAmount);
        reckstange.Rotate(new Vector3(0, 0, rotateAmount));
    }
        
    

    public CurrentAction JumpSwinging()
    {
        playerGameObject.transform.SetParent(null);
        
        playerRigidbody.isKinematic = false;
        playerRigidbody.useGravity = true;
        reckstange.transform.rotation = Quaternion.Euler(180, 0, 0);

        Vector3 absprungRichtung = playerRigidbody.transform.forward;
        Debug.Log(rotateAmount);
        playerRigidbody.velocity = absprungRichtung * rotateAmount * -acrobaticSettings.absprungPower;
        return CurrentAction.FREERUNNING;
    }
}
 

[System.Serializable]
public class AcrobaticSettings
{
    public float chillWinkel = 3;
    public float initialRotateSpeed = 3;
    public float acceleration = 0.1f;
    public float deceleration = 0.3f;
    public float overallSlowness = 15;
    public float absprungPower = 3;

}
