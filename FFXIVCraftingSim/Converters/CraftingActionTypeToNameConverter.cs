using FFXIVCraftingSim.Actions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FFXIVCraftingSim.Converters
{
    [ValueConversion(typeof(CraftingActionType), typeof(String))]
    public class CraftingActionTypeToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is CraftingActionType))
                return null;

            
            CraftingActionType type = (CraftingActionType)value;

            if (type.HasFlag(CraftingActionType.IsBuff))
                return "Buff:";

            if (type.HasFlag(CraftingActionType.IncreasesProgress))
                return "Increases Progress:";

            if (type.HasFlag(CraftingActionType.IncreasesQuality))
                return "Increases Quality:";

            return type.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
