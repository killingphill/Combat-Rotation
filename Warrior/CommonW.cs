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
		// Debugg
		// </summary>
		[JsonProperty("DeveloperDebugMode")]
        public bool developerDebugMode = false;
		
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
		
		public bool SpellReflect()
		{
			if(HasAura("Spell Reflect") || HasAura("Mass Spell Reflection"))
			{
				return true;
			}
			return false;
		}
		
		private bool ReturnFromCharge()
		{
			if(SpellCooldown("Charge") == 0 && SpellCooldown("Heroic Leap") == 0 && SpellCooldown("Pummel") == 0) 
			{
				return true;
			}
			return false;
		}
		
		public void InterruptTime()
		{
			if (Cast("Pummel", () => Target.IsCastingAndInterruptible() && SpellReflect() == false && Target.RemainingCastTime < 1000)) return;
			if (Cast("Intimidating Shout", () => Target.IsCasting && SpellReflect() == false)) return; //Added to fear Casters
			if (Cast("Storm Bolt", () => Target.IsCasting && SpellReflect() == false)) return; //Added to stun Casters
			if (Cast("Shockwave", () => Target.IsCasting && SpellReflect() == false)) return; //Added to stun Casters
			if (CastSelf("Spell Reflection", () => Target.IsCasting && Target.CombatRange <= 40 && Target.Target == Me && ifAddInSpellRangeCastingCC() )) return;
			if (CastSelf("Mass Spell Reflection", () => Target.IsCasting && ifAddInSpellRangeCastingCC() )) return; //Cast Mass spell reflect for CC spells
			if (Cast("Pummel", () => Me.Focus.IsCastingAndInterruptible() && Me.Focus.IsInCombatRangeAndLoS && SpellReflect() == false, Me.Focus)) return;
			if (Cast("Storm Bolt", () => Me.Focus.IsCastingAndInterruptible() && SpellReflect() == false, Me.Focus)) return;
			if (Cast("Charge", () => Me.Focus.IsCastingAndInterruptible() && SpellReflect() == false && ReturnFromCharge() == true, Me.Focus)) return;
		}
		
		public PlayerObject[] SetArenaTargets() {
			var players = API.Players.Where(u => u.IsEnemy).ToArray();
			DebugWrite("Inside of SetArenaTarget");
			HealerFocus(players);
			return players;
		}
		
		public void HealerFocus(PlayerObject[] players) 
		{
			DebugWrite("Inside of HealerFocus");
			for(int i =0; i < players.Length; i++)
			{
				if(players[i].IsHealer){
					if (Me.Focus == null) {
						Me.SetFocus(players[i]);
						DebugWrite("Set Healer As Focus");
					}
				}
			} 
			if (Me.Focus == null)
			{
				if (Target == players[0])
				{
					Me.SetFocus(players[1]);
				}else{
					Me.SetFocus(players[0]);
				}
			}
		
		}
		
		
		
		public bool doOutOfCombat(){

            if (CastSelf("Battle Shout", () =>  ShoutOpt == ShoutTyp.BattleShout &&!HasAura("Battle Shout") && !HasAura("Horn of Winter") && !HasAura("Trueshot Aura"))) return true;
			if (CastSelf("Commanding Shout", () => ShoutOpt == ShoutTyp.CommandingShout && !HasAura("Commanding Shout") && !HasAura("Blood Pact") && !HasAura("Power Word: Fortitude") && !HasAura("Qiraji Fortitude") && !HasAura("Savage Vigor") && !HasAura("Sturdiness") && !HasAura("Invigorating Roar"))) return true;
			return false;
		
		
		}
	
		public void DebugWrite(string s) {if (developerDebugMode){API.Print(s);}}
		public void DebugWriteBypass(string s) {API.Print(s);}
	
	}


}