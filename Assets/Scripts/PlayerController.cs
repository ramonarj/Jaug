using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    private const int PITCH_LIMIT = 75; //75 grados
    public int velocity = 3;

    private Vector3 mousePos;
    Camera cameraComp;

    bool jumping;

    // Start is called before the first frame update
    void Start()
    {
        jumping = false;
        mousePos = Input.mousePosition;
        cameraComp = GetComponentInChildren<Camera>();
        if (cameraComp != null)
            Debug.Log("Camara OK");
    }

    // Update is called once per frame
    void Update()
    {
        // ROTACIONES
        if (mousePos != Input.mousePosition)
        {
            UpdateCamera(Input.mousePosition - mousePos);
            mousePos = Input.mousePosition;
        }

        // TRASLACIONES
        if (Input.GetMouseButtonDown(0))
            Debug.Log("Fire");
        if (Input.GetKey(KeyCode.W))
            transform.position += (transform.forward * velocity * Time.deltaTime);
        if (Input.GetKey(KeyCode.S))
            transform.position -= (transform.forward * velocity * Time.deltaTime);
        if (Input.GetKey(KeyCode.A))
            transform.Translate(Vector3.left * velocity * Time.deltaTime);
        if (Input.GetKey(KeyCode.D))
            transform.Translate(Vector3.right * velocity * Time.deltaTime);
    }

    private void UpdateCamera(Vector2 mouseIncr)
    {
        // Pitch
        Pitch(-mouseIncr.y);

        // Yaw
        Yaw(mouseIncr.x);
    }

    // Rota en el eje X (solo la cámara)
    private void Pitch(float degrees) 
    {
        //Rotación actual en formato [-90, 90]
        float rotX = WrapAngle(cameraComp.transform.localRotation.eulerAngles.x);
        //Debug.Log("Actual rot. " + rotX + ", incr: " + degrees);


        //Capamos la rotación
        if (rotX + degrees > PITCH_LIMIT)
            degrees = PITCH_LIMIT - rotX;
        else if (rotX + degrees < -PITCH_LIMIT)
            degrees = -PITCH_LIMIT - rotX;

        // Rotamos
        cameraComp.transform.Rotate(degrees, 0f, 0f);
    }

    // Rota en el eje Y (tanto el jugador como la cámara)
    private void Yaw(float degrees) 
    {
        // Rotamos
        transform.Rotate(new Vector3(0f, degrees, 0f), Space.World);
    }

    // Rota en el eje Z
    private void Roll(float degrees) 
    {
        //No hace falta de momento
    }

    // Pasa de un formato a otro
    private static float WrapAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            return angle - 360;

        return angle;
    }
}
