using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrimsonlandTrainer.Game;
using CrimsonlandTrainer.Properties;

namespace CrimsonlandTrainer
{
    public class BuildModel {
        public IList<Weapon> Weapons { get; set; } = new List<Weapon>();
        public IList<Perk> Perks { get; set; } = new List<Perk>();
        public BuildSettingsModel Settings { get; set; } = new BuildSettingsModel();

        public class BuildSettingsModel {
            public bool ReplaceFireBulletsWith500Points { get; set; }
            public bool ReplacePlasmaOverloadWith500Points { get; set; }
        }
    }
}
