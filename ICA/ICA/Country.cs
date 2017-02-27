using System;
using System.Text;
using System.IO;

namespace ICA
{
	/// <summary>
	/// The country is like the individual on a GA.
	/// </summary>
	public class Country
	{
		/// <summary>
		/// Gets the atributes of this country
		/// </summary>
		public double[] Attributes { get; set; }



		/// <summary>
		/// Return the calculated cost.
		/// </summary>
		public double Cost { get; set; }



		/// <summary>
		/// 
		/// The ID of the empire it is (if IsEmpire==true) or
		/// The Id of empire it is colony of (IsEmpire==false)
		/// 
		/// Número do imperio que o país pertence. 
		/// Se este pais for um imperio este valor tera o 
		/// valor do indice do vetor de imperios ordenados 
		/// por custos presnete na classe ImperialistCompetition.
		/// </summary>
		public int IdEmpire { get; set; }



		/// <summary>
		/// If this countryis empire.
		/// </summary>
		public bool IsEmpire { get; set; }



		/// <summary>
		/// Construtor utilizado para clonagem de paises.
		/// </summary>
		/// <param name="c"></param>
		public Country(Country c)
		{
			// copiando o pais:
			// atributos.
			this.Attributes = new double[c.Attributes.Length];
			for (int i = 0; i < c.Attributes.Length; i++)
				this.Attributes[i] = c.Attributes[i];
			// custo.
			this.Cost = c.Cost;
			// id do imperio
			this.IdEmpire = c.IdEmpire;
			// eh imperio.
			this.IsEmpire = c.IsEmpire;

		}



		/// <summary>
		/// the void constructor as private makes this uninstantiable without a parameter.
		/// </summary>
		public Country(Random rnd, int numAttributes, double[] minBounds, double[] maxBounds)
		{
			this.Cost = double.MinValue;
			this.IdEmpire = 0;
			this.IsEmpire = false;
			this.Attributes = new double[numAttributes];
			//Rnd= rnd;
#pragma warning disable RECS0021 // Warns about calls to virtual member functions occuring in the constructor
			this.RandomizeAttributes(rnd, minBounds, maxBounds);
#pragma warning restore RECS0021 // Warns about calls to virtual member functions occuring in the constructor
			//this.AttributeChanged = true;
		}



		/// <summary>
		/// Randomize the country attributtes.
		/// </summary>
		public virtual void RandomizeAttributes(Random rnd, double[] minBounds, double[] maxBounds)
		{
			for (int i = 0; i < Attributes.Length; i++)
				Attributes[i] = (double)(minBounds[i] + (maxBounds[i] - minBounds[i]) * rnd.NextDouble());
		}



		public virtual void ProcessChangedAttributes(Random rnd, double[] MinBounds, double[] MaxBounds)
		{ }



		/// <summary>
		/// Return a description of object value.
		/// </summary>
		/// <returns>string</returns>
		public override string ToString()
		{
			var a = new StringBuilder();

			a.Append("\n" + (IsEmpire ? "Empire: " : "Colony of: ") + IdEmpire + "  \n");
			a.Append("\nCost: " + Cost + "\n");
			a.Append("attributes[");
			for (int i = 0; i < Attributes.Length - 1; i++)
			{
				a.Append(String.Format("{0:0.0000}", Attributes[i]) + ", ");
			}
			a.Append(String.Format("{0:0.0000}", Attributes[Attributes.Length - 1]) + "] ");
			return a.ToString();
		}




		public virtual Country Clone()
		{
			return new Country(this);
		}



		public virtual void Save(string filename, string path)
		{
			File.WriteAllText(Path.Combine(path, filename), this.ToString());
		}
	}
}
