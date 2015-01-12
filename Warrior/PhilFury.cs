using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReBot.API;
using Geometry;

namespace ReBot
{
    [Rotation("PhilFury", "Phil", WoWClass.Warrior, Specialization.WarriorFury)]
    public class PhilFury : CommonW
 	{
		private bool inArena = false;
		public PhilFury()
		{
			GroupBuffs = new[]
			{
				"Battle Shout"
			};

			PullSpells = new[]
			{
				"Heroic Throw", 
			};
		    UseExecute = true;
		}
		int LeapTimer = 0;

		public override bool OutOfCombat()
		{
			
			CastSelf("Battle Stance", () => !IsInShapeshiftForm("Battle Stance"));
			if(inArena) {
				inArena = false;
			}
			if(doOutOfCombat()) 
			{
				return true;
			}
			return false;
		}

		public override void Combat()
		{
			if(!inArena)
			{
				PlayerObject[] players = SetArenaTargets();
				inArena = true;
				DebugWrite("Set Arena Targets");
			}
			
			//Heal
			if (CastSelf("Rallying Cry", () => Me.HealthFraction <= 0.35 && !HasAura("Die by the Sword"))) return;
			if (CastSelf("Enraged Regeneration", () => Me.HealthFraction <= 0.8)) return;
			if (CastSelf("Impending Victory", () => Me.HealthFraction <= 0.8)) return;
			if (CastSelf("Berserker Rage", () => !Me.CanParticipateInCombat && EnrageFear)) return; //Added to Rage out of fear,sap,incapacitate
			if (Cast("Victory Rush", () => Me.HealthFraction < 0.9 && HasAura("Victorious"))) return;
			if (Cast("Impending Victory", () => Me.HealthFraction < 0.9 && HasAura("Victorious"))) return;
			if (CastSelf("Mass Spell Reflection", () => Target.IsCasting && ifAddInSpellRangeCastingCC() )) return; //Cast Mass spell reflect for CC spells
			if (Cast("Shattering Throw", () => Target.HasAura("Divine Shield") || Target.HasAura("Blessing of Protection") || Target.HasAura("Ice Block"))) return; //Break the Bubble
			if (CastSelf("Defensive Stance", () => Me.HealthFraction <= (DefenseStance / 100) && !IsInShapeshiftForm("Defensive Stance"))) return; //Defensive stance
			if (CastSelf("Battle Stance", () => Me.HealthFraction >= (BattleStance /100) && !IsInShapeshiftForm("Battle Stance"))) return;
			if (CastSelf("Shield Barrier", () => Me.HealthFraction >= .4 && IsInShapeshiftForm("Defensive Stance") && Me.GetPower(WoWPowerType.Rage) >= 30 && !HasAura("Shield Barrier"))) return;
			if (CastSelf("Berserker Rage", () => !HasAura("Enrage") && !EnrageFear)) return;


			//Near/In melee Range
			if (Target.IsInCombatRangeAndLoS)
			{
				if (T16Bonus4pc) {
					if (Cast("Execute", () => Me.HasAura("Death Sentence"))) return;
				}
			
			
				// interrupt casting or reflect
				InterruptTime();
				if (Cast("Hamstring", () => UseHamstring && (Target.IsPlayer || Target.IsFleeing) &&!Target.HasAura("Hamstring") && Me.GetPower(WoWPowerType.Rage) >= 30)) return;
				

				CastSelf("Die by the Sword", () => Me.HealthFraction < 0.6);


				int addsInRange = Adds.Count(x => x.DistanceSquared <= 8.74 * 8.74);
				//Rota AoE
				if (addsInRange > 1)
				{
					if (Cast("Intimidating Shout",  () => UseIntimShout &&  Me.HealthFraction <= 0.25)) return;
					if (CastOnTerrain("Ravager", Target.Position)) return;
					if (Cast("Bloodthirst", () => !HasAura("Enrage"))) return;
					if (Cast("Bladestorm", () => ((addsInRange > 0) && (!HasAura("Bladestorm"))))) return;
					if (Cast("Whirlwind", () => Me.GetPower(WoWPowerType.Rage) >= 20, "meat Cleaver")) return;
					if (Cast("Raging Blow", () => HasAura("Raging Blow!"))) return;	
					if (Cast("Recklessness")) return;
					if (Cast("Bloodbath")) return;
					if (Cast("Dragon Roar")) return;
					if (Cast("Bloodthirst", () => Me.GetPower(WoWPowerType.Rage) <= 20 )) return;
					if (Cast("Whirlwind", () => Me.GetPower(WoWPowerType.Rage) >= 60, "too much rage")) return;
					
				}
				//Rota DPS
				else
				{
					
				if (Cast("Execute", () => UseExecute && Me.GetPower(WoWPowerType.Rage) > 40 && Target.HealthFraction < 0.2)) return;
				if (Cast("Wild Strike", () => HasAura("Bloodsurge"))) return;
				if (Cast("Execute", () => HasAura("Sudden Death"))) return;
				if (Cast("Raging Blow", () => HasAura("Raging Blow!"))) return;	
				if (Cast("Bloodthirst", () => !HasAura("Enrage") || Me.GetPower(WoWPowerType.Rage) <= 80)) return;
				if (Cast("Recklessness")) return;
				if (Cast("Bloodbath")) return;					
				if (CastOnTerrain("Ravager", Target.Position)) return;
				if (Cast("Dragon Roar")) return;
				if (Cast("Siegebreaker")) return;				
				if (Cast("Wild Strike", () => Me.GetPower(WoWPowerType.Rage) >= 90, "too much rage")) return;
				if (Cast("Bloodthirst")) return;
	
				}
 				
			}
			else
			{
				LeapTimer -= 1;
				if (Cast("Charge", () => UseChargeLeap && LeapTimer == 0)) return;
				if (CastOnTerrain("Heroic Leap", Target.Position, () => UseChargeLeap)) { 
				LeapTimer = 5;
				}  return;
				if (!CastSelf("Spell Reflection", () => Target.IsCasting && Target.CombatRange <= 40 && Target.Target == Me))
				if (CastSelf("Mass Spell Reflection", () => Target.IsCasting)) return;
				if (Cast("Heroic Throw")) return;
			}
		}
	}
}