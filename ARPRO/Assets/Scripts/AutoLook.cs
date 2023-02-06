using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoLook : MonoBehaviour
{

    GameObject camara;

    private void OnEnable()
    {
        camara = GameObject.FindGameObjectWithTag("MainCamera");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(2 * transform.position - camara.transform.position);
    }
}
