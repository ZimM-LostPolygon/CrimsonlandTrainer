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
    class PerkTypeConverter : EnumConverter
    {
        public PerkTypeConverter(Type type) : base(type) {
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == typeof(string)) {
                PerkDescriptionAttribute description = ((Perk) value).GetDescription();
                string text = description.Name;
                if (description.MultiPickLimit != 0) {
                    text += " (Multipickable)";
                }

                return text;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
