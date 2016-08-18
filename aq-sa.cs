using System;
using System.Collections.Generic;

namespace AQsideproject
{
	class HabitableVolume
	{
		class AQPhysicalConstants
		{
			public static double GasConstant = 8.3144598f;					//in J/K/mol
			public struct StandardAmbientConditions
			{
				public static double MolarVolume = .024789598f;				//in cubic metres/mol
				public static double Temperature = 298.15f;					//in K
				public static double Pressure = 100000.0f;					//in Pa
			}
			public static Dictionary<string, double> MolarMass =			//in kg/mol
				new Dictionary<string, double>()
			{
				{"CarbonDioxide",0.04401f},							
				{"Oxygen", 0.0319988}
			};
		}	
		private class AQCrew
		{
			
			public float CrewNumber = 0.0f;
			public Dictionary<string, double> GasProduction =
				new Dictionary<string, double>();
			public void Initialise(Dictionary<string, double> GasProduction)
			{
				CrewNumber = 1.0f;
				GasProduction.Add ("Oxygen", -0.00025679080155881272);
				GasProduction.Add ("CarbonDioxide", 0.00025679080155881272);
				return;
			}
			public void UpdateAir(Dictionary<string, Dictionary<string,double>> Air,double LivingVolume)
			{
				foreach (KeyValuePair<string,double> GasProductionEntry in GasProduction)
				{
					Air[GasProductionEntry.Key]["Pressure"] += CrewNumber* GasProductionEntry.Value * 
						AQPhysicalConstants.GasConstant * AQPhysicalConstants.StandardAmbientConditions.Temperature / LivingVolume;
				}
				return;
			}
		}
		private AQCrew Crew = new AQCrew();
		private Dictionary<string, Dictionary<string,double>> Air =
			new Dictionary<string, Dictionary<string,double>>();
		
		public static double LivingVolume = 0.0f;

		private void Initialise()
		{
			LivingVolume = 2.0f;
			Air.Add("Oxygen",new Dictionary<string,double>());
			Air["Oxygen"].Add("Pressure",20000.0f);
			Air["Oxygen"].Add("MinRequiredPressure",8000.0f);
			Air["Oxygen"].Add("MaxToleratedPressure",300000.0f);
			Air["Oxygen"].Add("MolarMass",AQPhysicalConstants.MolarMass["Oxygen"]);
			Air.Add("CarbonDioxide",new Dictionary<string,double>());
			Air["CarbonDioxide"].Add("Pressure",0.0f);
			Air["CarbonDioxide"].Add("MaxToleratedPressure",8000.0f);
			Air["CarbonDioxide"].Add("MinRequiredPressure",-1.0f);
			Air["CarbonDioxide"].Add("MolarMass",AQPhysicalConstants.MolarMass["CarbonDioxide"]);
		}

		private bool IsBreatheable(Dictionary<string, Dictionary<string,double>> Air)
		{
			foreach (KeyValuePair<string, Dictionary<string,double>> Gas in Air)
				if ((Gas.Value["MaxToleratedPressure"] > 0) && (Gas.Value["Pressure"] > Gas.Value["MaxToleratedPressure"]))
					return false;																				//Poison gas pressure exceeded
			foreach (KeyValuePair<string, Dictionary<string,double>> Gas in Air)
				if ((Gas.Value["MinRequiredPressure"] > 0) && (Gas.Value["Pressure"] > Gas.Value["MinRequiredPressure"]))
					return true;																				//Enough breatheable gas
			return false;																						//Not enough breatheable gas
		}
			
		public static void Main (string[] args)
		{
			int Counter = 1;
			HabitableVolume Cabin = new HabitableVolume();
			Cabin.Crew = new AQCrew();
			Cabin.Initialise();
			Cabin.Crew.Initialise(Cabin.Crew.GasProduction);

			for (Counter = 1;Cabin.IsBreatheable (Cabin.Air); Counter++) 
			{
				Cabin.Crew.UpdateAir(Cabin.Air,LivingVolume);
			}

			Console.WriteLine (Cabin.Air["Oxygen"]["Pressure"]);
			Console.WriteLine (Counter);
		}
	}
}
