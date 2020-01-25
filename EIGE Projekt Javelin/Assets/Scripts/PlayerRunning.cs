using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunning
{

    public CurrentAction JumpRunning()
    {
        if (jumpInput != 0) {
            if (Grounded()) {
                    playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, moveSettings.jumpVelocity, playerRigidbody.velocity.z);
            }
        }
        return CurrentAction.FREERUNNING;
    }

    public void ForwardRunning()
    {
        if (doing == CurrentAction.FREERUNNING && Grounded()) {
            velocity.z = forwardInput * moveSettings.runSpeed * ((doing == CurrentAction.AIMING) ? 0.5f : 1f);
            velocity.x = sidewaysInput * moveSettings.runSpeed * ((doing == CurrentAction.AIMING) ? 0.5f : 1f);
            velocity.y = playerRigidbody.velocity.y;

            playerRigidbody.velocity = transform.TransformDirection(velocity);
        }
    }
}
