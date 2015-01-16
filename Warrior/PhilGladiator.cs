using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReBot.API;
using Geometry;

namespace ReBot
{
	[Rotation("PhilGladiator", "Phil", "2.0.0.2", WoWClass.Warrior, Specialization.WarriorProtection)]
    public class PhilGladiator : CommonW
	{		
		private bool inArena = false;
		public PhilGladiator()
		{
			GroupBuffs = new[]
			{
				"Battle Shout"
			};

			PullSpells = new[]
			{
				"Heroic Throw", "Charge"
			};
		    UseExecute = true;
		}
	
		int LeapTimer = 0;

		public override bool OutOfCombat()
		{
			CastSelf("Gladiator Stance", () => !IsInShapeshiftForm("Gladiator Stance"));	
			if(!inArena && API.MapInfo.Type == MapType.Arena)
			{
				PlayerObject[] players = SetArenaTargets();
				if(players.Length > 1)
				{
					inArena = true;
				}
				DebugWrite("Set Arena Targets");
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
				inArena = true;
				DebugWrite("Set Arena Targets");
			}
			
			//Heal + Survival
			Cast("Last Stand", () => Me.HealthFraction <= 0.2);
			if (CastSelf("Rallying Cry", () => Me.HealthFraction <= 0.25)) return;
			if (CastSelf("Enraged Regeneration", () => Me.HealthFraction <= 0.5)) return;
			if (CastSelf("Impending Victory", () => Me.HealthFraction <= 0.5)) return;
			if (Cast("Victory Rush", () => Me.HealthFraction < 0.9 && HasAura("Victorious"))) return;
			if (Cast("Impending Victory", () => Me.HealthFraction < 0.9 && HasAura("Victorious"))) return;
			if (CastSelf("Demoralizing Shout", () => Me.HealthFraction < 0.6)) return;
			if (CastSelf("Berserker Rage", () => !Me.CanParticipateInCombat && EnrageFear)) return; //Added to Rage out of fear,sap,incapacitate
			if (CastSelf("Shield Wall", () => Me.HealthFraction <= 0.4));
			if (CastSelf("Mass Spell Reflection", () => Target.IsCasting && ifAddInSpellRangeCastingCC() )) return; //Cast Mass spell reflect for CC spells
			if (Cast("Shattering Throw", () => Target.HasAura("Divine Shield") || Target.HasAura("Blessing of Protection"))) return; //Break the Bubble
			
			if (CastSelf("Berserker Rage", () => !HasAura("Enrage") && !EnrageFear)) return;
			
			//In melee Range
			if (Target.IsInCombatRangeAndLoS)
			{
				
				// interrupt casting or reflect
				if(Target.IsPlayer)
				{
					InterruptTime();
				}
				if (Cast("Hamstring", () => UseHamstring && (Target.IsPlayer || Target.IsFleeing) &&!Target.HasAura("Hamstring") && Me.GetPower(WoWPowerType.Rage) >= 30)) return;
				if (Cast("Shield Block", () => Me.GetPower(WoWPowerType.Rage) >= 20 && !HasAura("Shield Charge")));								
				if (CastSelf("Shield Barrier", () => Me.HealthFraction >= .4 && Me.GetPower(WoWPowerType.Rage) >= 30 && !HasAura("Shield Barrier")));

				// CD OFFGCD
				if (Cast("Heroic Strike", () => (HasAura("Shield Charge") || (HasAura("Unyielding Strikes") && Me.GetPower(WoWPowerType.Rage) >= (50-(AuraStackCount("Unyielding Strikes")*5)))) && Target.HealthFraction > 0.2)) return;
				if (Cast("Heroic Strike", () => HasAura("Ultimatum") || Me.GetPower(WoWPowerType.Rage) >= 100 || AuraStackCount("Unyielding Strikes") > 4)) return;

				
				//Talents
				if (Cast("Avatar")) return;
				if (Cast("Storm Bolt")) return;			
				// if (Cast("Shockwave")) return; Taking this out here
				
				int addsInRange = Adds.Count(x => x.DistanceSquared <= 8.74 * 8.74);
				
				//Rota AoE
				if (addsInRange > 1) // Moved number up to 1
				{
					if(Target.HealthFraction <= BurstPercentage /100f)
					{
						Burst();
					}
					if (Cast("Intimidating Shout",  () => UseIntimShout &&(addsInRange > 2 && Me.HealthFraction <= 0.25))) return;
					if (Cast("Shockwave")) return; //Added this here to get multiple target stuns
					if (Cast("Revenge")) return;
					if (Cast("Shield Slam")) return;
					if (Cast("Bloodbath")) return;
					if (Cast("Dragon Roar")) return;
					if (Cast("Thunder Clap", () => (addsInRange > 2))) return;
					if (Cast("Bladestorm", () => ((addsInRange > 0) && (!HasAura("Bladestorm"))))) return;
					if (Cast("Thunder Clap", () => (addsInRange > 4))) return;	
					if (Cast("Devastate")) return;
				}
				//Rota DPS
				else
				{
					if(Target.HealthFraction <= BurstPercentage /100f)
					{
						Burst();
					}
					if (Cast("Devastate", () => AuraStackCount("Unyielding Strikes")>0 && AuraStackCount("Unyielding Strikes")< 6 && AuraTimeRemaining("Unyielding Strikes")<1.5)) return;
					if (Cast("Shield Slam")) return;
					if (Cast("Revenge")) return;
					if (Cast("Execute", () => HasAura("Sudden Death"))) return;	
					if (Cast("Bloodbath")) return;					
					if (Cast("Dragon Roar")) return;
					if (Cast("Execute", () => UseExecute && Me.GetPower(WoWPowerType.Rage) > 60 && Target.HealthFraction < 0.2)) return;
					if (Cast("Devastate")) return;
				}
			}
			else
			{	
				//Not in melee range
				LeapTimer -= 1;
				if (Cast("Charge", () => UseChargeLeap && LeapTimer == 0 )) return;
				if (CastOnTerrain("Heroic Leap", Target.Position, () => UseChargeLeap)) { 
				LeapTimer = 5;
				}  return;
				if (!CastSelf("Spell Reflection", () => Target.IsCasting && Target.CombatRange <= 40 && Target.Target == Me))
				if (CastSelf("Mass Spell Reflection", () => Target.IsCasting || ifAddInSpellRangeCastingCC() )) return;
				if (Cast("Heroic Throw")) return;
			}				
		
		}
	}
}