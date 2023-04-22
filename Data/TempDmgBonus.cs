using UnityEngine;

namespace ShipMaker.Data
{
    public class TempDmgBonus
    {
        [Tooltip("0 == None (all), 1 == Energy, 2 == Cannon, 3 == Vulcan, 4 == Missile, 5 == MiningLaser, 7 == Plasma, 8 == Mine, 9 == Torpedo, 10 == pulse, 11 == railgun, 12 == repair")]
        public int type;
        public float bonus;
    }
}