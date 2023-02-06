using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateText : MonoBehaviour
{

    public GameObject objetoTexto;

    private void OnEnable()
    {
        objetoTexto.SetActive(false);
    }


    public void ActivarObjeto()
    {
        objetoTexto.SetActive(true);

    }
}
