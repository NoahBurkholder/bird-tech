/// Bird by Example Bird Identity
/// 
/// Noah James Burkholder 2020 (MIT License)

using UnityEngine;

/// <summary>
/// Used for introducing fluctuations in contextual perception.
/// In other words: If all birds were to share the same neurology, this script makes bird perceptrons interpret the environment differently.
/// This system will be toggleable in the end product, so that experimentation with social cohesion behaviour is afforded to curious players.
/// </summary>
[System.Serializable]
public class BirdIdentity
{
    // Number of archetypes used by spawning scripts as the max value for random archetype selection.
    public static int NumArchetypes = 16;

    // Enumerator for assigning particular archetypes to birds. Each modifies the baseline bird attitudes towards different contextual cues.
    public enum Archetype
    {
        Soldier,
        Gatherer,
        Leader,
        Maniac,
        Doctor,
        Socialite,
        Criminal,
        Ascetic,
        Explorer,
        Sadist,
        Hustler,
        Hunter,
        Glutton,
        Cultist,
        Hero,
        Contrarian
    }

    // The archetype of this bird.
    public Archetype thisArchetype;

    // Function accessed from spawner script to assign a random archetype to a new bird.
    public static Archetype RandomArchetype()
    {
        return (Archetype)UnityEngine.Random.Range(0, NumArchetypes);
    }

    // In case randomization of ideals is necessary in the future.
    public static int NumIdeals = 3;

    /// Enumerator for attitudes about the self vs. attitudes about others and the bird society at large.
    /// This just affords a little more complexity if down the line I want to differentiate how birds feel about (Ex.) "Magic" when magic manifests within the self vs. when it manifests in other birds.
    /// In other words: If a bird wants to be powerful, but doesn't want other birds to be powerful, their attitudes[Power, Self] would be a positive number, and attitudes[Power, Others] would be negative.
    /// I've added Society as a third option, because a thief may like to be dishonest (Self), dislike others who are dishonest (Others) but want society on a whole to be dishonest (Society) so that their thievery goes under the radar.
    public enum Ideal
    {
        Self = 0,
        Others = 1,
        Society = 2
    }

    // This is a bird's specific attitudes. Two dimensional array because they need attitudes for [Quality, Ideal].
    private float[,] attitudes;

    // The default bird's attitudes.
    private static float[,] BaseAttitudes = {
        //  {Self,     Others,   Society}
            {HATES,    AVOIDS,   AVOIDS},   // Death
            {HATES,    DISLIKES, DISLIKES}, // Suffering
            {DISLIKES, AVOIDS,   AVOIDS},   // Famine
            {AVOIDS,   HATES,    HATES},    // Deceit
            {LEANS,    LEANS,    LEANS},    // Divine
            {LIKES,    LEANS,    LEANS},    // Curiosity
            {LEANS,    DISLIKES, DISLIKES}, // Cowardice
            {LIKES,    LIKES,    LIKES},    // Company
            {LEANS,    AVOIDS,   LEANS},    // Power
            {LIKES,    AVOIDS,   LEANS},    // Wealth
            {AVOIDS,   DISLIKES, DISLIKES}, // Magic
            {AVOIDS,   DISLIKES, DISLIKES}, // Combat
            {LEANS,    LIKES,    LOVES},    // Development
            {DISLIKES, AVOIDS,   LIKES},    // Confinement
            {LEANS,    LIKES,    LOVES},    // Gathering
            {LEANS,    LEANS,    AVOIDS},   // Nature
            {LEANS,    LIKES,    LOVES},    // Crafts
            {LOVES,    AVOIDS,   AVOIDS},   // Destiny
            {LOVES,    AVOIDS,   AVOIDS},   // Legacy
            {AVOIDS,   HATES,    HATES}     // Unknowable
    };

    // Macros for tweaking values further down the line
    private readonly static float LOVES = 1f;
    private readonly static float LIKES = 0.66f;
    private readonly static float LEANS = 0.33f;
    private readonly static float NEUTRAL = 0; // Used rarely. Completely nullifies birds' reaction to a contextual quality.
    private readonly static float AVOIDS = -0.33f;
    private readonly static float DISLIKES = -0.66f;
    private readonly static float HATES = -1f;

    // Constructor for spawning script.
    public BirdIdentity()
    {
        // Copy over base attributes.
        attitudes = BaseAttitudes;
    }
    /// <summary>
    /// Returns a numerical value indicating the instanced bird's reactionary attitude to a contextual 'quality' with additional context of internal or external ideals.
    /// </summary>
    public float GetAttitude(Ideal ideal, Qualities.Quality quality)
    {
        return attitudes[(int)quality, (int)ideal];
    }

    /// <summary>
    /// Lengthy function which defines how archetypes vary compared to the base bird.
    /// </summary>
    public BirdIdentity SetPersonality(Archetype a)
    {
        BirdIdentity selectedPersonality = new BirdIdentity();
        thisArchetype = a; // Cached for later lookup.
        switch (a)
        {
            case Archetype.Soldier:
                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Combat] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Combat] = LIKES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Combat] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Cowardice] = HATES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Cowardice] = HATES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Cowardice] = HATES;

                return selectedPersonality;
            case Archetype.Gatherer:
                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Gathering] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Gathering] = LIKES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Gathering] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Wealth] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Wealth] = LIKES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Wealth] = LOVES;

                return selectedPersonality;
            case Archetype.Leader:
                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Development] = LIKES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Development] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Development] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Wealth] = LIKES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Wealth] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Wealth] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Power] = LIKES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Power] = DISLIKES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Power] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Deceit] = HATES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Deceit] = HATES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Deceit] = HATES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Divine] = DISLIKES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Divine] = LIKES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Divine] = LIKES;
                return selectedPersonality;
            case Archetype.Maniac:
                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Power] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Power] = HATES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Power] = HATES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Magic] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Magic] = HATES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Magic] = HATES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Unknowable] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Unknowable] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Unknowable] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Divine] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Divine] = HATES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Divine] = HATES;
                return selectedPersonality;
            case Archetype.Doctor:
                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Death] = HATES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Death] = HATES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Death] = HATES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Suffering] = HATES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Suffering] = HATES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Suffering] = HATES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Cowardice] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Cowardice] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Cowardice] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Divine] = DISLIKES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Divine] = DISLIKES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Divine] = DISLIKES;
                return selectedPersonality;
            case Archetype.Socialite:
                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Company] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Company] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Company] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Development] = LIKES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Development] = LIKES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Development] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Cowardice] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Cowardice] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Cowardice] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Combat] = HATES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Combat] = HATES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Combat] = HATES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Divine] = DISLIKES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Divine] = DISLIKES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Divine] = DISLIKES;
                return selectedPersonality;
            case Archetype.Criminal:
                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Deceit] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Deceit] = AVOIDS;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Deceit] = LIKES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Company] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Company] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Company] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Development] = HATES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Development] = NEUTRAL;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Development] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Divine] = LIKES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Divine] = LIKES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Divine] = LIKES;
                return selectedPersonality;
            case Archetype.Ascetic:
                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Suffering] = NEUTRAL;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Suffering] = NEUTRAL;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Suffering] = NEUTRAL;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Death] = AVOIDS;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Death] = NEUTRAL;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Death] = NEUTRAL;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Unknowable] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Unknowable] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Unknowable] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Curiosity] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Curiosity] = LEANS;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Curiosity] = LEANS;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Magic] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Magic] = LEANS;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Magic] = LEANS;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Crafts] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Crafts] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Crafts] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Divine] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Divine] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Divine] = LOVES;
                return selectedPersonality;
            case Archetype.Explorer:
                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Curiosity] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Curiosity] = AVOIDS;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Curiosity] = LIKES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Wealth] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Wealth] = AVOIDS;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Wealth] = LIKES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Gathering] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Gathering] = AVOIDS;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Gathering] = LIKES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Destiny] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Destiny] = AVOIDS;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Destiny] = AVOIDS;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Nature] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Nature] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Nature] = LOVES;
                return selectedPersonality;
            case Archetype.Sadist:
                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Suffering] = AVOIDS;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Suffering] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Suffering] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Death] = DISLIKES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Death] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Death] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Power] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Power] = DISLIKES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Power] = DISLIKES;

                return selectedPersonality;
            case Archetype.Hustler:
                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Wealth] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Wealth] = AVOIDS;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Wealth] = LIKES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Gathering] = LIKES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Gathering] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Gathering] = LIKES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Crafts] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Crafts] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Crafts] = LOVES;


                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Power] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Power] = AVOIDS;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Power] = LIKES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Destiny] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Destiny] = AVOIDS;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Destiny] = AVOIDS;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Nature] = DISLIKES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Nature] = DISLIKES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Nature] = DISLIKES;
                return selectedPersonality;
            case Archetype.Hunter:
                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Death] = DISLIKES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Death] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Death] = HATES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Suffering] = AVOIDS;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Suffering] = HATES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Suffering] = HATES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Gathering] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Gathering] = AVOIDS;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Gathering] = LIKES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Nature] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Nature] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Nature] = LOVES;
                return selectedPersonality;
            case Archetype.Glutton:
                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Famine] = HATES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Famine] = LIKES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Famine] = LIKES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Wealth] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Wealth] = HATES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Wealth] = HATES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Gathering] = HATES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Gathering] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Gathering] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Nature] = HATES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Nature] = HATES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Nature] = HATES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Confinement] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Confinement] = LEANS;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Confinement] = LEANS;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Development] = HATES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Development] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Development] = LOVES;
                return selectedPersonality;
            case Archetype.Cultist:
                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Unknowable] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Unknowable] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Unknowable] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Divine] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Divine] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Divine] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Destiny] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Destiny] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Destiny] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Death] = NEUTRAL;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Death] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Death] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Suffering] = AVOIDS;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Suffering] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Suffering] = LOVES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Development] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Development] = LOVES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Development] = LOVES;
                return selectedPersonality;
            case Archetype.Hero:
                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Cowardice] = HATES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Cowardice] = NEUTRAL;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Cowardice] = NEUTRAL;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Legacy] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Legacy] = NEUTRAL;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Legacy] = NEUTRAL;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Destiny] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Destiny] = NEUTRAL;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Destiny] = NEUTRAL;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Death] = AVOIDS;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Death] = HATES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Death] = HATES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Suffering] = AVOIDS;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Suffering] = HATES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Suffering] = HATES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Combat] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Combat] = HATES;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Combat] = HATES;

                attitudes[(int)Ideal.Self, (int)Qualities.Quality.Magic] = LOVES;
                attitudes[(int)Ideal.Others, (int)Qualities.Quality.Magic] = NEUTRAL;
                attitudes[(int)Ideal.Society, (int)Qualities.Quality.Magic] = NEUTRAL;

                return selectedPersonality;
            case Archetype.Contrarian:
                // Flips all base values.
                for (int i = 0; i < Qualities.NUM_QUALITIES; i++)
                {
                    for (int j = 0; j < NumIdeals; j++)
                    {
                        attitudes[i, j] *= -1;
                    }
                }
                return selectedPersonality;
        }
        return selectedPersonality;

    }

    // BEGIN NAME GENERATION

    // Coded string. a = vowel, s = single consonant, o = double vowel, d = double consonant.
    public static string[] wordFrame = { "as", "sa", "ad", "asa", "ada", "sas", "sad", "asas", "adas", "asad", "adad", "sasa", "sada", "so", "sos", "aso", "asos" };

    // Arrays containing replacements for the coded wordFrame strings.
    public static string[] vowels = { "e", "a", "o", "i", "u" };
    public static string[] doubleVowels = { "oo", "ee", "iu", "ui", "eu", "ei", "oi", "ia", "au", "ou", "oa", "eo", "io", "aa", "uu" };
    public static string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y", "z" };
    public static string[] doubleConsonants = { "bb", "dd", "ff", "gg", "kk", "ll", "mm", "nn", "pp", "rr", "ss", "tt", "zz",
        "bl", "cl", "dl", "fl", "gl", "kl", "pl", "rl", "sl", "tl", "zl", "lb", "lc", "ld", "lf", "lg", "lk", "lp", "lr", "ls", "lt", "lz",
        "br", "cr", "dr", "fr", "gr", "kr", "pr", "rr", "sr", "tr", "zr", "rb", "rc", "rd", "rf", "rg", "rk", "rp", "rr", "rs", "rt", "rz",
        "ch", "sh", "gh", "th", "wh", "st", "ts", "nt", "gt", "dt", "pt", "rs", "zy", "zw",
        "bs", "cs", "ds", "fs", "gs", "ks", "ps", "rs", "ts", "zs", "sc", "sd", "sf", "sg", "sk", "sp", "st",
        "bz", "cz", "dz", "fz", "gz", "kz", "pz", "rz", "tz"};

    // A few silly nicknames which display after the first name... I.e. Booga 'Two Toes'.
    public static string[] nicknames = {
        "Smasher",              "Beaks",                "Eggy",
        "No-Eggs",              "Grimey",               "Toady",
        "Bird",                 "Froggy",               "Boss",
        "Professor",            "Goopy",                "Goober",
        "Tasty",                "Snot",                 "The Law",
        "The Arm",              "Fishy",                "Birdy",
        "Brainy",               "Angst",                "Pitboss",
        "Baldy",                "Feathers",             "Beaky",
        "Claws",                "Cacaw",                "Sneezy",
        "Cheesy",               "Wheezy",               "Freezie",
        "Doc",                  "Bashful",              "Grumpy",
        "Sleepy",               "Dopey",                "Happy",
        "Breezy",               "On The Rocks",         "Ice Cold",
        "Spitter",              "Catty",                "Undertaker",
        "Bean Boy",             "Bean",                 "Jitters",
        "Thumbs",               "Knuckles",             "Squawky",
        "Screamy",              "Loud",                 "Bad",
        "Two Toes",             "Duck",                 "Lumpy",
        "Worms",                "Grainy",               "Simple",
        "Yellow Belly",         "Rocky"
    };

    // A few silly post-fix titles...
    public static string[] titles = {
        "The Destroyer",        "The Master",           "The Devious",
        "The Toad",             "The Bird",             "The Hawk",
        "The Frog",             "The Sentinel",         "The Boss",
        "The Emboldened",       "The Juicy",            "The Scamp",
        "The Arm",              "The Wing",             "The Claw",
        "The Bad One",          "The Envious",          "The Nasty",
        "The Narc",             "The Godfearing",       "The Youthful",
        "The Old",              "The Wise",             "The Black",
        "The White",            "The Red",              "The Brain",
        "The Teacher",          "The Maw",              "The Gullet",
        "The Stone",            "The Hurricane",        "The Woodwatcher",
        "The Flightless",       "The Wicked",           "The Addicted",
        "The Infamous",         "The Big Boy",          "The Terrible",
        "The Impaler",          "The Goober",           "The Handsome",
        "The Free",             "The Amazing",          "The Wall",
        ", PhD",
        "First of Their Name"
    };

    // A buncha prefixes...
    public static string[] prefixes = {
        "The",                  "Stone Cold",           "The Amazing",
        "The Infamous",         "Big Boy",              "Big Girl",
        "The Devious",          "The Fantastic",        "The Impossible",
        "The Wonderful",        "The Last",             "The One and Only",
        "The Envious",          "Old",                  "Ol'",
        "Wise Ol'",             "Lovely",               "Evil",
        "Godly",                "Salty",                "Skinny",
        "Egg Layin'",           "Big Wing",             "No-Egg",
        "The Whimsical",        "The Incredible",       "The First",
        "Red Hot",              "The Irresistable",     "Big Bone",
        "Lukewarm"
    };

    // Some honorifics for the heck of it...
    public static string[] honorifics = {
        "Mr.",                  "Ms.",                  "Mrs.",
        "Dr.",                  "Mx.",                  "Master",
        "Lord",                 "Sir",                  "Madame",
        "Lady",                 "Mistress",             "Sire",
        "Professor",            "Chancellor",           "Reverend",
        "Little",               "Papa",                 "Mama",
        "Father",               "Mother"
    };

    // Storage for the bird's name components.
    public string namePrefix;
    public string nameHonorific;
    public string nameFirst;
    public string nameNickname;
    public string nameLast;
    public string nameTitle;

    // Control bool for indicating to the generator whether to add spaces at various points of generating to the summed string.
    public bool addSpace;

    /// <summary>
    /// Generate a random name for a bird.
    /// </summary>
    public string RandomName()
    {
        addSpace = false; // Explicit reset to false in case the player wants to rename a bird.

        if (Utility.Chance(0.1f)) namePrefix = RandomPrefix();          // 10% chance to have a prefix.
        if (Utility.Chance(0.1f)) nameHonorific = RandomHonorific();    // 10% chance to have an honorific.
        nameFirst = RandomBlocked();                                    // 100% chance to have a first name.
        //if (Utility.Chance(0.1f)) nameNickname = RandomNickname();    // 10% chance to have a prefix. DISABLED CURRENTLY.
        if (Utility.Chance(0.2f)) nameLast = RandomBlocked();           // 20% chance to have a last name.
        //if (Utility.Chance(0.1f)) nameTitle = RandomTitle();          // 10% chance to have a title. DISABLED CURRENTLY.


        // Once these have been generated, assemble the complete string.
        string s = AssembleName();

        // Send off complete string.
        return s;
    }
    /// <summary>
    /// Assemble the finished name. Writes prefixes, honorifics, first/last names, and postfix titles.
    /// </summary>
    public string AssembleName()
    {
        // Start with empty string.
        string s = "";

        // If prefix...
        if (namePrefix != null) {

            addSpace = true; // Add a space to anything next.
            s += namePrefix; // Append string.
        }

        // If honorific...
        if (nameHonorific != null)
        {

            if (!addSpace) // Since it's possible there isn't a prefix, now we have to check.
            {
                addSpace = true; // Add a space to anything next.
                s += nameHonorific; // Append string.
            }
            else s += " " + nameHonorific;
        }

        // First name. (Everyone has one.)
        if (!addSpace)
        {
            addSpace = true; // Add a space to anything next.
            s += nameFirst; // Append string.
        }
        else s += " " + nameFirst;


        // If last name...
        if (nameLast != null)
        {
            // It can be assumed it needs a space at this point, because everyone has a first name.
            s += " " + nameLast; // Append string.
        }
        // If nickname...
        if (nameNickname != null)
        {
            s += " b" + nameNickname; // Append string.
        }
        // If title...
        if (nameTitle != null)
        {
            s += " a" + nameTitle; // Append string.
        }
        return s;
    }

    /// <summary>
    /// For generating a funky original name using vowels and consonants.
    /// </summary>
    string RandomBlocked()
    {
        // Start with empty string.
        string s = "";

        // Fetch coded word frame.
        string frame = wordFrame[UnityEngine.Random.Range(0, wordFrame.Length)];

        // For every letter in the word frame...
        for (int i = 0; i < frame.Length; i++)
        {
            // Decode to the empty empty string with replacement vowels and consonants.
            if (frame[i].Equals('a'))
            {
                s += vowels[UnityEngine.Random.Range(0, vowels.Length)];
            }
            else if (frame[i].Equals('o'))
            {
                s += doubleVowels[UnityEngine.Random.Range(0, doubleVowels.Length)];
            }
            else if (frame[i].Equals('s'))
            {
                s += consonants[UnityEngine.Random.Range(0, consonants.Length)];
            }
            else
            {
                s += doubleConsonants[UnityEngine.Random.Range(0, doubleConsonants.Length)];
            }
        }

        // Capitalize the first letter.
        s = char.ToUpper(s[0]) + s.Substring(1);

        // Return the full completed name.
        return s;
    }

    /// <summary>
    /// Fetches a random postfix title.
    /// </summary>
    private string RandomTitle()
    {
        float randomTitle = UnityEngine.Random.value;
        randomTitle *= titles.Length;
        return " " + titles[(int)randomTitle]; // Return the postfix title with a preceding space.


    }
    /// <summary>
    /// Fetches a random nickname.
    /// </summary>
    private string RandomNickname ()
    {
        float randomNick = UnityEngine.Random.value;
        randomNick *= nicknames.Length;
        return "\'" + nicknames[(int)randomNick] + "\'"; // Return the nickname within quotes.
    }

    /// <summary>
    /// Fetches a random prefix.
    /// </summary>
    private string RandomPrefix()
    {
        float randomPrefix = UnityEngine.Random.value;
        randomPrefix *= prefixes.Length;
        return prefixes[(int)randomPrefix] + ""; // Return the prefix.
    }

    /// <summary>
    /// Fetches a random honorific.
    /// </summary>
    private string RandomHonorific()
    {
        float randomHonorifics = UnityEngine.Random.value;
        randomHonorifics *= honorifics.Length;
        return honorifics[(int)randomHonorifics] + ""; // Return the honorific.
    }

}