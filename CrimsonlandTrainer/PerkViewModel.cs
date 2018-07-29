using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CrimsonlandTrainer.Game;

namespace CrimsonlandTrainer
{
    public class PerkViewModel : INotifyPropertyChanged
    {
        public Perk Perk { get; }
        public bool IsNext { get; set; }
        public string Error { get; set; }
        public Boolean HasError => !String.IsNullOrWhiteSpace(Error);

        public string PerkAsString => Perk.GetDescription().Name;

        public PerkViewModel(Perk perk) {
            Perk = perk;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
