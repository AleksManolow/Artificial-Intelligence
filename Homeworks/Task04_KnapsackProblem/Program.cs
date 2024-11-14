using System.Diagnostics;

namespace Task04_KnapsackProblem
{
	public class Item
	{
		public int Weight { get; }
		public int Value { get; }

		public Item(int weight, int value)
		{
			Weight = weight;
			Value = value;
		}
	}

	public class GeneticAlgKnapsack
	{
		private Random Random { get; set; }
		private int PopulationSize { get; set; }
		private double MutationRate { get; set; }
		private int MaxWeightKnapsack { get; set; }
		private int NumberOfItems { get; set; }
		private List<Item> Items { get; set; }

		public GeneticAlgKnapsack()
        {
            Random = new Random();
			PopulationSize = 1000;
			MutationRate = 0.01;
			MaxWeightKnapsack = 0;
			NumberOfItems = 0;
			Items = new List<Item>();
		}

		public void ReadInput()
		{
			string[] firstLine = Console.ReadLine().Split();
			MaxWeightKnapsack = int.Parse(firstLine[0]);
			NumberOfItems = int.Parse(firstLine[1]);

			for (int i = 0; i < NumberOfItems; i++)
			{
				string[] itemData = Console.ReadLine().Split();
				int weight = int.Parse(itemData[0]);
				int value = int.Parse(itemData[1]);
				Items.Add(new Item(weight, value));
			}

			Items.OrderByDescending(i => i.Value).OrderBy(i => i.Weight);
		}

		public void Solve()
		{
			List<bool[]> population = GenerateInitialPopulation();

			for (int i = 0; i < 10; i++)
			{
				population = population
					.Select(individual => (individual, Fitness(individual)))
					.OrderByDescending(individual => individual.Item2)
					.Select(d => d.individual)
					.Take(PopulationSize)
					.ToList();

				Console.WriteLine(population.Select(individual => Fitness(individual)).Max());

				List<bool[]> populationParents = population.Take(2).ToList();

				List<bool[]> populationChildren = Reproduction(populationParents);

				populationChildren = Mutate(populationChildren);

				population.AddRange(populationChildren);
			}
		}

		private List<bool[]>  GenerateInitialPopulation()
		{
			List<bool[]> population = new List<bool[]>();
			for (int i = 0; i < PopulationSize; i++)
			{
				int sumWeight = 0;
				bool[] individual = new bool[NumberOfItems];
				for (int j = 0; j < NumberOfItems; j++)
				{
					bool randomGetItem = Random.NextDouble() < 0.5;
					if (randomGetItem && (sumWeight + Items[j].Weight <= MaxWeightKnapsack))
					{
						individual[j] = randomGetItem;
						sumWeight += Items[j].Weight;
					}
					else
					{
						individual[j] = false;
					}
				}
				population.Add(individual);
			}
			return population;
		}
		private int Fitness(bool[] individual)
		{
			int totalWeight = 0;
			int totalValue = 0;
			for (int i = 0; i < individual.Length; i++)
			{
				if (individual[i])
				{
					totalWeight += Items[i].Weight;
					totalValue += Items[i].Value;
				}
			}
			return totalWeight <= MaxWeightKnapsack ? totalValue : 0;
		}
		private List<bool[]> SelectParents(List<bool[]> population)
		{
			List<bool[]> parents = new List<bool[]>();	
			foreach (bool[] individual in population)
			{
                if (Random.NextDouble() < 0.5)
					parents.Add(individual);

				if (parents.Count() == 2)
					return parents;
			}
			return parents.Take(2).ToList();
		}
		private List<bool[]> Reproduction(List<bool[]> populationParents)
		{
			List<bool[]> populationChildren = new List<bool[]>();
			bool[] parent1 = populationParents[0];
			bool[] parent2 = populationParents[1];
			for (int childCount = 0; childCount < PopulationSize / 2; childCount++)
			{
				int length = NumberOfItems;
				bool[] child = new bool[length];
				int sumWeight = 0;
				for (int i = 0; i < length; i++)
				{
					if (Random.NextDouble() < 0.5)
						child[i] = parent1[i];
					else
						child[i] = parent2[i];

					if(child[i])
					{
						if (Items[i].Weight + sumWeight <= MaxWeightKnapsack)
							sumWeight += Items[i].Weight;
						else
							child[i] = false;
					}
				}
				populationChildren.Add(child);
			}
			
			return populationChildren;
		}

		private List<bool[]> Mutate(List<bool[]> populationChildren)
		{
			for(int j = 0; j < populationChildren.Count(); j++) 
			{
				for (int i = 0; i < NumberOfItems; i++)
				{
					if (Random.NextDouble() < MutationRate)
						populationChildren[j][i] = !populationChildren[j][i];
				}
			}
			return populationChildren;
		}
	}

	public class Program
	{
		public static void Main()
		{
			GeneticAlgKnapsack geneticAlgKnapsack = new GeneticAlgKnapsack();
			geneticAlgKnapsack.ReadInput();

			Stopwatch stopwatch = Stopwatch.StartNew();
			geneticAlgKnapsack.Solve();
			stopwatch.Stop();

			Console.WriteLine($"{stopwatch.Elapsed.TotalSeconds:F2}");
		}
	}
}


