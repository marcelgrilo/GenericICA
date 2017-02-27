using System;

namespace ICA
{

	/// <summary>
	/// The Fitness interface.
	/// </summary>
	public interface IFitness
	{


		/// <summary>
		/// Eval the specified element.
		/// </summary>
		/// <returns>The eval.</returns>
		/// <param name="element">Element.</param>
		void Eval(ref Country element);


		/// <summary>
		/// Gets or sets the dimensions.
		/// </summary>
		/// <value>The dimensions.</value>
		int Dimensions { get; set; }


		/// <summary>
		/// Generates the countries.
		/// </summary>
		/// <returns>The countries.</returns>
		/// <param name="nPopulation">N population.</param>
		/// <param name="rnd">Random.</param>
		/// <param name="minBounds">Minimum bounds.</param>
		/// <param name="maxBounds">Max bounds.</param>
		Country[] GenerateCountries(int nPopulation, Random rnd, double[] minBounds, double[] maxBounds);


	}
}
