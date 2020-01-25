using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunning
{
    private PlayerBehaviour playerBehaviour;
    private Rigidbody playerRigidbody;
    private GameObject playerGameObject;

    private MoveSettings moveSettings;

    public PlayerRunning(PlayerBehaviour playerBehaviour)
    {
        this.playerBehaviour = playerBehaviour;
        this.playerRigidbody = playerBehaviour.gameObject.GetComponent<Rigidbody>();
        this.playerGameObject = playerBehaviour.gameObject;

    }

    public CurrentAction JumpRunning()
    {
        playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, moveSettings.jumpVelocity, playerRigidbody.velocity.z);
        return CurrentAction.FREERUNNING;
    }

    public void ForwardRunning(float forwardInput, float sidewaysInput, CurrentAction doing)
    {
        playerRigidbody.velocity = new Vector3( sidewaysInput * moveSettings.runSpeed * ((doing == CurrentAction.AIMING) ? 0.5f : 1f),
                                                playerRigidbody.velocity.y,
                                                forwardInput * moveSettings.runSpeed * ((doing == CurrentAction.AIMING) ? 0.5f : 1f));

        playerRigidbody.velocity = playerRigidbody.transform.TransformDirection(playerRigidbody.velocity);
    }
}

[System.Serializable]
public class MoveSettings
{
    public float runSpeed = 5;
    public float rotateVelocity = 150;
    public float lookupVelocity = 150;
    public float jumpVelocity = 8;
    public float distanceToGround = 1.3f;
    public LayerMask ground;
}
