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
        if(shooting)
    }

    // Dispara
    public void Shoot() 
    {
        if(gunType == GunType.SemiAutomatic && ammo > 0)
        {
            // Disparamos 
            ammo--;
            if(particulas != null) 
            {
                particulas.Play();
            }

            // Y ahora a ver si le damos a algo
            Transform cameraTrans = gameObject.transform.parent;

            // Le damos a algo
            RaycastHit hit;
            if (Physics.Raycast(cameraTrans.position, cameraTrans.forward, out hit, range))
            {
                Debug.Log("Tocado");
            }
        }
        else
            Debug.Log("Out of ammo");

    }

    // Recarga
    public void Reload() 
    {
        reloading = true;

        //Reloading animation.play()
    }
}
