using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrimsonlandTrainer.Game;

namespace CrimsonlandTrainer
{
    static class PerkListValidator
    {
        public static (int errorPerkIndex, string errorText)[] Validate(IList<Perk> perks) {
            List<(int errorPerkIndex, string errorText)> errors = new List<(int errorPerkIndex, string errorText)>();

            for (int i = 0; i < perks.Count; i++) {
                Perk perk = perks[i];
                PerkDescriptionAttribute perkDescription = perk.GetDescription();
                if (perkDescription.MultiPickLimit != -1) {
                    int samePerkCount = perks.Count(p => p == perk);
                    if (perkDescription.MultiPickLimit == 0) {
                        if (samePerkCount != 1) {
                            errors.Add((i, "Perk can only be added once"));
                        }
                    } else if (samePerkCount > perkDescription.MultiPickLimit) {
                        errors.Add((i, $"Perk is present more times than allowed"));
                    }
                }

                if (perkDescription.DependencyPerk != null) {
                    int indexOfDependencyPerk = perks.IndexOf(perkDescription.DependencyPerk.Value);
                    if (indexOfDependencyPerk == -1) {
                        errors.Add((i, $"Dependency perk {perkDescription.DependencyPerk} not present"));
                    } else if (indexOfDependencyPerk > i) {
                        errors.Add((i, $"Dependency perk {perkDescription.DependencyPerk} must come earlier"));
                    }
                }
            }

            return errors.ToArray();
        }
    }
}
