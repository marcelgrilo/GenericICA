using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA
{
	public abstract class StopCondition
	{
		string Name { get; set; }

		public bool Converged { get; internal set; }

		public abstract bool VerifyBreak(int currentDecade, Country[] cuntries, int[] idEmpires, int bestCostCounter, double lastBestCost, Country currentBestColony);
	}
}
