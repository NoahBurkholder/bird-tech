/// Bird by Example baseline interactable object. 
/// A GameObject which birds react neurologically to (based on what it 'evokes').
/// These evokes afford birds to transfer skills and behaviours based on shared attributes on disparate objects.
/// Birds can only focus on one Interactable type object at a time. Birds are also considered an Interactable.
/// Also holds information about its physical properties.
/// Noah James Burkholder 2020

using System.Collections;
using UnityEngine;

/// <summary>
/// The baseline type of gameplay object.
/// All interactables 'evoke' concepts, which let birds transfer skills and behaviour between objects.
/// </summary>
public class Interactable : MonoBehaviour
{
    // Static variable used to tweak the durability of all things simultaneously.
    public static float WorldwideDurability = 0.01f;

    [Header("Debug")]
    public bool trackLifecycle; // Turn this to true to have the object report its information wherever applicable.

    [HideInInspector] protected bool isAlive = true;
    public bool isPlayer;
    public bool isAvailable;
    public Bird parentBird;
    public string thisName;
    public Transform thisTransform;

    // For playing audio.
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] collisionSounds;
    public AudioClip[] breakSounds;

    // Some attributes of an instanced interactable.
    public bool canBePickedUp = false;
    public float weight = 1f;
    public bool canBeDestroyed = false;
    public float maxHitpoints = 100f;
    public float integrity = 100f;

    // Pardon the ugly column. A side-effect of using Unity Engine serialized sliders.
    [Range(-1f, 1f)]
    public float evokesDeath;
    [Range(-1f, 1f)]
    public float evokesSuffering;
    [Range(-1f, 1f)]
    public float evokesFamine;
    [Range(-1f, 1f)]
    public float evokesDeceit;
    [Range(-1f, 1f)]
    public float evokesDivine;
    [Range(-1f, 1f)]
    public float evokesCuriosity;
    [Range(-1f, 1f)]
    public float evokesCowardice;
    [Range(-1f, 1f)]
    public float evokesCompany;
    [Range(-1f, 1f)]
    public float evokesPower;
    [Range(-1f, 1f)]
    public float evokesWealth;
    [Range(-1f, 1f)]
    public float evokesMagic;
    [Range(-1f, 1f)]
    public float evokesCombat;
    [Range(-1f, 1f)]
    public float evokesDevelopment;
    [Range(-1f, 1f)]
    public float evokesConfinement;
    [Range(-1f, 1f)]
    public float evokesGathering;
    [Range(-1f, 1f)]
    public float evokesNature;
    [Range(-1f, 1f)]
    public float evokesCrafts;
    [Range(-1f, 1f)]
    public float evokesDestiny;
    [Range(-1f, 1f)]
    public float evokesLegacy;
    [Range(-1f, 1f)]
    public float evokesUnknown;

    public bool isReporting;

    // Consolidated array of the ugly column.
    public float[] evokes;
    public virtual float[] Evokes
    {
        get {
            if (evokes.Length <= 0)
            {
                evokes = UpdateEvokes();

                return evokes;
            }
            else
            {
                return evokes;
            }
        }

        set {
            evokes = value;
        }

    }

    // Used for giving birds dynamic nicknames based on the types of Interactables they interact most with.
    public string[] possibleTitles;

    private void Awake()
    {
        thisTransform = this.transform;
        isAlive = true;
    }
    /// <summary>
    /// Handle incoming blunt trauma.
    /// </summary>
    public virtual void BluntDamage(float bluntDamage)
    {
        if (trackLifecycle) Debug.Log(gameObject.name + " damaged by " + bluntDamage);
        if (isAlive)
        {
            integrity -= bluntDamage;
            if (integrity <= 0)
            {
                isAlive = false;
                StartCoroutine(Collapsing());
            }
        } else
        {
            Debug.LogWarning("Damaging dead entity!");
        }
    }

    /// <summary>
    /// Handle incoming slash damage.
    /// </summary>
    public virtual void SlashDamage (float slashDamage)
    {
        if (trackLifecycle) Debug.Log(gameObject.name + " damaged by " + slashDamage);
        if (isAlive)
        {
            integrity -= slashDamage;
            if (integrity <= 0)
            {
                isAlive = false;
                StartCoroutine(Collapsing());
            }
        }
        else
        {
            Debug.LogWarning("Damaging dead entity!");
        }
    }
    /// <summary>
    /// Handle incoming stabbing.
    /// </summary>
    public virtual void StabDamage(float stabDamage)
    {
        if (trackLifecycle) Debug.Log(gameObject.name + " damaged by " + stabDamage);
        if (isAlive)
        {
            integrity -= stabDamage;
            if (integrity <= 0)
            {
                isAlive = false;
                StartCoroutine(Collapsing());
            }
        }
        else
        {
            Debug.LogWarning("Damaging dead entity!");
        }
    }

    /// <summary>
    /// For handling destruction of any sort of interactable object.
    /// </summary>
    public virtual IEnumerator Collapsing()
    {
        // Do things that need to happen before dying:
        if (trackLifecycle) Debug.Log(gameObject.name + " is about to delete self.");

        // Then self-destroy.
        Destroy(gameObject);
        yield break;
    }
    public virtual void OnPickup()
    {
        // TODO: Populate this.
    }

    /// <summary>
    /// Returns a neat array of all the evocations.
    /// </summary>
    public float[] UpdateEvokes()
    {
        float[]newEvokes = new float[Qualities.NumQualities];
        newEvokes[(int)Quality.Death] = evokesDeath;
        newEvokes[(int)Quality.Suffering] = evokesSuffering;
        newEvokes[(int)Quality.Famine] = evokesFamine;
        newEvokes[(int)Quality.Deceit] = evokesDeceit;
        newEvokes[(int)Quality.Divine] = evokesDivine;
        newEvokes[(int)Quality.Curiosity] = evokesCuriosity;
        newEvokes[(int)Quality.Cowardice] = evokesCowardice;
        newEvokes[(int)Quality.Company] = evokesCompany;
        newEvokes[(int)Quality.Power] = evokesPower;
        newEvokes[(int)Quality.Wealth] = evokesWealth;
        newEvokes[(int)Quality.Magic] = evokesMagic;
        newEvokes[(int)Quality.Combat] = evokesCombat;
        newEvokes[(int)Quality.Development] = evokesDevelopment;
        newEvokes[(int)Quality.Confinement] = evokesConfinement;
        newEvokes[(int)Quality.Gathering] = evokesGathering;
        newEvokes[(int)Quality.Nature] = evokesNature;
        newEvokes[(int)Quality.Crafts] = evokesCrafts;
        newEvokes[(int)Quality.Destiny] = evokesDestiny;
        newEvokes[(int)Quality.Legacy] = evokesLegacy;
        newEvokes[(int)Quality.Unknowable] = evokesUnknown;
        return newEvokes;
    }

    /// <summary>
    /// Returns the evocations for bird perceptron inputs.
    /// </summary>
    public virtual float[] ReadQualities()
    {
        return Evokes;
    }
    // Used for directing the interest of birds.
    private float maxPossibleInterest;
    protected float MaxPossibleInterest
    {
        get
        {
            if (maxPossibleInterest == 0)
            {
                UpdateMaxPossibles();
            }
            return maxPossibleInterest;
        }
        set { maxPossibleInterest = value; }
    }

    // Used for calculating a bird's attitude.
    private float maxPossibleAttitude;
    protected float MaxPossibleAttitude
        {
        get
        {
            if (maxPossibleAttitude == 0)
            {
                UpdateMaxPossibles();
}
            return maxPossibleAttitude;
        }
        set { maxPossibleAttitude = value; }
    }

    /// <summary>
    /// Helper function.
    /// </summary>
    private void UpdateMaxPossibles()
    {
        MaxPossibleInterest = Qualities.NumQualities; // For each quality.
        MaxPossibleAttitude = Qualities.NumQualities; // Done.

        MaxPossibleInterest *= (Bird.MaxViewRadius + 1); // Assume minimal distance.
        MaxPossibleInterest += Qualities.NumQualities; // Assume player.

    }

    /// <summary>
    /// Returns wether something is alive.
    /// </summary>
    public bool GetAlive()
    {
        return isAlive;
    }
}
