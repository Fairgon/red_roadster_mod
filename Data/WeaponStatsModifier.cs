using System.Collections.Generic;
using UnityEngine;

namespace RedRoadster.Data
{
    public class WeaponStatsModifier
    {
        public float dmgBonus;
        public List<TempDmgBonus> tempDmgBonus;
        public float criticalChanceBonus;
        public float criticalDamageBonus;
        [Tooltip("0.1 == 10% armor pen")]
        public float armorPenBonus;
        public bool massKiller;
        public int rangeBonus;
        public float rangeBonusPerc;
        [Tooltip("Close targets receive damage modified by this, no change when max range.")]
        public float proximityDmgMod;
        [Tooltip("Adds AOE to any projectile weapon.")]
        public float aoeBonus;
        public int projectileSpeedBonus;
        public float projectileSpeedPerc;
        [Tooltip("Should it automatically get a new target when missing one? Missiles only")]
        public bool autoTargeting;
        public int heatCapBonus;
        public float heatCapMod;
        public float weaponHeatMod;
        public float chargeTime;
        public float chargedFireTime;
        public float chargedFireCooldown;
        public float fluxDamageMod = 1f;
        public bool spinalMounted;
        [Tooltip("Chance to give explosive weapons AOE bonus equal to 'explodeBoost'")]
        public int explodeBoostChance;
        [Tooltip("float. Example: 0.2 == 20% bonus")]
        public float explodeBoost;
        [Tooltip("float. Example: 0.2 == +20% size")]
        public float sizeMod;
        public float weaponChargedBaseDamageBoost = 1f;
    }
}