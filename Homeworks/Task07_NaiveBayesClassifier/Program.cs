using System;
using System.Collections.Generic;
using System.Linq;

namespace NaiveBayesClassifier
{
	class Program
	{
		static void Main(string[] args)
		{
			int counter = 0;
			string line;
			var arr = new string[435][];

			// Read the file and display it line by line.  
			System.IO.StreamReader file =
				new System.IO.StreamReader(@"D:\programing\Artificial-Intelligence\Homeworks\Task07_NaiveBayesClassifier\house-votes-84.data");
			while ((line = file.ReadLine()) != null)
			{
				arr[counter] = line.Split(',');
				counter++;
			}
			file.Close();

			var model = new PoliticRecognizerModel(arr);
			model.TrainAndTest();
		}
	}
	class PoliticRecognizerModel
	{
		private int votesNum = 16;

		private string[][] arr;

		private int[][] precomputeValuesRepublicant;
		private double republicantCount;

		private int[][] precomputeValuesDemocrat;
		private double democratCount;

		public PoliticRecognizerModel(string[][] data)
		{
			this.arr = data;
			this.InitializeData();
		}

		private void InitializeData()
		{
			this.democratCount = 0;
			this.republicantCount = 0;
			this.precomputeValuesRepublicant = new int[this.votesNum][];
			this.precomputeValuesDemocrat = new int[this.votesNum][];
			for (int i = 0; i < this.votesNum; i++)
			{
				this.precomputeValuesRepublicant[i] = new int[3];
				this.precomputeValuesDemocrat[i] = new int[3];
			}
		}

		private void TrainModel(int startIndex, int endIndex)
		{
			for (int i = 0; i < arr.Length; i++)
			{
				if (i == startIndex)
				{
					i = endIndex;
				}

				var item = arr[i];
				if (item[0] == "democrat")
				{
					this.democratCount++;
					this.ComputeSingleValue(precomputeValuesDemocrat, item);
				}
				else
				{
					this.republicantCount++;
					this.ComputeSingleValue(precomputeValuesRepublicant, item);
				}


			}
		}

		private void ComputeSingleValue(int[][] precomputeValues, string[] votes)
		{
			for (int i = 0; i < this.votesNum; i++)
			{
				switch (votes[i + 1])
				{
					case "y":
						precomputeValues[i][0]++;
						break;
					case "n":
						precomputeValues[i][1]++;
						break;
					case "?":
						precomputeValues[i][2]++;
						break;
					default:
						throw new Exception();
				}
			}
		}

		private int TestModel(int startIndex, int endIndex)
		{
			double accuracy = 0;
			for (int i = startIndex; i < endIndex; i++)
			{
				double republicant = Math.Log(democratCount / 387);
				double democrat = Math.Log(republicantCount / 387);
				for (int j = 1; j <= this.votesNum; j++)
				{
					switch (arr[i][j])
					{
						case "y":
							democrat += Math.Log(ValueOrDefault(precomputeValuesDemocrat[j - 1][0]));
							republicant += Math.Log(ValueOrDefault(precomputeValuesRepublicant[j - 1][0], false));
							break;
						case "n":
							democrat += Math.Log(ValueOrDefault(precomputeValuesDemocrat[j - 1][1]));
							republicant += Math.Log(ValueOrDefault(precomputeValuesRepublicant[j - 1][1], false));

							break;
						case "?":
							democrat += Math.Log(ValueOrDefault(precomputeValuesDemocrat[j - 1][2]));
							republicant += Math.Log(ValueOrDefault(precomputeValuesRepublicant[j - 1][2], false));
							break;

						default:
							throw new Exception();
					}
				}
				//decimal d = Decimal.Parse(Math.Round(democrat, 12).ToString(), System.Globalization.NumberStyles.Float);
				//decimal r = Decimal.Parse(Math.Round(republicant, 12).ToString(), System.Globalization.NumberStyles.Float);
				//Console.WriteLine("Democrat   : {0}", d);
				//Console.WriteLine("Republicant: {0}", r);
				//Console.WriteLine("Actually {0}\n", arr[i][0]);

				if (democrat > republicant && arr[i][0] == "democrat") accuracy++;
				if (republicant > democrat && arr[i][0] == "republican") accuracy++;

			}

			Console.WriteLine("Accurancy: {0}", (accuracy / 43).ToString("#0.##%"));
			return (int)accuracy;
		}

		private double ValueOrDefault(int value, bool democate = true)
		{
			if (democate)
			{
				return (value == 0 ? 1 : value) / (democratCount + 3);
			}
			else
			{
				return (value == 0 ? 1 : value) / (republicantCount + 3);
			}
		}

		public void TrainAndTest()
		{
			double accuracySum = 0;
			this.Shuffle();
			for (int i = 0; i < 10; i++)
			{
				this.InitializeData();
				this.TrainModel(i * 43, i * 43 + 43);

				accuracySum += this.TestModel(i * 43, i * 43 + 43);
			}

			Console.WriteLine("Average accurancy: {0}", (accuracySum / 430).ToString("#0.##%"));
		}

		private void Shuffle()
		{
			Random rnd = new Random();
			this.arr = arr.OrderBy(x => rnd.Next()).ToArray();
		}

	}
}
