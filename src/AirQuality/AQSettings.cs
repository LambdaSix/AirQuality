using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace AirQuality
{   
	public class AQsettings : IConfigNode
	{
		public double SimulationStep;
		public double MaxScaleFactor;
		public Dictionary<string, AQGas> StartingAir;
		private AQGas Gas = new AQGas();
		public void Load(ConfigNode node)
		{
			float f;
			if (node.HasValue(AQConventions.Values.SimulationStep) && float.TryParse(node.GetValue(AQConventions.Values.SimulationStep), out f))
			{
				SimulationStep = f;
			}
			if (node.HasValue(AQConventions.Values.MaxScaleFactor) && float.TryParse(node.GetValue(AQConventions.Values.MaxScaleFactor), out f))
			{
				MaxScaleFactor = f;
			}
			if (node.HasNode(AQConventions.Values.StartingAir))
			{
				StartingAir = new Dictionary<string, AQGas>();
				foreach (ConfigNode ChildNode in node.GetNode(AQConventions.Values.StartingAir).GetNodes())
				{
					Gas.Load(ChildNode);
					StartingAir.Add(ChildNode.name, Gas);
				}
			}
			return;
		}
		public void Save(ConfigNode node)
		{
			return;						//not supposed to save settings in-game, only load from the config
		}
	}
}

