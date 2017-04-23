using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace AirQuality
{
	public class AQGasResourceExchange : PartModule
	{
		//public Dictionary<string, AQReaction> Reactions;
		public List<AQReaction> Reactions;
		AQsettings InstanceAQSettings;
		[KSPField(isPersistant = true, guiActive = true)]
		public string Description;
		[KSPField(isPersistant = true, guiActive = true)]
		public double LastUpdate;
		public void UpdateAll(Vessel vessel, AQAir Air, double LivingVolume, double ScaleFactor)
		{
			KeyValuePair<string, double> limitingreagent; 
			foreach (AQReaction reaction in Reactions)
			{
				print("[AQ:GRE] Evaluating Reaction " + reaction.Name);
				limitingreagent = reaction.CalculateLimit(ScaleFactor, LivingVolume, Air, vessel);
				if (limitingreagent.Value - 1.0f < float.Epsilon)
				{
					print("[AQ:GRE] reaction " + reaction.Name + " is not limited");
				}
				else
				{
					print("[AQ:GRE] reaction " + reaction.Name + " is limited by reagent " + limitingreagent.Key + " to scale of " + limitingreagent.Value);
				}
				print("[AQ:GRE] reaction " + reaction.Name + " updating resources");
				reaction.UpdateResources(part, ScaleFactor); 
				print("[AQ:GRE] finished updating resources, limiting factor " + limitingreagent.Value);
				print("[AQ:GRE] reaction " + reaction.Name + " updating AQGases");
				reaction.UpdateAir(Air, LivingVolume,ScaleFactor);
				print("[AQ:GRE] Finished simulating " + reaction.Name);
			}
			return;
		}
		public override void OnAwake()
		{
			print("[AQ:GRE] OnAwake");
			if (Reactions == null)
			{
				print("[AQ:GRE] initializing Reactions");
				Reactions = new List<AQReaction>();
			}
			if (InstanceAQSettings == null)
			{
                print("[AQ:GRE] initializing InstanceAQSettings");
				InstanceAQSettings = new AQsettings();
			}
			base.OnAwake();
		}
		public override void OnLoad(ConfigNode node)
		{
			print("[AQ:GRE] OnLoad: " + node);
			AQReaction reactionloader;
			foreach (ConfigNode inode in GameDatabase.Instance.GetConfigNodes(AQNodeNames.Config))
			{
				print("[AQ:GRE] Loading aqsettings from " + inode);
				InstanceAQSettings.Load(inode);
			}
			if (node.HasValue("Descrition"))
			{
				Description = node.GetValue("Description");
			}
			if (node.HasNode("AQReactions"))
			{
				print("[AQ:GRE] Loading reactions for " + Description);
				Reactions = new List<AQReaction>();
				foreach (ConfigNode reactionnode in node.GetNode("AQReactions").GetNodes())
				{
					print("[AQ:GRE] discovered " + node.GetNode("AQReactions").GetNodes().Count() + " reactions");
					reactionloader = new AQReaction();
					print("[AQ:GRE] Loading volatile properties from " + reactionnode);
					reactionloader.Load(reactionnode);
					print("[AQ:GRE] Looking for reaction definitions with the name " + reactionloader.Name);
					foreach (ConfigNode reactiondefinition in GameDatabase.Instance.GetConfigNodes(reactionloader.Name))
					{
						print("[AQ:GRE] Loading invariant properties from " + reactiondefinition);
						reactionloader.LoadInvariant(reactiondefinition);
					}
					Reactions.Add(reactionloader);
					print("[AQ:GRE] Loaded " + Reactions.Last().Name + " with " +
					      Reactions.Last().GasReagents.Count + " gas reagents and " +
					      Reactions.Last().ResourceReagents.Count + " resource reagents");
				}
				print("[AQ:GRE] Finished loading " + Reactions.Count + " reactions");
			}
			base.OnLoad(node);
		}
		public override void OnSave(ConfigNode node)
		{
			ConfigNode inode;
			print("[AQ:GRE] OnSave: " + node);
			if (node.HasValue("Description"))
			{
				node.SetValue("Description", Description);
			}
			else
			{
				node.AddValue("Description", Description);
			}
			if (node.HasNode("AQReactions"))
			{
				print("[AQ:GRE] Saving to existing AQReaction node");
				inode = node.GetNode("AQReactions");
			}
			else
			{
				print("[AQ:GRE] Creating AQReactions node");
				inode = node.AddNode("AQReactions");
			}
			print("[AQ:GRE] Saving " + Reactions.Count + " reactions");
			foreach (AQReaction reaction in Reactions)
			{
				if (!inode.HasNode(reaction.Name))
				{
					print("[AQ:GRE] Creating node for " + reaction.Name);
					inode.AddNode(reaction.Name);
				}
				print("[AQ:GRE] Saving reaction " + reaction.Name + " in status " + reaction.Status);
				reaction.Save(inode.GetNode(reaction.Name));
				//Reactions[reactionname].Save(inode.GetNode(reactionname));
			}
			print("[AQ:GRE] Saved " + inode.GetNodes().Count() + "reactions");
			base.OnSave(node);
		}
		public override void OnUpdate()
		{
			if (Time.timeSinceLevelLoad < 1.0f || !FlightGlobals.ready)
			{
				return;
			}
			if (LastUpdate == 0.0f)         //exact float point comparison is done intentionally to catch the case of uninitialized variable
			{
				// Just started running
				LastUpdate = Planetarium.GetUniversalTime();
				print("[AQ:GRE] Starting simulation, listing reactions");
				foreach (AQReaction reaction in Reactions)
				{
					print("[AQ:GRE] Reaction " + reaction.Name + "has name " + reaction.Name);
					print("[AQ:GRE]           Status:" + reaction.Status);
					print("[AQ:GRE]     AlwaysActive:" + reaction.AlwaysActive);
					print("[AQ:GRE]             Type:" + reaction.Type);
					print("[AQ:GRE] Listing AQGas reagents of " + reaction.Name);
					if (reaction.GasReagents.Count == 0)
					{
						print("[AQ:GRE] Reaction" + reaction.Name + " has no AQGasReagents defined");
					}
					foreach (AQGasReagent gasreagentiterator in reaction.GasReagents)
					{
						print("[AQ:GRE] AQGasReagent " + gasreagentiterator.Name);
						print("[AQ:GRE]               IsLimiting:" + gasreagentiterator.IsLimiting);
						print("[AQ:GRE]               Production:" + gasreagentiterator.Production);
					}
					print("[AQ:GRE] Listing resource reagents of " + reaction.Name);
					if (reaction.GasReagents.Count == 0)
					{
						print("[AQ:GRE] Reaction" + reaction.Name + " has no resource reagents defined");
					}
					foreach (AQResourceReagent resourcereagentiterator in reaction.ResourceReagents)
					{
						print("[AQ:GRE] AQResourceReagent " + resourcereagentiterator.Name);
						print("[AQ:GRE]               IsLimiting:" + resourcereagentiterator.IsLimiting);
						print("[AQ:GRE]               Production:" + resourcereagentiterator.Production);
					}
				}
				return;
			}
			if ((Planetarium.GetUniversalTime() - LastUpdate) > InstanceAQSettings.SimulationStep)
			{
				double ScaleFactor = Math.Min(((Planetarium.GetUniversalTime() - LastUpdate) / InstanceAQSettings.SimulationStep), InstanceAQSettings.MaxScaleFactor);
				LastUpdate+= ScaleFactor* InstanceAQSettings.SimulationStep;
				print("[AQ:GRE] Updating Air of " + part.Modules.OfType<HabitableVolume>().Single().Air.Count + " gases");
				UpdateAll(vessel, part.Modules.OfType<HabitableVolume>().Single().Air, part.Modules.OfType<HabitableVolume>().Single().LivingVolume, ScaleFactor);
			}
			base.OnUpdate();
		}
	}
}
