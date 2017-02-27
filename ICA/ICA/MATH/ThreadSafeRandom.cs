using System;
using System.Collections;
using System.Collections.Generic;

namespace ICA
{


	/// <summary>
	/// Versão segura a threads da classe  <see cref="Random"/>.
	/// Thread safe version of the <see cref="Random"/> class.
	/// </summary>
	/// 
	/// <remarks><para>A classe extende <see cref="Random"/> e sobrescreve seus metodos de 
	/// geracao de numeros aleatorios provendo seguranca para a thread que chama o
	/// metodo, guardando a chamada da classe base com um lock. Veja a documentacao 
	/// de <see cref="Random"/> para mais informacoes sobre esta classe base.
	/// 
	/// </para><para>The class inherits the <see cref="Random"/> and overrides
	/// its random numbers generation methods providing thread safety by guarding call
	/// to the base class with a lock. See documentation to <see cref="Random"/> for
	/// additional information about the base class.</para></remarks>
	/// 
	public sealed class ThreadSafeRandom : Random
	{
		/// <summary>
		/// o objeto para sincronia.
		/// the sync object
		/// </summary>
		private object sync = new object();



		/// <summary>
		/// Initializes a new instance of the <see cref="ThreadSafeRandom"/> class.
		/// Inicializa uma nova instancia da classe <see cref="ThreadSafeRandom"/>
		/// </summary>
		/// 
		/// <remarks>See <see cref="Random.Next()"/> for more information.</remarks>
		/// <remarks>See <see cref="Random.Next()"/> Veja para mais informacoes.</remarks>
		public ThreadSafeRandom()
			: base()
		{
		}



		/// <summary>
		/// Initializes a new instance of the <see cref="ThreadSafeRandom"/> class.
		/// Inicializa uma nova instancia da classe <see cref="ThreadSafeRandom"/>
		/// </summary>
		/// 
		/// <remarks>A number used to calculate a starting value for the pseudo-random number sequence.
		/// If a negative number is specified, the absolute value of the number is used.</remarks>
		/// 
		/// <remarks>See <see cref="Random.Next()"/> for more information.</remarks>
		/// 
		public ThreadSafeRandom(int seed)
			: base(seed)
		{
		}



		/// <summary>
		/// Returns a nonnegative random number.
		/// </summary>
		/// 
		/// <returns>Returns a 32-bit signed integer greater than or equal to zero and less than
		/// <see cref="int.MaxValue"/>.</returns>
		/// 
		/// <remarks>See <see cref="Random.Next()"/> for more information.</remarks>
		/// 
		public override int Next()
		{
			lock (sync)
				return base.Next();
		}



		/// <summary>
		/// Returns a nonnegative random number less than the specified maximum.
		/// </summary>
		/// 
		/// <param name="maxValue">The exclusive upper bound of the random number to be generated.
		/// <paramref name="maxValue"/> must be greater than or equal to zero.</param>
		/// 
		/// <returns>Returns a 32-bit signed integer greater than or equal to zero, and less than <paramref name="maxValue"/>;
		/// that is, the range of return values ordinarily includes zero but not <paramref name="maxValue"/>.</returns>
		/// 
		/// <remarks>See <see cref="Random.Next(int)"/> for more information.</remarks>
		/// 
		public override int Next(int maxValue)
		{
			lock (sync)
				return base.Next(maxValue);
		}



		/// <summary>
		/// Returns a random number within a specified range.
		/// </summary>
		/// 
		/// <param name="minValue">The inclusive lower bound of the random number returned.</param>
		/// <param name="maxValue">The exclusive upper bound of the random number returned.
		/// <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
		/// 
		/// <returns>Returns a 32-bit signed integer greater than or equal to <paramref name="minValue"/> and less
		/// than <paramref name="maxValue"/>; that is, the range of return values includes
		/// <paramref name="minValue"/> but not <paramref name="maxValue"/>.</returns>
		/// 
		/// <remarks>See <see cref="Random.Next(int,int)"/> for more information.</remarks>
		///
		public override int Next(int minValue, int maxValue)
		{
			lock (sync)
				return base.Next(minValue, maxValue);
		}



		/// <summary>
		/// Fills the elements of a specified array of bytes with random numbers.
		/// </summary>
		/// 
		/// <param name="buffer">An array of bytes to contain random numbers.</param>
		/// 
		/// <remarks>See <see cref="Random.NextBytes(byte[])"/> for more information.</remarks>
		///
		public override void NextBytes(byte[] buffer)
		{
			lock (sync)
				base.NextBytes(buffer);
		}



		/// <summary>
		/// Returns a random number between 0.0 and 1.0.
		/// </summary>
		/// 
		/// <returns>Returns a double-precision floating point number greater than or equal to 0.0, and less than 1.0.</returns>
		/// 
		/// <remarks>See <see cref="Random.NextDouble()"/> for more information.</remarks>
		///
		public override double NextDouble()
		{
			lock (sync)
				return base.NextDouble();
		}



		/// <summary>
		///   Generates normally distributed numbers. Each operation makes two Gaussians for the price of one, and apparently they can be cached or something for better performance, but who cares.
		/// </summary>
		/// <param name = "mu">Mean of the distribution</param>
		/// <param name = "sigma">Standard deviation. sigma = 0.269 varia o resultado aproximadamente entre mu+0 e mu+1</param>
		/// <returns></returns>
		public double NextGaussian(double mu = 0, double sigma = 0.269)
		{
			var u1 = NextDouble();
			var u2 = NextDouble();

			var rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) *
								Math.Sin(2.0 * Math.PI * u2);

			var rand_normal = mu + sigma * rand_std_normal;

			lock (sync)
				return rand_normal;
		}



		/// <summary>
		///   Generates values from a triangular distribution.
		/// </summary>
		/// <remarks>
		/// See http://en.wikipedia.org/wiki/Triangular_distribution for a description of the triangular probability distribution and the algorithm for generating one.
		/// </remarks>
		/// <param name = "a">Minimum</param>
		/// <param name = "b">Maximum</param>
		/// <param name = "c">Mode (most frequent value)</param>
		/// <returns></returns>
		public double NextTriangular(double a, double b, double c)
		{
			var u = NextDouble();

			lock (sync)
				return u < (c - a) / (b - a)
						   ? a + Math.Sqrt(u * (b - a) * (c - a))
						   : b - Math.Sqrt((1 - u) * (b - a) * (b - c));
		}



		/// <summary>
		///   Equally likely to return true or false. Uses <see cref="Random.Next()"/>.
		/// </summary>
		/// <returns></returns>
		public bool NextBoolean()
		{
			lock (sync)
				return Next(2) > 0;
		}



		/// <summary>
		///   Shuffles a list in O(n) time by using the Fisher-Yates/Knuth algorithm.
		/// </summary>
		/// <param name = "list"></param>
		public void Shuffle(IList list)
		{
			for (var i = 0; i < list.Count; i++)
			{
				var j = Next(0, i + 1);

				var temp = list[j];
				list[j] = list[i];
				list[i] = temp;
			}
		}



		/// <summary>
		/// Returns n unique random numbers in the range [1, n], inclusive. 
		/// This is equivalent to getting the first n numbers of some random permutation of the sequential numbers from 1 to max. 
		/// Runs in O(k^2) time.
		/// </summary>
		/// <param name="n">Maximum number possible.</param>
		/// <param name="k">How many numbers to return.</param>
		/// <returns></returns>
		public int[] Permutation(int n, int k)
		{
			var result = new List<int>();
			var sorted = new SortedSet<int>();

			for (var i = 0; i < k; i++)
			{
				var r = Next(1, n + 1 - i);

				foreach (var q in sorted)
					if (r >= q)
						r++;

				result.Add(r);
				sorted.Add(r);
			}

			return result.ToArray();
		}
	}

}
