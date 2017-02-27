using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA
{
	public class Logger
	{
		public string Path { get; set; }

		public bool LogOnConsole { get; set; }

		private StringBuilder b = new StringBuilder();

		public void LogLine(string str)
		{
			if (LogOnConsole)
				Console.WriteLine(str);
			b.AppendLine(str);
		}


		public void Overrite(string str)
		{
			if (LogOnConsole)
				Console.Write("\r{0}                        ", str);
			b.Append(string.Format("\r{0}                        ", str));
		}


		public void Save()
		{
			try
			{
				File.AppendAllText(Path, b.ToString());
			}
			catch
			{
			}
		}

		public void Clear()
		{
			b.Clear();
			Console.Clear();
		}
	}
}
