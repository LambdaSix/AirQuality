using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace AirQuality
{
	public class AQReaction : ScriptableObject, IConfigNode, IAQGasExchange
	{
		public string Name;                     //unique
		public string Status;                   //active, inactive, out of blah gas, lacking blah resource, no storage for blahblah, broken, no crew to operate, whatever
		public bool AlwaysActive;               //if false, a user interface should be displayed to toggle
		public string Type;                     //Supported types: Leak, Scrub, backfill //todo refactor supported types into a .cfg split Leak into a separate PartModule
		double LimitingFactor = 1.0f;            //how much a reaction run is scaled down when a limiting resource shortage doesn't allow for full power
		string LimitingReagent;					//which reagent is most lacking to prevent full power run
		public List<AQGasReagent> GasReagents;
		public List<AQResourceReagent> ResourceReagents;
		public void UpdateResources(Part part, double ScaleFactor)
		{
			double actualproduction;
			foreach (AQResourceReagent rreagent in ResourceReagents)
			{
				actualproduction = part.RequestResource(PartResourceLibrary.Instance.GetDefinition(rreagent.Name).id, 
				                                        -ScaleFactor * LimitingFactor * rreagent.Production);
				if (Math.Abs(actualproduction) < Math.Abs(rreagent.Production) * ScaleFactor * LimitingFactor)
				{
					if (rreagent.IsProduct())
					{
						//this means resource storage is overflowing, but we are not going to do anything about it
					}
					else if (rreagent.IsConsumable() && rreagent.IsLimiting)
					{
						//something bad has happened, and we have requested more resource from the vessel than is available
						//normally CalculateLimit should've taken care of this however once we're here we should 
						//wind back the rest of the reaction so that we don't risk overconsuming AQGas'es.
						LimitingReagent = rreagent.Name;
						LimitingFactor = actualproduction / rreagent.Production / ScaleFactor;
					}
				}
			}
			return; 
		}
		public void UpdateAir(AQAir Air, double LivingVolume, double ScaleFactor)
		{
			if (Type == "Leak")     //todo split leak into a separate partmodule maybe
			{                    //Single AQGas reagent is expected, all gases are equally affected, production is interpreted as retention. 
				foreach (AQGasReagent gasreagent in GasReagents)
				{
					foreach (string aqgasname in Air.Keys)
					{
						Air[aqgasname].Pressure *= Math.Pow(gasreagent.Production, ScaleFactor);
					}
				}
			}
			else if (Type == "Scrub" || Type == "Backfill")               //absolute production values in moles
			{
				foreach (AQGasReagent gasreagent in GasReagents)
				{
					Air[gasreagent.Name].Pressure += ScaleFactor * LimitingFactor * gasreagent.Production *
						AQPhysicalConstants.GasConstant * AQPhysicalConstants.StandardAmbientConditions.Temperature / LivingVolume;
				}
			}
			return;
		}
		public KeyValuePair<string,double> CalculateLimit(double ScaleFactor, double Volume, AQAir Air, Vessel vessel)
		{
			KeyValuePair<string, double> defaultkvp = new KeyValuePair<string, double>("", 1.0f);
			Dictionary<string, double> LimitingReagents = new Dictionary<string, double>();
			Status = "Nominal";
			if (Type == "Leak")
			{
				LimitingFactor = 1.0f;
				LimitingReagent = "";
				return defaultkvp;
			}
			else if ((Type == "Scrub") || (Type == "Backfill"))
			{
				LimitingReagents.Add(defaultkvp.Key, defaultkvp.Value);
				foreach (AQGasReagent greagent in GasReagents)
				{
					if (greagent.IsLimiting && greagent.IsConsumable() && 
					    (Air[greagent.Name].Quantity(Volume) < Math.Abs(greagent.Production) * ScaleFactor))
					{
						LimitingReagents.Add(greagent.Name, Air[greagent.Name].Quantity(Volume)/ScaleFactor * Math.Abs(greagent.Production));
					}
				}
				foreach (AQResourceReagent rreagent in ResourceReagents)
				{
					if (rreagent.IsLimiting && rreagent.IsConsumable() && 
					    (AQGetResourceAmount(vessel, rreagent.Name) < Math.Abs(rreagent.Production) * ScaleFactor))
					{
						LimitingReagents.Add(rreagent.Name, AQGetResourceAmount(vessel, rreagent.Name) / ScaleFactor * Math.Abs(rreagent.Production));
					}
				}
			}
			if (LimitingReagents.Min().Value < float.Epsilon)
			{
				Status = "Lacking " + LimitingReagents.Min().Key;
			}
			else if (LimitingReagents.Min().Value < 1.0f)
			{
				Status = "Limited by " + LimitingReagents.Min().Key;
			}
			LimitingFactor = LimitingReagents.Min().Value;
			LimitingReagent = LimitingReagents.Min().Key;
			return LimitingReagents.Min();
		}
		public double AQGetResourceAmount(Vessel vessel, string resourcename)
		{
			double availableresourceamount = 0;
			foreach (Part part in vessel.GetActiveParts())
			{
				foreach (PartResource presource in part.Resources)
				{
					if (presource.flowState && presource.resourceName == resourcename)
					{
						availableresourceamount += presource.amount;
					}
				}
			}
			return availableresourceamount;
		}
		public void Save(ConfigNode node)
		{
			if (node.HasValue("Name"))
			{
				node.SetValue("Name", Name);
			}
			else
			{
				node.AddValue("Name", Name);
			}
			if (node.HasValue("Status"))
			{
				node.SetValue("Status", Status);
			}
			else
			{
				node.AddValue("Status", Status);
			}
			return;
		}
		public void Load(ConfigNode node)
		{
			if (node.HasValue("Name"))
			{
				Name = node.GetValue("Name");
			}
			if (node.HasValue("Status"))
			{
				Status = node.GetValue("Status");
			}
			return;
		}
		public void LoadInvariant(ConfigNode node)
		{
			bool b;
			AQGasReagent AQGasReagentLoader;
			AQResourceReagent AQResourceReagentLoader;
			if (node.HasValue("AlwaysActive") && bool.TryParse(node.GetValue("AlwaysActive"), out b))
			{
				AlwaysActive = b;
			}
			if (node.HasValue("Type"))
			{
				Type = node.GetValue("Type");
			}
			GasReagents = new List<AQGasReagent>();
			foreach (ConfigNode reagentnode in node.GetNodes("AQGasReagent"))
			{
				AQGasReagentLoader = new AQGasReagent();
				AQGasReagentLoader.Load(reagentnode);
				GasReagents.Add(AQGasReagentLoader);                                                //verify if this actually works
			}
			ResourceReagents = new List<AQResourceReagent>();
			foreach (ConfigNode reagentnode in node.GetNodes("AQResourceReagent"))
			{
				AQResourceReagentLoader = new AQResourceReagent();
				AQResourceReagentLoader.Load(reagentnode);
				ResourceReagents.Add(AQResourceReagentLoader);                                            //verify if this actually works
			}
			return;
		}
	}
}