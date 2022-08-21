using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arma : MonoBehaviour
{
    #region Atributos
    //TODO: quiz�s hacer 2 clases distintas en vez de un enum, que hereden
    public enum GunType { Automatic, SemiAutomatic};


    // Para el editor
    public float damage = 10;
    public int maxAmmo = 30;
    public int cadencia = 5; //balas/segundo
    public float tiempoRecarga = 1f;
    public float aimDistance;
    public GunType gunType;

    // Privadas
    private bool reloading; // Control para las subrutinas
    private bool shooting;
    private bool aiming;
    public int ammo { get; set; }

    private float range = 100f;
    private float defaultFOV;
    private Vector3 initialPos;

    // Referencias
    private Camera camara;
    private ParticleSystem particulas;

    // Timers
    private float reloadTimer;
    private float shootTimer;

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
        initialPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }

    #region M�todos p�blicos

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
        float startingFOV = camara.fieldOfView;
        float endingFOV = defaultFOV - aimDistance;

        // Tiempo que tardar� en completar el apuntado
        float aimTime = ((startingFOV - endingFOV) * AIM_TIME / aimDistance); //regla de 3
        float counter = 0;

        while (camara.fieldOfView > endingFOV && aiming)
        {
            camara.fieldOfView = Mathf.Lerp(startingFOV, endingFOV, counter / aimTime);
            counter += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator StopAimCoroutine()
    {
        // Donde empieza y termina el apuntado
        float startingFOV = camara.fieldOfView;
        float endingFOV = defaultFOV;

        // Tiempo que tardar� en completar el 'desapuntado'
        float aimTime = ((endingFOV - startingFOV) * AIM_TIME / aimDistance); //regla de 3
        float counter = 0;

        while (camara.fieldOfView < endingFOV && !aiming)
        {
            camara.fieldOfView = Mathf.Lerp(startingFOV, endingFOV, counter / aimTime);
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
            Debug.Log("Tocado");
        }
    }

    #endregion
}
