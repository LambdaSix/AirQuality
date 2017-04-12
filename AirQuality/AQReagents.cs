using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace AirQuality
{ 	/*	
		Classes that Describe possible reagents and products of AQReaction objects. 
		AQResourceReagent is a representation of a KSP resource
		AQGasGeagents us a representation of an AQGas object	
	*/
	public class AQResourceReagent : IConfigNode
	{
		public bool IsProduct()
		{
			return (Production > 0);
		}
		public bool IsConsumable()
		{
			return (Production < 0);
		}
		public void Save(ConfigNode node)
		{
			return;                        										 //not supposed to save reagents in-game
		}
		public void Load(ConfigNode node)
		{
			float f;
			bool b;
			if (node.HasValue("Name"))
			{
				Name = node.GetValue("Name");
			}
			if (node.HasValue("IsLimiting") && bool.TryParse(node.GetValue("IsLimiting"), out b))
			{
				IsLimiting = b;
			}
			if (node.HasValue("Production") && float.TryParse(node.GetValue("Production"), out f))
			{
				Production = f;
			}
			return;
		}
		public bool IsLimiting;                //Limiting consumeables will stop or wind down the reaction if lacking todo possibly eliminate, replacing with IsConsumeable, as seeming there's no point consuming the resource unless it is limiting
		public string Name;                    //Displayeable resource name, doubling as its unique identifier
		public float Production;               //negative if consumed, in KSP units/second (probably SNC Litre/sec under RO conventions?)
	}
	public class AQGasReagent : IConfigNode
	{
		public bool IsProduct()
		{
			return (Production > 0);
		}
		public bool IsConsumable()
		{
			return (Production < 0);
		}
		public void Save(ConfigNode node)
		{
			return;																//not supposed to save reagents in-game
		}
		public void Load(ConfigNode node)
		{
			float f;
			bool b;
			if (node.HasValue("Name"))
			{
				Name = node.GetValue("Name");
			}
			if (node.HasValue("IsLimiting") && bool.TryParse(node.GetValue("IsLimiting"), out b))
			{
				IsLimiting = b;
			}
			if (node.HasValue("Production") && float.TryParse(node.GetValue("Production"), out f))
			{
				Production = f;
			}
			return;
		}
		public bool IsLimiting;                //limiting gases will stop/wind down the reaction if consumed and absent/lacking in the air
		public string Name;						//Displayable name, same as AQGas.Longname
		public float Production;               //proportional if Reaction.Type is "Leak", negative if consumed, in mol/sec todo split Leak in a separate PartModule!
	}
}