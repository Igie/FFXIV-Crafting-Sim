using SaintCoinach;
using SaintCoinach.Xiv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV_Crafting_Sim
{
    public static class G
    {
        public static ARealmReversed GameData { get; private set; }

        public static void Init()
        {
            GameData = new ARealmReversed(@"C:\Program Files (x86)\SquareEnix\FINAL FANTASY XIV - A Realm Reborn", SaintCoinach.Ex.Language.English);
            
        }


    }
}
