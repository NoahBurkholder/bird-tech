/// Bird by Example helper script which outlines the different qualities which Interactable.cs (and child classes) can evoke.
/// Noah James Burkholder 2020

using UnityEngine;

/// <summary>
/// Evocation qualities used within Interactables.
/// </summary>
public class Qualities : MonoBehaviour
{
    /// <summary>
    /// Different qualities an object can evoke to birds' perceptron inputs.
    /// These afford the transferral of behaviour and skills between objects with similar evocations.
    /// </summary>
    public enum Quality
    {
        Death = 0,
        Suffering = 1,
        Famine = 2,
        Deceit = 3,
        Divine = 4,
        Curiosity = 5,
        Cowardice = 6,
        Company = 7,
        Power = 8,
        Wealth = 9,
        Magic = 10,
        Combat = 11,
        Development = 12,
        Confinement = 13,
        Gathering = 14,
        Nature = 15,
        Crafts = 16,
        Destiny = 17,
        Legacy = 18,
        Unknowable = 19
    }

    // Used elsewhere in iterators.
    public static readonly int NUM_QUALITIES = 20;
}
