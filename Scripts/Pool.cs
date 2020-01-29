/// Bird by Example object and particle pooler. 
/// Recycles objects to save computation, writing and memory.
/// Used in place of instantiation calls throughout the game. See Flora.cs for an example.
/// Noah James Burkholder 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton designed to dynamically organize and recycle gameplay objects to avoid waste.
/// Keeps track of Objects, Particles, and Debris, which are all recycled slightly differently.
/// TODO: Use polymorphism to handle different types of object pools as extended types.
/// </summary>
public class Pool : MonoBehaviour
{
    private static Pool _instance;

    public static Pool instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (Pool)FindObjectOfType(typeof(Pool));

                if (_instance == null)
                {
                    Debug.LogError("An instance of " + typeof(Pool) +
                        " is needed in the scene, but there is none.");
                }
            }

            return _instance;
        }
    }

    // Enumerators which decode which integer refers to what type of object.
    public enum ObjectType
    {
        Orange = 0
        // TODO: Add Raspberries...
    }
    public enum ParticleType
    {
        Dust = 0
        // TODO: Add blood cloud...
    }

    public enum DebrisType
    {
        OrangeSlice = 0
        // TODO: Add wood splinters?
    }

    /// <summary>
    /// Object pool which tracks visual debris objects.
    /// </summary>
    public class DebrisPool
    {
        // Members.
        public int type; // The type of object this pool is tracking.
        public int cap = 20; // Maximum number of this type of object.
        public List<RecycleableInteractable> list; // Actual list.

        /// <summary>
        /// Creates new debris pooler.
        /// </summary>
        public DebrisPool(int type)
        {
            this.type = type;
            list = new List<RecycleableInteractable>();
        }
        /// <summary>
        /// Gets an available object.
        /// </summary>
        public GameObject GetAvailable(GameObject g)
        {
            TrackObject(new RecycleableInteractable(g, false, g.GetComponent<Interactable>()));
            return g;
        }

        /// <summary>
        /// Begins tracking a new piece of debris.
        /// </summary>
        public void TrackObject(RecycleableInteractable r)
        {
            if (list == null) list = new List<RecycleableInteractable>();
            list.Add(r);

            Debug.Log("New debris of " + r.obj.name);
        }
        /// <summary>
        /// Stops tracking piece of debris.
        /// </summary>
        public void UntrackObject(RecycleableInteractable r)
        {
            if (list == null) list = new List<RecycleableInteractable>();
            list.Remove(r);
        }

        /// <summary>
        /// Untracks all objects.
        /// </summary>
        public void CleanTracked()
        {
            if (list == null)
                return;
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Clean();
            }
            list.Clear();
            list = new List<RecycleableInteractable>();
        }

        /// <summary>
        /// Cleaves off any extra debris past the maximum.
        /// </summary>
        public void Hardcap()
        {
            while (list.Count > cap)
            {
                list[0].Clean();
                list.RemoveAt(0);
            }
        }

    }

    /// <summary>
    /// Subclass which supplements GameObjects/Interactables with recycling capabilities.
    /// </summary>
    public class RecycleableInteractable
    {
        // Members.
        public GameObject obj;
        public Interactable interactionScript;
        public bool isAvailable = false;

        /// <summary>
        /// Converts a GameObject with an Interactable script to a new RecycleableInteractable.
        /// </summary> 
        public RecycleableInteractable(GameObject g, bool isAvailable, Interactable script)
        {
            this.isAvailable = isAvailable;
            interactionScript = script;
            obj = g;
        }

        /// <summary>
        /// Extendable function which removes an object responsibly.
        /// </summary> 
        public void Clean()
        {
            Destroy(obj);
        }

        /// <summary>
        /// Returns this RecycleableInteractable's GameObject.
        /// </summary> 
        public GameObject GetObject()
        {
            return obj;
        }
    }

    /// <summary>
    /// Object pool which tracks objects with the Interactable component attached.
    /// </summary>
    public class InteractablePool
    {
        // Enumerator for the type of object.
        public int type;

        // The actual tracked list.
        public List<RecycleableInteractable> list;

        /// <summary>
        /// Creates a new interactable pool of a supplied type.
        /// </summary>
        public InteractablePool(int type)
        {
            this.type = type;
            list = new List<RecycleableInteractable>();
        }

        /// <summary>
        /// Fetch an available object.
        /// </summary>
        public GameObject GetAvailable()
        {
            foreach (RecycleableInteractable r in list)
            {
                if (r.isAvailable) // If an object is available.
                {
                    return r.GetObject(); // Return it immediately and exit.
                }
            }

            // If no object has been found, dynamically increase the pool size.

            GameObject g; // Declare new GO...
            
            // Instantiate from the disk. This is an expensive operation and is used as a last resort.
            g = Instantiate(instance.objects[type]) as GameObject;

            // Convert it to a recycleable object and begin tracking it in the pool.
            TrackObject(new RecycleableInteractable(g, false, g.GetComponent<Interactable>()));
            return g; // Return NEW GameObject.


        }

        /// <summary>
        /// Fetches an available object and moves it to a position.
        /// </summary>
        public GameObject GetAvailable(Vector3 position)
        {
            foreach (RecycleableInteractable r in list)
            {
                if (r.interactionScript.isAvailable) // If an object is available.
            {
                    GameObject go = r.GetObject(); // Cache it...
                    go.transform.position = position; // Move its position...
                    return go; // And return it, exiting immediately.
                }
            }

            // If no object has been found, dynamically increase the pool size.

            GameObject g; // Declare new GO...

            // Instantiate from the disk, at a certain position. This is an expensive operation and is used as a last resort.
            g = Instantiate(instance.objects[type], position, Quaternion.identity) as GameObject;

            // Convert it to a recycleable object and begin tracking it in the pool.
            TrackObject(new RecycleableInteractable(g, false, g.GetComponent<Interactable>()));
            return g; // Return NEW GameObject.

        }

        /// <summary>
        /// Adds a recycleable object to the pool. Done as little as possible.
        /// </summary> 
        public void TrackObject(RecycleableInteractable r)
        {
            // Double check the list exists...
            if (list == null) list = new List<RecycleableInteractable>();

            // Add new item 'r'.
            list.Add(r);

            Debug.Log("New " + r.obj.name); // Report a new tracked item.
        }

        /// <summary>
        /// Used in place of destroying GameObjects.
        /// </summary> 
        public void UntrackObject(RecycleableInteractable r)
        {
            if (list == null) list = new List<RecycleableInteractable>();
            list.Remove(r);
        }

        /// <summary>
        /// Wipes the whole pool of objects.
        /// </summary> 
        public void CleanTracked()
        {
            if (list == null)
                return;
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Clean();
            }
            list.Clear();
            list = new List<RecycleableInteractable>();
        }
        
    }

    /// <summary>
    /// Object pool which tracks Particle systems. 
    /// Defined separately so pool maximums can be changed independently.
    /// </summary> 
    public class ParticlePool
    {

        // The enumerated type of particle.
        public int type;

        // The tracked list.
        public List<RecycleableParticle> list;

        /// <summary>
        /// Makes a new particle of a given (enumerated) type.
        /// </summary> 
        public ParticlePool(int type)
        {
            this.type = type;
            list = new List<RecycleableParticle>();
        }

        /// <summary>
        /// Return any available particle system.
        /// </summary> 
        public GameObject GetAvailable()
        {
            foreach (RecycleableParticle r in list)
            {
                if (r.isAvailable) // If one is available...
                {
                    return r.GetObject(); // Recycle it and return it immediately.
                }
            }

            // At this point, we can assume we couldn't find a pooled particle.

            // So we declare a new one.
            GameObject g;

            // Intantiate it...
            g = Instantiate(instance.particles[type]) as GameObject;

            // And track this new instantiated particle so it's available later.
            TrackObject(new RecycleableParticle(g, false, g.GetComponent<ParticleRecycle>()));
            return g; // Return NEW particle system.


        }
        /// <summary>
        /// Fetches an available particle system and moves it to a position.
        /// </summary> 
        public GameObject GetAvailable(Vector3 position)
        {
            foreach (RecycleableParticle r in list)
            {
                if (r.particleRecycleScript.CheckAvailable())
                {
                    GameObject go = r.GetObject();
                    go.transform.position = position;
                    return go;
                }
            }
            // At this point, we can assume we couldn't find a pooled particle.

            // So we declare a new one.
            GameObject g;

            // Intantiate it at a particular location...
            g = Instantiate(instance.particles[type], position, Quaternion.identity) as GameObject;
            
            // And track this new instantiated particle so it's available later.
            TrackObject(new RecycleableParticle(g, false, g.GetComponent<ParticleRecycle>()));
            return g; // Return NEW particle system.

        }
        /// <summary>
        /// Adds a recycleable object to the pool. Done as little as possible.
        /// </summary> 
        public void TrackObject(RecycleableParticle r)
        {
            if (list == null) list = new List<RecycleableParticle>();
            list.Add(r);
        }

        /// <summary>
        /// Used in place of destroying GameObjects.
        /// </summary> 
        public void UntrackObject(RecycleableParticle r)
        {
            if (list == null) list = new List<RecycleableParticle>();
            list.Remove(r);
        }
        /// <summary>
        /// Wipes the whole list of particles.
        /// </summary> 
        public void CleanTracked()
        {
            if (list == null)
                return;
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Clean();
            }
            list.Clear();
            list = new List<RecycleableParticle>();
        }
        /// <summary>
        /// Supplements a particle system with recycleability.
        /// </summary> 
        public class RecycleableParticle
        {
            // Members.
            public GameObject obj;
            public ParticleRecycle particleRecycleScript;
            public bool isAvailable = false;

            /// <summary>
            /// Converts a particle system to be recycleable.
            /// </summary> 
            public RecycleableParticle(GameObject g, bool isAvailable, ParticleRecycle ps)
            {
                this.isAvailable = isAvailable;
                particleRecycleScript = ps;
                obj = g;
            }

            /// <summary>
            /// Dispose of particle system responsibly.
            /// </summary> 
            public void Clean()
            {
                Destroy(obj);
            }

            /// <summary>
            /// Return the GameObject of this RecycleableParticle
            /// </summary> 
            public GameObject GetObject()
            {
                return obj;
            }
        }
    }

    // Prefabs on disk. Populated in inspector. Indexes correspond to enumerators.
    public List<Object> objects;
    public List<Object> particles;

    // All the entity pools.
    private static List<Bird> AllBirds;
    private static List<InteractablePool> AllObjects;
    private static List<ParticlePool> AllParticles;
    private static List<DebrisPool> AllDebris;

    private void Awake()
    {
        Initialize();
        StartCoroutine(PoolMaintenance());
    }

    /// <summary>
    /// Create necessary pools.
    /// </summary>
    public static void Initialize()
    {
        AllObjects = new List<InteractablePool>();
        for (int i = 0; i < System.Enum.GetNames(typeof(ObjectType)).Length; i++)
        {
            AllObjects.Add(new InteractablePool(i));

        }

        AllParticles = new List<ParticlePool>();
        for (int i = 0; i < System.Enum.GetNames(typeof(ParticleType)).Length; i++)
        {
            AllParticles.Add(new ParticlePool(i));
        }

        AllDebris = new List<DebrisPool>();
        for (int i = 0; i < System.Enum.GetNames(typeof(DebrisType)).Length; i++)
        {
            AllDebris.Add(new DebrisPool(i));
        }
    }

    /// <summary>
    /// Does repeating tasks at a low rate to make sure nothing goes wrong for too long,
    /// </summary> 
    private IEnumerator PoolMaintenance() // Silly name.
    {
        while (true)
        {
            for (int i = 0; i < AllDebris.Count; i++)
            {
                AllDebris[i].Hardcap();
            }
            yield return GameManager.Wait1;
        }
    }

    /// <summary>
    /// Track a new bird for the bird pool.
    /// </summary> 
    public static void TrackBird(Bird b)
    {
        if (AllBirds == null) AllBirds = new List<Bird>();
        AllBirds.Add(b);
    }

    /// <summary>
    /// Untrack a bird which is in the pool already.
    /// </summary> 
    public static void UntrackBird(Bird b)
    {
        if (AllBirds == null) AllBirds = new List<Bird>();
        AllBirds.Remove(b);
    }

    /// <summary>
    /// Empties everything.
    /// </summary> 
    public static void CleanAllTracked()
    {
        // Objects
        if (AllObjects == null)
            return;
        for (int i = 0; i < AllObjects.Count; i++)
        {
            AllObjects[i].CleanTracked();
        }
        AllObjects.Clear();

        // Debris
        if (AllDebris == null)
            return;
        for (int i = 0; i < AllDebris.Count; i++)
        {
            AllDebris[i].CleanTracked();
        }
        AllDebris.Clear();

        // Particles
        if (AllParticles == null)
            return;
        for (int i = 0; i < AllParticles.Count; i++)
        {
            AllParticles[i].CleanTracked();
        }
        AllParticles.Clear();

        Initialize();

        // Birds

        if (AllBirds == null)
            return;
        for (int i = 0; i < AllBirds.Count; i++)
        {
            Destroy(AllBirds[i].gameObject);
        }
        AllBirds.Clear();
        AllBirds = new List<Bird>();
    }
    /// <summary>
    /// Responsibly murders a bird. The circle of life.
    /// </summary> 
    public static void CleanBird(Bird b)
    {
        if (AllBirds == null)
            return;

        AllBirds.Remove(b);
        (b.gameObject).SetActive(false); // Disable instead of deleting.

    }
    /// <summary>
    /// Returns current bird population.
    /// Used for balancing ecosystem based on computer performance.
    /// </summary> 
    public static int GetPopulation()
    {
        int population = 0;
        foreach (Bird b in AllBirds)
        {
            if (b.Body.isAlive)
            {
                population++;
            }
        }
        return population;
    }

    /// <summary>
    /// Returns pooled object of a particular type by enumerator.
    /// </summary> 
    public static GameObject GetObject(ObjectType type)
    {
        GameObject g = AllObjects[(int)type].GetAvailable();
        return g;
    }

    /// <summary>
    /// Returns pooled debris of a particular type by enumerator.
    /// </summary> 
    public static void GetDebris(DebrisType type, GameObject g)
    {
        AllDebris[(int)type].GetAvailable(g);
    }
    /// <summary>
    /// Returns pooled particle system of a particular type by enumerator.
    /// </summary> 
    public static GameObject GetParticle(ParticleType type)
    {
        GameObject g = AllParticles[(int)type].GetAvailable();
        return g;
    }

    /// <summary>
    /// Returns a pooled particle system of a particular type by enumerator, at a given position.
    /// </summary> 
    public static GameObject GetParticle(ParticleType type, Vector3 position)
    {
        GameObject g = AllParticles[(int)type].GetAvailable(position);
        return g;
    }
}
