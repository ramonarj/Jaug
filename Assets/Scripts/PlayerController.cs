using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // Par�metros para el editor
    
    public int velocity = 3;
    public float jumpForce = 5;

    // Variables
    private uint mouseSensivity = 1;
    bool jumping;
    Camera cameraComp;
    Rigidbody rigidComp;
    BoxCollider colliderComp;
    private int weaponSelected = 0;
    private List<Gun> armas;


    private bool crouching = false;
    private float originalHeight;
    private float crouchingHeight;

    // Referencias
    public Slider sensivitySlider;
    public GameObject weaponSelector;
    public TextMeshProUGUI ammoText;

    private float CROUCH_TIME = 0.2f;
    private float MAX_WEAPONS = 4;


    // Constantes
    private const int PITCH_LIMIT = 75; //75 grados


    // Start is called before the first frame update
    void Start()
    {
        // Rat�n en medio de la pantalla
        //Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        jumping = false;
        cameraComp = GetComponentInChildren<Camera>();
        rigidComp = GetComponent<Rigidbody>();
        colliderComp = GetComponent<BoxCollider>();
        if (cameraComp == null)
            Debug.LogWarning("ERROR: no hay c�mara");
        if (rigidComp == null)
            Debug.LogWarning("ERROR: no hay Rigidbody");

        armas = new List<Gun>();
        foreach (Transform child in cameraComp.transform)
        {
            Gun g = child.GetComponent<Gun>();
            if (g != null)
                armas.Add(g);
        }

        Debug.Log(armas.Count);
        UpdateWeaponGUI();

        originalHeight = colliderComp.size.y;
        crouchingHeight = originalHeight / 2f;
    }

    // Update is called once per frame
    void Update()
    {
        // De momento hemos metido aqu� todo el input
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
                rigidComp.AddForce(new Vector3(0, jumpForce * 50, 0));
                jumping = true;
            }


            // AGACHARSE
            if (Input.GetKeyDown(KeyCode.LeftShift)) 
            {
                Debug.Log("Crouch");
                crouching = true;
                StartCoroutine(CrouchCoroutine());
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                crouching = false;
                StartCoroutine(UnCrouchCoroutine());
            }


            // DISPARO
            // Semiautom�tico
            if (Input.GetMouseButtonDown(0))
            {
                armas[weaponSelected].Shoot();
                UpdateWeaponGUI();
            }
            // Autom�tico
            else if (Input.GetMouseButton(0) && armas[weaponSelected].gunType == Gun.GunType.Automatic)
            {
                armas[weaponSelected].Shoot();
                UpdateWeaponGUI();
            }
            // Levantamos click izquierdo
            else if (Input.GetMouseButtonUp(0))
                armas[weaponSelected].StopShooting();


            // APUNTADO
            if (Input.GetMouseButtonDown(1))
                armas[weaponSelected].Aim();
            else if (Input.GetMouseButtonUp(1))
                armas[weaponSelected].StopAiming();

            // RECARGA
            if (Input.GetKeyDown(KeyCode.R))
            {
                armas[weaponSelected].Reload();
                UpdateWeaponGUI();
            }


            // CAMBIO DE ARMA
            float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
            if (mouseScroll != 0) 
            {
                //TODO: capar a izquierda y derecha la selecci�n
                // N�mero de armas
                //int weaponsNo = weaponSelector.transform.parent.childCount;

                cameraComp.transform.GetChild(weaponSelected).gameObject.SetActive(false);

                // Cambiamos de arma
                mouseScroll *= 10; //Para pasar de -0.1/0.1 a -1/1
                weaponSelected += (int)mouseScroll;
                Gun gunComp = cameraComp.transform.GetChild(weaponSelected).GetComponent<Gun>();
                if (gunComp == null)
                    return;
                armas[weaponSelected] = gunComp;


                // Actualizamos GUI
                weaponSelector.transform.position = weaponSelector.transform.parent.GetChild(weaponSelected + 1).position;
                armas[weaponSelected].gameObject.SetActive(true);


                UpdateWeaponGUI();
            }


            // TIRAR UN ARMA
            if (Input.GetKeyDown(KeyCode.Q)) 
            {
                if (armas[weaponSelected] == null) // Si no hay arma en ese slot, no hacemos nada
                    return;

                // Quitamos el sprite del selector 
                weaponSelector.transform.parent.GetChild(weaponSelected + 1).GetChild(0).
                    GetComponent<Image>().sprite = null;

                // Lo desequipamos de nuestro inventario
                Gun weaponThrown = armas[weaponSelected];
                armas[weaponSelected] = null;
                weaponThrown.transform.parent = null;

                // Ponemos en su lugar un objeto vac�o
                GameObject mockGO = new GameObject("Vacio" + weaponSelected);
                mockGO.transform.parent = cameraComp.transform;
                mockGO.transform.SetSiblingIndex(weaponSelected - 1);


                // Lo tiramos y le activamos las colisiones
                Rigidbody gunRigid = weaponThrown.GetComponent<Rigidbody>();
                gunRigid.isKinematic = false;
                gunRigid.AddForce(cameraComp.transform.forward * 200);
                weaponThrown.GetComponent<CapsuleCollider>().isTrigger = false;
            }

            // COGER UN ARMA
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Interact");
                RaycastHit hit;
                if (Physics.Raycast(cameraComp.transform.position, cameraComp.transform.forward, out hit, 3f))
                {
                    Gun floorGun = hit.transform.GetComponent<Gun>();
                    if (floorGun != null)
                    {
                        // Soltamos el arma anterior



                        // Nos lo ponemos en la mano
                        armas[weaponSelected] = floorGun;
                        armas[weaponSelected].transform.parent = cameraComp.transform;
                        armas[weaponSelected].transform.SetSiblingIndex(weaponSelected - 1);
                        armas[weaponSelected].transform.localPosition = armas[weaponSelected].defaultTrans.localPosition;
                        armas[weaponSelected].transform.localRotation = armas[weaponSelected].defaultTrans.localRotation;

                        // Actualizamos el sprite del GUI
                        weaponSelector.transform.parent.GetChild(weaponSelected + 1).GetChild(0).
                    GetComponent<Image>().sprite = armas[weaponSelected].sprite;

                        // Le quitamos cosas f�sicas
                        floorGun.GetComponent<Rigidbody>().isKinematic = true;
                        floorGun.GetComponent<CapsuleCollider>().isTrigger = true;
                    }
                }
            }
        }


        // MENU
        if (Input.GetKeyDown(KeyCode.Escape))
            GameManager.Instance().PauseGame();
            
    }

    // Las cosas de la c�mara es recomendable ponerlas en el LateUpdate
    private void LateUpdate()
    {
        if (!GameManager.Instance().IsGamePaused())
        {   
            // ROTACIONES DE C�MARA
            Vector2 mouseIncr = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            UpdateCamera(mouseIncr);
        }
    }

    // Colisi�n con el suelo
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Suelo")) 
        {
            jumping = false;
        }
    }

    private void UpdateCamera(Vector2 mouseIncr)
    {
        // Pitch
        Pitch(-mouseIncr.y * mouseSensivity);

        // Yaw
        Yaw(mouseIncr.x * mouseSensivity);
    }

    // Rota en el eje X (solo la c�mara)
    private void Pitch(float degrees) 
    {
        // Rotaci�n actual en formato [-90, 90]
        float rotX = WrapAngle(cameraComp.transform.localRotation.eulerAngles.x);

        // Capamos la rotaci�n
        if (rotX + degrees > PITCH_LIMIT)
            degrees = PITCH_LIMIT - rotX;
        else if (rotX + degrees < -PITCH_LIMIT)
            degrees = -PITCH_LIMIT - rotX;

        // Rotamos
        cameraComp.transform.Rotate(degrees, 0f, 0f);
    }

    // Rota en el eje Y (tanto el jugador como la c�mara)
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

    // Actualiza la munici�n del arma actual
    private void UpdateWeaponGUI() 
    {
        ammoText.GetComponent<TextMeshProUGUI>().text = armas[weaponSelected].ammo + " / " + armas[weaponSelected].maxAmmo;
    }

    private IEnumerator CrouchCoroutine()
    {
        float counter = 0;
        while (crouching && colliderComp.size.y > crouchingHeight)
        {
            // Calculamso el incremento
            Vector3 newSize = colliderComp.size;
            newSize.y = Mathf.Lerp(originalHeight, crouchingHeight, counter / CROUCH_TIME);

            // Actualizamos el collider
            colliderComp.size = newSize;
            counter += Time.deltaTime;
            yield return null;
        }
        
    }

    private IEnumerator UnCrouchCoroutine()
    {
        float counter = 0;
        while (!crouching && colliderComp.size.y < originalHeight)
        {
            // Calculamso el incremento
            Vector3 newSize = colliderComp.size;
            newSize.y = Mathf.Lerp(crouchingHeight, originalHeight, counter / CROUCH_TIME);

            // Actualizamos el collider
            colliderComp.size = newSize;
            counter += Time.deltaTime;
            yield return null;
        }

    }
}