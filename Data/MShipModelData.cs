using System.Collections.Generic;
using UnityEngine;


namespace ShipMaker.Data
{
    [System.Serializable]
    public class MShipModelData
    {
        public int id;
        public string shipModelName;
        public TFaction manufacturer;
        public ShipClassLevel shipClass = ShipClassLevel.Yacht;
        public ShipRole shipRole;
        public List<string> modelBonus;
        public int sellChance = 100;
        public int level = 5;
        public int hullPoints = 100;

        public int weaponSpace = 3;
        public int equipSpace = 15;
        public int cargoSpace = 25;
        public int passengers;

        public int hangarDroneSpace;
        public int hangarShipSpace;

        public CrewSeat[] crewSpace;

        public int speed = 10;
        public int agility = 10;
        public int mass = 70;

        public int sortPower = 1;
        public int rarity = 1;

        public float drawScale = 20f;
        public float sizeScale = 1f;

        public ReputationRequisite repReq;

        public TFaction[] factions;

        public List<CraftMaterial> craftingMaterials;
        public Vector3 extraSurFXScale = Vector3.one;
    }
}