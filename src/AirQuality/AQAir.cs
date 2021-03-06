using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace AirQuality
{
	public class AQAir : Dictionary<string, AQGas>, IConfigNode
	{
		public float Temperature;
		public float Quantity(string gasname, double Volume)
		{
			if (ContainsKey(gasname))
			{
				return (float)this[gasname].Quantity(Volume);
			}
			else return (float)AQConventions.floatzero;
		}
		public void RegisterGas(string gasname)
		{
			Add(gasname, new AQGas());
			this[gasname].LongName = gasname;
			this[gasname].Pressure = AQConventions.floatzero;
			foreach (ConfigNode AQGasLibraryNode in GameDatabase.Instance.GetConfigNodes(AQNodeNames.GasLibrary))
			{
				if (AQGasLibraryNode.HasNode(gasname))
				{
					this[gasname].LoadInvariant(AQGasLibraryNode.GetNode(gasname));
				}
			}
		}
		public bool IsBreatheable()
		{
			foreach (string GasEntry in Keys)
			{
				if (this[GasEntry].isPoison() && (this[GasEntry].Pressure > this[GasEntry].MaxToleratedPressure))
				{
					return false;   //poisonous
				}
			}
			foreach (string GasEntry in Keys)
			{
				if (this[GasEntry].isBreatheable() && (this[GasEntry].Pressure > this[GasEntry].MinRequiredPressure))
				{
					return true;    //breatheable and not poisonous
				}
			}
			return false;           //unbreatheable
		}
		public bool IsPressurised()
		{
			double TotalPressure = AQConventions.floatzero;
			foreach (string GasEntry in Keys)
			{
				TotalPressure += this[GasEntry].Pressure;
			}
			return (TotalPressure > AQPhysicalConstants.ArmstrongLimit);
		}
		public float TotalNarcoticPotential()
		{
			double l_TotalNarcoticPotential = AQConventions.floatzero;
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
				if (GasNode.HasValue(AQConventions.Values.LongName))
				{
					Add(GasNode.GetValue(AQConventions.Values.LongName), new AQGas());
					this[GasNode.GetValue(AQConventions.Values.LongName)].Load(GasNode);
					foreach (ConfigNode AQGasLibraryNode in GameDatabase.Instance.GetConfigNodes(AQNodeNames.GasLibrary))
					{
						if (AQGasLibraryNode.HasNode(GasNode.GetValue(AQConventions.Values.LongName)))
						{
							this[GasNode.GetValue(AQConventions.Values.LongName)].LoadInvariant(AQGasLibraryNode.GetNode(GasNode.GetValue(AQConventions.Values.LongName)));
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
}