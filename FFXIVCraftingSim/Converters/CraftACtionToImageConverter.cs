using FFXIVCraftingSimLib.Actions;
using FFXIVCraftingSimLib.Types.GameData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FFXIVCraftingSim.Converters
{

    [ValueConversion(typeof(CraftingAction), typeof(BitmapSource))]
    public class CraftActionToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CraftingAction action = value as CraftingAction;
            ClassJobInfo info = ClassJobInfo.CRP;
            if (parameter != null)info = (ClassJobInfo)parameter;
            if (action == null) return null;
            if (G.Actions == null) return null;
            return G.Actions[action.Name].Images[info];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
