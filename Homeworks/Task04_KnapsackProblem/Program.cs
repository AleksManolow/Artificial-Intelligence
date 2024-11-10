/*using System;
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
		static int maxGenerations = 10; // Максимален брой генерации
		static int populationSize = 1	00; // Размер на популацията
		static double mutationRate = 0.01; // Вероятност за мутация

		public static void Main()
		{
			string[] firstLine = Console.ReadLine().Split();
			int M = int.Parse(firstLine[0]);
			int N = int.Parse(firstLine[1]);

			List<Item> items = ReadInput(N);


			// Генериране на начална популация
			List<bool[]> population = GenerateInitialPopulation(items.Count);

			// Започваме с първото поколение
			for (int generation = 1; generation <= maxGenerations; generation++)
			{
				// Оценка на популацията
				List<(bool[], int)> evaluatedPopulation = population
					.Select(individual => (individual, Fitness(individual, items, M)))
					.OrderByDescending(individual => individual.Item2)
					.ToList();

				// Показване на най-добрата конфигурация на избрани генерации
				if (generation == 1 || generation == maxGenerations || generation % (maxGenerations / 10) == 0)
				{
					Console.WriteLine($"Generation {generation}: Best Value = {evaluatedPopulation[0].Item2}");
				}

				// Избор и размножаване
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
*/
using System;
using System.Collections.Generic;
using System.Linq;

namespace KnapsackGeneticAlgo
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

	class Program
	{
		static Random rand = new Random();

		public static void Main()
		{
			// Четем входните данни за капацитета и броя на елементите
			string[] firstLine = Console.ReadLine().Split();
			int maxWeightCapacity = int.Parse(firstLine[0]);
			int numberOfItems = int.Parse(firstLine[1]);

			// Четем списъка с елементи
			List<Item> items = ReadInput(numberOfItems);

			int populationSize = 1000;

			// Инициализираме популацията
			bool[,] population = GenerateInitialPopulation(populationSize, numberOfItems);

			// Показваме началната популация
			//DisplayPopulation(population, 1, "Initial Population");

			// Основен цикъл на генетичния алгоритъм
			for (int generation = 0; generation < 10; generation++)
			{
				// Оценка на фитнес функцията
				double[] fitnessValues = EvaluateFitness(population, items, maxWeightCapacity);

				// Селектиране на родители
				var parents = SelectParents(population, fitnessValues);

				// Кръстоска
				bool[,] offspring = Crossover(parents);

				// Мутация
				Mutate(offspring);

				// Обновяваме популацията за следващото поколение
				population = CreateNewPopulation(parents, offspring);

				// Показваме популацията на всяко поколение
				//DisplayPopulation(population, generation + 1, $"Generation {generation + 1}");

				Console.WriteLine(fitnessValues.Max());
			}
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

		public static bool[,] GenerateInitialPopulation(int populationSize, int numberOfItems)
		{
			bool[,] population = new bool[populationSize, numberOfItems];
			for (int i = 0; i < populationSize; i++)
			{
				for (int j = 0; j < numberOfItems; j++)
				{
					population[i, j] = rand.NextDouble() > 0.5;
				}
			}
			return population;
		}

		public static double[] EvaluateFitness(bool[,] population, List<Item> items, int maxWeightCapacity)
		{
			int populationSize = population.GetLength(0);
			double[] fitnessValues = new double[populationSize];

			for (int i = 0; i < populationSize; i++)
			{
				double weightSum = 0;
				double valueSum = 0;

				for (int j = 0; j < items.Count; j++)
				{
					if (population[i, j])
					{
						weightSum += items[j].Weight;
						valueSum += items[j].Value;
					}
				}

				fitnessValues[i] = (weightSum <= maxWeightCapacity) ? valueSum : 0;
			}

			return fitnessValues;
		}

		public static (bool[], bool[]) SelectParents(bool[,] population, double[] fitnessValues)
		{
			int bestIndex = Array.IndexOf(fitnessValues, fitnessValues.Max());
			int secondBestIndex = fitnessValues
				.Select((val, index) => new { val, index })
				.Where(x => x.index != bestIndex)
				.OrderByDescending(x => x.val)
				.First().index;

			bool[] parent1 = new bool[population.GetLength(1)];
			bool[] parent2 = new bool[population.GetLength(1)];

			for (int i = 0; i < population.GetLength(1); i++)
			{
				parent1[i] = population[bestIndex, i];
				parent2[i] = population[secondBestIndex, i];
			}

			return (parent1, parent2);
		}

		public static bool[,] Crossover((bool[] parent1, bool[] parent2) parents)
		{
			int numberOfItems = parents.parent1.Length;
			bool[,] offspring = new bool[2, numberOfItems];

			int crossoverPoint = rand.Next(1, numberOfItems - 1);
			for (int i = 0; i < numberOfItems; i++)
			{
				offspring[0, i] = (i < crossoverPoint) ? parents.parent1[i] : parents.parent2[i];
				offspring[1, i] = (i < crossoverPoint) ? parents.parent2[i] : parents.parent1[i];
			}

			return offspring;
		}

		public static void Mutate(bool[,] offspring)
		{
			int mutateRow = rand.Next(0, offspring.GetLength(0));
			int mutateCol = rand.Next(0, offspring.GetLength(1));

			offspring[mutateRow, mutateCol] = !offspring[mutateRow, mutateCol];
		}

		public static bool[,] CreateNewPopulation((bool[] parent1, bool[] parent2) parents, bool[,] offspring)
		{
			bool[,] newPopulation = new bool[4, parents.parent1.Length];
			for (int j = 0; j < parents.parent1.Length; j++)
			{
				newPopulation[0, j] = parents.parent1[j];
				newPopulation[1, j] = parents.parent2[j];
				newPopulation[2, j] = offspring[0, j];
				newPopulation[3, j] = offspring[1, j];
			}
			return newPopulation;
		}

		public static void DisplayPopulation(bool[,] population, int generation, string title)
		{
			Console.WriteLine($"\n\n\t-------------- {title} -----------------\n\n");
			for (int i = 0; i < population.GetLength(0); i++)
			{
				Console.Write($"Chromosome {i + 1}: ");
				for (int j = 0; j < population.GetLength(1); j++)
				{
					Console.Write($"{(population[i, j] ? 1 : 0)} ");
				}
				Console.WriteLine();
			}
		}
	}
}
