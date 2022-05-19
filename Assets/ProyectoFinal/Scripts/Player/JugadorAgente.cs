/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Inform�tica de la Universidad Complutense de Madrid (Espa�a).

   Autor: Federico Peinado 
   Contacto: email@federicopeinado.com
*/


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase para modelar el controlador del jugador como agente
/// </summary>
public class JugadorAgente : Agente
{
    /// <summary>
    /// El componente de cuerpo r�gido
    /// </summary>
    private Rigidbody _cuerpoRigido;
    RaycastHit hit;

    /// <summary>
    /// Direcci�n del movimiento
    /// </summary>
    private Vector3 _dir;

    /// <summary>
    /// Al despertar, establecer el cuerpo r�gido
    /// </summary>
    private void Awake()
    {
        _cuerpoRigido = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// En cada tick, mover el avatar del jugador seg�n las �rdenes de este �ltimo
    /// </summary>
    public override void Update()
    {
        velocidad.x = Input.GetAxis("Horizontal");
        velocidad.z = Input.GetAxis("Vertical");
        // Faltaba por normalizar el vector
        velocidad.Normalize();
        velocidad *= velocidadMax;
    }

    /// <summary>
    /// En cada tick fijo, haya cuerpo r�gido o no, hago simulaci�n f�sica y cambio la posici�n de las cosas (si hay cuerpo r�gido aplico fuerzas y si no, no)
    /// </summary>
    public override void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, -transform.up, out hit, 1))
        {
            transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
        if (_cuerpoRigido == null)
        {
            transform.Translate(velocidad * Time.deltaTime, Space.World);
        }
        else
        {
            // El cuerpo r�gido no podr� estar marcado como cinem�tico
            _cuerpoRigido.AddRelativeForce(velocidad * Time.deltaTime, ForceMode.VelocityChange); // Cambiamos directamente la velocidad, sin considerar la masa (pidiendo que avance esa distancia de golpe)
        }

    }

    /// <summary>
    /// En cada parte tard�a del tick, encarar el agente
    /// </summary>
    public override void LateUpdate()
    {
        if (_cuerpoRigido.velocity.sqrMagnitude > Mathf.Epsilon)
        {
            transform.rotation = Quaternion.LookRotation(_cuerpoRigido.velocity.normalized);
        }
    }
}

