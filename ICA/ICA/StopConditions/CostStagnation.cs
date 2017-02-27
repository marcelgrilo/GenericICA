using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA
{
	/// <summary>
	/// Condicao de parada por estagnacao de custo.
	/// Verificado apenas quando existir apenas um império.
	/// </summary>
	class CostStagnation : StopCondition
	{

		public double CostStagnationValue { get; set; }

		public int CostStagnationCounter { get; set; }

		private int _costStagnationCounter;

		public CostStagnation(double value, int iterations)
		{
			CostStagnationValue = value;
			CostStagnationCounter = iterations;
			_costStagnationCounter = 0;
		}

		/// <summary>
		/// Condicao de parada.
		/// </summary>
		/// <param name="currentDecade"></param>
		/// <param name="cuntries"></param>
		/// <param name="idEmpires"></param>
		/// <param name="bestCostCounter"></param>
		/// <param name="lastBestCost"></param>
		/// <param name="currentBestColony"></param>
		/// <returns>true se a diferenca entre o custo anterior e o atual nao diferenciaram em CostStagnationValue durante CostStagnationCounter decadas.</returns>
		public override bool VerifyBreak(int currentDecade, Country[] cuntries, int[] idEmpires, int bestCostCounter, double lastBestCost, Country currentBestColony)
		{
			if (Math.Abs(Math.Abs(lastBestCost - currentBestColony.Cost) / currentBestColony.Cost) < CostStagnationValue)
				_costStagnationCounter++;
			else
				_costStagnationCounter = 0;

			return _costStagnationCounter > CostStagnationCounter;
		}
	}
}
