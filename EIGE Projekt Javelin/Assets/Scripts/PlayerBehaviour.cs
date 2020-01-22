using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class PlayerBehaviour : MonoBehaviour
{
    private CurrentAction doing;

    public CurrentAction getCurrentAction() {
        return doing;
    }

    /* ++++++++++++++++++++++++++++++++ */
    /*         ++++++++++++++++         */
    /*          INITIALISATION          */
    /*         ++++++++++++++++         */
    /* ++++++++++++++++++++++++++++++++ */

    void Awake() {
        playerRigidbody = gameObject.GetComponent<Rigidbody>();
        velocity = Vector3.zero;
        forwardInput = sidewaysInput = lookupInput = turnInput = jumpInput = 0;
        targetRotation = transform.rotation;
        moving = false;
        crosshair.SetActive(false);
        doing = CurrentAction.FREERUNNING;
        Cursor.visible = false;
    }
   


    /* +++++++++++++++ */
    /*     +++++++     */
    /*      INPUT      */
    /*     +++++++     */
    /* +++++++++++++++ */

    public MoveSettings moveSettings;
    public InputSettings inputSettings;
    private float forwardInput, sidewaysInput, turnInput, lookupInput, jumpInput;
    private bool mouseOnePressed, mouseTwoPressed, mouseTwoHeld, mouseTwoReleased;

    void GetInput() {
        if (inputSettings.FORWARD_AXIS.Length != 0) {
            forwardInput = Input.GetAxis(inputSettings.FORWARD_AXIS);
        }

        if (inputSettings.SIDEWAYS_AXIS.Length != 0) {
            sidewaysInput = Input.GetAxis(inputSettings.SIDEWAYS_AXIS);
        }

        if (inputSettings.JUMP_AXIS.Length != 0)
            jumpInput = Input.GetAxis(inputSettings.JUMP_AXIS);
        if (inputSettings.LOOKUP_AXIS.Length != 0)
            lookupInput = Input.GetAxis(inputSettings.LOOKUP_AXIS);
        if (inputSettings.TURN_AXIS.Length != 0)
            turnInput = Input.GetAxis(inputSettings.TURN_AXIS);

        mouseOnePressed = Input.GetMouseButtonDown(0);
        mouseTwoHeld = Input.GetMouseButton(1);
        mouseTwoPressed = Input.GetMouseButtonDown(1);
        mouseTwoReleased = Input.GetMouseButtonUp(1);
    }

    /* ++++++++++++++++++++ */
    /*      ++++++++++      */
    /*       MOVEMENT       */
    /*      ++++++++++      */
    /* ++++++++++++++++++++ */

    private Rigidbody playerRigidbody;
    private Vector3 velocity;
    private Quaternion targetRotation;
    private bool moving;

    public bool isMoving() {
        return moving;
    }

    private float maxWinkel;
    private float nullWinkel;
    private float hangWinkel;
    private bool runterwärts;
    private float rotateAmount;
    
    public AcrobaticSettings acrobaticSettings;
    
    void Forward() {
        if (doing == CurrentAction.SWINGING) {
            rotateAmount = 0;

            float hangWinkelAlt = hangWinkel;
            hangWinkel = reckstange.rotation.eulerAngles.z - 180;
            if (hangWinkelAlt * hangWinkel < 0) {
                runterwärts = !runterwärts;
                if (forwardInput == 0) {
                    if (maxWinkel >= acrobaticSettings.chillWinkel + acrobaticSettings.deceleration)
                    {
                        maxWinkel -= (acrobaticSettings.deceleration + maxWinkel) / 8f;
                    }else
                    {
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

        } else if (Grounded()) {
            velocity.z = forwardInput * moveSettings.runSpeed * ((doing == CurrentAction.AIMING)? 0.5f : 1f);
            velocity.x = sidewaysInput * moveSettings.runSpeed * ((doing == CurrentAction.AIMING) ? 0.5f : 1f);
            velocity.y = playerRigidbody.velocity.y;

            playerRigidbody.velocity = transform.TransformDirection(velocity);
        }
    }


    void Jump() {
        if (jumpInput != 0) {
            if (doing == CurrentAction.FREERUNNING) {
                if (Grounded()) {
                    playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, moveSettings.jumpVelocity, playerRigidbody.velocity.z);
                }
            } else if (doing == CurrentAction.SWINGING) {
                this.transform.SetParent(null);
                doing = CurrentAction.FREERUNNING;
                playerRigidbody.isKinematic = false;
                playerRigidbody.useGravity = true;
                reckstange.transform.rotation = Quaternion.Euler(180, 0, 0);

                Vector3 absprungRichtung = playerRigidbody.transform.forward;
                Debug.Log(rotateAmount);
                playerRigidbody.velocity = absprungRichtung * rotateAmount * -acrobaticSettings.absprungPower;

            }
        }
    }

    /* +++++++++++++++++++++++++++++++ */
    /*         +++++++++++++++         */
    /*          JAVELIN THROW                 -- - - - - - ----- ====================HH>             */
    /*         +++++++++++++++         */
    /* +++++++++++++++++++++++++++++++ */

    public GameObject crosshair;
    
    private void MaybeAim() {
        
        if (doing == CurrentAction.FREERUNNING) {
            if (mouseTwoPressed) {
                doing = CurrentAction.AIMING;
                crosshair.SetActive(true);
            }
        } else if (doing == CurrentAction.AIMING) {
            if (mouseTwoReleased)
            {
                doing = CurrentAction.FREERUNNING;
                
                crosshair.SetActive(false);
                
            }
            /*if (!mouseTwoHeld) {
                doing = CurrentAction.FREERUNNING;
            }*/
        }
    }


    /* ++++++++++++++++ */
    /*     ++++++++     */
    /*      SHOOTING      */
    /*     ++++++++     */
    /* ++++++++++++++++ */

    public void maybeShoot() {
       
        if (doing == CurrentAction.AIMING) {

           
                if (Input.GetMouseButtonDown(0)) {

                Throw();
                    
                }
            
        }
    }

    public GameObject direction;
    public Rigidbody spear;
    public float throwForce;

    void Throw()
    {
        spear.transform.parent = null;
        spear.isKinematic = false;
        spear.AddForce(Camera.main.transform.TransformDirection(Vector3.forward) * throwForce, ForceMode.Impulse);
        spear.AddTorque(spear.transform.TransformDirection(Vector3.up) * 100, ForceMode.Impulse);
    }
    

    /*
     * 
    /* ++++++++++++++++ */
    /*     ++++++++     */
    /*      CAMERA      */
    /* ++++++++++++++++ */
    
      
         public Transform viewDirection;
         public Transform viewVertical;

    void Turn() {
        if (doing == CurrentAction.FREERUNNING || doing == CurrentAction.AIMING)
        {
            
            float turningAmount = moveSettings.rotateVelocity * turnInput * Time.deltaTime;
            if (Mathf.Abs(turnInput) > 0)
            {
                targetRotation *= Quaternion.AngleAxis(turningAmount, Vector3.up);
            }
            transform.rotation = targetRotation;

            float lookupAmount = moveSettings.lookupVelocity * lookupInput * Time.deltaTime * (-1);
            viewVertical.Rotate(lookupAmount, 0, 0, Space.Self);

        }
    }

    

 



    /* +++++++++++++++++++++++++++++++++++++++++++++++++++ */
    /*              +++++++++++++++++++++++++              */
    /*               ENVIRONMENT INTERACTION               */
    /*              +++++++++++++++++++++++++              */
    /* +++++++++++++++++++++++++++++++++++++++++++++++++++ */

    private Transform reckstange;

    public Transform getReckstange()
    {
        return reckstange;
    }

    bool Grounded() {
        return Physics.Raycast(transform.position, Vector3.down, moveSettings.distanceToGround, moveSettings.ground);
    }

    void OnTriggerEnter(Collider other) {
        Collider col = other.gameObject.GetComponent<Collider>();
        Collider myCol = this.gameObject.GetComponent<Collider>();

        if (other.gameObject.tag == "kantenCollider")
        {
            Debug.Log("Klettern");
            float zielhoehe = other.gameObject.transform.position.y + myCol.bounds.extents.y;
            float zielposiX = 0;
            float zielposiZ = 0;
            if (playerRigidbody.velocity.x > 0.1)
            {
                zielposiX = 2;
            }
            else if (playerRigidbody.velocity.x < -0.1)
            {
                zielposiX = -2;
            }

            if (playerRigidbody.velocity.y > 0.1)
            {
                zielposiZ = -2;
            }
            else if (playerRigidbody.velocity.y < -0.1)
            {
                zielposiZ = 2;
            }

            if (Mathf.Abs(playerRigidbody.velocity.x) > Mathf.Abs(playerRigidbody.velocity.z))
            {
                zielposiZ = 0;
            }
            else
            {
                zielposiX = 0;
            }
            playerRigidbody.velocity = Vector3.zero;
            transform.position = new Vector3(transform.position.x + zielposiX, zielhoehe, transform.position.z + zielposiZ);
        }
        else if (other.tag == "Reckstange")
        {
            if (doing != CurrentAction.SWINGING)
            {
                this.doing = CurrentAction.SWINGING;
                playerRigidbody.useGravity = false;
                playerRigidbody.isKinematic = true;
                reckstange = other.transform.GetChild(0);

                hangWinkel = 0;

                float winkelOfDoom, ankathete, gegenkathete;

                gegenkathete = Mathf.Abs(Mathf.Abs(reckstange.position.y) - Mathf.Abs(transform.position.y));
                ankathete = Mathf.Abs(Mathf.Abs(reckstange.position.x) - Mathf.Abs(transform.position.x));
                winkelOfDoom = Mathf.Rad2Deg * Mathf.Atan2(gegenkathete, ankathete);
                ankathete *= reckstange.position.x > transform.position.x ? 1 : -1;

                winkelOfDoom = 180 - 90 - winkelOfDoom;

                Debug.Log("Ankathete: " + ankathete + "\tGegenkathete: " + gegenkathete + "\tWinkel: " + winkelOfDoom);

                //transform.rotation = Quaternion.LookRotation(reckstange.rotation.eulerAngles);
                Quaternion.LookRotation(reckstange.right, reckstange.forward);
                //                transform.RotateAround(transform.position, reckstange.up, 90);
                transform.RotateAround(transform.position, reckstange.forward, (ankathete > 0 ? winkelOfDoom : -winkelOfDoom));
                //transform.RotateAround(transform.position, reckstange.up, (ankathete > 0 ? 90 : -90));


                reckstange.Rotate(0, 0, winkelOfDoom + (ankathete > 0 ? 0 : -90));
                this.transform.SetParent(reckstange);

                maxWinkel = winkelOfDoom < 3 ? 3 : (winkelOfDoom + (ankathete > 0 ? 0 : 90));
                nullWinkel = -winkelOfDoom;
                runterwärts = true;
            }

        }
        else if (other.gameObject.tag == "DeathZone")
        {
            Debug.Log("TOT");
            Spawn();

        }
        else if (other.gameObject.tag == "Hindernis")
        {
            Debug.Log("Treffer");
            
        }


    }
    public Transform spawnPoint;
    public void Spawn()
    {
        transform.position = spawnPoint.position;
    }
    /* +++++++++++++++++++ */
    /*      +++++++++      */
    /*       UPDATES       */
    /*      +++++++++      */
    /* +++++++++++++++++++ */

    void Update() {
        GetInput();
        Turn();
        MaybeAim();
        Forward();
        Jump();
        maybeShoot();
    }

    void FixedUpdate() {
        
        
        
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

[System.Serializable]
public class MoveSettings {
    public float runSpeed = 5;
    public float rotateVelocity = 150;
    public float lookupVelocity = 150;
    public float jumpVelocity = 8;
    public float distanceToGround = 1.3f;
    public LayerMask ground;

    [SerializeField]
    [Range(10,100)]
    public int range;
}

[System.Serializable]
public class InputSettings {
    
    public string FORWARD_AXIS = "Vertical";
    public string SIDEWAYS_AXIS = "Horizontal";
    public string TURN_AXIS = "Mouse X";
    public string LOOKUP_AXIS = "Mouse Y";
    public string JUMP_AXIS = "Jump";
}
