using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReBot.API;
using Geometry;

namespace ReBot
{
    [Rotation("PhilArms", "Phil", "2.0.0.3", WoWClass.Warrior, Specialization.WarriorArms)]
    public class PhilArms : CommonW
 	{
		private bool inArena = false;
		public PhilArms()
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
			if(inArena && API.MapInfo.Type == MapType.Arena) {
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
			if(!inArena && API.MapInfo.Type == MapType.Arena)
			{
				PlayerObject[] players = SetArenaTargets();
				if(players.Length > 1)
				{
					inArena = true;
				}
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
				if(Target.IsPlayer)
				{
					InterruptTime();
				}
				if (Cast("Hamstring", () => UseHamstring && (Target.IsPlayer || Target.IsFleeing) &&!Target.HasAura("Hamstring") && Me.GetPower(WoWPowerType.Rage) >= 30)) return;
				

				CastSelf("Die by the Sword", () => Me.HealthFraction < 0.6);


				int addsInRange = Adds.Count(x => x.DistanceSquared <= 8.74 * 8.74);
				//Rota AoE
				if (addsInRange > 1)
				{
					if(Target.HealthFraction <= BurstPercentage /100f)
					{
						Burst();
					}
					if (Cast("Intimidating Shout",  () => UseIntimShout &&  Me.HealthFraction <= 0.25)) return;
					if (CastOnTerrain("Ravager", Target.Position)) return;
					if (CastSelf("Sweeping Strikes", () => !HasAura("Sweeping Strikes"))) return;
					if (CastOnTerrain("Ravager", Target.Position)) return;
					if (Cast("Bladestorm", () => ((addsInRange > 0) && (!HasAura("Bladestorm"))))) return;
					if (Cast("Rend", () => !HasAura("Rend"))) return;
					if (Cast("Whirlwind", () => Me.GetPower(WoWPowerType.Rage) >= 60, "too much rage")) return;
					
					
				}
				//Rota DPS
				else
				{
					if(Target.HealthFraction <= BurstPercentage /100f && Target.IsPlayer)
					{
						Burst();
					}
					if (Cast("Hamstring", () => !Target.HasAura("Hamstring"))) return;
					if (Cast("Rend", () => !Target.HasAura("Rend"))) return;
					if (Cast("Mortal Strike", () => !Target.HasAura("Mortal Wounds"))) return;
					if (Cast("Execute", () => HasAura("Sudden Death"))) return;	
					if (Cast("Execute", () => UseExecute && Me.GetPower(WoWPowerType.Rage) > 40 && Target.HealthFraction < 0.2)) return;
					if (Cast("Colossus Smash", () =>  Me.GetPower(WoWPowerType.Rage) > 60))
					if (Cast("Mortal Strike")) return;				
					if (Cast("Dragon Roar",() =>  Me.GetPower(WoWPowerType.Rage) < 70)) return;
					if (Cast("Siegebreaker")) return;
					if (Cast("Whirlwind", () => Me.GetPower(WoWPowerType.Rage) >= 60, "too much rage")) return;
	
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