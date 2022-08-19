using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arma : MonoBehaviour
{
    // Para el editor
    public float damage = 10;

    public int maxAmmo = 30;

    // Privadas
    private bool reloading;
    public int ammo { get; set; }


    // Start is called before the first frame update
    void Start()
    {
        reloading = false;
        ammo = maxAmmo;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Dispara
    public void Shoot() 
    {
        if (ammo > 0)
        {
            ammo--;
        }
        else
            Debug.Log("Out of ammo");
    }

    // Recarga
    public void Reload() 
    {
        ammo = maxAmmo;
    }
}
