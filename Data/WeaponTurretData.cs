using UnityEngine;

namespace ShipMaker.Data
{
    [System.Serializable]
    public class WeaponTurretData
    {
        public string turretName;
        public sbyte turretIndex = -1;
        public WeaponTurretType type;
        public float degreesLimit = 10f;
        public float turnSpeed = 20f;
        public string spriteName = "";
        public bool spinalMount;
        [Tooltip("0 == unlimited")]
        public float totalSpace;
        [Tooltip("0 == unlimited")]
        public int maxInstalledWeapons;

        [Tooltip("1 == Player ship, 2 == AI ship")]
        public int turretMode = 1;

        public bool manned;
        [Tooltip("If true shows the star on the turret")]
        public bool hasSpecialStats;

        public WeaponStatsModifier baseWeaponMods;

        public string[] extraGuns;

        //[Tooltip("Alternate extra fire, when available?")]
        public bool alternateFire;
        public Vector2 iconTranslate;

        public string extraGunsParent;
        public Transform extraGunsParentTransform;
        public Transform[] extraGunsTransforms;
    }
}