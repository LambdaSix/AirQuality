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
			return (MinRequiredPressure > AQConventions.floatzero);
		}
		public bool isPoison()
		{
			return (MaxToleratedPressure > AQConventions.floatzero);
		}
		public void Load(ConfigNode node) //load part-specific data only
		{                                   //see LoadInvariant() to initialise fields with invariant information
			float f;
			if (node.HasValue(AQConventions.Values.LongName))
			{
				LongName = node.GetValue(AQConventions.Values.LongName);
			}
			if (node.HasValue(AQConventions.Values.Pressure) && float.TryParse(node.GetValue(AQConventions.Values.Pressure), out f))
			{
				Pressure = f;
			}
			return;
		}
		public void LoadInvariant(ConfigNode node) //load part-invariant gas properties from global gas definitions
		{                   // see Load() to load flight-specific data
			float f;
			if (node.HasValue(AQConventions.Values.ShortName))
			{
				ShortName = node.GetValue(AQConventions.Values.ShortName);
			}
			if (node.HasValue(AQConventions.Values.MinRequiredPressure) && float.TryParse(node.GetValue(AQConventions.Values.MinRequiredPressure), out f))
			{
				MinRequiredPressure = f;
			}
			if (node.HasValue(AQConventions.Values.MaxToleratedPressure) && float.TryParse(node.GetValue(AQConventions.Values.MaxToleratedPressure), out f))
			{
				MaxToleratedPressure = f;
			}
			if (node.HasValue(AQConventions.Values.CondensationPressure) && float.TryParse(node.GetValue(AQConventions.Values.CondensationPressure), out f))
			{
				CondensationPressure = f;
			}
			if (node.HasValue(AQConventions.Values.MolarMass) && float.TryParse(node.GetValue(AQConventions.Values.MolarMass), out f))
			{
				MolarMass = f;
			}
			if (node.HasValue(AQConventions.Values.NarcoticPotential) && float.TryParse(node.GetValue(AQConventions.Values.NarcoticPotential), out f))
			{
				NarcoticPotential = f;
			}
			return;
		}
		public void Save(ConfigNode node)                   //only save part-specific data
		{
			if (node.HasValue(AQConventions.Values.LongName))
			{
				node.SetValue(AQConventions.Values.LongName, LongName);
			}
			else
			{
				node.AddValue(AQConventions.Values.LongName, LongName);
			}
			if (node.HasValue(AQConventions.Values.Pressure))
			{
				node.SetValue(AQConventions.Values.Pressure, Pressure);
			}
			else
			{
				node.AddValue(AQConventions.Values.Pressure, Pressure);
			}
			return;
		}
	}
}