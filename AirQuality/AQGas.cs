using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace AirQuality
{														/* a class describing a gas as a component of cabin atmosphere */

	public class AQGas : IConfigNode
	{
		public string LongName;
		public string ShortName;
		public double Pressure;
		public double MinRequiredPressure;
		public double MaxToleratedPressure;
		public double CondensationPressure;
		public double MolarMass;
		public double NarcoticPotential;
		public double Quantity(double Volume)
		{
			return (Pressure * Volume / AQPhysicalConstants.GasConstant / AQPhysicalConstants.StandardAmbientConditions.Temperature);
		}
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
			return;
		}
		public void LoadInvariant(ConfigNode node) //load part-invariant gas properties from global gas definitions
		{                   // see Load() to load flight-specific data
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
		public void Save(ConfigNode node)                   //only save part-specific data
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
}