/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Inform�tica de la Universidad Complutense de Madrid (Espa�a).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisi�n: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using System;
using UnityEngine;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * Las unidades de extracci�n sirven para obtener los recursos del escenario, no son capaces de atacar pero s� reciben da�o.
     */ 
    public class ExtractionUnit : Unit
    {
        // La cantidad de recursos que puede extraer de golpe
        [SerializeField] private int _extractableAmmount = 1000;  
        public int ExtractableAmmount { get { return _extractableAmmount; } protected set { _extractableAmmount = value; } }

        // Los recursos que transporta ahora mismo la unidad
        public int Resources { get; set; } = 0;

        // Se solicita el movimiento de esta unidad a una posici�n del escenario de batalla. 
        // La unidad extractora intentar� extraer los recursos del campos m�s cercano a la posici�n dada. 
        // Posibles mejoras:
        // - Podr�a tener dos opciones: apuntar a un objeto concreto al que puedes seguir (ej. su transformada) o simplemente una posici�n fija.
        // - Procurar que el �rbol de comportamiento sea bastante aut�nomo... s�lo cambiar algunas variables compartidas y autom�ticamente el �rbol sabr� por donde ir.
        // - Se podr�a devolver un booleano para saber si el movimiento se pudo realizar sin problemas.
        // - Considerar si es interesante que haya una variable en la unidad para saber a qu� instalaci�n de procesamiento est� 'asociada'.
        public override void Move(RTSController controller, Transform transform)
        {
            if (controller == null)
                throw new ArgumentNullException("No se ha pasado un controlador.");
            if (transform == null)
                throw new ArgumentNullException("No se ha pasado una transformada para el movimiento.");

            int index = RTSGameManager.Instance.GetIndex(controller);
            if (index != GetControllerIndex())
                throw new ArgumentException("El controlador " + index + " no es quien controla a esta unidad");

            // Llamada primero a la clase Unit en general
            base.Move(controller, transform);

            // Luego se pueden consultar algunas variables compartidas del �rbol de comportamiento, si hiciera falta...
            //var enemy = behaviors[i].GetVariable("Enemy") as SharedTransform;

            // Se podr�a modificar una variable compartida del �rbol de comportamiento para indicar el objetivo...
            //BehaviorTree.SetVariableValue("Target", GameObject.Find("Village"));  o mejor otra opci�n que no se usando el nombre
        }
    }
}