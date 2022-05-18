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
        Esta asignatura se imparte en la Facultad de Inform�tica de la Universidad Complutense de Madrid (Espa�a).
        Contacto: email@federicopeinado.com
*/
namespace es.ucm.fdi.iav.rts
{
    using UnityEngine;
    using System.Collections.Generic;

    // Puntos representativos o v�rtice (com�n a todos los esquemas de divisi�n, o a la mayor�a de ellos)
    [System.Serializable]
    public class Vertex 
    {
        /// <summary>
        /// Identificador del v�rtice 
        /// </summary>
        public int id;
        public int[] influence = {0,0,0};
        public int lastInfluence = 0;
        public int GetInstanceID() { return id; }
        /// <summary>
        /// Vecinos del v�rtice
        /// </summary>
        public List<Edge> vecinos;

        // V�rtice previo
        [HideInInspector]
        public Vertex prev;
    }
}
