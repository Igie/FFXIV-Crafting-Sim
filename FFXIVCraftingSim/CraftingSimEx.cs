
using FFXIVCraftingSimLib;
using FFXIVCraftingSimLib.Actions.Buffs;
using FFXIVCraftingSimLib.Types;
using FFXIVCraftingSimLib.Types.GameData;
using System.ComponentModel;
using System.Diagnostics;

namespace FFXIVCraftingSim
{
    public class CraftingSimEx : CraftingSim, INotifyPropertyChanged
    {
        public new const int MaxActions = 40;

        public override int Level
        {
            get
            {
                return level;
            }
            set
            {
                if (level == value) return;
                ActualLevel = Utils.GetPlayerLevel(value);
                level = value;
                if (CurrentRecipe != null)
                    LevelDifference = Utils.GetCraftingLevelDifference(ActualLevel - CurrentRecipe.Level);
                ExecuteActions();
                PropertyChanged(this, new PropertyChangedEventArgs("Level"));
            }
        }


        public override int BaseCraftsmanship
        {
            get { return _BaseCraftsmanship; }
            set
            {
                if (_BaseCraftsmanship == value) return;
                _BaseCraftsmanship = value;
                ExecuteActions();
                PropertyChanged(this, new PropertyChangedEventArgs("BaseCraftsmanship"));
            }
        }

        public override int BaseControl
        {
            get { return _BaseControl; }
            set
            {
                if (_BaseControl == value) return;
                _BaseControl = value;
                PropertyChanged(this, new PropertyChangedEventArgs("BaseControl"));
            }
        }

        public override int BaseMaxCP
        {
            get { return _BaseMaxCP; }
            set
            {
                if (_BaseMaxCP == value) return;
                _BaseMaxCP = value;

                PropertyChanged(this, new PropertyChangedEventArgs("BaseMaxCP"));
                PropertyChanged(this, new PropertyChangedEventArgs("MaxCP"));
                ExecuteActions();
            }
        }

        private int _CraftsmanshipBuff { get; set; }
        public override int CraftsmanshipBuff
        {
            get { return _CraftsmanshipBuff; }
            set
            {
                if (_CraftsmanshipBuff == value) return;
                _CraftsmanshipBuff = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CraftsmanshipBuff"));
                PropertyChanged(this, new PropertyChangedEventArgs("Craftsmanship"));
            }
        }

        private int _ControlBuff { get; set; }
        public override int ControlBuff
        {
            get { return _ControlBuff; }
            set
            {
                if (_ControlBuff == value) return;
                _ControlBuff = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ControlBuff"));
                PropertyChanged(this, new PropertyChangedEventArgs("Control"));
            }
        }

        private int _MaxCPBuff { get; set; }
        public override int MaxCPBuff
        {
            get { return _MaxCPBuff;  }
            set
            {
                if (_MaxCPBuff == value) return;
                _MaxCPBuff = value;
                PropertyChanged(this, new PropertyChangedEventArgs("MaxCPBuff"));
                PropertyChanged(this, new PropertyChangedEventArgs("MaxCP"));
            }
        }

        private int _CurrentDurability { get; set; }
        public override int CurrentDurability
        {
            get { return _CurrentDurability; }
            set
            {
                _CurrentDurability = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentDurability"));
            }
        }

        private int _CurrentProgress { get; set; }
        public override int CurrentProgress
        {
            get { return _CurrentProgress;  }
            set
            {
                //if (_CurrentProgress == value) return;
                _CurrentProgress = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentProgress"));
            }
        }

        private int _CurrentQuality { get; set; }
        public override int CurrentQuality
        {
            get { return _CurrentQuality; }
            set
            {
                //if (_CurrentQuality == value) return;
                _CurrentQuality = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentQuality"));
            }
        }

        public int _CurrentCP { get; set; }
        public override int CurrentCP
        {
            get
            { return _CurrentCP; }

            set
            {
                if (_CurrentCP == value) return;
                _CurrentCP = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentCP"));
            }
        }



        public override RecipeInfo CurrentRecipe
        {
            get { return _CurrentRecipe;  }
            protected set
            {
                if (_CurrentRecipe == value) return;
                _CurrentRecipe = value;
                ExecuteActions();
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentRecipe"));
                
            }
        }
        
        public override int CraftingActionsLength
        {
            get
            {
                return _CraftingActionsLength;
            }
            protected set
            {
                _CraftingActionsLength = value;
                if (value > MaxActions)
                    Debugger.Break();
                PropertyChanged(this, new PropertyChangedEventArgs("CraftingActionsLength"));
                PropertyChanged(this, new PropertyChangedEventArgs("CraftingActionsTime"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public CraftingSimEx()
            : base() { }

        public new CraftingSimEx Clone(bool copyActions = false)
        {
            CraftingSimEx result = new CraftingSimEx();
            result.SetRecipe(CurrentRecipe);
            CopyTo(result, copyActions);
            return result;
        }

        public virtual void CopyTo(CraftingSimEx sim, bool copyActions = false)
        {
            sim.Level = Level;
            sim.BaseCraftsmanship = BaseCraftsmanship;
            sim.BaseControl = BaseControl;
            sim.BaseMaxCP = BaseMaxCP;
            sim.CraftsmanshipBuff = CraftsmanshipBuff;
            sim.ControlBuff = ControlBuff;
            sim.MaxCPBuff = MaxCPBuff;
            sim.StepSettings = new CraftingSimStepSettings[MaxActions];

            for (int i = 0; i < MaxActions; i++)
                sim.StepSettings[i] = StepSettings[i].Clone();
            ;
            if (copyActions)
            {
                sim.RemoveActions();
                sim.AddActions(false, GetCraftingActions());

                sim.Step = Step;
                sim.CurrentDurability = CurrentDurability;
                sim.CurrentProgress = CurrentProgress;
                sim.CurrentQuality = CurrentQuality;
                sim.CurrentCP = CurrentCP;

                sim.InnerQuietBuff = InnerQuietBuff?.Clone() as InnerQuietBuff;
                if (sim.InnerQuietBuff != null) sim.CraftingBuffs.Add(sim.InnerQuietBuff);

                sim.WasteNotBuff = WasteNotBuff?.Clone() as WasteNotBuff;
                if (sim.WasteNotBuff != null) sim.CraftingBuffs.Add(sim.WasteNotBuff);

                sim.VenerationBuff = VenerationBuff?.Clone() as VenerationBuff;
                if (sim.VenerationBuff != null) sim.CraftingBuffs.Add(sim.VenerationBuff);

                sim.GreatStridesBuff = GreatStridesBuff?.Clone() as GreatStridesBuff;
                if (sim.GreatStridesBuff != null) sim.CraftingBuffs.Add(sim.GreatStridesBuff);

                sim.InnovationBuff = InnovationBuff?.Clone() as InnovationBuff;
                if (sim.InnovationBuff != null) sim.CraftingBuffs.Add(sim.InnovationBuff);

                sim.MuscleMemoryBuff = MuscleMemoryBuff?.Clone() as MuscleMemoryBuff;
                if (sim.MuscleMemoryBuff != null) sim.CraftingBuffs.Add(sim.MuscleMemoryBuff);

                sim.ManipulationBuff = ManipulationBuff?.Clone() as ManipulationBuff;
                if (sim.ManipulationBuff != null) sim.CraftingBuffs.Add(sim.ManipulationBuff);

                sim.ObserveBuff = ObserveBuff?.Clone() as ObserveBuff;
                if (sim.ObserveBuff != null) sim.CraftingBuffs.Add(sim.ObserveBuff);

                sim.NameOfTheElementsBuff = NameOfTheElementsBuff?.Clone() as NameOfTheElementsBuff;

                sim.NameOfTheElementsUsed = NameOfTheElementsUsed;
            }
        }
    }
}
