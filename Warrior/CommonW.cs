//
//
//

using Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReBot.API;
using ReBot;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace ReBot{

	public abstract class CommonW : CombatRotation{
	
		// <summary> 
		// Should it use Execute
		// </summary>
		[JsonProperty("Use Execute")]
        public bool UseExecute { get; set; }
		
		// <summary>
		// Which Shout Should it use
		// </summary>
		[JsonProperty("Shout Buff"), JsonConverter(typeof(StringEnumConverter))]							
		public ShoutTyp ShoutOpt = ShoutTyp.CommandingShout;
		
		// <summary>
		// Should it use charge and Heroic Leap
		// </summary>
		[JsonProperty("Use Charge and Heroic Leap")]
        public bool UseChargeLeap  = true;
		
		// <summary>
		// Does it have the t16 bonus
		// </summary>
		[JsonProperty("T16 4pc Bonus")]
		public bool T16Bonus4pc  = false;
		
		// <summary>
		// Should it use Intimidating Shout
		// </summary>
		[JsonProperty("Intimidating Shout")]
        public bool UseIntimShout  = false;
		
		// <summary>
		// Should it use Hamstring
		// </summary>
		[JsonProperty("Hamstring")]
        public bool UseHamstring  = true;
		
		// <summary>
		// What percentage should the bot be below to switch to Defensive Stance
		// </summary>
		[JsonProperty("Defensive Stance Percentage")]
		public int DefenseStance = 50;
		
		// <summary>
		// What percentage should the bot be above to switch to Battle Stance
		// </summary>
		[JsonProperty("Battle Stance Percentage")]
		public int BattleStance = 75;
		
		// <summary>
		// Should the bot save Berserker Rage
		// </summary>
		[JsonProperty("Save Berserker Rage for fear")]
		public bool EnrageFear= false;
		
	public enum ShoutTyp
	{
		NoShout ,
		CommandingShout ,
		BattleShout ,
	}
		
		public bool CrowdControl(UnitObject add) //checks spells to see if CC
			{
				
				if (add.CastingSpellId == 5782) return true; //Fear
				if (add.CastingSpellId == 10326) return true; //Turn Evil
				if (add.CastingSpellId == 33786) return true; //Cyclone
				if (add.CastingSpellId == 118) return true; //polymorph
				if (add.CastingSpellId == 61305) return true; // polymorph black Cat
				if (add.CastingSpellId == 61721) return true; // polymorph rabbit
				if (add.CastingSpellId == 28271) return true; // polymorph Turtle
				if (add.CastingSpellId == 28272) return true; // polymorph Pig
				if (add.CastingSpellId == 20066) return true; //repentance
				if (add.CastingSpellId == 605) return true; //Dominate Mind
				if (add.CastingSpellId == 51514) return true; //Hex
				
				return false;
			
			}
		
		public bool ifAddInSpellRangeCastingCC(){
			// loops through all the adds you're currently fighting
			foreach(UnitObject add in Adds){
				// if the add is in range (x yards) 40 yards.
				if(Vector3.Distance(Me.Position, add.Position) <= 40){

					/* If you want to check if the add is casting anything */

					if(CrowdControl(add)) {
						return true;
					}
						
				}
				
			}
			if (CrowdControl(Target)){
				return true;
			}
			return false;
		} 
		
		
		public bool doOutOfCombat(){

            if (CastSelf("Battle Shout", () =>  ShoutOpt == ShoutTyp.BattleShout &&!HasAura("Battle Shout") && !HasAura("Horn of Winter") && !HasAura("Trueshot Aura"))) return true;
			if (CastSelf("Commanding Shout", () => ShoutOpt == ShoutTyp.CommandingShout && !HasAura("Commanding Shout") && !HasAura("Blood Pact") && !HasAura("Power Word: Fortitude") && !HasAura("Qiraji Fortitude") && !HasAura("Savage Vigor") && !HasAura("Sturdiness") && !HasAura("Invigorating Roar"))) return true;
			return false;
		
		
		}
	
	
	}


}