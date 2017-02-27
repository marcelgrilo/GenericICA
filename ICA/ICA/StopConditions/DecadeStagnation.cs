using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA
{
	/// <summary>
	/// Condicao de parada por contagem de decadas. 
	/// Verificada apenas quando existir apenas um império.
	/// </summary>
	class DecadeStagnation : StopCondition
	{

		public int DecadeStagnationValue { get; set; }

		public DecadeStagnation(int value)
		{
			DecadeStagnationValue = value;
		}

		private int _sameCostCounter = 0;

		/// <summary>
		/// Condicao de parada.
		/// </summary>
		/// <param name="currentDecade"></param>
		/// <param name="cuntries"></param>
		/// <param name="idEmpires"></param>
		/// <param name="bestCostCounter"></param>
		/// <param name="lastBestCost"></param>
		/// <param name="currentBestColony"></param>
		/// <returns> true se o custo ficou o mesmo durante DecadeStagnationValue decadas</returns>
		public override bool VerifyBreak(int currentDecade, Country[] cuntries, int[] idEmpires, int bestCostCounter, double lastBestCost, Country currentBestColony)
		{
			if (currentBestColony.Cost == lastBestCost)
				_sameCostCounter++;
			else
				_sameCostCounter = 0;

			return (_sameCostCounter >= DecadeStagnationValue);
		}
	}
}
