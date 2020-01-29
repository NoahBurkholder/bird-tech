/// Bird by Example bodily functions script.
/// 
/// Noah James Burkholder 2020

using System.Collections;
using UnityEngine;

/// <summary>
/// The type of death, for post-death behaviour purposes, and perhaps analytics in the future.
/// </summary>
public enum DeathType
{
    Crushed, // Blunt trauma.
    Exploded, // Violent ballistics.
    Stabbed, // Arrows, spears, low-calibre bullets.
    Slashed, // Swords, knives, axes.
    Fell, // Similar to crushed, but administered from excessive falling.
    Starved, // When stomach reaches 0.
    Sickness, // Future possibility. Radiation falls into this category.
    Bled, // Blood loss from slashes, stabs.
    Electrocuted, // From exposed wires.
    Incinerated // Hot hot burny.

}

/// <summary>
/// The body of a bird. Manages health, RPG stats, and visuals.
/// </summary>
[System.Serializable]
public class BirdBody
{
    // Who does this body belong to?
    public Bird parentBird;

    // Control bools.
    public bool isAlive = true; // Important.
    public bool isAirborn = false;
    public bool isUnconscious;

    // TODO: Neuroplasticity goes down as birds age?
    // TODO: Weights start from zero? Children get constant additive bonus to neuroplasticity to introduce weight?
    // TODO: Birds under 16 are smaller in size?

    // Bodily stats.
    public float age = 20f; // Age. Default is 20.
    public float vitality = 100f; // Your maximum health, or hunger.
    public float metabolism = 4f; // Rate of regeneration and hunger.
    public float bleeding = 0; // Rate of bleeding.
    public float poise = 40f; // Max damage before knocked down.
    public float control = 1f; // Amount of PID control.
    public float health = 100f; // Health out of maximum Vitality.
    public float stomach = 100f; // Hunger out of maximum Vitality.

    // Helper caches.
    private float prevHealth = 100f; // Health out of maximum Vitality.
    private float prevStomach = 100f; // Health out of maximum Vitality.

    // RPG stats.
    public float strength = 10f; // Damage multiplier, and max inventory space.
    public float intelligence = 10f; // Related to number of neurons/layers?
    public float agility = 10f; // Movement speed?
    public float dexterity = 10f; // Used for trades, skills and crafts?
    public float charisma = 10f; // Bonus to social encounters?

    // Visuals.
    public int featherMat;
    public int beakMat;
    public int eyeMat;
    public int beakType;
    public int pupilType;
    public float eyeScale;

    // If a bird is knocked out, keep track of the timeline with this coroutine.
    public IEnumerator knockoutRoutine;

    public BirdBody(Bird parentBird)
    {
        this.parentBird = parentBird;

        isAlive = true; // Important.

        // Assign random attributes.
        age = Random.Range(16f, 60f);
        strength = Random.Range(5f, 20f);
        intelligence = Random.Range(5f, 20f);
        agility = Random.Range(5f, 20f);
        dexterity = Random.Range(5f, 20f);
        charisma = Random.Range(5f, 20f);
        vitality = Random.Range(50f, 150f);

        // Assign calculated attributes.
        poise = (vitality * 0.2f) + (strength * 2);
        metabolism = (vitality * 0.004f) + Random.Range(-0.08f, 0.08f); // 0.20 < Vitality < 0.60, + Random +/- 0.08f. Result is between 0.12 and 0.76;

        // Initialize attributes to full capacity.
        health = vitality;
        stomach = vitality;
        prevHealth = health;
        prevStomach = stomach;

        // Initial test of consciousness.
        control = CalculateControl();

        parentBird.StartCoroutine(BodilyFunctions());
    }

    /// <summary>
    /// Slow bodily functions, such as metabolism and hunger. Updates once per second.
    /// </summary>
    /// <returns></returns>
    public IEnumerator BodilyFunctions()
    {
        yield return GameManager.Wait1;

        while (isAlive) // Stop calculating values upon death.
        {
            BleedFromWounds(); // Tick bleed effects.
            Metabolism(); // Tick metabolism.

            if (!parentBird.testRagdoll) // Debug exception for testing ragdoll.
            {
                if (!isUnconscious) // If conscious...
                {
                    control = CalculateControl(); // update the amount of control the bird has.
                    SetStrength(control); // And set the PID values.
                }
            }
            yield return GameManager.Wait1;
        }
        yield break;

    }

    /// <summary>
    /// Tick bleeding forward by one frame.
    /// </summary>
    private void BleedFromWounds()
    {
        // Begin bleed code.
        if (bleeding > 0)
        {
            if (parentBird.isPlayer) UIManager.ShowNotification(UIManager.Notification.Blood);

            health -= bleeding;
            if (health < 0)
            {
                // Assume bird has bled to death.
                parentBird.CascadeDeath(DeathType.Stabbed);
            }

            // Heal bleeding.
            bleeding -= metabolism * 0.1f;
            if (bleeding < 0) bleeding = 0;
        }
    }

    /// <summary>
    /// Tick metabolism forward by one frame.
    /// </summary>
    private void Metabolism()
    {
        // Assuming the stomach is not empty, and the bird is hurt...
        if (stomach > 0)
        {
            if (health <= vitality) {
                // Begin metabolism of stomach energy into health...

                // Subtract from stomach by metabolism rate of this particular bird.
                stomach -= metabolism;

                // Add health based on metabolism. Multiply metabolism by factor that gives less returns on a fuller stomach.
                health += metabolism * (1.5f - Mathf.Pow((stomach / vitality), 2f));

                /// Elaboration on the factor: 
                /// 
                /// Being near starvation gives you nearly ~150% efficiency.
                /// Being full or over-full gives you <= 50% efficiency
                /// Being >= ~122.47% fullness has you take DAMAGE.
                /// 
                ///              Graphical Explanation:
                ///      +------------------------------------+        
                ///   1.5|_________                           |
                ///      |         \_____                     |
                ///      |               \___                 |
                ///      |                   \__            __|
                ///      |                      \_       __/  |
                /// H 1.0|-                       \   __/     |
                /// E    |                         \_/        |
                /// A    |                      __/ \         |
                /// L    |                   __/     \_       |
                /// I    |                __/          \      |
                /// N 0.5|-            __/              \     |
                /// G    |          __/                  \    |
                ///      |       __/                     |    |
                ///      |    __/                         \   |
                ///      | __/                             \  |
                ///     0+/--------------|-------------|----\-+
                ///      0.0            0.5           1.0    \ Now taking damage...
                ///                   FULLNESS
                        
                // Now that the metabolism is handled...

                // If this bird is the player...
                if (parentBird.isPlayer) {

                    // Begin a check to see if the player is hungry or starving.
                    if (stomach < vitality / 10) // 10% stomach.
                    {
                        UIManager.ShowNotification(UIManager.Notification.Consume); // Flash UI text repeatedly.

                    }
                    else if (stomach < vitality / 2) // 50% stomach.
                    {
                        // Make sure this only happens once.
                        if (prevStomach > vitality / 2)
                        {
                            UIManager.ShowNotification(UIManager.Notification.Hunger); // Flash UI text once.
                        }
                    }

                    // Begin a check to see if the player is hurt or dying.
                    if (health < vitality / 10) // 10% health.
                    {
                        UIManager.ShowNotification(UIManager.Notification.Death); // Flash UI text repeatedly.
                    }
                    else if (health < vitality / 2) // 50% health.
                    {
                        // Make sure this only happens once.
                        if (prevHealth > vitality / 2)
                        {
                            UIManager.ShowNotification(UIManager.Notification.Mortal); // Flash UI text once.
                        }
                    }
                }
            }
            

            // Health cannot be over maximum, unlike the stomach.
            if (health > vitality) health = vitality;
        }
        else // If stomach is empty...
        {
            // Kill the bird through starvation.
            parentBird.CascadeDeath(DeathType.Starved);
        }

        // Replace helper caches with new values for next cycle.
        prevHealth = health;
        prevStomach = stomach;
    }

    /// <summary>
    /// Slash this bird. TODO: Consolidate damage types using Enum.
    /// </summary>
    public void ReceiveSlash(float damage)
    {
        if (isAlive)
        {
            // 1. Deliver damage.
            health -= damage;

            // 2. Check for death.
            if (health < 0)
            {
                parentBird.CascadeDeath(DeathType.Slashed);
            }

            // 3. Play sound.
            parentBird.PlayVoice(VocalizationType.Pain);

            // 4. TODO: Begin reaction animation.

            // 5. TODO: Bump ReceivedViolence perceptron.
        }
    }

    /// <summary>
    /// For checking if a ragdoll has become relatively unmoving. TODO: Needs tweaking.
    /// </summary>
    public bool MostlyAsleep()
    {
        float s = 0;
        for (int i = 0; i < parentBird.pids.Length; i++)
        {
            if (parentBird.pids[i].thisRigidbody.velocity.magnitude < 0.05f)
            {
                s++;
            }
        }
        return (s / parentBird.pids.Length > 0.5);

    }
    /// <summary>
    /// Stab this bird. TODO: Consolidate damage types using Enum.
    /// </summary>
    public void ReceiveStab(float damage, float bleed)
    {
        if (isAlive)
        {
            /// TODO:
            /// 1. Deliver damage.
            health -= damage;
            /// 2. Check for death.
            if (health < 0)
            {
                parentBird.CascadeDeath(DeathType.Stabbed);
            }
            /// 3. Deliver bleeding.
            bleeding += bleed;
            /// 4. Play sound.
            parentBird.PlayVoice(VocalizationType.Pain);

            /// 5. Begin reaction animation.
            /// 6. Bump ReceivedViolence input.
        }
    }

    /// <summary>
    /// Crush this bird. TODO: Consolidate damage types using Enum.
    /// </summary>
    public void ReceiveCrush(float damage)
    {
        if (isAlive)
        {
            /// TODO:
            /// 1. Deliver damage.
            health -= damage;
            /// 2. Check for death or ragdolling.
            if (health < 0)
            {
                parentBird.CascadeDeath(DeathType.Crushed);
            }
            else if (damage > poise)
            {
                float timeUnconscious = ((damage - poise) / health) * 5f;
                if (knockoutRoutine != null)
                {
                    parentBird.StopCoroutine(knockoutRoutine);
                }
                knockoutRoutine = KnockUnconscious(timeUnconscious);
                parentBird.StartCoroutine(knockoutRoutine);
            }
            /// 3. Play sound.
            parentBird.PlayVoice(VocalizationType.Pain);
            /// 4. Begin reaction animation.
            /// 5. Bump ReceivedViolence input.
        }
    }
    /// <summary>
    /// Unpopulated function. TODO: Consolidate with current punching system.
    /// </summary>
    public void Attack()
    {
        /// TODO: 
        /// 1. Begin animation.
        /// 2. Calculate damage with Random.Range(0, WeaponDamage) * Strength;
        /// 3. Deliver damage.
        /// 4. Deplete self hunger levels.
        /// 5. Bump "DidViolence" input.
    }

    /// <summary>
    /// Calculate how capable the bird currently is in piloting its own body.
    /// This is used to smoothly transition between consciousness and unconsciousness.
    /// Also used to weaken the bird as it starves to death.
    /// </summary>
    public float CalculateControl()
    {
        if (isAlive)
        {
            float c = 1f;
            c *= (stomach / vitality) + 0.2f;
            c *= (health / vitality) + 0.2f;
            c *= recovered;
            return Mathf.Clamp01(c);
        } else
        {
            return 0;
        }
                

    }

    // Helper factor (a sort of timer) for recovery.
    private float recovered = 1f;

    /// <summary>
    /// Coroutine which knocks out bird for a given time.
    /// </summary>
    private IEnumerator KnockUnconscious(float time)
    {
        if (!isUnconscious)
        {
            isUnconscious = true; // Allows the bird to stop perceiving and thinking.
            Debug.Log("Knocked unconscious with time of " + time);
            parentBird.PlayVoice(VocalizationType.Pain);
            if (parentBird.isPlayer) UIManager.ShowNotification(UIManager.Notification.Sleep);
            if (time > 5) time = 5f; // For user experience, don't lose control for more than 5 seconds.
            recovered = 0; // Renders physically unconscious.

            if (!parentBird.testRagdoll)
            {
                // Make sure to apply the loss of control. Can be assumed to be 0.
                SetStrength(0);
            }

            // Wait...
            for (float t = 0; t < time; t += 0.1f)
            {
                yield return GameManager.Wait01;
            }

            // Bird starts to become conscious again.
            isUnconscious = false;
            parentBird.SyncGhost();
            parentBird.PlayVoice(VocalizationType.Pain);

            // Begin recovering...
            for (recovered = 0; recovered < 1f; recovered += (0.1f / time))
            {
                if (!parentBird.testRagdoll)
                {
                    // Remember to re-calculate amount of control the bird has.
                    control = CalculateControl();
                    SetStrength(control);
                }
                yield return GameManager.Wait01;
            }


            // Full recovery.
            recovered = 1f;
        }
    }

    /// <summary>
    /// Set the strength of the bird's physics PIDs. 
    /// This makes the bird floppier at lower strengths.
    /// </summary>
    public void SetStrength(float str)
    {
        for (int i = 0; i < parentBird.pids.Length; i++)
        {
            parentBird.pids[i].SetStrength(str, isAirborn);
        }
    }
    /// <summary>
    /// Used for jumping and falling.
    /// </summary>
    public void SetAirborn()
    {
        isAirborn = true; // Toggle helper bool.
        SetStrength(control); // Recalculate the degree of control the bird has over their body.

    }
    public void SetGrounded()
    {
        isAirborn = false; // toggle helper bool.
        SetStrength(control); // Recalculate the degree of control the bird has over their body.
    }

    /// <summary>
    /// Dynamic fall function.
    /// Can knock the bird out if they fall too hard.
    /// </summary>
    public void ReceiveFall(float time)
    {
        if (knockoutRoutine != null)
        {
            parentBird.StopCoroutine(knockoutRoutine);
        }
        knockoutRoutine = KnockUnconscious(time);
        parentBird.StartCoroutine(knockoutRoutine);
    }
}
