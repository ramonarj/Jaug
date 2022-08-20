using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arma : MonoBehaviour
{

    //TODO: quizás hacer 2 clases distintas en vez de un enum, que hereden
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
    }

    // Update is called once per frame
    void Update()
    {
        // Recargando el arma
        if (reloading) 
        {
            reloadTimer += Time.deltaTime;
            if(reloadTimer >= tiempoRecarga) 
            {
                reloading = false;
                ammo = maxAmmo;
                reloadTimer = 0;

            }
        }

        //
        //if(shooting)
    }

    // Dispara
    public void Shoot() 
    {
        // No queda munición
        if (ammo <= 0) 
        {
            Debug.Log("Out of ammo");
            return;
        }
            
        // Disparo semiautomático (1 vez y ya está)
        if (gunType == GunType.SemiAutomatic)
            ShootPrivate();

        // Disparo automático (empieza una corutina)
        else if (gunType == GunType.Automatic && !shooting) 
        {
            shooting = true;
            StartCoroutine(ShootAuto());
        }
    }

   
    IEnumerator ShootAuto()
    {
       while(shooting && ammo > 0) 
       {
            ShootPrivate();
            yield return new WaitForSeconds(1f / cadencia);
       }
    }

    // Método privado que ya hace la funcionalidad de disparar
    private void ShootPrivate() 
    {
        // Disparamos y activamos partículas
        ammo--;
        if (particulas != null)
        {
            particulas.Play();
        }

        // Y ahora a ver si le damos a algo o qué plan
        Transform cameraTrans = gameObject.transform.parent;
        RaycastHit hit;
        if (Physics.Raycast(cameraTrans.position, cameraTrans.forward, out hit, range))
        {
            Debug.Log("Tocado");
        }
    }

    // Recarga
    public void Reload() 
    {
        reloading = true;

        //Reloading animation.play()
    }

    // Deja de disparar en automático
    public void StopShooting() 
    {
        shooting = false;
    }
}
