using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.InputSystem;

public class PathMarker
{
    public MapLocation location;
    public float G;
    public float H;
    public float F;
    public GameObject marker;
    public PathMarker parent;


    public PathMarker(MapLocation l, float g, float h, GameObject marker, PathMarker p)
    {
        location = l;
        G = g;
        H = h;
        F = g + h;
        this.marker = marker;
        parent = p;
    }
    public PathMarker(MapLocation l, float g, float h, float f, GameObject marker, PathMarker p)
    {
        location = l;
        G = g;
        H = h;
        F = f;
        this.marker = marker;
        parent = p;
    }

    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            return location.Equals(((PathMarker)obj).location);
        }
    }

    public override int GetHashCode()
    {
        return 0;
    }
}


public class FindPathAStar : MonoBehaviour
{
    public Maze maze;
    public Material closedMaterial;
    public Material openMaterial;

    List<PathMarker> open = new List<PathMarker>();   //Lista OPEN de PathMakers
    List<PathMarker> closed = new List<PathMarker>(); //Lista CLOSED de PathMakers

    public GameObject start;
    public GameObject end;
    public GameObject pathP;

    public float updateFreq = 0.2f;

    PathMarker goalNode;  //PathMaker que marca la posicion final (DESTINO)
    PathMarker startNode; //PathMaker que marca la posicion inicial (ORIGEN)

    PathMarker lastPos; //Nodo en el que estamos en cada momento, cuando estamos
                        //buscando el DESTINO y cuando estamos dibujando el CAMINO
    bool done = false;  //Booleano que se utiliza para decir si se ha alcanzado el objetivo

    bool algorithmStarted = false;  //Hemos inicializado el juego
    bool animationRunning = false;

    void RemoveAllMarkers()
    {
        //TODO: Eliminar todos los GameObjects que puedan haber en el mapa (maze)
        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");
        foreach (GameObject m in markers)
            Destroy(m);
    }

    void BeginSearch()
    {
        //Aquí inicializamos las posiciones inicial, final y las listas OPEN y CLOSED

        //Esta variable controlará si el problema está solucionado
        done = false;
        //Eliminar posibles GameObjects en el mapa
        RemoveAllMarkers();
        CancelInvoke();
        animationRunning = false;

        algorithmStarted = true;

        //Creamos una lista de localizaciones validas, esto es, objetos del tipo MapLocation
        List<MapLocation> locations = new List<MapLocation>();
        //Ahora generamos el grafo comprobando si, en cada posición, tenemos un muro o no (-1 o 1)
        // En el caso de no tener un muro, agregamos un nuevo MapLocation con esa coordenada a la lista
        for (int z = 1; z < maze.depth - 1; z++)
            for (int x = 1; x < maze.width - 1; x++)
            {
                if (maze.map[x, z] != 1)
                    locations.Add(new MapLocation(x, z));
            }
        //Las listas tienen un método Shuffle que nos permite barajar los elementos que contiene
        // Barajamos las posiciones para que no estén contiguas
        locations.Shuffle();

        //Escogemos una localización al azar (la que se quedó en la primera posición, por ejemplo) y esa será 
        // la posición de salida, almacenada en un Vector3
        // Cuidado: Para mantener la ubicación consistente, tenemos que multiplicar la coordenada por la escala del laberinto (maze.scale)
        Vector3 startLocation = new Vector3(locations[0].x * maze.scale, 0, locations[0].z * maze.scale);
        //Ahora podemos crear un nuevo PathMarker, el objeto que utitlizaremos para representar una posición dentro del laberinto de forma visual
        //Un PathMarker necesita:
        // - MapLocation
        // - Los valores de la función f:
        //      - g(n)
        //      - h(n)
        // - El GameObject con el que representaremos esta posición
        // - Y otro PathMarker que será el "padre" de este nodo

        //Inicializamos goalNode como posicion de llegada, creando de la misma forma un Vector3 con las coordenadas, 
        // y después un PathMarker con esta información
        Vector3 goalLocation = new Vector3(locations[1].x * maze.scale, 0, locations[1].z * maze.scale);
        goalNode = new PathMarker(new MapLocation(locations[1].x, locations[1].z), 0, 0, 0, Instantiate(end, goalLocation, Quaternion.identity), null);

        // Este nodo empieza con su valor de la heurística (distancia euclidea hasta el objetivo).
        float h_n = Vector2.Distance(locations[0].ToVector(), goalNode.location.ToVector());
        GameObject startBlock = Instantiate(start, startLocation, Quaternion.identity);

        startNode = new PathMarker(locations[0], 0, h_n, startBlock, null);

        TextMesh[] values = startBlock.GetComponentsInChildren<TextMesh>();
        values[0].text = "g(n): " + startNode.G.ToString("0.00");
        values[1].text = "h(n): " + startNode.H.ToString("0.00");
        values[2].text = "f(n): " + startNode.F.ToString("0.00");


        //Eliminamos lo que contenga las listas de OPEN y CLOSED por si se ha vuelto a ejecutar el algoritmo
        open.Clear();
        closed.Clear();

        //Comenzamos añadiendo el nodo startNode a CLOSED
        closed.Add(startNode);

        //Y para cuestiones relacionadas con la UI, almacenamos una referencia al nodo inicial en lastPos, por ahora
        lastPos = startNode;
    }

    //TODO: Completad este método
    void Search(PathMarker thisNode)
    {
        //Este método ejecutará la búsqueda para encontrar el camino

        // Si el algortmo aún no ha comenzado, salimos sin hacer nada
        if (!algorithmStarted || done || thisNode == null)
        {
            if (done)
            {
                CancelInvoke();
                animationRunning = false;
            }
            return;
        }

        //Comprobamos si este nodo desde donde empezamos es ya solución
        if (thisNode.location.Equals(goalNode.location))
        {
            done = true;
            CancelInvoke();
            animationRunning = false;
            return;
        }

        //Comenzamos buscando vecinos de este nodo
        // MapLocation almacena una coordenada x,z y también una lista de posibles direcciones (adelante/atrás/izquierda/derecha)
        foreach (MapLocation dir in maze.directions)
        {
            //Inicializamos la localizacion "neighbour"
            MapLocation neighbour = thisNode.location + dir;
            //Ojo, en este punto neighbour es solo una posicion en el mapa

            //Antes de procesar este vecino chequeamos que cumpla ciertas condiciones:
            //Antes de procesarlo, nos preguntamos:
            //1.- ¿Es un muro? Si es así, continuamos el bucle
            if (neighbour.x < 1 || neighbour.x >= maze.width - 1 || neighbour.z < 1 || neighbour.z >= maze.depth - 1)
                continue;

            if (maze.map[neighbour.x, neighbour.z] == 1)
                continue;

            //2.- En esta dirección, ¿Sigo dentro del laberinto? Tienes que estar entre 1 y maze.width para el eje X o entre 1 y maze.depth para el eje Z

            //3.- ¿He visitado ya al vecino en una iteración anterior? o lo que es lo mismo, ¿Está en CLOSED?
            if (IsClosed(neighbour))
                continue;

            //Aqui ya sabemos que el vecino es válido
            //Sumamos la G que teniamos a la distancia entre los nodos vecinos
            float g = thisNode.G + Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector());
            float h = Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector());
            float f = g + h;

            //Creamos un gameObject "pathBlock" para poner en este punto de nuestro camino
            GameObject pathBlock = Instantiate(pathP, new Vector3(neighbour.x * maze.scale, 0, neighbour.z * maze.scale), Quaternion.identity);

            //"pathBlock" tiene el texto G, F, H adjunto como subcomponentes, los inicializamos esto strings
            Renderer r = pathBlock.GetComponent<Renderer>();
            if (r != null)
                r.material = openMaterial;

            TextMesh[] values = pathBlock.GetComponentsInChildren<TextMesh>();
            if (values.Length >= 3)
            {
                values[0].text = "g(n): " + g.ToString("0.00");
                values[1].text = "h(n): " + h.ToString("0.00");
                values[2].text = "f(n): " + f.ToString("0.00");
            }

            //Ahora tenemos que comprobar si este nodo ya está en la lista de OPEN
            // Si está en la lista, actualizaremos sus valores para g(n) y h(n) , así como el padre que sera este ahora
            // Si no está lo añadiremos a la lista OPEN
            // Esta comprobación la puede realizar la función "UpdateMarker"
            if (UpdateMarker(neighbour, g, h, f, thisNode))
            {
                Destroy(pathBlock);
            }
            else
            {
                PathMarker pm = new PathMarker(neighbour, g, h, f, pathBlock, thisNode);
                open.Add(pm);
            }
        }

        //Ahora tenemos que elegir el siguiente nodo a expandir
        // De open, nos quedamos con el que menor f(n) tenga
        PathMarker bestNode = null;
        float bestF = float.MaxValue;

        foreach (PathMarker p in open)
        {
            if (p.F < bestF)
            {
                bestF = p.F;
                bestNode = p;
            }
        }

        if (bestNode == null)
        {
            done = true;
            CancelInvoke();
            animationRunning = false;
            return;
        }

        // Ese nodo lo añadimos a CLOSED
        closed.Add(bestNode);
        // Lo quitamos de OPEN
        // Marcamos este PathMarker como nodo CLOSED
        // Quitamos el pathMaker de la open list
        open.Remove(bestNode);
        // Indicamos, cambiando el material, que este marcador tendrá el color de los puntos "CLOSED"
        Renderer bestRenderer = bestNode.marker.GetComponent<Renderer>();
        if (bestRenderer != null)
            bestRenderer.material = closedMaterial;
        // Indicamos que este es el lastPos donde nos hemos quedado
        lastPos = bestNode;

        if (lastPos.location.Equals(goalNode.location))
        {
            done = true;
            CancelInvoke();
            animationRunning = false;
        }
    }
    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)
    {
        //Para cada pathMaker en al open list
        //   Si pos esta en la lista:
        //      actualizamos G H F y parent
        //      devolvemos TRUE
        //Devolvemos FALSE
        foreach (PathMarker p in open)
        {
            if (p.location.Equals(pos))
            {
                if (g < p.G)
                {
                    p.G = g;
                    p.H = h;
                    p.F = f;
                    p.parent = prt;

                    TextMesh[] values = p.marker.GetComponentsInChildren<TextMesh>();
                    if (values.Length >= 3)
                    {
                        values[0].text = "g(n): " + p.G.ToString("0.00");
                        values[1].text = "h(n): " + p.H.ToString("0.00");
                        values[2].text = "f(n): " + p.F.ToString("0.00");
                    }
                }
                return true;
            }
        }
        return false;
    }

    bool IsClosed(MapLocation marker)
    {
        //Devolvemos TRUE si marker esta en la lista de closed
        foreach (PathMarker p in closed)
        {
            if (p.location.Equals(marker)) return true;
        }
        return false;
    }


    // Start is called before the first frame update
    void Start()
    {

    }



    //TODO: Completad este método
    void GetPath()
    {
        //Este método dibujará el camino desde el inicio hasta el destino

        if (!done)
            return;

        CancelInvoke();
        animationRunning = false;

        //Borrar todos los pathMakers
        RemoveAllMarkers();
        //Inicializamos begin a lastPos
        PathMarker begin = lastPos;

        //Mientras startNode no sea begin y begin no sea NULL
        //    Instanciamos un nuevo pathMaker que senyalara el camino de vuelta
        //    begin pasara ahora a señalar al su nodo padre
        while (begin != null)
        {
            GameObject pathBlock = Instantiate(pathP, new Vector3(begin.location.x * maze.scale, 0.5f, begin.location.z * maze.scale), Quaternion.identity);

            TextMesh[] values = pathBlock.GetComponentsInChildren<TextMesh>();
            if (values.Length >= 3)
            {
                values[0].text = "";
                values[1].text = "";
                values[2].text = "";
            }

            if (begin.location.Equals(startNode.location))
                break;

            begin = begin.parent;
        }

        //Instanciamos un ultimo pathMaker que senyalara la posicion inicial
        Instantiate(start, new Vector3(startNode.location.x * maze.scale, 0, startNode.location.z * maze.scale), Quaternion.identity);
        Instantiate(end, new Vector3(goalNode.location.x * maze.scale, 0, goalNode.location.z * maze.scale), Quaternion.identity);

    }

    void Search()
    {
        Search(lastPos);
    }

    // Update is called once per frame
    void Update()
    {
        //Si presionamos la letra "P" inicializamos el juego
        if (Keyboard.current[Key.P].wasPressedThisFrame)
        {

            BeginSearch();

        }

        //Si presionamos la letra "C" calculamos el siguiente movimiento del alg. A*
        if (Keyboard.current[Key.C].wasPressedThisFrame && !done)
        {
            Search(lastPos);
        }
        //Si presionamos la letra "M" dejamos solo los pathMakers que forman el path optimo
        if (Keyboard.current[Key.M].wasPressedThisFrame)
        {
            GetPath();
        }

        //Si presionamos la tecla A, se reproduce la búsqueda a modo animación con una frecuencia dada
        if (Keyboard.current[Key.A].wasPressedThisFrame)
        {
            if (algorithmStarted && !done && !animationRunning)
            {
                animationRunning = true;
                InvokeRepeating(nameof(Search), 0.0f, updateFreq);
            }
        }
    }
}