/// Bird by Example fruit-bearing flora. An example of the Pool.cs being used to recycle fruit.
/// Noah James Burkholder 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fruit-bearing flora. Grows fruit of a particular type in a given time, at given positions.
/// </summary>
public class Flora : Interactable
{
    // Grow this particular type of object from the stems.
    public Pool.ObjectType growThis;

    // The fruit attached to this plant.
    public List<Fruit> attachedFruit;

    // The rigidbody attached to this plant.
    public Rigidbody thisRigidbody;

    // The maximum number of fruit this plant can carry.
    public int maxFruits;

    // The spawn points of this plant.
    public Transform[] fruitSpawns;

    // The time it takes for a fruit to grow.
    public float fruitGrowTime;

    // The amount of integrity regenerated per second.
    public float regenAmount;

    // The amount of time between fruits being dropped.
    private float dropCooldown = 0.5f;

    // Routine which makes sure only one fruit is dropped at a time.
    private Coroutine DislodgeRoutine;

    // Set up delegate. This listens for if the world is being reset, and re-initializes.
    private void OnEnable()
    {
        GameManager.NewWorld += Init;
    }

    // Remove delegate.
    private void OnDisable()
    {
        GameManager.NewWorld -= Init;
    }

    /// <summary>
    /// Initialize the flora.
    /// </summary>
    private void Init()
    {
        thisTransform = this.transform;
        UpdateEvokes();
        isAlive = true;
        attachedFruit = new List<Fruit>();
        maxFruits = fruitSpawns.Length;
        for (int i = 0; i < maxFruits; i++)
        {

            NewFruit(i, 1f);
        }
        StartCoroutine(RegenerateHealth()); // Make sure the flora is slowly healing.
    }

    /// <summary>
    /// Physics engine integrated method for collisions.
    /// </summary>
    /// <param name="collision">The collision supplied by Unity which has occurred to warrant an call of this function.</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (isAlive) // Only check when the flora is alive.
        {
            if (collision.rigidbody != null) // Make sure the other object has a rigidbody.
            {
                // Calculate damage using the global durability factor plus the physics calculation found in Utility. (Takes into account normals, relative inertia.)
                float damage = Interactable.WorldwideDurability * Utility.CollisionEnergy(thisRigidbody, collision.relativeVelocity, collision.contacts[0].normal, (collision.rigidbody.mass / thisRigidbody.mass));
                    
                // Apply blunt force damage.
                BluntDamage(damage);
            }
            
        }
    }

    /// <summary>
    /// Flora reacts differently than base objects to blunt trauma. (Dislodging fruits.) Therefore this is an override of the virtual method.
    /// </summary>
    /// <param name="damage">The amount of damage done.</param>
    public override void BluntDamage(float damage)
    {
        if ((damage >= 0.7f) && (DislodgeRoutine == null)) DislodgeRoutine = StartCoroutine(DislodgeRandom()); // Dislodge a random fruit.
    }

    /// <summary>
    /// Innate recovery over time.
    /// </summary>
    private IEnumerator RegenerateHealth()
    {
        while (isAlive) // Only heal while alive. (Avoids zombie trees.)
        {
            if (integrity < maxHitpoints) // Integrity != hitpoints. Integrity is structural. Creatures have both integrity (for overkill-gibbing) and hitpoints (for stuff like lacerations.) Flora only have integrity.
            {
                integrity += regenAmount; // Flora heals by regenAmount every second.
                if (integrity > regenAmount) integrity = maxHitpoints; // Cap the healing to the max value.
            }
            yield return GameManager.Wait1; // Wait a second before continuing loop.
        }
        yield break;
    }


    /// <summary>
    /// Drops a random (ripe) fruit.
    /// </summary>
    private IEnumerator DislodgeRandom()
    {
        if (attachedFruit.Count > 0) // There must be a fruit to dislodge...
        {
            int fruitIndex = Random.Range(0, attachedFruit.Count); // Select a random index from the list of fruit attached to flora.

            if (attachedFruit[fruitIndex].isRipe) // Only ripe fruit fall.
            {
                attachedFruit[fruitIndex].isHanging = false; // This fruit is now considered 'falling'.
                attachedFruit[fruitIndex].thisRigidbody.isKinematic = false; // Physically unlocks the fruit so it falls.
                attachedFruit.Remove(attachedFruit[fruitIndex]); // This fruit is now not part of the attachedFruit list.
                yield return GameManager.Wait1; // Wait just a second...
                if (attachedFruit.Count < maxFruits) NewFruit(fruitIndex, 0); // Spawn a new fruit of maxSize 0.
            }
        }
        yield return GameManager.Wait05; // Make sure there's a little bit of buffer time before nullifying the routine. (Avoids single-frame errors.)
        DislodgeRoutine = null; // Nullify routine.
        yield break; // Explicit end.

    }

    /// <summary>
    /// Create (or recycle) new fruit of given type.
    /// </summary>
    /// <param name="spawnIndex">The spawn index.</param>
    /// <param name="maxSize">The maximum size this fruit can be upon starting growing. This is so that if all the fruit is created on load, some fruits will be partially grown already.</param>
    private Fruit NewFruit(int spawnIndex, float maxSize)
    {
        // Declare fruit script.
        Fruit f;
        // Create or recycle a fruit object of the selected type.
        GameObject g = Pool.GetObject(growThis);

        // Assign fruit script on the gameObject.
        f = g.GetComponent<Fruit>();

        // Make fruit appear from size 0 to the maxSize parameter.
        f.growTimer = Random.Range(0, maxSize);

        // No fruit spawns ripe.
        f.isRipe = false;

        // Begin ripening the fruit.
        f.StartCoroutine(f.Ripen());
        f.thisTransform.localScale = Vector3.one * f.growTimer; // Initialize the size of the fruit.
        f.thisTransform.SetParent(thisTransform); // Set the fruit's parent to be this flora.
        f.thisTransform.localPosition = fruitSpawns[spawnIndex].localPosition; // Find the position for the fruit to spawn.
        f.thisRigidbody.isKinematic = true; // Freeze the fruit in place.
        attachedFruit.Add(f); // Add to the array of fruit.

        return f; // Return a reference, in case needed.
    }
}
