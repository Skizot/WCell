/*************************************************************************
 *
 *   file		: BaseClass.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-04-08 17:02:58 +0800 (Tue, 08 Apr 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 244 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Data;
using WCell.RealmServer.Content;
using WCell.Util.Variables;

namespace WCell.RealmServer.RacesClasses
{
    /// <summary>
    /// Defines the basics of a class.
    /// </summary>
    public abstract class BaseClass
    {

        public static int DefaultStartLevel = 1;

        #region Fields

        /// <summary>
        /// The class type this class represents.
        /// </summary>
        public abstract ClassId Id { get; }

        public int BaseHealth;

        public int BasePower;

        /// <summary>
        /// Basic class settings per level
        /// </summary>
        public ClassLevelSetting[] Settings = new ClassLevelSetting[RealmServerConfiguration.MaxCharacterLevel];

        /// <summary>
        /// All SpellLines of this class
        /// </summary>
        public SpellLine[] SpellLines;

        //internal Archetype Archetype;

        #endregion

        public virtual int StartLevel
        {
            get { return 1; }
        }

        /// <summary>
        /// The PowerType this class uses.
        /// </summary>
        public virtual PowerType PowerType
        {
            get { return PowerType.Mana; }
        }

        #region Methods

        public ClassLevelSetting GetLevelSetting(int level)
        {
            if (level >= Settings.Length)
            {
                level = Settings.Length - 1;
            }
            else if (level < 1)
            {
                level = 1;
            }

            var setting = Settings[level];
            if (setting == null)
            {
                ContentHandler.OnInvalidDBData("{0} has no ClassLevelSetting for level {1}", this, level);
                return new ClassLevelSetting();
            }
            return setting;
        }

        //private static float GetLevelBonus(float bonus, int level)
        //{
        //    float totalbonus;

        //    if (level > 1)
        //        totalbonus = bonus * (float)Math.Pow(1.1, level) - (float)Math.Pow(1.1, level - 1) * bonus;
        //    else
        //        totalbonus = bonus * (float)Math.Pow(1.1, level);

        //    return totalbonus;    
        //}

        /// <summary>
        /// Calculates attack power for the class at a specific level, Strength and Agility.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="strength">the player's Strength</param>
        /// <param name="agility">the player's Agility</param>
        /// <returns>the total attack power</returns>
        public abstract int CalculateMeleeAP(int level, int strength, int agility);

        /// <summary>
        /// Calculates the dodge amount for the class at a specific level and Agility.
        /// TODO: Find the constant that can be used to get 1 agi = X % dodge (diminishing returns)
        /// http://wowwiki.com/Dodge
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="agility">the player's Agility</param>
        /// <returns>the total dodge amount</returns>
        public virtual float CalculateDodge(int level, int agility, int baseAgility, int defenseSkill, int dodgeRating, int defenseRating)
        {
            var dodge = GameTables.BaseDodge[(int)Id] + agility * GameTables.BaseMeleeCritChance[(int)Id-1];
            dodge += GameTables.GetUnModifiedClassMeleeCritChanceValue(level, Id)/GameTables.CritAgiMod[(int) Id];
            return dodge;

            // Diminished dodge (by level) prototype
            /*
            //Not correct because CritAgiMod is scales per level
            var baseDodge = baseAgility / (1f / (GameTables.BaseMeleeCritChance[(int)Id] * GameTables.CritAgiMod[(int)Id - 1])) + (defenseSkill - level * 5) * 0.04f;
            var dodgeFromGearAgi = (agility - baseAgility) / GameTables.BaseMeleeCritChance[(int)Id - 1] * GameTables.CritAgiMod[(int)Id];
            var dodgeFromRating = dodgeRating / GameTables.GetCRTable(CombatRating.Dodge)[level - 1];
            var dodgeFromDef = defenseRating / GameTables.GetCRTable(CombatRating.DefenseSkill)[level - 1] * 0.04f;

            var preDiminishedDodge = dodgeFromDef + dodgeFromGearAgi + dodgeFromRating;
            var diminishedDodge = (preDiminishedDodge * GameTables.StatCap[(int)Id]) /
                                  (preDiminishedDodge +
                                   GameTables.DiminisherConstant[(int)Id] * GameTables.StatCap[(int)Id]);
            dodge += baseDodge + diminishedDodge;
            //return dodge;

            var aAgi = agility - baseAgility;
            */
        }

        /// <summary>
        /// Calculates the parry chance from parry rating.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="parryRating"></param>
        /// <param name="str">Strength, used for DKs</param>
        /// <returns></returns>
        public virtual float CalculateParry(int level, int parryRating, int str)
        {
            return parryRating / GameTables.GetCRTable(CombatRating.Parry)[level-1];
        }

        /// <summary>
        /// Calculates the amount of health regeneration for the class at a specific level and Spirit.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="spirit">the player's Spirit</param>
        /// <returns>the total health regeneration amount</returns>
        public virtual int CalculateHealthRegen(int level, int spirit)
        {
            return (spirit / 12);
        }

        /// <summary>
        /// Calculates the magic critical chance for the class at a specific level, base Intellect and added Intellect.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <returns>the total magic critical chance</returns>
        public virtual float CalculateMagicCritChance(int level, int intellect)
        {

            //if(level == 80)
            //{
            //    return (float) (((intellect / 166.6667) + classConstant) + (critRating/45.91));
            //}
            //return (intellect/80f);
            var critBase = GameTables.BaseSpellCritChance[(int)Id-1];
            var critMod = GameTables.GetClassSpellCritChanceValue(level, Id);

            // Crit from crit rating.
            var critRating = 0;
            return 1f / critBase + intellect * critMod + critRating;
        }

        /// <summary>
        /// Calculates the melee critical chance for the class at a specific level, base Agility and added Agility.
        /// TODO: Figure out BaseMeleeCritChance and implement diminishing returns
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="baseAgi">the player's base Agility</param>
        /// <returns>the total magic critical chance</returns>
        public float CalculateMeleeCritChance(int level, int agility)
        {
            //level = MathUtil.ClampMinMax(level, 1, 100);

            //return
            //    ((GameTables.BaseMeleeCritChance[((int)Id) - 1] +
            //      (GameTables.GetClassMeleeCritChanceValue(level, Id) *
            //       baseAgi)));
            var crit = agility/GameTables.GetClassMeleeCritChanceValue(level, Id);
            var baseCrit = GameTables.BaseMeleeCritChance[(int) Id-1];
            return crit + baseCrit;
        }

        /// <summary>
        /// TODO: Ranged Crit Chance
        /// http://www.wowwiki.com/Formulas:Critical_hit_chance
        /// http://www.wowwiki.com/Formulas:Agility
        /// </summary>
        public float CalculateRangedCritChance(int level, int baseAgi)
        {
            level = MathUtil.ClampMinMax(level, 1, 100);

            return
                ((GameTables.BaseMeleeCritChance[((int)Id)-1] +
                  (GameTables.GetClassMeleeCritChanceValue(level, Id) *
                   baseAgi))) * 100f;
        }

        /// <summary>
        /// Calculates the amount of power regeneration for the class at a specific level, Intellect and Spirit.
        /// Changed in 3.1, overrides for casters are redundant.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="spirit">the player's Spirit</param>
        /// <returns>The total power regeneration amount per RegenTick. Scales with RegenTickDelay (mana/5s will stay the same even if you change TickDelay).</returns>
        public virtual int CalculatePowerRegen(Character chr)
        {
			// default mana generation
            //return (10f + (spirit / 7f));
			var regen = (0.001 + Math.Pow(chr.Intellect, 0.5) * chr.Spirit * GameTables.BaseRegen[chr.Level]) * Unit.RegenTickSeconds;
			if (chr.IsInCombat)
			{
				regen = (regen * chr.ManaRegenPerTickInterruptedPct) / 100;
			}
			return (int)regen;
        }

        /// <summary>
        /// Calculates ranged attack power for the class at a specific level, Strength and Agility.
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <param name="strength">the player's Strength</param>
        /// <param name="agility">the player's Agility</param>
		/// <returns>the total ranged attack power</returns>
		public virtual int CalculateRangedAP(int level, int strength, int agility)
		{
			return agility - 10;
		}

        /// <summary>
        /// Gets the total health gained for the class at a specific level. 
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <returns>the total health gained up until the given level</returns>
        public int GetHealthForLevel(int level)
        {
            return GetLevelSetting(level).Health;
        }

        /// <summary>
        /// Gets the total power gained for the class at a specific level. 
        /// </summary>
        /// <param name="level">the player's level</param>
        /// <returns>the total power gained up until the given level</returns>
        public virtual int GetPowerForLevel(int level)
        {
            return GetLevelSetting(level).Mana;
        }

        /// <summary>
        /// Runs any needed initialization for a player that has just been created.
        /// </summary>
        /// <param name="character">the <see cref="Character">Character</see> that needs to be initialized</param>
        public virtual void InitializeStartingPlayer(Character character)
        {
        }

        #endregion

        public override string ToString()
        {
            return Id.ToString();
        }

        public void FinalizeAfterLoad()
        {
            ArchetypeMgr.BaseClasses[(uint)Id] = this;
        }
    }
}
