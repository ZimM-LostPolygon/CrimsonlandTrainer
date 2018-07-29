using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrimsonlandTrainer.Game;

namespace CrimsonlandTrainer
{
    class WeaponTypeConverter : EnumConverter
    {
        public WeaponTypeConverter(Type type) : base(type) {
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == typeof(string)) {
                WeaponDescriptionAttribute description = ((Weapon) value).GetDescription();
                string text = description.Name;
                if (description.IsInternal) {
                    text += " (Internal)";
                }

                return text;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
