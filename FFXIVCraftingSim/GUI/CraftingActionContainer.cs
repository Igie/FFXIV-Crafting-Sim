using FFXIVCraftingSim.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FFXIVCraftingSim.GUI
{
    public class CraftingActionContainer : IDisposable
    {
        public string IncreaseString
        {
            get
            {
                if (ProgressIncreased != 0 && QualityIncreased == 0)
                    return ProgressIncreased.ToString();
                if (ProgressIncreased == 0 && QualityIncreased != 0)
                    return QualityIncreased.ToString();
                if (ProgressIncreased != 0 && QualityIncreased != 0)
                    return $"{ProgressIncreased}\r\n{QualityIncreased}";
                return "";
            }
        }
        public BitmapSource Source { get; set; }
        public CraftingAction Action { get; set; }

        public int ProgressIncreased { get; set; }
        public int QualityIncreased { get; set; }

        public CraftingActionContainer(BitmapSource source, CraftingAction action)
        {
            Source = source;
            Action = action;
        }

        public void Dispose()
        {
            Source = null;
            Action = null;
            ProgressIncreased = 0;
            QualityIncreased = 0;
        }
    }
}
