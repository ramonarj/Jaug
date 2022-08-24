using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

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

    private const float CROUCH_TIME = 0.2f;
    private const int MAX_WEAPONS = 5;


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
        colliderComp = GetComponent<BoxCollider>();
        if (cameraComp == null)
            Debug.LogWarning("ERROR: no hay cámara");
        if (rigidComp == null)
            Debug.LogWarning("ERROR: no hay Rigidbody");

        armas = new List<Gun>(MAX_WEAPONS);
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
        // De momento hemos metido aquí todo el input
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
            // Semiautomático
            if (Input.GetMouseButtonDown(0))
            {
                armas[weaponSelected].Shoot();
                UpdateWeaponGUI();
            }
            // Automático
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
                //TODO: capar a izquierda y derecha la selección
                // Número de armas
                //int weaponsNo = weaponSelector.transform.parent.childCount;

                // Desactivamos el viejo arma
                cameraComp.transform.GetChild(weaponSelected).gameObject.SetActive(false);

                // Cambiamos de arma
                mouseScroll *= 10; //Para pasar de -0.1/0.1 a -1/1
                weaponSelected += (int)mouseScroll;
                Gun gunComp = cameraComp.transform.GetChild(weaponSelected).GetComponent<Gun>();
                armas[weaponSelected] = gunComp;

                // Activamos el nuevo arma
                cameraComp.transform.GetChild(weaponSelected).gameObject.SetActive(true); //-> se llama al Awake pero no al Start

                // Actualizamos GUI
                weaponSelector.transform.position = weaponSelector.transform.parent.GetChild(weaponSelected + 1).position;
                UpdateWeaponGUI();
            }


            // TIRAR UN ARMA
            if (Input.GetKeyDown(KeyCode.Q)) 
            {
                // Tiramos el arma actual
                DropGun();

                // Ponemos en su mismo lugar un objeto vacío
                GameObject mockGO = new GameObject("Vacio_" + weaponSelected);
                mockGO.transform.parent = cameraComp.transform;
                mockGO.transform.SetSiblingIndex(weaponSelected);

                // Actualizamos el GUI
                UpdateWeaponGUI();
            }

            // COGER UN ARMA
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Lanzamos un rayo; si le damos a algo y es un arma, la cogemos
                RaycastHit hit;
                if (Physics.Raycast(cameraComp.transform.position, cameraComp.transform.forward, out hit, 3f))
                {
                    Gun floorGun = hit.transform.GetComponent<Gun>();
                    if (floorGun != null) 
                    {
                        PickGun(floorGun);
                        UpdateWeaponGUI();
                    }  
                }
            }

            // ABRIR EL INVENTARIO
            if (Input.GetKeyDown(KeyCode.I))
            {
                Debug.Log("Opening Inventory...");
            }
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
            // ROTACIONES DE CÁMARA
            Vector2 mouseIncr = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            UpdateCamera(mouseIncr);
        }
    }

    // Colisión con el suelo
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

    // Actualiza la munición del arma actual
    private void UpdateWeaponGUI() 
    {
        // Número de balas que le quedan al arma
        string s = "- / -";
        if (armas[weaponSelected] != null)
            s = armas[weaponSelected].ammo + " / " + armas[weaponSelected].maxAmmo;

        ammoText.GetComponent<TextMeshProUGUI>().text = s;
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

    // Tira el arma actual al suelo, quitando el sprite correspondiente del HUD y activando las propiedades físicas del arma
    private void DropGun() 
    {
        // Si no hay arma en ese slot, solamente eliminamos el objeto vacío que había
        if (armas[weaponSelected] == null) 
        {
            Destroy(cameraComp.transform.GetChild(weaponSelected).gameObject);
            return;
        } 


        // Quitamos el sprite del selector 
        weaponSelector.transform.parent.GetChild(weaponSelected + 1).GetChild(0).
            GetComponent<Image>().sprite = null;

        // Lo desequipamos de nuestro inventario
        Gun weaponThrown = armas[weaponSelected];
        armas[weaponSelected] = null;
        weaponThrown.transform.parent = null;

        // Lo tiramos y le activamos las colisiones
        Rigidbody gunRigid = weaponThrown.GetComponent<Rigidbody>();
        gunRigid.isKinematic = false;
        gunRigid.GetComponent<CapsuleCollider>().isTrigger = false;
        gunRigid.AddForce(cameraComp.transform.forward * 200);
        
    }

    // Recoge el arma especificada, lo equipa en la mano y actualiza el HUD con su sprite.
    // Si ya había un arma en ese slot, la tira
    private void PickGun(Gun gun) 
    {
        if (gun == null) //no hay nada que coger
            return;

        // Soltamos el arma que tenemos en la mano para dar hueco a la nueva
        DropGun();

        // Nos lo ponemos en la mano
        armas[weaponSelected] = gun;
        armas[weaponSelected].transform.parent = cameraComp.transform;
        armas[weaponSelected].transform.SetSiblingIndex(weaponSelected);
        armas[weaponSelected].transform.localPosition = armas[weaponSelected].defaultTrans.localPosition;
        armas[weaponSelected].transform.localRotation = armas[weaponSelected].defaultTrans.localRotation;

        // Le quitamos cosas físicas
        gun.GetComponent<Rigidbody>().isKinematic = true;
        gun.GetComponent<CapsuleCollider>().isTrigger = true;

        // Actualizamos el sprite del GUI
        weaponSelector.transform.parent.GetChild(weaponSelected + 1).GetChild(0).
    GetComponent<Image>().sprite = armas[weaponSelected].sprite;
    }
}