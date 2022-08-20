using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arma : MonoBehaviour
{

    //TODO: quiz�s hacer 2 clases distintas en vez de un enum, que hereden
    public enum GunType { Automatic, SemiAutomatic};


    // Para el editor
    public float damage = 10;

    public int maxAmmo = 30;

    public int cadencia = 5; //balas/segundo
    public float tiempoRecarga = 1f;
    public GunType gunType;

    // Privadas
    private bool reloading;
    private bool shooting;
    public int ammo { get; set; }

    private float range = 100f;

    // Referencias
    private Camera camara;
    private ParticleSystem particulas;

    // Timers
    private float reloadTimer;
    private float shootTimer;



    // Start is called before the first frame update
    void Start()
    {
        reloadTimer = 0;
        reloading = false;
        ammo = maxAmmo;
        particulas = GetComponentInChildren<ParticleSystem>();
        camara = gameObject.transform.parent.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // Esto est� vac�o porque tanto la recarga como el disparo
        // autom�tico lo hemos hecho con corutinas
        // Recargando el arma
        //if (reloading) 
        //{
        //    reloadTimer += Time.deltaTime;
        //    if(reloadTimer >= tiempoRecarga) 
        //    {
        //        reloading = false;
        //        ammo = maxAmmo;
        //        reloadTimer = 0;

        //    }
        //}

        //
        //if(shooting)
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
        camara.fieldOfView = 40;
    }

    // Deja de apuntar
    public void StopAiming()
    {
        camara.fieldOfView = 60;
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
