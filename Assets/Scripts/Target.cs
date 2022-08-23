using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public float health = 100;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(float amount) 
    {
        Debug.Log("Tocado");
        health -= amount;
        if (health <= 0)
            Die();
    }

    private void Die() 
    {
        Debug.Log("Me morí");
        Destroy(gameObject);
    }
}
