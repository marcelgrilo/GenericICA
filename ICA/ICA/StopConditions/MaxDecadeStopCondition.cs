using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA
{
	/// <summary>
	/// Class responsible to make the ICA to stop on the 'StopDecade' decade.
	/// Classe responsavel por fazer o ICA parar na decada 'StopDecade'.
	/// 
	/// Esta classe nao precisa ser instanciada diretamente, assim que o metodo run e chamado o sistema verifica se existe uma instancia dela nalista de condicoes, caso nao, adiciona uma nova instancia, caso sim, altera o numero de decadas.
	/// </summary>
	class MaxDecadeStopCondition : StopCondition
	{
		/// <summary>
		/// The decade in witch the stop will occour.
		/// Decada em que a parada ocorrera.
		/// </summary>
		public int StopDecade { get; set; }

		/// <summary>
		/// Construtor unico para a classe.
		/// </summary>
		/// <param name="decades"></param>
		public MaxDecadeStopCondition(int decades)
		{
			StopDecade = decades;
		}

		/// <summary>
		/// Implementacao da classe abstrata StopCondition.
		/// </summary>
		/// <param name="currentDecade"></param>
		/// <param name="cuntries"></param>
		/// <param name="idEmpires"></param>
		/// <param name="bestCostCounter"></param>
		/// <param name="lastBestCost"></param>
		/// <param name="currentBestColony"></param>
		/// <returns> true se o numero de decadas passadas for >= StopDecade </returns>
		public override bool VerifyBreak(int currentDecade, Country[] cuntries, int[] idEmpires, int bestCostCounter, double lastBestCost, Country currentBestColony)
		{
			return (currentDecade >= StopDecade);
		}
	}
}
