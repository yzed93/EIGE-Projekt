using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class PlayerBehaviour : MonoBehaviour
{
    protected CurrentAction doing;
    private PlayerThrowing playerThrowing;
    private PlayerAcrobatics playerAcrobatics;
    private PlayerRunning playerRunning;

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
        this.playerAcrobatics = new PlayerAcrobatics(this);
        
        velocity = Vector3.zero;
        forwardInput = sidewaysInput = lookupInput = turnInput = jumpInput = 0;
        targetRotation = transform.rotation;
        moving = false;
        //crosshair.SetActive(false);
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

    protected Rigidbody playerRigidbody;
    protected Vector3 velocity;
    protected Quaternion targetRotation;
    protected bool moving;

    public bool isMoving() {
        return moving;
    }
    
    public AcrobaticSettings acrobaticSettings;

    void Forward() {
        switch (doing)
        {
            case (CurrentAction.SWINGING):
                playerAcrobatics.ForwardSwinging(forwardInput);
                break;
            case (CurrentAction.FREERUNNING):
                playerRunning.ForwardRunning(forwardInput, sidewaysInput, doing);
                break;
        }
    }
    void Jump() {
        if (jumpInput != 0)
        {
            switch (doing)
            {
                case (CurrentAction.SWINGING):
                    playerAcrobatics.JumpSwinging();
                    break;
                case (CurrentAction.FREERUNNING):
                    if (Grounded())
                        playerRunning.JumpRunning();
                    break;
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

                    playerThrowing.Throw();
                    
                }
            
        }
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
                playerAcrobatics.hangOntoReck(other);
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

    /* SkillsetInteraction */
    public void changeDoing(CurrentAction newState)
    {
        this.doing = newState;
    }
}

[System.Serializable]
public class InputSettings {
    
    public string FORWARD_AXIS = "Vertical";
    public string SIDEWAYS_AXIS = "Horizontal";
    public string TURN_AXIS = "Mouse X";
    public string LOOKUP_AXIS = "Mouse Y";
    public string JUMP_AXIS = "Jump";
}
