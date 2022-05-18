/*    
    Obra original:
        Copyright (c) 2018 Packt
        Unity 2018 Artificial Intelligence Cookbook - Second Edition, by Jorge Palacios
        https://github.com/PacktPublishing/Unity-2018-Artificial-Intelligence-Cookbook-Second-Edition
        MIT License

    Modificaciones:
        Copyright (C) 2020-2022 Federico Peinado
        http://www.federicopeinado.com

        Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
        Esta asignatura se imparte en la Facultad de Informática de la Universidad Complutense de Madrid (España).
        Contacto: email@federicopeinado.com
*/
namespace es.ucm.fdi.iav.rts
{
    using UnityEngine;
    using System.Collections.Generic;

    // Puntos representativos o vértice (común a todos los esquemas de división, o a la mayoría de ellos)
    [System.Serializable]
    public class Vertex 
    {
        /// <summary>
        /// Identificador del vértice 
        /// </summary>
        public int id;
        public int[] influence = {0,0,0};
        public int lastInfluence = 0;
        public int GetInstanceID() { return id; }
        /// <summary>
        /// Vecinos del vértice
        /// </summary>
        public List<Edge> vecinos;

        // Vértice previo
        [HideInInspector]
        public Vertex prev;
    }
}
