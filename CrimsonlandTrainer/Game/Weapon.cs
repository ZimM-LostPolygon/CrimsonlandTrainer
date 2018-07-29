using System;
using System.ComponentModel;
using System.Reflection;

namespace CrimsonlandTrainer.Game {
    [TypeConverter(typeof(WeaponTypeConverter))]
    public enum Weapon {
        [WeaponDescription("No Weapon")]
        NoWeapon = 0,

        [WeaponDescription("Pistol")]
        Pistol = 1,

        [WeaponDescription("Assault Rifle")]
        AssaultRifle = 2,

        [WeaponDescription("Shotgun")]
        Shotgun = 3,

        [WeaponDescription("Sawed-off Shotgun")]
        SawedOffShotgun = 4,

        [WeaponDescription("Submachine Gun")]
        SubmachineGun = 5,

        [WeaponDescription("Gauss Gun")]
        GaussGun = 6,

        [WeaponDescription("Mean Minigun")]
        MeanMinigun = 7,

        [WeaponDescription("Flamethrower")]
        Flamethrower = 8,

        [WeaponDescription("Plasma Rifle")]
        PlasmaRifle = 9,

        [WeaponDescription("Multi-Plasma")]
        MultiPlasma = 10,

        [WeaponDescription("Plasma Minigun")]
        PlasmaMinigun = 11,

        [WeaponDescription("Rocket Launcher")]
        RocketLauncher = 12,

        [WeaponDescription("Seeker Rockets")]
        SeekerRockets = 13,

        [WeaponDescription("Plasma Shotgun")]
        PlasmaShotgun = 14,

        [WeaponDescription("Blow Torch")]
        BlowTorch = 15,

        [WeaponDescription("HR Flamer")]
        HrFlamer = 16,

        [WeaponDescription("Mini-Rocket Swarmers")]
        MiniRocketSwarmers = 17,

        [WeaponDescription("Rocket Minigun")]
        RocketMinigun = 18,

        [WeaponDescription("Pulse Gun")]
        PulseGun = 19,

        [WeaponDescription("Jackhammer")]
        Jackhammer = 20,

        [WeaponDescription("Ion Rifle")]
        IonRifle = 21,

        [WeaponDescription("Ion Minigun")]
        IonMinigun = 22,

        [WeaponDescription("Ion Cannon")]
        IonCannon = 23,

        [WeaponDescription("Gauss Shotgun")]
        GaussShotgun = 24,

        [WeaponDescription("Ion Shotgun")]
        IonShotgun = 25,

        [WeaponDescription("Plasma Cannon")]
        PlasmaCannon = 26,

        [WeaponDescription("3-Way Ion Rifle")]
        MultiIon = 27,

        [WeaponDescription("Gauss Minigun")]
        GaussMinigun = 28,

        [WeaponDescription("Blade Gun")]
        BladeGun = 29,

        [WeaponDescription("Splitter Gun")]
        SplitterGun = 30,

        [WeaponDescription("Shrinkifier 5k")]
        Shrinkifier_5K = 31,

        [WeaponDescription("Bubblegun", true)]
        Bubblegun = 32,

        [WeaponDescription("Unknown", true)]
        Unknown = 63,

        [WeaponDescription("Spider Plasma", true)]
        SpiderPlasma = 44,

        [WeaponDescription("Fire bullets", true)]
        FireBullets = 45,

        [WeaponDescription("Plasma Overload", true)]
        PlasmaOverload = 46,

        [WeaponDescription("Monster Plasma", true)]
        MonsterPlasma = 47,

        [WeaponDescription("Mega Laser", true)]
        MegaLaser = 48
    }

    public static class WeaponExtensions {
        public static WeaponDescriptionAttribute GetDescription(this Weapon weapon) {
            return
                typeof(Weapon)
                    .GetField(weapon.ToString())
                    .GetCustomAttribute<WeaponDescriptionAttribute>();
        }
    }

    public class WeaponDescriptionAttribute : Attribute {
        public string Name { get; }
        public bool IsInternal { get; }

        public WeaponDescriptionAttribute(string name, bool isInternal = false) {
            Name = name;
            IsInternal = isInternal;
        }
    }
}
