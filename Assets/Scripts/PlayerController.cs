using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerController : MonoBehaviour
{
    // Parámetros para el editor
    
    public int velocity = 3;
    public float jumpForce = 5;

    // Variables
    private uint mouseSensivity = 1;
    bool jumping;
    Camera cameraComp;
    Rigidbody rigidComp;

    // Referencias
    public Slider sensivitySlider;


    // Constantes
    private const int PITCH_LIMIT = 75; //75 grados


    // Start is called before the first frame update
    void Start()
    {
        // Ratón en medio de la pantalla
        //Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        jumping = false;
        cameraComp = GetComponentInChildren<Camera>();
        rigidComp = GetComponent<Rigidbody>();
        if (cameraComp == null)
            Debug.LogWarning("ERROR: no hay cámara");
        if (rigidComp == null)
            Debug.LogWarning("ERROR: no hay Rigidbody");
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance().IsGamePaused()) 
        {
            // MOVIMIENTO
            if (Input.GetKey(KeyCode.W))
                transform.position += (transform.forward * velocity * Time.deltaTime);
            if (Input.GetKey(KeyCode.S))
                transform.position -= (transform.forward * velocity * Time.deltaTime);
            if (Input.GetKey(KeyCode.A))
                transform.position -= (transform.right * velocity * Time.deltaTime);
            if (Input.GetKey(KeyCode.D))
                transform.position += (transform.right * velocity * Time.deltaTime);

            // SALTO
            if (Input.GetKeyDown(KeyCode.Space) && !jumping)
            {
                rigidComp.AddForce(new Vector3(0, jumpForce * 5000, 0));
                jumping = true;
            }


            // DISPARO
            if (Input.GetMouseButtonDown(0))
                Debug.Log("Fire");
        }


        // MENU
        if (Input.GetKeyDown(KeyCode.Escape))
            GameManager.Instance().PauseGame();
            
    }

    // Las cosas de la cámara es recomendable ponerlas en el LateUpdate
    private void LateUpdate()
    {
        if (!GameManager.Instance().IsGamePaused())
        {

            Debug.Log("Ratón: {" + Input.GetAxisRaw("Mouse X") + ", " + Input.GetAxisRaw("Mouse Y") + "}");
            
            // ROTACIONES DE CÁMARA
            Vector2 mouseIncr = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            //if (mouseIncr.magnitude > 0.5f)//mousePos != Input.mousePosition) //mousePos - Input.mousePosition).magnitude > 0.5
            //{
            //    UpdateCamera(mouseIncr);
            //}
            UpdateCamera(mouseIncr);
        }
    }

    // Colisión con el suelo
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Suelo")) 
        {
            jumping = false;
            Debug.Log("Toco suelo");
        }
    }

    private void UpdateCamera(Vector2 mouseIncr)
    {
        //mouseIncr /= 7;
        // Pitch
        Pitch(-mouseIncr.y * mouseSensivity);

        // Yaw
        Yaw(mouseIncr.x * mouseSensivity);
    }

    // Rota en el eje X (solo la cámara)
    private void Pitch(float degrees) 
    {
        // Rotación actual en formato [-90, 90]
        float rotX = WrapAngle(cameraComp.transform.localRotation.eulerAngles.x);

        // Capamos la rotación
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

    public void ChangeSensivity() 
    {
        mouseSensivity = (uint)sensivitySlider.value;
    }
}
