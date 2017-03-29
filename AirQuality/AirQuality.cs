using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace AirQuality
{   /*an interface that mandates a method to update cabin atmosphere, adjusted by scalefactor and cabin living volume   */
	interface IAQGasExchange
	{
		void UpdateAir(Dictionary<string, AQGas> Air, double LivingVolume, double ScaleFactor);
	}
	/* a class containing static variables describing abstract 'standard conditions'
	 *	and some handy physical constants. Standard temperature is assumed
	 *	to be kept constant in the cabin all the time, but standard pressure 
	 *	and molar volume should only be used for initial terrestrial calculations */
	class AQPhysicalConstants
	{
		public static double ArmstrongLimit = 6250.0f;					//in Pa
		public static double GasConstant = 8.3144598f;                  //in J/K/mol
		public struct StandardAmbientConditions
		{
			public static double MolarVolume = .024789598f;             //in cubic metres/mol
			public static double Temperature = 298.15f;                 //in K
			public static double Pressure = 100000.0f;                  //in Pa
		}
		public static Dictionary<string, double> MolarMass =            //in kg/mol
			new Dictionary<string, double>								
		{
			{"CarbonDioxide",0.04401f},									//these are actually quite bad, because they
			{"Oxygen", 0.0319988}										//are terrestrial values and will change on
		};																//other planets. Luckily ISRU is out of scope.
	}
	/*a class contating configuration node names. Apparently they are case-sensitive*/
	class AQNodeNames
	{
		public static string Config = "AQSettings";
		public static string GasLibrary = "AQGasDefinitions";
	}
	/* a class containing mod's global configuration, simulation settings, and starting atmospheric conditions
	 * they are supposed to be loaded from the AQNodeNames.Config configuration node in AQSettings.cfg file, 
	 * but not supposed to be ever saved. */
	public class AQsettings : IConfigNode
	{
		public double SimulationStep;
		public double MaxScaleFactor;
		public Dictionary<string, AQGas> StartingAir;
		private AQGas Gas = new AQGas();
		public Dictionary<string, double> GasProduction;
		public void Load(ConfigNode node)
		{
			float f;
			if (node.HasValue("SimulationStep") && float.TryParse(node.GetValue("SimulationStep"), out f))
			{
				SimulationStep = f;
			}
			if (node.HasValue("MaxScaleFactor") && float.TryParse(node.GetValue("MaxScaleFactor"), out f))
			{
				MaxScaleFactor = f;
			}
			if (node.HasNode("StartingAir"))
			{
				StartingAir = new Dictionary<string, AQGas>();
				foreach (ConfigNode ChildNode in node.GetNode("StartingAir").GetNodes())
				{
					Gas.Load(ChildNode);
					StartingAir.Add(ChildNode.name, Gas);
				}
			}
			if (node.HasNode("GasProduction"))
			{
				GasProduction = new Dictionary<string, double>();
				foreach (ConfigNode ChildNode in node.GetNode("GasProduction").GetNodes())
				{
					if ((ChildNode.HasValue("LongName")) && (ChildNode.HasValue("Production")) && (float.TryParse(ChildNode.GetValue("Production"), out f)))
					{
						GasProduction.Add(ChildNode.GetValue("LongName"),f);
					}
				}
			}
			return;
		}
		public void Save(ConfigNode node)
		{
			return;						//not supposed to save settings in-game, only load from the config
		}
	}
	/* a class describing a gas as a component of cabin atmosphere */
	public class AQGas : IConfigNode
	{
		public string LongName;
		public string ShortName;
		public double Pressure;
		public double MinRequiredPressure;
		public double MaxToleratedPressure;
		public double CondensationPressure;
		public double MolarMass;
		public double Quantity;
		public double NarcoticPotential;
		public bool isBreatheable()
		{
			return (MinRequiredPressure > 0);
		}
		public bool isPoison()
		{
			return (MaxToleratedPressure > 0);
		}
		public void Load(ConfigNode node) //load part-specific data only
		{                                   //see LoadInvariant() to initialise fields with invariant information
			float f;
			if (node.HasValue("LongName"))
			{
				LongName = node.GetValue("LongName");
			}
			if (node.HasValue("Pressure") && float.TryParse(node.GetValue("Pressure"), out f))
			{
				Pressure = f;
			}
			if (node.HasValue("Quantity") && float.TryParse(node.GetValue("Quantity"), out f))
			{
				Quantity = f;
			}
			return;
		}
		public void LoadInvariant(ConfigNode node) //load part-invariant gas properties from global gas definitions
		{					// see Load() to load flight-specific data
			float f;
			if (node.HasValue("ShortName"))
			{
				ShortName = node.GetValue("ShortName");
			}
			if (node.HasValue("MinRequiredPressure") && float.TryParse(node.GetValue("MinRequiredPressure"), out f))
			{
				MinRequiredPressure = f;
			}
			if (node.HasValue("MaxToleratedPressure") && float.TryParse(node.GetValue("MaxToleratedPressure"), out f))
			{
				MaxToleratedPressure = f;
			}
			if (node.HasValue("CondensationPressure") && float.TryParse(node.GetValue("CondensationPressure"), out f))
			{
				CondensationPressure = f;
			}
			if (node.HasValue("MolarMass") && float.TryParse(node.GetValue("MolarMass"), out f))
			{
				MolarMass = f;
			}
			if (node.HasValue("NarcoticPotential") && float.TryParse(node.GetValue("NarcoticPotential"), out f))
			{
				NarcoticPotential = f;
			}
			return;
		}
		public void Save(ConfigNode node)					//only save part-specific data
		{
			if (node.HasValue("LongName"))
			{
				node.SetValue("LongName", LongName);
			}
			else
			{
				node.AddValue("LongName", LongName);
			}
			if (node.HasValue("Pressure"))
			{
				node.SetValue("Pressure", Pressure);
			}
			else
			{
				node.AddValue("Pressure", Pressure);
			}
			return;											
		}
	}
	/* a class describing total atmospheric composition of a habitable volume */
	public class AQAir : Dictionary<string, AQGas>, IConfigNode
	{
		public float Temperature;
		public bool IsBreatheable()
		{
			foreach (string GasEntry in Keys)
			{
				if (this[GasEntry].isPoison() && (this[GasEntry].Pressure > this[GasEntry].MaxToleratedPressure))
				{
					return false;	//poisonous
				}
			}
			foreach (string GasEntry in Keys)
			{
				if (this[GasEntry].isBreatheable() && (this[GasEntry].Pressure > this[GasEntry].MinRequiredPressure))
				{
					return true;	//breatheable and not poisonous
				}
			}
			return false;			//unbreatheable
		}
		public bool IsPressurised()
		{
			double TotalPressure = 0.0f;
			foreach (string GasEntry in Keys)
			{
				TotalPressure += this[GasEntry].Pressure;
			}
			return (TotalPressure > AQPhysicalConstants.ArmstrongLimit);
		}
		public float TotalNarcoticPotential()
		{
			double l_TotalNarcoticPotential = 0.0f;
			foreach (string GasEntry in Keys)
			{
				l_TotalNarcoticPotential += this[GasEntry].NarcoticPotential * this[GasEntry].Pressure; 
			}
			return (float)l_TotalNarcoticPotential;
		}
		public void Load(ConfigNode AQAirNode)
		{
			foreach (ConfigNode GasNode in AQAirNode.GetNodes())
			{
				if (GasNode.HasValue("LongName"))
				{
					Add(GasNode.GetValue("LongName"), new AQGas());
					this[GasNode.GetValue("LongName")].Load(GasNode);
					foreach (ConfigNode AQGasLibraryNode in GameDatabase.Instance.GetConfigNodes(AQNodeNames.GasLibrary))
					{
						if (AQGasLibraryNode.HasNode(GasNode.GetValue("LongName")))
						{
							this[GasNode.GetValue("LongName")].LoadInvariant(AQGasLibraryNode.GetNode(GasNode.GetValue("LongName")));
						}
					}
				}
			}
			return;
		}
		public void Save(ConfigNode AQAirNode)
		{
			ConfigNode GasNode;	
			foreach (string GasEntry in Keys)
			{
				if (AQAirNode.HasNode(this[GasEntry].LongName))
				{
					GasNode = AQAirNode.GetNode(this[GasEntry].LongName);
				}
				else
				{
					GasNode = AQAirNode.AddNode(this[GasEntry].LongName);
				}
				this[GasEntry].Save(GasNode);
			}
			return;
		}
	}
	/* a class describing the part's crew, particularly their capacity to influence cabin air */
	public class AQCrew : IAQGasExchange
	{
		public float CrewNumber;
		public Dictionary<string, double> GasProduction = new Dictionary<string, double>();
	//	public void Initialise(int CrewCount, AQsettings Settings)
	//	{
	//		CrewNumber = protoCrew.Count;
	//		foreach (string GasName in Settings.GasProduction.Keys)
	//		{
	//			GasProduction.Add(GasName, CrewCount * Settings.GasProduction[GasName]);
	//		}
			//	GasProduction.Add("Oxygen", -0.00025679080155881272);
			//	GasProduction.Add("CarbonDioxide", 0.00025679080155881272);
	//		return;
	//	}
		public void UpdateAir(Dictionary<string, AQGas> Air, double LivingVolume, double ScaleFactor)
		{
			foreach (string GasProductionEntry in GasProduction.Keys)
			{
				if (!Air.ContainsKey(GasProductionEntry))
				{
					Air.Add(GasProductionEntry, new AQGas());
					Air[GasProductionEntry].LongName = GasProductionEntry;
					Air[GasProductionEntry].Pressure = 0.0f;
					foreach (ConfigNode AQGasLibraryNode in GameDatabase.Instance.GetConfigNodes(AQNodeNames.GasLibrary))
					{
						if (AQGasLibraryNode.HasNode(GasProductionEntry))
						{
							Air[GasProductionEntry].LoadInvariant(AQGasLibraryNode.GetNode(GasProductionEntry));
						}
					}
				}
				Air[GasProductionEntry].Pressure += ScaleFactor * CrewNumber * GasProduction[GasProductionEntry] *
					AQPhysicalConstants.GasConstant * AQPhysicalConstants.StandardAmbientConditions.Temperature / LivingVolume;
			}
			return;
		}
	}
	public class HabitableVolumeConfig : IConfigNode               //should be moved to namespace level
	{
		public double Volume;
		public void Load(ConfigNode node)
		{
			float f;
			if (node.HasValue("LivingVolume") && float.TryParse(node.GetValue("LivingVolume"), out f))
			{
				Volume = f;
			}
		}
		public void Save(ConfigNode node)
		{
			node.AddValue("LivingVolume", Volume);
		}
	}
	/* the main class describing a habitable volume. This is attached to KSP parts and allows them to house some
	 * AQAir
	 * as well as to have gas exchange modules like
	 * AQCrew 
	 * AQScrubber
	 * AQAirFreezer
	 * AQGreenhouse
	 * AQAirVent
	 * AQLeak
	 * inherent properties of a habitable volume should be restricted to its physical size and shape,
	 * while it should expose methods to determine various aspect's of cabin habitability, depending on shape and 
	 * air composition. As planned:
	 * bool Air.IsBreatheable for sufficient oxygen and little enough poison gases to breathe without a mask
	 * bool Air.IsPressurised for pressure exceeding the partial pressure of water vapour under 36.6 degrees centigrade
	 * bool Air.IsExplosive   for dangerously explosive combinations of combustible gases with oxidisers
	 * float TotalNarcoticPotential for total lipid solubility of components under cabin pressure.
	 * but heaps of others can be thought of*/
	public class HabitableVolume : PartModule
	{
		AQsettings InstanceAQSettings = new AQsettings();
		[KSPField(isPersistant = true, guiActive = true)]
		public float Debug_CarbonDioxidePressure;
		[KSPField(isPersistant = true, guiActive = true)]
		public float Debug_OxygenPressure;

		[KSPField(isPersistant = true, guiActive = true)]
		private double LastUpdate;
		public AQAir Air;
		public AQCrew Crew;
		
		[KSPField(isPersistant = true, guiActive = true)]
		public double LivingVolume;

		public override void OnAwake()
		{
			print("OnAwake");
			Air = new AQAir();
			Crew = new AQCrew();
			InstanceAQSettings = new AQsettings();
			base.OnAwake();
		}
		public override void OnStart(StartState state)
		{
			print("Onstart: " + state);

			base.OnStart(state);
		}
		public override void OnLoad(ConfigNode node)
		{
			
			/* First, load the save-independent settings from the global settings node */
			print("[AQ] Onload: " + node);
			print("[AQ] Going to load from node " + AQNodeNames.Config);
			foreach (ConfigNode inode in GameDatabase.Instance.GetConfigNodes(AQNodeNames.Config))
			{
				print("[AQ] Loading aqsettings from " + inode);
				InstanceAQSettings.Load(inode);
				foreach (string GasProductionEntry in InstanceAQSettings.GasProduction.Keys)
				{
					print("[AQ] Gas production rate for " + GasProductionEntry + " is " + InstanceAQSettings.GasProduction[GasProductionEntry]);
				}
			}
			/* now, load gases from the confignode supplied with the argument */
			if (node.HasNode("AQAir") && (node.GetNode("AQAir").CountNodes > 0))
			{
				print("[AQ] Loading air from " + node.GetNode("AQAir"));
				Air.Load(node.GetNode("AQAir"));
				print("[AQ] Loaded " + Air.Count + " gases.");
			}
			else /*load from the starting air node*/
			{
				print ("[AQ] Going to load from StartingAir subnode of the node " + AQNodeNames.Config);
				foreach (ConfigNode inode in GameDatabase.Instance.GetConfigNodes(AQNodeNames.Config))
				{
					if (inode.HasNode("StartingAir"))
					{
						print("[AQ] Loading starting air from " + inode.GetNode("StartingAir"));
						Air.Load(inode.GetNode("StartingAir"));
						print("[AQ] Loaded " + Air.Count + " gases.");
					}
				}
			}
		//	print("[AQ] Starting simulation, initialising crew of " + base.part.protoModuleCrew.Count);
		//	Crew.Initialise(1, InstanceAQSettings);
		//	foreach (string GasProductionEntry in Crew.GasProduction.Keys)
		//	{
		//		print("[AQ] Crew has gas production rate of " + Crew.GasProduction[GasProductionEntry] + " for " + GasProductionEntry);
		//	}
			base.OnLoad(node);
		}
		public override void OnSave(ConfigNode node)
		{
			ConfigNode Airnode;
			print("[AQ] OnSave called for " + node);
			if (Air.Count == 0)
			{
				print("[AQ] Won't save empty air. Gases contained " + Air.Count);
				return;
			}
			if (node.HasNode("AQAir"))
			{
				print("[AQ] AQAir node found " + node.GetNode("AQAir"));
				Airnode = node.GetNode("AQAir");
			}
			else
			{
				Airnode = node.AddNode("AQAir");
				print("[AQ] Created AQAir node " + node.GetNode("AQAir"));
			}
			print("[AQ] Saving " + Air.Count +" gases to " + Airnode);
			Air.Save(Airnode);
			print("[AQ] Saved " + Airnode.CountNodes + "gases");
			base.OnSave(node);
		}
		public override void OnUpdate()
		{
			if (Time.timeSinceLevelLoad < 1.0f || !FlightGlobals.ready)
			{
				return;
			}
			if (LastUpdate == 0.0f)			//exact float point comparison is done intentionally to catch the case of uninitialized variable
			{
				// Just started running
				LastUpdate = Planetarium.GetUniversalTime();
				return;
			}
			if ((Planetarium.GetUniversalTime() - LastUpdate) > InstanceAQSettings.SimulationStep)
			{
				if ((Crew.GasProduction.Count == 0) || ((int)Crew.CrewNumber != (int)part.protoModuleCrew.Count))
				{
					foreach (string GasName in InstanceAQSettings.GasProduction.Keys)
					{
						Crew.GasProduction.Add(GasName, part.protoModuleCrew.Count * InstanceAQSettings.GasProduction[GasName]);
					}
					Crew.CrewNumber = part.protoModuleCrew.Count;
				}
				double ScaleFactor = 1.0f;
				//print("simulating at " + LastUpdate);
				ScaleFactor = Math.Min(((Planetarium.GetUniversalTime() - LastUpdate) / InstanceAQSettings.SimulationStep), InstanceAQSettings.MaxScaleFactor);
				LastUpdate+= ScaleFactor * InstanceAQSettings.SimulationStep;
				print ("[AQ] Updating air of " + Air.Count() + " gases in " + LivingVolume + " volume units with a scale factor of " + ScaleFactor);
				print("[AQ] part.protoModuleCrew.Count " + part.protoModuleCrew.Count);
				Crew.UpdateAir(Air, LivingVolume, ScaleFactor);
				Debug_CarbonDioxidePressure = (float)Air["CarbonDioxide"].Pressure;
				Debug_OxygenPressure = (float)Air["Oxygen"].Pressure;
			}
		}
	}
	public class AQGasResourceExchange : PartModule, IAQGasExchange
	{
		public void UpdateAir(Dictionary<string, AQGas> Air, double LivingVolume, double ScaleFactor)
		{
			return;
		}
		public void UpdateResources(double Scalefactor)
		{
			return;
		}
		public class AQReaction : IConfigNode
		{
			public string Status;                   //active, inactive, out of blah gas, lacking blah resource, no storage for blahblah, broken, no crew to operate, whatever
			public bool AlwaysActive;               //if false, a user interface should be displayed to toggle
			public string Type;                     //to get a rough idea of what the reaction is, e.g. Leak, Backfill, Vent, Scrub, Freeze, Photosynthetise
			float LimitingFactor;					//how much a reaction run is scaled down when a limiting resource shortage doesn't allow for full power
			public void Save(ConfigNode node)
			{
				return;
			}
			public void Load(ConfigNode node)
			{
				return;
			}
			public class AQResourceReagent : IConfigNode
			{
				public void Save(ConfigNode node)
				{
					return;
				}
				public void Load(ConfigNode node)
				{
					return;
				}
				bool IsLimiting;                //limiting resouces will stop the reaction if consumed and absent, or if produced and no storage available
				string Name;                    //Displayeable resource name. probably resource name from CRP?
				float Production;               //negative if consumed, in KSP units/second (probably SNC Litre/sec under RO conventions?)
			}
			public struct AQGasReagent : IConfigNode
			{
				public void Save(ConfigNode node)
				{
					return;
				}
				public void Load(ConfigNode node)
				{
					return;
				}
				bool Islimiting;                //limiting gases will stop the reaction if consumed and absent in the air
				bool EntireAtmosphereAffected;  //whether an individual gas is affected or the entire atmosphere of the cabin. other AQGasReagents will be ignored if this is true
				bool ProductionIsRelative;      //production will be treated as proportional to the total pressure/quantity
				string Name;
				float Production;               //proportional if ProductionIsRelative, negative if consumed, in mol/sec
			}
		}
		public Dictionary<string, AQReaction> Reactions;
		public string Description;
	}
}

