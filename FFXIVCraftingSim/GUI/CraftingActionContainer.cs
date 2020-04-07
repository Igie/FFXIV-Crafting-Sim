using FFXIVCraftingSim.Actions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FFXIVCraftingSim.GUI
{
    public class CraftingActionContainer : IDisposable, INotifyPropertyChanged
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

        private CraftingSim Sim { get; set; }

        public BitmapSource Source { get; set; }
        public CraftingAction Action { get; set; }

        public int ProgressIncreased { get; set; }
        public int QualityIncreased { get; set; }

        public int CPCost
        {
            get
            {
                if (Action == null)
                    return 0;
                return Action.GetCPCost(Sim);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };


        public int ProgressIncrease
        {
            get
            {
                return Sim.GetProgressIncrease(Action.GetEfficiency(Sim));
            }
        }

        public int QualityIncrease
        {
            get
            {
                return Sim.GetQualityIncrease(Action.GetEfficiency(Sim));
            }
        }

        private PropertyChangedEventArgs ProgressIncreaseArgs, QualityIncreaseArgs;

        public CraftingActionContainer(BitmapSource source, CraftingSim sim, CraftingAction action)
        {
            Source = source;
            Sim = sim;
            Action = action;

            ProgressIncreaseArgs = new PropertyChangedEventArgs("ProgressIncrease");
            QualityIncreaseArgs = new PropertyChangedEventArgs("QualityIncrease");
        }

        public void Dispose()
        {
            Source = null;
            Action = null;
            ProgressIncreased = 0;
            QualityIncreased = 0;
        }

        public void Update()
        {
            PropertyChanged(this, ProgressIncreaseArgs);
            PropertyChanged(this, QualityIncreaseArgs);
        }
    }
}
