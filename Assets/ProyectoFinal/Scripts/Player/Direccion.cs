
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase auxiliar para representar la dirección/direccionamiento con el que corregir el movimiento
/// </summary>
public class Direccion
{
    public float angular;
    public Vector3 lineal;
    public Direccion()
    {
        angular = 0.0f;
        lineal = new Vector3();
    }
}
