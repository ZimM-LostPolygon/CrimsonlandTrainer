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
    public class WeaponViewModel : INotifyPropertyChanged
    {
        public Weapon Weapon{ get; }
        public bool IsNext { get; set; }

        public string WeaponAsString => Weapon.GetDescription().Name;

        public WeaponViewModel(Weapon weapon) {
            Weapon = weapon;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
