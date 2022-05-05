# Documentación Práctica 2
Javier Muñoz Martín de la Sierra

## [Video Demostrativo]()

## Resumen del enunciado

Extender las funcionalidades y contenidos implementados en la Práctica 2 para elaborar un Proyecto Final Individual. 
Tras exponer el Punto del Partida en el que me baso expondré mi idea a realizar como Proyecto Final.

## Punto de Partida

El **[punto de partida](https://github.com/IAV22-G20/P2)** proporcionado para esta practica genera un mapa por casillas mediante un grafo, las casillas son de dos tipos, **suelo** de color **blanco**, u **obstaculo** de color **gris oscuro**.

### Modo Depuración
El prototipo permite marcar haciendo **clic** sobre el mapa un punto de **inicio** para el calculo de una ruta, luego al pulsar **espacio** se calcula una ruta desde es el punto de inicio hasta un **destino** que corresponde a la posicion actual en la que se encuentre el **cursor** en el mapa.
Esto se representa mediante esferas que se colocan en las casillas del mapa, una esfera **verde** representa el inicio del recorrido, la esfera **roja** (posicion actual del cursor) representa el destino, y las esferas **amarillas** representan el recorrido que se realizaria en la busqueda de este recorrido casilla a casilla.

![](https://github.com/IAV22-G20/P2/blob/main/images%20documentacion/punto_de_partida_1.JPG)
![](https://github.com/IAV22-G20/P2/blob/main/images%20documentacion/punto_de_partida_2.JPG)

### Modo InGame
Al ejecutar el punto de partida vemos dos entidades, el **Player** y el **Minotauro**. Dotados de controles por teclado y una inteligencia artificial basada en algoritmos de pathfinding de A* con Heurística respectivamente. El jugador puede desplazarse libremente por el área de juego, esquivando al minotauro o usando el Hilo como ya se explicó en la documentación de la Práctica 2.

![](https://github.com/javimu06/IAV22-MunozMartindelaSierra/blob/main/images%20documentacion/InGame0.png)

## Proyecto Final
Mi proyecto final consistirá en expandir el punto de Partida en diferentes aspectos explicados a continuación:
Expandir la gamificación del jugador, dándole objetivos dentro del tablero de juego, como buscar objetos concretos (llaves, salidas) o la capacidad de recoger objetos que mejoren sus capacidades (como la visión, la velocidad...).
Expandir el comportamiento del minotauro a través de una máquina de estados. Ya no solo seguirá al jugador o merodeará por el mapa, si no que dependiendo del estado del juego tentrá que realizar una serie de tareas concretas. Para la elaboración de esta máquina de estados utilizaré las extensiones indicadas por el profesor para la resolución de la Práctica 3.
Elaborar una generación de mapas pseudo aleatoria, teniendo siempre unas zonas concretas como la entrada/salida de la mazmorra, pero elaborando el resto de caminos de manera procedimental.
Además de un rework estético general, para ganar una apariencia mucho más elaborada con animaciones, sombras y un entorno menos prototipado.
Dependiendo del tiempo que tarde en elaborar las expansiones que quiero realizar replantearé migrar el proyecto a Realidad Virtual. De tal manera que el jugador pueda jugar en primera persona con unas gafas de realidad virtual, desplazándose por el entorno e interaccionando de esta manera.

## Referentes
### Juegos
Pacman, Namco
### Libros
AI for Games, Ian Milligton

## Pseudocódigo

### Teseo

#### Movimiento por casillas

El movimiento fijo por casillas como en "Pacman". Es decir que será un movimiento continuo con giros de 90 grados y de "casilla en casilla".
Hemos decidido plantear este tipo de movimiento tanto para Teseo como para el Minotauro para reducir el coste de las iteraciones. De esta manera podemos saber exactamente cuando el minotauro corte el Hilo de Ariadna, para actualizarlo, y cuando el minotauro bloquea un camino posible, cosa que con un movimiento continuo sería mucho más ambiguo.
Como resultado a lo mejor tendremos un movimiento más artificial, sin embargo pensamos que merece la pena por la versatilidad que nos ofrece al basar la escena en un tablero distribuido por casillas.

El movimiento de Teseo definitivo estará basado en el siguiente pseudocódigo:
```
#El input se recoge cada iteracción
#El personaje cambiará a la dirección deseada una vez se encuentre en el centro de la casilla hacia la que se movía

Vector2D nextStep;  //Guarda la posición de la casilla a la que se tiene que desplazar


void Update(){
    actualInput = eventHandler();
    if(Se puede mover){
        //Gira hacia la dirección del ultimo input
        if(anteriorInput != actualInput){
            anteriorInput = actualInput;
            //Actualiza la siguiente casilla
        }
    } else {
        //Se sigue moviendo hacia la casilla a la que se estaba desplazando

    }
}

bool sePuedeMover(){
    if(El personaje se encuentra en el centro de una casilla){
        return true;
    } else return false;
}
```

### Hilo de Ariadna

El Hilo de Ariadna se basa en el algoritmo de pathfinding A*, debido a que la intención es encontrar el camino con menor coste desde una posición hasta otra. Una vez tengamos este camino lo marcaremos.

#### A* Pathfinding

Es un algoritmo que trabaja en iteraciones. Cada iteración considera un nodo del grafo y busca el camino más optimo dentro de sus posibles conexiones.

```
public List<Vertex> GetPathAstar (GameObject srcO, GameObject dstO, Heuristic h = null){
    # This structure is used to keep track of the
    # information we need for each node.
    class NodeRecord{
        node: Node
        connection: Connection
        costSoFar: float
        estimatedTotalCost: float
    }
    # Initialize the record for the start node.
    startRecord = new NodeRecord()
    startRecord.node = start
    startRecord.connection = null
    startRecord.costSoFar = 0
    startRecord.estimatedTotalCost = heuristic.estimate(start)

    # Initialize the open and closed lists.
    open = new PathfindingList()
    open += startRecord
    closed = new PathfindingList()

    # Iterate through processing each node.
    while length(open > 0){
        # Find the smallest element in the open list (using the
        # estimatedTotalCost).
        current = open.smallestElement();

        # If it is the goal node, then terminate.
        if (current.node == goal)
            break;

        # Otherwise get its outgoing connections.
        connections = graph.getConnections(current);

        # Loop through each connection in turn.
        for connection in connections{
            # Get the cost estimate for the end node.
            endNode = connection.getToNode()
            endNodeCost = current.costSoFar + connection.getCost()

            # If the node is closed we may have to skip, or remove it
            # from the closed list.
            if closed.contains(endNode){
                # Here we find the record in the closed list
                # corresponding to the endNode.
                endNodeRecord = closed.find(endNode)

                # If we didn’t find a shorter route, skip.
                if endNodeRecord.costSoFar <= endNodeCost:
                continue

                # Otherwise remove it from the closed list.
                closed -= endNodeRecord

                # We can use the node’s old cost values to calculate
                # its heuristic without calling the possibly expensive
                # heuristic function.
                endNodeHeuristic = endNodeRecord.estimatedTotalCost -
                endNodeRecord.costSoFar
            }
            # Skip if the node is open and we’ve not found a better
            # route.
            else if open.contains(endNode){
                # Here we find the record in the open list
                # corresponding to the endNode.
                endNodeRecord = open.find(endNode)

                # If our route is no better, then skip.
                if endNodeRecord.costSoFar <= endNodeCost:
                continue

                # Again, we can calculate its heuristic.
                endNodeHeuristic = endNodeRecord.cost -
                endNodeRecord.costSoFar
            }
            # Otherwise we know we’ve got an unvisited node, so make a
            # record for it.
            else:
                endNodeRecord = new NodeRecord()
                endNodeRecord.node = endNode

                # We’ll need to calculate the heuristic value using
                # the function, since we don’t have an existing record
                # to use.
                endNodeHeuristic = heuristic.estimate(endNode)

            # We’re here if we need to update the node. Update the
            # cost, estimate and connection.
            endNodeRecord.cost = endNodeCost
            endNodeRecord.connection = connection
            endNodeRecord.estimatedTotalCost = endNodeCost + endNodeHeuristic

            # And add it to the open list.
            if not open.contains(endNode){
                Open += endNodeRecord
            }
        }
        # We’ve finished looking at the connections for the current
        # node, so add it to the closed list and remove it from the
        # open list.
        open -= current
        closed += current
    }
    # We’re here if we’ve either found the goal, or if we’ve no more
    # nodes to search, find which.
    if current.node != goal{
                # We’ve run out of nodes without finding the goal, so there’s
                # no solution.
                return null
    } else {
        # Compile the list of connections in the path.
        path = []

        # Work back along the path, accumulating connections.
        while current.node != start{
            path += current.connection
            current = current.connection.getFromNode()
        }
        # Reverse the path, and return it.
        return reverse(path)
    }
}
```

##### Otras estructuras utilizadas en el codigo

##### Connection
```
class Connection(){
    float cost;
    Node fromNode;
    Node toNode;

    float getCost(){return cost;}
    
    Node getFromNode(){return fromNode;}
    
    Node getToNode() {return toNode;}
}
```
##### Heuristic
```
class Heuristic{
    # Stores the goal node that this heuristic is estimating for.
    Node goalNode;
    
    # Estimated cost to reach the stored goal from the given node.
    float estimate(Node fromNode ){return estimate(fromNode, goalNode);}

    # Estimated cost to move between any two nodes.
    float estimate(Node fromNode, Node toNode);

    which can then be used to call the pathfinder in code such as:
    pathfindAStar(graph, start, end, new Heuristic(end));
}

```

### Minotauro

El minotauro tiene 2 comportamientos, Merodear y Perseguir.
#### Merodear
El comportamiento de Merodear comparte el algoritmo de A* mencionado anteriormente, calcula el camino más corto desde su posición actual hasta el nodo de destino, que se selecciona de materia aleatoria en un radio respecto a la entidad.

#### Perseguir
El comportamiento de Perseguir comparte el algoritmo de A* mencionado anteriormente, ya que busca el camino más corto hacia el jugador en cada iteración.



