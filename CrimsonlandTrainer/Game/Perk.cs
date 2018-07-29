using System;
using System.ComponentModel;
using System.Reflection;

namespace CrimsonlandTrainer.Game {
    [TypeConverter(typeof(PerkTypeConverter))]
    public enum Perk {
        [PerkDescription("Bloody Mess")]
        BloodyMess = 1,

        [PerkDescription("Sharpshooter")]
        Sharpshooter = 2,

        [PerkDescription("Fastloader")]
        Fastloader = 3,

        [PerkDescription("Lean Mean Exp Machine", multiPickLimit: 3)]
        LeanMeanExpMachine = 4,

        [PerkDescription("Long Distance Runner")]
        LongDistanceRunner = 5,

        [PerkDescription("Out of Thin Air")]
        OutOfThinAir = 6,

        [PerkDescription("Instant Winner", multiPickLimit: -1)]
        InstantWinner = 7,

        [PerkDescription("Grim Deal")]
        GrimDeal = 8,

        [PerkDescription("Plaguebearer")]
        Plaguebearer = 9,

        [PerkDescription("Eagle Eyes")]
        EagleEyes = 10,

        [PerkDescription("Ammo Maniac")]
        AmmoManiac = 11,

        [PerkDescription("Radioactive")]
        Radioactive = 12,

        [PerkDescription("Fastshot")]
        Fastshot = 13,

        [PerkDescription("Fatal Lottery", multiPickLimit: -1)]
        FatalLottery = 14,

        [PerkDescription("Random Weapon", multiPickLimit: -1)]
        RandomWeapon = 15,

        [PerkDescription("Mr. Melee")]
        MrMelee = 16,

        [PerkDescription("Slow Time, High Damage")]
        SlowTimeHighDamage = 17,

        [PerkDescription("Final Revenge")]
        FinalRevenge = 18,

        [PerkDescription("Telekinetic")]
        Telekinetic = 19,

        [PerkDescription("Perk Expert")]
        PerkExpert = 20,

        [PerkDescription("Unstoppable")]
        Unstoppable = 21,

        [PerkDescription("Regression Ammo")]
        RegressionAmmo = 22,

        [PerkDescription("Infernal Contract")]
        InfernalContract = 23,

        [PerkDescription("Poison Projectiles")]
        PoisonProjectiles = 24,

        [PerkDescription("Dodger")]
        Dodger = 25,

        [PerkDescription("Lucky")]
        Lucky = 26,

        [PerkDescription("Uranium Filled Bullets")]
        UraniumFilledBullets = 27,

        [PerkDescription("Doctor")]
        Doctor = 28,

        [PerkDescription("Hot Tempered")]
        HotTempered = 29,

        [PerkDescription("Bonus Economist")]
        BonusEconomist = 30,

        [PerkDescription("Thick Skinned")]
        ThickSkinned = 31,

        [PerkDescription("Barrel Greaser")]
        BarrelGreaser = 32,

        [PerkDescription("Ammunition Within")]
        AmmunitionWithin = 33,

        [PerkDescription("Bad Blood")]
        BadBlood = 34,

        [PerkDescription("Highlander")]
        Highlander = 35,

        [PerkDescription("Regeneration")]
        Regeneration = 36,

        [PerkDescription("Pyromaniac")]
        Pyromaniac = 37,

        [PerkDescription("Ninja", dependencyPerk: Dodger)]
        Ninja = 38,

        [PerkDescription("Cold-blooded", dependencyPerk: BadBlood)]
        ColdBlooded = 39,

        [PerkDescription("Jinxed")]
        Jinxed = 40,

        [PerkDescription("Perk Master", dependencyPerk: PerkExpert)]
        PerkMaster = 41,

        [PerkDescription("Reflex Boosted")]
        ReflexBoosted = 42,

        [PerkDescription("Greater Regeneration", dependencyPerk: Regeneration)]
        GreaterRegeneration = 43,

        [PerkDescription("Breathing Room", multiPickLimit: -1)]
        BreathingRoom = 44,

        [PerkDescription("Death Clock")]
        DeathClock = 45,

        [PerkDescription("My Favourite Weapon")]
        MyFavouriteWeapon = 46,

        [PerkDescription("Bandage", multiPickLimit: -1)]
        Bandage = 47,

        [PerkDescription("Angry Reloader")]
        AngryReloader = 48,

        [PerkDescription("Ion Gun Master")]
        IonGunMaster = 49,

        [PerkDescription("Stationary Reloader")]
        StationaryReloader = 50,

        [PerkDescription("Man Bomb")]
        ManBomb = 51,

        [PerkDescription("Fire Cough")]
        FireCough = 52,

        [PerkDescription("Living Fortress")]
        LivingFortress = 53,

        [PerkDescription("Tough Reloader")]
        ToughReloader = 54,

        [PerkDescription("Lifeline 50-50", multiPickLimit: -1)]
        LifelineFiftyFifty = 55,
    }

    public static class PerkExtensions {
        public static PerkDescriptionAttribute GetDescription(this Perk perk) {
            return
                typeof(Perk)
                .GetField(perk.ToString())
                .GetCustomAttribute<PerkDescriptionAttribute>();
        }
    }

    public class PerkDescriptionAttribute : Attribute {
        public string Name { get; }
        public Perk? DependencyPerk { get; }
        public int MultiPickLimit { get; }

        public PerkDescriptionAttribute(string name, Perk dependencyPerk = 0, int multiPickLimit = 0) {
            Name = name;
            DependencyPerk = dependencyPerk != 0 ? dependencyPerk : (Perk?) null;
            MultiPickLimit = multiPickLimit;
        }
    }
}
