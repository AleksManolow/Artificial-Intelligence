using System;
using System.Collections.Generic;
using System.Linq;

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

	public class Program
	{
		static Random random = new Random();
		static int populationSize = 1000; 
		static double mutationRate = 0.01; 

		public static void Main()
		{
			string[] firstLine = Console.ReadLine().Split();
			int M = int.Parse(firstLine[0]);
			int N = int.Parse(firstLine[1]);

			List<Item> items = ReadInput(N);

			List<bool[]> population = GenerateInitialPopulation(items.Count);

			for (int generation = 1; generation <= 10; generation++)
			{
				List<(bool[], int)> evaluatedPopulation = population
					.Select(individual => (individual, Fitness(individual, items, M)))
					.OrderByDescending(individual => individual.Item2)
					.ToList();

				Console.WriteLine(evaluatedPopulation[0].Item2);

				List<bool[]> newPopulation = new List<bool[]>();
				for (int i = 0; i < populationSize / 2; i++)
				{
					var parent1 = SelectParent(evaluatedPopulation);
					var parent2 = SelectParent(evaluatedPopulation);
					var (child1, child2) = Crossover(parent1, parent2);
					newPopulation.Add(Mutate(child1));
					newPopulation.Add(Mutate(child2));
				}

				population = newPopulation;
			}
		}

		public class Item
		{
			public int Weight { get; set; }
			public int Value { get; set; }

			public Item(int weight, int value)
			{
				Weight = weight;
				Value = value;
			}
		}

		static int Fitness(bool[] individual, List<Item> items, int maxWeight)
		{
			int totalWeight = 0;
			int totalValue = 0;
			for (int i = 0; i < individual.Length; i++)
			{
				if (individual[i])
				{
					totalWeight += items[i].Weight;
					totalValue += items[i].Value;
				}
			}
			return totalWeight <= maxWeight ? totalValue : 0;
		}

		static List<bool[]> GenerateInitialPopulation(int geneLength)
		{
			List<bool[]> population = new List<bool[]>();
			for (int i = 0; i < populationSize; i++)
			{
				bool[] individual = new bool[geneLength];
				for (int j = 0; j < geneLength; j++)
				{
					individual[j] = random.NextDouble() < 0.5;
				}
				population.Add(individual);
			}
			return population;
		}

		static bool[] SelectParent(List<(bool[], int)> evaluatedPopulation)
		{
			int totalFitness = evaluatedPopulation.Sum(x => x.Item2);
			int randomPoint = random.Next(totalFitness);
			int currentSum = 0;
			foreach (var individual in evaluatedPopulation)
			{
				currentSum += individual.Item2;
				if (currentSum >= randomPoint)
					return individual.Item1;
			}
			return evaluatedPopulation[0].Item1;
		}

		static (bool[], bool[]) Crossover(bool[] parent1, bool[] parent2)
		{
			int length = parent1.Length;
			int crossoverPoint = random.Next(1, length - 1);
			bool[] child1 = new bool[length];
			bool[] child2 = new bool[length];
			for (int i = 0; i < length; i++)
			{
				if (i < crossoverPoint)
				{
					child1[i] = parent1[i];
					child2[i] = parent2[i];
				}
				else
				{
					child1[i] = parent2[i];
					child2[i] = parent1[i];
				}
			}
			return (child1, child2);
		}

		static bool[] Mutate(bool[] individual)
		{
			for (int i = 0; i < individual.Length; i++)
			{
				if (random.NextDouble() < mutationRate)
					individual[i] = !individual[i];
			}
			return individual;
		}

		public static List<Item> ReadInput(int itemCount)
		{
			List<Item> items = new List<Item>();
			for (int i = 0; i < itemCount; i++)
			{
				string[] itemData = Console.ReadLine().Split();
				int weight = int.Parse(itemData[0]);
				int value = int.Parse(itemData[1]);
				items.Add(new Item(weight, value));
			}
			return items;
		}
	}
}


