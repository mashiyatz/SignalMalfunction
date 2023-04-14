using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnBodyClick : MonoBehaviour
{
    public ParticleController controller;

    private void OnMouseDown()
    {
        if (controller.currentState == ParticleController.State.Idle) controller.wasMousePressed = true;
        else controller.wasMousePressed = false;
    }
}
