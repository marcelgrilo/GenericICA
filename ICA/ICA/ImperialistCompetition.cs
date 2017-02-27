using System;
using System.Collections.Generic;
using System.Linq;
using tasks = System.Threading.Tasks; //Parallel.For do Net 4.0
using System.Collections;

namespace ICA
{

	public enum MovementType
	{
		Linear, Original, Refined, Distortion, RefinedWithOriginal, RefinedWithDistortion, DistortionWithOriginal, All
	}



	public class ImperialistCompetition
	{
		private static float FLOAT_ZERO = 0.000001f;


		/// <summary>
		/// A variavel randomica para a fixar uma semente.
		/// The random variable to set a seed.
		/// </summary>
		private ThreadSafeRandom _rnd;



		/// <summary>
		/// membro de controle para a propriedade IFitness.
		/// </summary>
		private IFitness _fitness;



		#region propriedades ICA

		/// <summary>
		/// Numero de atributos ou variaveis do problema em questao.
		/// Number of attributes or variables of the problem.
		/// </summary>
		public int Dimensions { get; internal set; }



		/// <summary>
		/// Valor da populacao inicial.
		/// Value of the start population.
		/// </summary>
		public int StartPopulation { get; set; } = 128;



		/// <summary>
		/// Define a quantidade de paises serao selecionados como imperios na inicializacao do problema
		/// Defines the quantities of countries will be selected as empires at the problem initialization.
		/// </summary>
		public double ScopeImpPercentage { get; set; } = .1;



		/// <summary>
		/// Define uma condicao de parada do algoritmo pelo numero de decadas passadas.
		/// Defines a stop condition for the algorithm with the number of decades.
		/// </summary>
		public int MaxDecades { get; set; } = 2048;



		/// <summary>
		/// Decada atial em que o ica se encontra.
		/// </summary>
		public int CurrentDecade { get; internal set; }



		/// <summary>
		/// Este valor define se haverá ou não revolucao de um pais. este valor pode ser incrementado ou decrementado durante o processo evolucionario.
		/// Revolucao e o processo no qual as caracteristicas de um pais mudam drasticamente( asemelha-se a mutacao no algoritmo genetico canonico.)
		/// This value defines if there will ve or not a revolution of some colonies. This value can be incremented or decremented durring the evolutionary process.
		/// Revulution is the process where the characteristics of a colony changes drasticaly( Resembles the mutation process at the canonical GA.)
		/// </summary>
		public double RevolutionRate { get; set; } = .9;



		/// <summary>
		/// E o valor que representa a taxa de alteracao com que a taxa de revolucao sera alterada durante o processo evolucionario.
		/// Se RevolutionDampRatio menor 1 : diminui-se a quantida de de revolucoes(RevolutionRate) que ocorrem durante as epocas.
		/// Se RevolutionDampRatio igual 1: a quantida de de revolucoes(RevolutionRate) que ocorrem durante as epocas nao se altera.
		/// Se RevolutionDampRatio maior 1 : aumenta-se a quantida de de revolucoes(RevolutionRate) que ocorrem durante as epocas.
		/// It is a value that represent the changing rate of the revolution rate.
		/// If RevolutionDampRatio lesser 1 : the number of revolutions that occour durring the epochs will slow down.
		/// If RevolutionDampRatio equal 1: the number of revolutions that occour durring the epochs do not change.
		/// If RevolutionDampRatio greater 1 : the number of revolutions that occour durring the epochs will grow up.
		/// </summary>
		public double RevolutionDampRatio { get; set; } = .95;



		/// <summary>
		/// <para>
		/// O coeficiente de assimilacao
		/// No paper original o coeficiente de assimilacao e chamado de "beta".
		/// Este coeficiente funciona como uma forca de atracao que um imperio causa em sua colonia.<br />
		/// Valores maiores que 1 tem a chance de explorar solucoes fora do caminho direto entre a colonia e o imperio, podendo extrapolar tanto para cima quanto para baixo dependendo do valor deste coeficiente.<br />
		///  s     *    e     s<br />
		///  s     c *  e     s<br />
		///  s     c    e  *  s<br />
		///  s  *  c    e     s<br />
		///  s     c  * e     s<br />
		///  s     c    *     s<br />
		/// OBS: O movimento ainda continua sempre direcionado para o imperio.
		/// Valores menores ou iguais a 1 fazem com que o passo tenha um tamanho aleatorio, porem nunca extrapolam o limite da distancia entre colonia e imperio.<br />
		///   s     *    e     s<br />
		///   s     c*   e     s<br />
		///   s     c *  e     s<br />
		///   s     c  * e     s<br />
		///   s     c   *e     s<br />
		///   s     c    *     s<br />
		/// </para>
		/// <para>
		/// The assimilate coeficient.
		/// In the original paper, this coeficient is called 'beta'.
		/// This coeficient works as a force of attraction that the empire causes at its colonies.<br />
		/// Values grater than 1 have a chance to extrapolate the solution and goes out of the direct way beyond the colony and its empirem. It can extrapolate up or down. <br />
		///  s     *    e     s<br />
		///  s     c *  e     s<br />
		///  s     c    e  *  s<br />
		///  s  *  c    e     s<br />
		///  s     c  * e     s<br />
		///  s     c    *     s<br />
		/// OBS: The movement is always directed to its empire.<br />
		/// Values lesser or equal to one makes that the pass have an aleatory size, but never extrapolates the distance limit between the colony and the country.<br />
		///   s     *    e     s<br />
		///   s     c*   e     s<br />
		///   s     c *  e     s<br />
		///   s     c  * e     s<br />
		///   s     c   *e     s<br />
		///   s     c    *     s<br />
		/// </para>
		/// </summary>
		public double Beta { get; set; } = 2.000001;



		/// <summary>
		/// Gama eh um ajuste grosseiro dado ao passo do pais durante o movimento que gera um ruido semelhante a mutacao.
		/// </summary>
		public double Gama { get; set; } = Math.PI / 4;


		private double _p = 0.1;
		/// <summary>
		/// define a propriedade P usada pela função de movimento refinada.
		/// </summary>
		public double P
		{
			get { return _p; }
			set { _p = Math.Max(Math.Min(value, 1), 0); }
		}




		/// <summary>
		/// Define um valor que representa a porcentagem do tamanho do espaco de busca o que fará com que um imperio seja englobadop por outro imperio ou nao.
		/// Defines a value that represents a percentage of the size of the space search, what will makes the an empire unite with another empire or not.
		///  </summary>
		public double UnitingThreshold { get; set; } = 0.005;



		/// <summary>
		/// Define uma porcentagem de ocorrência para a competicao imperialista.
		/// .98 significa que ha uma chance de 98% de se ocorrer uma competicao.
		/// Defines a percentage od occurence for the imperialist competition.
		/// .98 means the there will be 98% chance that it occours.
		/// </summary>
		public double ImperialistCompetitionFactor { get; set; } = .95;



		/// <summary>
		/// Valor usado para calcular o poder de cada imperio. 
		/// Deve variar de 0 a 1.
		/// Significa o quanto a media dos custos das colonias 
		/// influenciara no custo total do imperio(poder) que 
		/// eh calculado da seguinte forma:
		/// Poder[i] = imperio[i].custo + Epsilon * Media(imperio[i].colonias);
		/// 
		/// Value used to calculate the power of each empire.
		/// It must vary between 0 and 1.
		/// It is a value that means how much the mean of the cost of an empire colonies will influencete on its total cost and power.
		/// It is calculated as:
		/// Power[i] = empire[i].cost + Epsilon * Mean(empire[i].colonies);
		/// </summary>
		public double Epsilon { get; set; } = 0.05;




		/// <summary>
		/// Limite inferior para cada dimensao.
		/// Inferior limit for each dimension.
		/// </summary>
		public double[] MinBounds { get; set; }



		/// <summary>
		/// Limite superior para cada dimensao.
		/// Superior limit for each dimension.
		/// </summary>
		public double[] MaxBounds { get; set; }



		/// <summary>
		/// Tamanho do espaco de busca para cada dimensao.
		/// Size of the search space for each dimension.
		/// SpaceSearchSize[i] = MaxBounds[i] - MinBounds[i]
		/// usado apenas para calculo da norma.
		/// </summary>
		public double[] SpaceSearchSize { get; set; }



		/// <summary>
		/// Todas as instancias de paises do problema. 
		/// Possui tanto os imperios quanto as colonias.
		/// All the instances of countries of the problem. 
		/// Has both empires as the colonies.
		/// </summary>
		public Country[] Countries { get; private set; }




		/// <summary>
		/// A funcao fitness para a avaliacao dos custos.
		/// The fitness function for the evaluation of the costs .
		/// </summary>
		public IFitness Fitness
		{
			get { return _fitness; }
			set
			{
				_fitness = value;
				Dimensions = _fitness.Dimensions;
			}
		}
		#endregion




		#region Condicoes de parada

		public bool UseMaxDecadeStopCondition { get; set; } = true;
		public bool UseDecadeStagnationStopCondition { get; set; } = false;
		public bool UseCostStagnationStopCondition { get; set; } = false;

		/// <summary>
		/// Variavel de controle que define se o algoritmo deve convergir para apenas um imperio antes de comecar a avaliar as condicoes de parada.
		/// </summary>
		public bool ConvergeToOneEmpire { get; set; } = false;



		/// <summary>
		/// Valor para a condicao de parada fixa de estagnacao por custo identico durante decadas.
		/// </summary>
		public int DecadeStagnationValue { get; set; } = 100;



		/// <summary>
		/// Valor que define o intervalo da condicao de parada fixa de estagnacao por intervalo de custo
		/// </summary>
		public double CostStagnationPercent { get; set; } = 0.1;



		/// <summary>
		/// Valor para a condicao de parada fixa de estagnacao por intervalo de custo.
		/// </summary>
		public int CostStagnationCounter { get; set; } = 100;



		/// <summary>
		/// lista privada de condicoes de parada.
		/// </summary>
		private List<StopCondition> _stopConditions;



		/// <summary>
		/// Lista de condicoes de parada atingidas durante uma parada.
		/// </summary>
		public List<StopCondition> AchievedStopCondition { get; set; }



		/// <summary>
		/// Adiciona uma condicao de parada na lista de condicoes de paradas do ica.
		/// </summary>
		/// <param name="stopCondition">A condicao de parada.</param>
		public void AddStopCondition(StopCondition stopCondition)
		{
			if (_stopConditions == null)
				_stopConditions = new List<StopCondition>();

			_stopConditions.Add(stopCondition);

		}
		#endregion



		#region Notificações
		/// <summary>
		/// Variavel de controle de notificacao.
		/// </summary>
		public bool NotifyByDecadePassed { get; set; } = false;



		/// <summary>
		/// Delegate para os listeners de DecadePassedHandler.
		/// </summary>
		/// <param name="countries"> lista de paises.</param>
		/// <param name="decade"> decada.</param>
		public delegate void DecadePassedHandler(Country[] countries, int decade);



		/// <summary>
		/// Handler de gatilho para funcoes do tipo DecadePassedHandler que estejam escutando sejam notificadas.
		/// DecadePassedHandler(Country[] countries, int decade)
		/// </summary>
		public DecadePassedHandler OnDecadePassed;
		#endregion



		#region Utilidades e Debug

		/// <summary>
		/// Gets or sets a value indicating whether this instance is parallel.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is parallel; otherwise, <c>false</c>.
		/// </value>
		public bool IsParallel { get; set; } = true;



		/// <summary>
		/// Modo de movimento ate o imperialista.
		/// </summary>
		public MovementType SelectedMovementType { get; set; } = MovementType.Refined;


#if Log
		/// <summary>
		/// Logger de adquivo.
		/// </summary>
		public Logger Logger { get; set; }
#endif
		#endregion



		/// <summary>
		/// Construtor padrão.
		/// </summary>
		public ImperialistCompetition()
		{
			_stopConditions = new List<StopCondition>();
			AchievedStopCondition = new List<StopCondition>();
			_rnd = new ThreadSafeRandom(0);
		}



		/// <summary>
		/// Construtor com uma semente de random.
		/// </summary>
		/// <param name="RandowSeed">semente.</param>
		public ImperialistCompetition(int RandowSeed) : this()
		{
			_rnd = new ThreadSafeRandom(RandowSeed);
		}



		/// <summary>
		/// Propriedade que retorna um clone do melhor pais.
		/// </summary>
		public Country BestCountryClone { get { return Countries.OrderBy(o => o.Cost).ToArray()[0].Clone(); } }



		/// <summary>
		/// Roda o processamento do ica.
		/// </summary>
		public void Run()
		{


			if (UseMaxDecadeStopCondition)
			{
				// condicao de parada por numero maximo de decadas.
				var md = _stopConditions.Where(sc => sc is MaxDecadeStopCondition).FirstOrDefault();
				if (md == null)
					_stopConditions.Add(new MaxDecadeStopCondition(MaxDecades));
				else
					md = new MaxDecadeStopCondition(MaxDecades);
			}

			if (UseDecadeStagnationStopCondition)
			{

				// condicao de parada por estagnacao de custo identico durante as decadas.
				if (DecadeStagnationValue == 0)
					DecadeStagnationValue = MaxDecades / 4;
				var ds = _stopConditions.Where(sc => sc is DecadeStagnation).FirstOrDefault();
				if (ds == null)
					_stopConditions.Add(new DecadeStagnation(DecadeStagnationValue));
				else
					ds = new DecadeStagnation(DecadeStagnationValue);

			}

			if (UseCostStagnationStopCondition)
			{
				//condicao de parada por estagnacao de custo em um intervalo durante as decadas.
				if (CostStagnationCounter == 0)
					CostStagnationCounter = MaxDecades / 2;
				var cs = _stopConditions.Where(sc => sc is CostStagnation).FirstOrDefault();
				if (cs == null)
					_stopConditions.Add(new CostStagnation(CostStagnationPercent, CostStagnationCounter));
				else
					cs = new CostStagnation(CostStagnationPercent, CostStagnationCounter);

			}
#if Log
			//log stuf.
			//if (this.Logger == null)
			//{
			//	this.Logger = new Logger() { Path = Directory.CreateDirectory("LOG").FullName + "/ICA_LOG_" + DateTime.Now.ToString("dd-MM-yyyy HH_mm_ss") + ".txt", LogOnConsole = true };
			//}
			//Logger.LogLine(string.Format("####################################\nRunningICA\n\nCountries:{0},\nDecades:{1},\nRevolution Rate:{2},\nRevolution Damp Ratio:{3},\nAssimilate Coeficient:{4},\nUniting Threshold:{5},\nImperialist Competition Factor:{6},\nEpsilon:{7},\nDimension:{8},\nScope Imperialists Percentage:{9}\n####################################\n", StartPopulation, MaxDecades, RevolutionRate, RevolutionDampRatio, Beta, UnitingThreshold, ImperialistCompetitionFactor, Epsilon, Dimension, ScopeImpPercentage));
#endif

			// inicializando os valores constantes
			// initializing the constant values
			InitializeSpaceSearch();
			int nPopulacao = StartPopulation;
			int nImperios = (int)Math.Max(Math.Round(nPopulacao * ScopeImpPercentage), 1);
			int nColonias = nPopulacao - nImperios;

			// inicializando os paizes
			// initializing the countries. 
			Countries = Fitness.GenerateCountries(nPopulacao, _rnd, MinBounds, MaxBounds);

			// Calculando oos custos paralelamente.
			// Calculating the costs paralelly.
			if (IsParallel)
			{
				tasks.Parallel.For(0, Countries.Length, i =>
				{
					Fitness.Eval(ref Countries[i]);
				});
			}
			else
			{
				for (int i = 0; i < Countries.Length; i++)
				{
					Fitness.Eval(ref Countries[i]);
				}
			}

			// Definindo imperios 
			// Defining the empires.
			int[] IdEmpire = Countries
				.Select((c, n) => new { indice = n, colony = c })
				.OrderBy(p => p.colony.Cost)
				.Select(p => p.indice)
				.Take(nImperios).ToArray();

			// Normalizando o custo dos imperios.
			// Normalizing the empires.
			double maxi = Countries[IdEmpire[nImperios - 1]].Cost;
			maxi = maxi > 0 ? maxi * 1.3 : maxi * 0.7;
			double[] power = new double[nImperios];
			for (int i = 0; i < nImperios; i++)
			{
				Countries[IdEmpire[i]].IsEmpire = true;
				Countries[IdEmpire[i]].IdEmpire = i;
				// Poder inicial do imperio
				// Initial power of the empire.
				power[i] += maxi - Countries[IdEmpire[i]].Cost;
			}
			double sumOfAllImperialistsPower = power.Sum();

			// defininco o numero inicial de colonias para o imperio.
			// defining the initial number of colonies for each imperialist.
			int[] nOfColonies = new int[nImperios];
			if (Math.Abs(sumOfAllImperialistsPower) < FLOAT_ZERO)
			{
				for (int i = 0; i < nImperios; i++)
					nOfColonies[i] = (nColonias / nImperios);
			}
			else
				for (int i = 0; i < nImperios; i++)
					nOfColonies[i] = (int)Math.Floor(power[i] / sumOfAllImperialistsPower * nColonias);

			// corrige o número de colonias
			// fix the colony number
			var dif = Math.Max(nColonias - nOfColonies.Sum(), 0);
			int idif = 0;
			while (dif > 0)
			{
				// Ultima colonia com os restantes de paises
				// Last colony with the rest of the colonies.
				nOfColonies[idif % nImperios]++;
				dif--;
				idif++;
			}



			// Misturando colonia.
			// Shuffling the colonies.
			// OBS: Nao pode ser usado este metodo (1) pois ele não usa a variavel randomica com a semente utilizada.
			// OBS: Cant use the method (1) because it dont uses the Random variable with our seed value.
			// (1)Colonies = Colonies.OrderBy(a => Guid.NewGuid()).ToArray();
			Countries = Countries
				.ToList()
				.Select(t => new { Index = _rnd.Next(), Value = t })
				.OrderBy(p => p.Index)
				.Select(p => p.Value)
				.ToArray();
			// reconfigurando o vetor IdEmpire
			// reconfigure the IdEmpire array.
			IdEmpire = Countries
				.Select((c, n) => new { indice = n, colony = c })
				.OrderBy(p => p.colony.Cost)
				.Select(p => p.indice)
				.Take(nImperios).ToArray();

			// Distribuindo as colonias entre os imperios aleatoriamente.
			// Distributing the colonies among the imperialists randomly.
			int nCountry = 0;
			for (int i = 0; i < nImperios; i++)
			{
				int j = 0;
				while (j < nOfColonies[i])
				{
					if (!Countries[nCountry].IsEmpire)
					{
						Countries[nCountry].IdEmpire = IdEmpire[i];
						j++;
					}
					nCountry++;
				}
			}

			// Se existe um imperio sem uma colonia, uma colonia aleatoria sera dada a ele.
			// if there is an empire without a colony it will be given one colony to this empire. 
			if (nOfColonies[nImperios - 1] == 0)
			{
				nCountry = 0;
				while (nCountry < nPopulacao)
				{
					if (!Countries[nCountry].IsEmpire)
					{
						Countries[nCountry].IdEmpire = nImperios - 1;
						break;
					}
					else
					{
						nCountry++;
					}
				}
			}

			//
			//   ->
			//  ^  |
			//  |  v
			//   <-
			//
			// Loop principal.
			// Main Loop
			//int lastDecade = 0;
			CurrentDecade = 0;
			double lastBestCost = 0;
			int bestCostCounter = 0;
			//for (int decade = 0; decade < MaxDecades; decade++)
			do
			{

				if (NotifyByDecadePassed)
				{
					if (OnDecadePassed != null)
						OnDecadePassed(Countries, CurrentDecade);
				}

				// se nao houver imperios (ERRO!!).
				// if there is no empires (ERROR!!)
				if (IdEmpire.Length == 0)
					throw new Exception("There can't be 0 Empires.");

				var bestCountry = Countries.OrderBy(o => o.Cost).ToArray()[0];
				bestCostCounter = (Math.Abs(lastBestCost - bestCountry.Cost) < FLOAT_ZERO) ? (bestCostCounter + 1) : 0;
				lastBestCost = bestCountry.Cost;
#if Log
				//Logger.Clear();
				//Logger.LogLine("\n\nDecade: " + CurrentDecade + " Empire count: " + IdEmpire.Length + " Best Cost: " + bestCountry.Cost + " on Empire: " + bestCountry.IdEmpire + " counter " + _costStagnationCounter + "\n");
#endif
				//      ____
				//     /    \
				//    | stop |
				//     \____/
				//
				// come'c a contar a condi'c±ao de parada apenas quando o numero de imperios 'e =1
				//bool verifyStopConditions = ConvergeToOneEmpire ? IdEmpire.Count() == 1 : true;
				if (bestCountry.Cost <= 0)
					return;

				if ((ConvergeToOneEmpire ? IdEmpire.Count() == 1 : true) /* || (MaxDecades < CurrentDecade)*/)
				{

					// Condicoes de parada.
					// Stop conditions.
					bool stopSatisfied = false;
					foreach (var stopCondition in _stopConditions)
					{
						// resultado da condicao de parada
						//stop condition result.
						bool sc = stopCondition.VerifyBreak(CurrentDecade, Countries, IdEmpire, bestCostCounter, lastBestCost, bestCountry);
						stopCondition.Converged = sc;
						// flag stopCondition para agregar os resultados
						// stopCondotion flag to agregate the results.
						stopSatisfied |= sc;
						// adiciona todas as condicoes de parada alcancadas para a lista.
						// Add all the stop conditions achived to the list.
						if (sc == true)
						{
							AchievedStopCondition.Add(stopCondition);
						}
					}

					if (stopSatisfied)
					{ break; }
				}

				// Atualiza o valor da taxa de revolucao.
				// Updates the value of the RevolutionRate.
				NextRevolutionRate();
#if Log
				Logger.LogLine("\n======Movement And Revolution\n");
#endif
				//
				// Para cada imperio faz a movimentacao e a revolucao de cada colonia.
				// foreach empire makes the movement and the revolution of each colony.
				foreach (var e in IdEmpire)
				{
					// pega a lista de colonias para o imperio e.
					// gets the list of colonies of the empire e.
					Country[] empireColonies = Countries.Where(c => c.IdEmpire == e && c.IsEmpire == false).ToArray();
#if Log
					Logger.LogLine("\nempireColonies[" + e + "]: " + empireColonies.Length + "\t");
#endif
					// Assimilacao: move as colonias em direcao aos seus imperialistas.
					// Assimilation: move colonies towards its empires.

					switch (SelectedMovementType)
					{
						case MovementType.Linear:
							AssimilateColoniesLinear(e, empireColonies);
							break;
						case MovementType.Original:
							AssimilateColoniesOriginal(e, empireColonies);
							break;
						case MovementType.Refined:
							RefinedAssimilateColonies(e, empireColonies);
							break;
						case MovementType.Distortion:
							DistortionAssimilateColonies(e, empireColonies);
							break;
						case MovementType.RefinedWithDistortion:
							RefinedWithDistortionAssimilateColonies(e, empireColonies);
							break;
						case MovementType.RefinedWithOriginal:
							RefinedWithOriginalAssimilateColonies(e, empireColonies);
							break;
						case MovementType.DistortionWithOriginal:
							DistortionWithOriginalAssimilateColonies(e, empireColonies);
							break;
						case MovementType.All:
							AllAssimilateColonies(e, empireColonies);
							break;
					}

					// Revolucao:   muda a posicao e direcao de algumas colonias para evitar que se fique preso em um minimo local.
					// Revolution:  changes the position and direction of some colonies to avoid local minimuns.
					RevolveColonies(empireColonies);
				}
#if Log
				Logger.LogLine("\n======Fitness Evaluation\n");
#endif
				// Recalcula os custos.
				// Recalculate the costs.
				if (IsParallel)
				{
					tasks.Parallel.For(0, Countries.Length, i =>
					{
						Fitness.Eval(ref Countries[i]);
					});
				}
				else
				{
					for (int i = 0; i < Countries.Length; i++)
					{
						Fitness.Eval(ref Countries[i]);
					}
				}
#if Log
				Logger.LogLine("\n======Verificating if a Colony will posses an Empire\n");
#endif
				//
				// Para cada imperio verifica se existe uma colonia com custo menor que seu imperio..
				// foreach empire verifies if exists a colony with its cost lower than of its empire. .
				foreach (var e in IdEmpire)
				{
					// Pocessao de imperio: se houver uma colonia com custo menor que o do imperio, esta se tornara o novo imperio.
					// Possession of empire: if there is a colony with its cost lesser thas of its empire, this colony becomes the new empire.
					ColonyPossesEmpire(e, ref IdEmpire);
				}
#if Log
				Logger.LogLine("\n======Uniting simimlar empires\n");
#endif
				// Funde imperios similares.
				// Unite similar empires.
				// DONE-TODO: check the return statement on the end of the if(distance <= thresholdDistance) , and verify if its correct.
				UniteSimilarEmpires(ref IdEmpire);
#if Log
				Logger.LogLine("\n======Imperialistic Competition\n");
#endif
				// Inicia a competicao imperialista.
				// Imperialist Competition 
				ImperialisticCompetition(ref IdEmpire);

				//	lastDecade++;
				CurrentDecade++;
			}
			while (true);
			//while (CurrentDecade < int.MaxValue);
#if Log
			Logger.LogLine("\n\nImperialistCompetition.MainLoop ended");
			Logger.Save();
#endif
		}



		/// <summary>
		/// Inicializa todos os limites inferiores com min e superiores com max
		/// Nota:
		///		os limites nem sempre serao os mesmos para cada dimensao, isso depende do problema e
		///		deverá ser configuraddo manualmente ou via arquivo.
		/// Initializes all lower limits with min and superiors with max
		/// Note: 
		///		the limits will be not always the same for each dimension, 
		///		it depends on the problem and must be configured manually or via file.
		/// </summary>
		/// <param name="min">valor para os limites inferiores.</param>
		/// <param name="max">valor para os limites superiores.</param>
		public void InitalizeBounds(double min, double max)
		{
			MinBounds = new double[Dimensions];
			MaxBounds = new double[Dimensions];
			for (int i = 0; i < Dimensions; i++)
			{
				MinBounds[i] = min;
				MaxBounds[i] = max;
			}

		}



		/// <summary>
		/// Inicializa o espaco de busca.
		/// Initializes the space search.
		/// </summary>
		private void InitializeSpaceSearch()
		{
			SpaceSearchSize = new double[Dimensions];
			for (int i = 0; i < Dimensions; i++)
				SpaceSearchSize[i] = Math.Abs(MinBounds[i] - MaxBounds[i]);

		}



		/// <summary>
		/// Calcula a NORMA de um array.
		/// Calculates the NORM of an array.
		/// </summary>
		/// <returns>a Norma.</returns>
		/// <param name="array">Array.</param>
		private double CalculateNorm2(double[] array)
		{
			double sum = 0;
			for (int i = 0; i < array.Length; i++)
				sum += array[i] * array[i];
			return Math.Sqrt(sum);
		}



		/// <summary>
		/// Calcula a proxima taxa de revolucao.
		/// Calculates the next revolution rate.
		/// </summary>
		/// <returns> a proxima taxa de revolucao</returns>
		private double NextRevolutionRate()
		{
			RevolutionRate *= RevolutionDampRatio;
			return RevolutionRate;
		}

		//double assimilateRandomValue;
		/// <summary>
		/// Pega o proximo coeficiente de assimilacao(um valor randomico proporcional).
		/// Gets the next Assimilation coeficient( a proportional random value ).
		/// </summary>
		/// <returns>value</returns>
		private double NextAssimilateCoeficient()
		{
			return RandomRange(0, Beta);
		}



		/// <summary>
		/// Calcula o proximo coeficiente angular de assimilacao(um valor aleatorio proporcional).
		/// Funciona como um sistema que busca valores aleatorios dentro da area de uma circunferencia.
		/// Calculates the next assimilation angular coeficient(a proportional aleatory value).
		/// Works as a system thet gets an aleatory value inside a circle/arc. 
		/// </summary>
		/// <returns>an aleatory Assimilate value based on a random angle between [-45,45].</returns>
		double NextAssimilateAngleCoeficient()
		{
			//return 1 / (Math.Exp(RandomRange(-Gama, Gama)) + 1);
			return Math.Tan(RandomRange(-Gama, Gama));
		}



		/// <summary>
		/// calcula um valor aleatorio entre min e max
		/// Returns an aleatory valye between min and max.
		/// </summary>
		/// <param name="min">valor minimo</param>
		/// <param name="max">valor maximo</param>
		/// <returns>valor aleatorio entre minimo e maximo</returns>
		double RandomRange(double min, double max)
		{
			return (min + (max - min) * _rnd.NextDouble());
		}


		/// <summary>
		/// Assimila as colonias movendo-as para seus imperios.
		/// Assimilate the colonies moving its positions toward their imperialist.
		/// </summary>
		void AssimilateColoniesLinear(int idEmpire, Country[] empireColonies)
		{
			double distVector;
			double assimilateCoeficient;

			double x = 0;
			double newValue = 0;
			for (int i = 0; i < empireColonies.Length; i++)
			{
				assimilateCoeficient = NextAssimilateCoeficient();
				distVector = 0;

				for (int j = 0; j < Dimensions; j++)
				{
					distVector = (Countries[idEmpire].Attributes[j] - empireColonies[i].Attributes[j]);
					x = assimilateCoeficient;

					newValue =
						// A posicao atual.
						empireColonies[i].Attributes[j] +
						// Mais o passo direcionado.
						x * distVector;


					// clampa o novo valor calculado nas bounds do problema.
					newValue = Math.Max(Math.Min(newValue, MaxBounds[j]), MinBounds[j]);

					empireColonies[i].Attributes[j] = newValue;

				}
			}
		}


		/// <summary>
		/// Assimila as colonias movendo-as para seus imperios.
		/// Assimilate the colonies moving its positions toward their imperialist.
		/// </summary>
		private void AssimilateColoniesOriginal(int idEmpire, Country[] empireColonies)
		{
			double dist;
			double assimilateCoeficient;

			double x = 0;
			double newValue = 0;
			for (int i = 0; i < empireColonies.Length; i++)
			{
				assimilateCoeficient = NextAssimilateCoeficient();

				dist = 0;

				for (int j = 0; j < Dimensions; j++)
				{
					double theta = RandomRange(-Gama, Gama);

					//	dist = (empireColonies[i].Attributes[j] - Colonies[idEmpire].Attributes[j]);
					dist = (Countries[idEmpire].Attributes[j] - empireColonies[i].Attributes[j]);

					x = assimilateCoeficient;

					newValue =
						// A posicao atual. The current position.
						empireColonies[i].Attributes[j] +
						// Mais o passo direcional. Plus the directional step.
						x * dist + theta;

					// clampa o novo valor calculado nas bounds do problema.
					newValue = Math.Max(Math.Min(newValue, MaxBounds[j]), MinBounds[j]);

					empireColonies[i].Attributes[j] = newValue;
				}
			}
		}


		/// <summary>
		/// Refineds the assimilate colonies.
		/// </summary>
		/// <param name="idEmpire">The identifier empire.</param>
		/// <param name="empireColonies">The empire colonies.</param>
		void RefinedAssimilateColonies(int idEmpire, Country[] empireColonies)
		{
			double dist;
			double assimilateCoeficient;
			var u = .2;
			double v = ((double)CurrentDecade / (MaxDecades + 1));
			double p = P * (u - (v > u ? u : v));
			double x = 0;
			double newValue = 0;

			for (int i = 0; i < empireColonies.Length; i++)
			{
				assimilateCoeficient = NextAssimilateCoeficient();

				dist = 0;


				for (int j = 0; j < Dimensions; j++)
				{
					dist = (Countries[idEmpire].Attributes[j] - empireColonies[i].Attributes[j]);

					//x = (assimilateCoeficient * p + NextAssimilateAngleCoeficient() * (1 - p));
					x = assimilateCoeficient;
					//var p2 = dist / SpaceSearchSize[i];

					newValue =
						// A posicao atual. The current position.
						empireColonies[i].Attributes[j] +
						// Mais o passo direcional. Plus the directional step.
						x * dist +
						_rnd.NextTriangular(MinBounds[j], MaxBounds[j], dist) * ((dist / SpaceSearchSize[j]) + p);


					// clampa o novo valor calculado nas bounds do problema.
					newValue = Math.Max(Math.Min(newValue, MaxBounds[j]), MinBounds[j]);

					empireColonies[i].Attributes[j] = newValue;
				}
			}
		}



		/// <summary>
		/// Refineds the assimilate colonies.
		/// </summary>
		/// <param name="idEmpire">The identifier empire.</param>
		/// <param name="empireColonies">The empire colonies.</param>
		private void DistortionAssimilateColonies(int idEmpire, Country[] empireColonies)
		{
			double dist;
			double assimilateCoeficient;


			double[] maxDists = new double[Dimensions];

			for (int i = 0; i < empireColonies.Length; i++)
			{
				for (int j = 0; j < Dimensions; j++)
				{
					dist = Math.Abs(Countries[idEmpire].Attributes[j] - empireColonies[i].Attributes[j]);
					maxDists[j] = Math.Max(maxDists[j], dist);
				}
			}
			var u = .2;
			double v = ((double)CurrentDecade / (MaxDecades + 1));
			double p = P * (u - (v > u ? u : v));

			dist = 0;
			double x = 0;
			double newValue = 0;


			for (int i = 0; i < empireColonies.Length; i++)
			{
				assimilateCoeficient = NextAssimilateCoeficient();

				dist = 0;

				for (int j = 0; j < Dimensions; j++)
				{
					dist = ((Countries[idEmpire].Attributes[j] + _rnd.NextGaussian() * maxDists[j] * (p <= 0 ? 0 : p)) - empireColonies[i].Attributes[j]);

					x = assimilateCoeficient;

					newValue =
						// A posicao atual. The current position.
						empireColonies[i].Attributes[j] +
						// Mais o passo direcional. Plus the directional step.
						x * dist /*+ 0.05 * SpaceSearchSize[i] * _rnd.NextGaussian()*/;

					newValue = Math.Max(Math.Min(newValue, MaxBounds[j]), MinBounds[j]);

					empireColonies[i].Attributes[j] = newValue;
				}
			}
		}






		/// <summary>
		/// Refineds the assimilate colonies.
		/// </summary>
		/// <param name="idEmpire">The identifier empire.</param>
		/// <param name="empireColonies">The empire colonies.</param>
		private void RefinedWithDistortionAssimilateColonies(int idEmpire, Country[] empireColonies)
		{
			double dist;
			double realDist;
			double assimilateCoeficient;

			double[] maxDists = new double[Dimensions];

			for (int i = 0; i < empireColonies.Length; i++)
			{
				for (int j = 0; j < Dimensions; j++)
				{
					dist = Math.Abs(Countries[idEmpire].Attributes[j] - empireColonies[i].Attributes[j]);
					maxDists[j] = Math.Max(maxDists[j], dist);
				}
			}

			var u = .2;
			double v = ((double)CurrentDecade / (MaxDecades + 1));
			double p = P * (u - (v > u ? u : v));
			double x = 0;
			double newValue = 0;



			for (int i = 0; i < empireColonies.Length; i++)
			{
				assimilateCoeficient = NextAssimilateCoeficient();

				dist = 0;
				realDist = 0;

				for (int j = 0; j < Dimensions; j++)
				{

					dist = ((Countries[idEmpire].Attributes[j] + (_rnd.NextGaussian() * maxDists[j] * (p <= 0 ? 0 : p))) - empireColonies[i].Attributes[j]);
					realDist = (Countries[idEmpire].Attributes[j] - empireColonies[i].Attributes[j]);
					x = assimilateCoeficient;

					newValue =
						// A posicao atual. The current position.
						empireColonies[i].Attributes[j] +
						// Mais o passo direcional. Plus the directional step.
						x * dist
						+ _rnd.NextTriangular(MinBounds[j], MaxBounds[j], realDist) * ((realDist / SpaceSearchSize[j]) + p);

					newValue = Math.Max(Math.Min(newValue, MaxBounds[j]), MinBounds[j]);

					empireColonies[i].Attributes[j] = newValue;
				}
			}
		}



		/// <summary>
		/// Refineds the assimilate colonies.
		/// </summary>
		/// <param name="idEmpire">The identifier empire.</param>
		/// <param name="empireColonies">The empire colonies.</param>
		private void RefinedWithOriginalAssimilateColonies(int idEmpire, Country[] empireColonies)
		{
			double dist;
			double assimilateCoeficient;


			double p = .65;
			double x = 0;
			double newValue = 0;

			for (int i = 0; i < empireColonies.Length; i++)
			{
				assimilateCoeficient = NextAssimilateCoeficient();

				dist = 0;

				for (int j = 0; j < Dimensions; j++)
				{
					double theta = RandomRange(-Gama, Gama);

					dist = (Countries[idEmpire].Attributes[j] - empireColonies[i].Attributes[j]);

					x = (assimilateCoeficient * p + NextAssimilateAngleCoeficient() * (1 - p));

					newValue =
						// A posicao atual. The current position.
						empireColonies[i].Attributes[j] +
						// Mais o passo direcional. Plus the directional step.
						x * dist +
						theta
						;

					newValue = Math.Max(Math.Min(newValue, MaxBounds[j]), MinBounds[j]);

					empireColonies[i].Attributes[j] = newValue;
				}
			}
		}




		/// <summary>
		/// Refineds the assimilate colonies.
		/// </summary>
		/// <param name="idEmpire">The identifier empire.</param>
		/// <param name="empireColonies">The empire colonies.</param>
		private void DistortionWithOriginalAssimilateColonies(int idEmpire, Country[] empireColonies)
		{
			double dist;
			double assimilateCoeficient;


			double[] maxDists = new double[Dimensions];

			for (int i = 0; i < empireColonies.Length; i++)
			{
				for (int j = 0; j < Dimensions; j++)
				{
					dist = (Countries[idEmpire].Attributes[j] - empireColonies[i].Attributes[j]);
					maxDists[j] = Math.Max(maxDists[j], dist);
				}
			}


			dist = 0;
			double x = 0;
			double newValue = 0;


			for (int i = 0; i < empireColonies.Length; i++)
			{
				assimilateCoeficient = NextAssimilateCoeficient();

				dist = 0;


				for (int j = 0; j < Dimensions; j++)
				{
					double theta = RandomRange(-Gama, Gama);

					dist = ((Countries[idEmpire].Attributes[j] + _rnd.NextGaussian() * maxDists[j]) - empireColonies[i].Attributes[j]);

					x = assimilateCoeficient;

					newValue =
						// A posicao atual. The current position.
						empireColonies[i].Attributes[j] +
						// Mais o passo direcional. Plus the directional step.
						x * dist
						+ theta
						;

					// clampa o novo valor calculado nas bounds do problema.
					newValue = Math.Max(Math.Min(newValue, MaxBounds[j]), MinBounds[j]);

					empireColonies[i].Attributes[j] = newValue;
				}
			}
		}



		/// <summary>
		/// Refineds the assimilate colonies.
		/// </summary>
		/// <param name="idEmpire">The identifier empire.</param>
		/// <param name="empireColonies">The empire colonies.</param>
		private void AllAssimilateColonies(int idEmpire, Country[] empireColonies)
		{
			double dist;

			double assimilateCoeficient;

			double[] maxDists = new double[Dimensions];

			for (int i = 0; i < empireColonies.Length; i++)
			{
				for (int j = 0; j < Dimensions; j++)
				{
					dist = (Countries[idEmpire].Attributes[j] - empireColonies[i].Attributes[j]);
					maxDists[j] = Math.Max(maxDists[j], dist);
				}
			}


			double p = .65;
			double x = 0;
			double newValue = 0;


			for (int i = 0; i < empireColonies.Length; i++)
			{
				assimilateCoeficient = NextAssimilateCoeficient();

				dist = 0;

				for (int j = 0; j < Dimensions; j++)
				{
					double theta = RandomRange(-Gama, Gama);

					dist = ((Countries[idEmpire].Attributes[j] + _rnd.NextGaussian() * maxDists[j]) - empireColonies[i].Attributes[j]);


					x = (assimilateCoeficient * p + NextAssimilateAngleCoeficient() * (1 - p));

					newValue =
						// A posicao atual. The current position.
						empireColonies[i].Attributes[j] +
						// Mais o passo direcional. Plus the directional step.
						x * dist +
						theta
						;

					newValue = Math.Max(Math.Min(newValue, MaxBounds[j]), MinBounds[j]);

					empireColonies[i].Attributes[j] = newValue;
				}
			}
		}



		/// <summary>
		/// Faz uma colonia de um imperio se revoltar.
		/// Isto e similar a uma perturbacao que tenta evitar que os individuos fiquem travados em um minimo local.
		/// It makes the colonies of Empire revolt.
		/// This and similar to a disturbance trying to prevent individuals from getting caught in a local minimum.
		/// </summary>
		void RevolveColonies(Country[] empireColonies)
		{
			List<int> lint = new List<int>();
			int numOfRevolvingColonies;
			int rand;

			// Numero de colonias que revoltarao.
			// Get the number of colonies to revolve
			numOfRevolvingColonies = (int)Math.Round((RevolutionRate * empireColonies.Length)) > 0 ? 1 : 0;


			if (numOfRevolvingColonies == 0)
				return;
#if Log
			Logger.LogLine("\n\t[***]Revolving " + numOfRevolvingColonies + " Colonie(s)");
#endif

			// faz as colonias se revoltarem.
			// make aleatory colonies to revolve.
			for (int i = 0; i < numOfRevolvingColonies; i++)
			{
				rand = (int)RandomRange(0, empireColonies.Length);
				while (lint.Contains(rand))
				{
					rand = (int)RandomRange(0, empireColonies.Length);
				}
				lint.Add(rand);
				empireColonies[rand].RandomizeAttributes(_rnd, MinBounds, MaxBounds);
			}
		}


		public int ColonyPossesEmpireCounter { get; set; } = 0

;
		/// <summary>
		/// Verifica se uma colonia ira se tornar imperio e o imperio sua colonia.
		/// Finds whether the colony will become empire and the empire its colony.
		/// </summary>
		void ColonyPossesEmpire(int idCurrentEmpire, ref int[] IdEmpires)
		{
			//return;
			Country[] empireColonies = Countries.Where(c => c.IdEmpire == idCurrentEmpire && c.IsEmpire == false).ToArray();

			// se este imperio nao tem mais colonias
			// if this empire have no more colonies
			if (empireColonies.Length == 0)
			{
#if Log
				Logger.LogLine("TODO:Eliminate empire when empireColonies.Length == 0");
#endif
				var bestEmpire = Countries.Where(c => c.IsEmpire && c.IdEmpire != idCurrentEmpire).OrderBy(cc => cc.Cost).FirstOrDefault();
				var bestEmpireIndex = bestEmpire.IdEmpire;


				// juntando os imperiops
				// merging the empires

				// o pior imperio vira uma colonia do melhor.
				// the worse empite turns a colony of the better empire. 
				Countries[idCurrentEmpire].IsEmpire = false;
				Countries[idCurrentEmpire].IdEmpire = bestEmpireIndex;

				// passa todas as colonias do pior imperio para o melhor imperio.
				// pass the colonies of the worse empire to the better empire.
				Array.ForEach(Countries, d => { if (d.IdEmpire == idCurrentEmpire && d.IsEmpire == false) d.IdEmpire = bestEmpireIndex; });

				// remove o pior imperio da lista de indices de imperios 
				// removing the worse empire from the list of indexes that are empires.
				int[] newIdEmpire = new int[IdEmpires.Length - 1];
				for (int c = 0, b = 0; c < IdEmpires.Length; c++, b++)
				{
					if (IdEmpires[c] != idCurrentEmpire)
					{
						int id = IdEmpires[c];
						newIdEmpire[b] = id;
						Countries[id].IdEmpire = b;
					}
					else
						b--;
				}
				IdEmpires = newIdEmpire;
#if Log
				Logger.LogLine("o imperio[" + bestEmpireIndex + "] consumiu o imperio[" + idCurrentEmpire + "] e suas colonias.");
#endif

				return;
			}

			// ordena as colonias deste imperio para que o melhor elemento seja o primeiro.
			// Sorts the colonies of this empire for the best element is the first.
			empireColonies = empireColonies.OrderBy(o => o.Cost).ToArray();

			// se o custo da melhor colonia for melhor que o custo do imperio.
			// If this cost is lower(best) than the one of the imperialist.
			if (empireColonies[0].Cost < Countries[idCurrentEmpire].Cost)
			{
				ColonyPossesEmpireCounter++;

				// obtendo os indices do novo imperio no array de colonias e o indice do imperio no array de imperios.
				// getting the indices of the new empire in the colonies array and the empire of empires index in the array.
				int indexOfNewEmpire = Array.IndexOf(Countries, empireColonies[0]);
				int indexOfCurrentEmpireOnIdArray = Array.IndexOf(IdEmpires, idCurrentEmpire);

				// the clony becomes the empire.
				// a colonia se torna o novo imperio.
				Countries[indexOfNewEmpire].IsEmpire = true;
				Countries[indexOfNewEmpire].IdEmpire = indexOfCurrentEmpireOnIdArray;

				// o antigo imperio se torna uma colonia do novo imperio.
				// the old empire becomes a colony of the new empire.
				Countries[idCurrentEmpire].IsEmpire = false;
				Countries[idCurrentEmpire].IdEmpire = indexOfNewEmpire;

				//todas as colonias do antigo imperio sao agora do novo imperio.
				//all colonies of the former empire are now the new empire.
				Array.ForEach(Countries, d => { if (d.IdEmpire == idCurrentEmpire && d.IsEmpire == false) d.IdEmpire = indexOfNewEmpire; });

				// altera a lista de indices de imperios.
				// change the list of empire indexes.
				IdEmpires[indexOfCurrentEmpireOnIdArray] = indexOfNewEmpire;
#if Log
				Logger.LogLine("\n\t[^^^]Colony[" + indexOfNewEmpire + "] posses empire[" + idCurrentEmpire + "]");
#endif
			}
		}


		/// <summary>
		/// Unites the similar empires.
		/// </summary>
		/// <param name="IdEmpire">Identifier empire.</param>
		private void UniteSimilarEmpires(ref int[] IdEmpire)
		{
			double thresholdDistance;
			double numOfEmpires;



			// pega a distancia minima necessaria para unir imperios.
			// Get the threshold distance that is needed to unite two empires. 
			thresholdDistance = UnitingThreshold * CalculateNorm2(SpaceSearchSize);



			// pega o numero de imperios.
			// Get the number of empires 
			numOfEmpires = IdEmpire.Length;



			// Compara os imperios.
			// Compare each empire with the other ones 
			for (int i = 0; i < (numOfEmpires - 1); i++)
			{
				for (int j = i + 1; j < numOfEmpires; j++)
				{
					// calcula a distancia entre dois imperios.
					// Compute the distance between the two empires i and j 
					var distanceVector = new double[Countries[IdEmpire[i]].Attributes.Count()];


					// para cada dimensao.
					// for each dimension. 
					for (int k = 0; k < Countries[IdEmpire[i]].Attributes.Count(); k++)
					{
						// calcula a subtracao de cada dimensao,
						// calculate the subtraction of each dimension of i and j 
						distanceVector[k] = Countries[IdEmpire[i]].Attributes[k] - Countries[IdEmpire[j]].Attributes[k];
					}


					// calcula a norma, que resulta na distancia euclideana.
					// calculate the norm, that results in the final euclidian distance.
					var distance = CalculateNorm2(distanceVector);



					// se os imperios estao proximos.
					// If the empires are too close 
					if (distance <= thresholdDistance)
					{
						// verifica qual eh o melhor e o pior imperio.
						// Get the best and worst empires of the two 
						int betterEmpireInd;
						int worseEmpireInd;
						if (Countries[IdEmpire[i]].Cost < Countries[IdEmpire[j]].Cost)
						{
							betterEmpireInd = IdEmpire[i];
							worseEmpireInd = IdEmpire[j];
						}
						else
						{
							betterEmpireInd = IdEmpire[j];
							worseEmpireInd = IdEmpire[i];
						}



						// juntando os imperiops
						// merging the empires
						if (betterEmpireInd != worseEmpireInd)
						{
							// o pior imperio vira uma colonia do melhor.
							// the worse empite turns a colony of the better empire. 
							Countries[worseEmpireInd].IsEmpire = false;
							Countries[worseEmpireInd].IdEmpire = betterEmpireInd;

							// passa todas as colonias do pior imperio para o melhor imperio.
							// pass the colonies of the worse empire to the better empire.
							Array.ForEach(Countries, d => { if (d.IdEmpire == worseEmpireInd && d.IsEmpire == false) d.IdEmpire = betterEmpireInd; });

							// remove o pior imperio da lista de indices de imperios 
							// removing the worse empire from the list of indexes that are empires.
							int[] newIdEmpire = new int[IdEmpire.Length - 1];
							for (int c = 0, b = 0; c < IdEmpire.Length; c++, b++)
							{
								if (IdEmpire[c] != worseEmpireInd)
								{
									int id = IdEmpire[c];
									newIdEmpire[b] = id;
									Countries[id].IdEmpire = b;
									//Array.ForEach(Colonies, d => { if (d.IdEmpire == id && d.IsEmpire == true) d.IdEmpire = b; });
								}
								else
									b--;
							}
							IdEmpire = newIdEmpire;
#if Log
							Logger.LogLine("o imperio[" + betterEmpireInd + "] consumiu o imperio[" + worseEmpireInd + "] e suas colonias.");
#endif
						}



						// retorna pois apenas uma unificacao eh feita por decada.
						// return becaus only one unification will be made at each decade.
						return;
					}
				}
			}
		}



		/// <summary>
		/// Execute the Imperialistic competition.
		/// Aqui uma colonia aleatoria do pior imperio eh passada para o melhor imperio.
		/// </summary>
		private void ImperialisticCompetition(ref int[] IdEmpire)
		{
			//
			// Verifica se ocorrera a competicao imperialista de acordo com o ImperialistCompetitionFactor.
			// Verifies if there will be the imperialist competition according to the ImperialistCompetitionFactor
			if (_rnd.NextDouble() > ImperialistCompetitionFactor)
				return;



			// Caso haja apenas um imperio não existe competicao imperialista.
			// In the case of the existence of only one empire there will be no competition. 
			if (Countries.Length <= 1)
				return;



			// Numero de imperios.
			// Empire count.
			int empireCount = IdEmpire.Length;



			// Como uma variavel 'ref' nao pode ser colocado dentro de expressoes lambda,
			// metodos anonimos ou query, fazemos uma copia dele. 
			// Passamos a referencia do array caso ele venha a ter 
			// seu tamanho alterado nesta funcao.
			// As 'ref' variable cannot be inserted inside a lambda expression,
			// anonimous methods or query, we made a clone of it.
			// This variable is passed as reference because it size 
			// can be changed inside this method.
			int[] IdEmpireTemp = (int[])IdEmpire.Clone();



			// vetor contendo o poder do imperio.
			// array to contain the powers of the empires.
			double[] power = new double[empireCount];



			// calculando o poder do imperio baseando-se no seu [custo] mais na [proporcao da media dos custos de suas colonias].
			// calculating the power of the empires based on its [cost] plus a [proportion of mean of its colonies costs].
			for (int i = 0; i < IdEmpire.Length; i++)
				power[i] = Countries[IdEmpireTemp[i]].Cost + Countries.Where(c => c.IdEmpire == IdEmpireTemp[i]).ToArray().Average(s => s.Cost) * Epsilon;




			// Pega a 
			// id real (dentre todas as colonias) em 'weakestEmpire' e a 
			// id imperial(entro os imperios, mais especificamente no vetor IdEmpire) em 'weakestEmpireId'
			// do imperio mais fraco (aquele que tem o maior custo).
			//
			// Gets the 
			// real id (over all colonies) and set it on 'weakestEmpire' and the
			// imperial id(over all empires, more especificaly at the IdEmpire array) and set it on 'weakestEmpireId'
			// of the weakest empire (that one that have the higher cost).
			int weakestEmpire = Array.IndexOf(power, power.Max());
			int weakestEmpireId = IdEmpireTemp[weakestEmpire];



			// Calcula o valor normalizado dos custos totais(poder) de cada imperio.
			// Calculates the normalized value of the total costs(powers) of each empire.
			double sumOfAllPowers = 0;
			for (int i = 0; i < empireCount; i++)
			{
				//N.T.Cn = T.Cn     - MAXi{T.Ci}
				power[i] = power[i] - power[weakestEmpire];
				sumOfAllPowers += power[i];
			}




			// Calcula a probabilidade de possessao de cada imperio
			// Calculates the possession probability of each empire.
			// Total Costs           = [ 1, 2, 3 ]
			// normalized power      = [ 0, 1 ,2 ]
			// possessionProbability = [ 0, 1/6, 2/6 ]
			var possessionProbability = new double[empireCount];
			for (int i = 0; i < empireCount; i++)
				possessionProbability[i] = power[i] / sumOfAllPowers;



			// Seleciona um imperio de acordo com as suas probabilidades.(metodo da roleta)
			// Select an empire according to their probabilities.(roulete method)
			int selectedEmpireId = IdEmpireTemp[SelectAnEmpire(possessionProbability, empireCount)];


			// Gera um inteiro aleatorio, e seleciona a colonia do pior imperio.
			// Generate a random integer, and select a colony of the weakes empire 
			int numOfColoniesOfWeakestEmpire = Countries.Where(c => c.IdEmpire == weakestEmpireId && c.IsEmpire == false).Count();


			// passa a colonia selecionada para o imperio selecionado e remove-a do pior imperio.
			// pass the selected colony to the selected empire and remove it from the weakes empire. 
			if (numOfColoniesOfWeakestEmpire > 0)
			{
				Countries
					.Where(c => c.IdEmpire == weakestEmpireId && c.IsEmpire == false)
					.ElementAt(_rnd.Next(0, numOfColoniesOfWeakestEmpire))
						.IdEmpire = selectedEmpireId;

				// recalcula o numero de colonias do pior imperio.
				// recalculate the number of colonies of the weakest empire 
				numOfColoniesOfWeakestEmpire = Countries.Where(c => c.IdEmpire == weakestEmpireId && c.IsEmpire == false).Count();
			}



			// Se um impreio nao tem mais colonias, entao ele ira colapsar, sendo inserido como colonia do imperio selecionado.
			// If it has not more than 0 colony, then make it disapear/collapse It is then absorbed by the selected empire
			if (numOfColoniesOfWeakestEmpire <= 0)
			{
				Countries[weakestEmpireId].IsEmpire = false;
				Countries[weakestEmpireId].IdEmpire = selectedEmpireId;

				int[] newIdEmpire = new int[IdEmpire.Length - 1];
				for (int c = 0, b = 0; c < IdEmpire.Length; c++, b++)
				{
					if (IdEmpire[c] != weakestEmpireId)
					{
						int id = IdEmpire[c];
						newIdEmpire[b] = id;
						Countries[id].IdEmpire = b;
					}
					else
						b--;
				}
				IdEmpire = newIdEmpire;
			}
		}



		/// <summary>
		/// Seleciona um imperio de acordo com suas probabilidades.
		/// Selects an empire according to their probabilities
		/// </summary>
		/// <returns>The empire selected index.</returns>
		private int SelectAnEmpire(double[] probability, int empireCount)
		{
			// cria um vetor de numeros aleatorios.
			// Create a vector of random numbers 
			var randVector = new double[probability.Length];
			for (int i = 0; i < empireCount; i++)
				randVector[i] = _rnd.NextDouble();



			// subtrai de cada elemento deste vetor o valor correspondente do vetor de probabilidade.
			// Substract to each element of this vector the corresponding value of the probability vector.
			var dVector = new double[probability.Length];
			for (int i = 0; i < probability.Length; i++)
				dVector[i] = Math.Abs(probability[i]) < FLOAT_ZERO ? double.MinValue : probability[i] - randVector[i];



			// retornan o indie do valor maximo do vetor.
			// Return the index of the maximum value of the vector 
			return Array.IndexOf(dVector, dVector.Max());
		}


	}

}

