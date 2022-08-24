using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    #region Atributos
    //TODO: quiz�s hacer 2 clases distintas en vez de un enum, que hereden
    public enum GunType { Automatic, SemiAutomatic};


    // Par�metros configurables del arma para el Editor
    [Tooltip("Da�o del arma")]
    public float damage = 10;
    [Tooltip("El n�mero de balas que caben en el cargador")]
    public int maxAmmo = 30;
    [Tooltip("La cantidad de balas que dispara por segundo")]
    public int cadencia = 5; //balas/segundo
    [Tooltip("Los segundos que tarda en recargarse")]
    public float tiempoRecarga = 1f;
    [Tooltip("Los metros de zoom que tiene la mirilla")]
    public float aimDistance;
    [Tooltip("Tipo de arma (autom�tica, semiautom�tica, de r�fagas")]
    public GunType gunType;
    [Tooltip("La posici�n en resposo del arma")]
    public Transform defaultTrans;
    [Tooltip("La posici�n que tendr� el arma al apuntar")]
    public Transform aimTrans;
    [Tooltip("La part�cula que aparecer� al impactar un objetivo con el arma")]
    public GameObject impactParticle;

    // Variables rivadas
    private bool reloading; // Control para las subrutinas
    private bool shooting;
    private bool aiming;
    public int ammo { get; set; }

    private float range = 100f;
    private float defaultFOV;

    // Referencias
    private Camera camara;
    private ParticleSystem particulas;

    // Timers
    private float reloadTimer;

    // Constantes
    const float AIM_TIME = 0.5f;
    #endregion




    // Start is called before the first frame update
    void Start()
    {
        reloadTimer = 0;
        reloading = false;
        aiming = false;
        shooting = false;
        ammo = maxAmmo;
        particulas = GetComponentInChildren<ParticleSystem>();
        camara = gameObject.transform.parent.GetComponent<Camera>();
        defaultFOV = camara.fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {

    }

    #region M�todos p�blicos

    // TODO: "Arma" no deber�a depender de Camera, sino recibir una direcci�n en la que disparar
    // Dispara
    public void Shoot() 
    {
        // No podemos disparar en este momento
        if (ammo <= 0 || reloading) 
        {
            Debug.Log("Can't shoot right now");
            return;
        }
            
        // Disparo semiautom�tico (1 vez y ya est�)
        if (gunType == GunType.SemiAutomatic)
            ShootPrivate();

        // Disparo autom�tico (empieza una corutina)
        else if (gunType == GunType.Automatic && !shooting) 
        {
            shooting = true;
            StartCoroutine(ShootAuto());
        }
    }

    // Deja de disparar (para el modo autom�tico)
    public void StopShooting()
    {
        shooting = false;
    }

    // Recarga
    public void Reload() 
    {
        reloading = true;
        StartCoroutine(ReloadCoroutine());
        //Reloading animation.play()
    }

    // Apunta
    public void Aim() 
    {
        Debug.Log("Aiming");
        aiming = true;
        StartCoroutine(AimCoroutine());
    }

    // Deja de apuntar
    public void StopAiming()
    {
        aiming = false;
        StartCoroutine(StopAimCoroutine());
    }

    
    #endregion

    #region M�todos privados

    IEnumerator ShootAuto()
    {
        while (shooting && ammo > 0)
        {
            ShootPrivate();
            yield return new WaitForSeconds(1f / cadencia);
        }
    }

    IEnumerator ReloadCoroutine()
    {
        while (reloadTimer < tiempoRecarga)
        {
            reloadTimer += Time.deltaTime;
            yield return null;
        }

        // Ya hemos acabado
        reloading = false;
        ammo = maxAmmo;
        reloadTimer = 0;
    }

    // Estas dos corutinas son tela de parecidas, habr�a que mezclarlas
    IEnumerator AimCoroutine()
    {
        // Donde empieza y termina el apuntado
        // NOTA: estas variables son necesarias porque el jugador puede empezar a apuntar y soltar
        // el click derecho antes de apuntar del todo; por tanto, la fase inicial del apuntado es 
        // variable y no siempre ser� la posici�n que tiene el arma por defecto (puede estar a camino)
        float startingFOV = camara.fieldOfView;
        float endingFOV = defaultFOV - aimDistance;
        Vector3 startingPos = transform.localPosition;
        Quaternion startingRot = transform.localRotation;

        // Tiempo que tardar� en completar el apuntado
        float aimTime = ((startingFOV - endingFOV) * AIM_TIME / aimDistance); //regla de 3
        float counter = 0;

        while (camara.fieldOfView > endingFOV && aiming)
        {
            float progress = counter / aimTime;
            camara.fieldOfView = Mathf.Lerp(startingFOV, endingFOV, progress);
            transform.localPosition = Vector3.Lerp(startingPos, aimTrans.localPosition, progress);
            transform.localRotation = Quaternion.Lerp(startingRot, aimTrans.localRotation, progress);
            counter += Time.deltaTime;
            yield return null;
        }        
    }

    IEnumerator StopAimCoroutine()
    {
        // Donde empieza y termina el apuntado
        float startingFOV = camara.fieldOfView;
        float endingFOV = defaultFOV;
        Vector3 startingPos = transform.localPosition;
        Quaternion startingRot = transform.localRotation;

        // Tiempo que tardar� en completar el 'desapuntado'
        float aimTime = ((endingFOV - startingFOV) * AIM_TIME / aimDistance); //regla de 3
        float counter = 0;

        while (camara.fieldOfView < endingFOV && !aiming)
        {
            camara.fieldOfView = Mathf.Lerp(startingFOV, endingFOV, counter / aimTime);
            transform.localPosition = Vector3.Lerp(startingPos, defaultTrans.localPosition, counter / aimTime);
            transform.localRotation = Quaternion.Lerp(startingRot, defaultTrans.localRotation, counter / aimTime);
            counter += Time.deltaTime;
            yield return null;
        }
    }

    // M�todo privado que ya hace la funcionalidad de disparar
    private void ShootPrivate()
    {
        // Disparamos y activamos part�culas
        ammo--;
        if (particulas != null)
        {
            particulas.Play();
        }

        // Y ahora a ver si le damos a algo o qu� plan
        Transform cameraTrans = gameObject.transform.parent;
        RaycastHit hit;
        if (Physics.Raycast(cameraTrans.position, cameraTrans.forward, out hit, range))
        {
            //Quitamos vida al otro si es el caso
            Target tocado = hit.transform.GetComponent<Target>();
            if (tocado != null)
                tocado.TakeDamage(damage);

            // Le aplicamos una peque�a fuerza
            // No s� si deber�a seguir con "-hit.normal" como direcci�n
            hit.rigidbody.AddForceAtPosition(cameraTrans.forward * damage * 10, hit.point);


            // TODO; tener una part�cula para cada superficie (tierra, agua, carne, metal...)
            // Part�cula del contacto
            GameObject particulas = Instantiate(impactParticle, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(particulas, 2f);
        }
    }

    #endregion
}
