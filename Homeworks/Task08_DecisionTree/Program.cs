using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Task08_DecisionTree
{
	class Program
	{
		static void Main()
		{
			// Въвеждане на параметър за подрязване
			Console.WriteLine("Enter pruning method (0 = Pre-pruning, 1 = Post-pruning, 2 = Both): ");
			int pruningMethod = int.Parse(Console.ReadLine());

			string line;
			var arr = new List<string[]>();

			// Четене на данни
			System.IO.StreamReader file =
				new System.IO.StreamReader(@"D:\programing\Artificial-Intelligence\Homeworks\Task08_DecisionTree\breast-cancer.data");
			while ((line = file.ReadLine()) != null)
			{
				arr.Add(line.Split(','));
			}
			file.Close();

			var model = new DecisionTree(pruningMethod);
			model.TrainAndTest(arr.ToArray());
		}
	}

	class DecisionTree
	{
		private int K = 6;
		private int classIndex = 9;
		private HashSet<int> takenIndexes;
		private int pruningMethod;

		// Параметри за предварително подрязване
		private int maxDepth = 10;
		private int minExamplesInLeaf = 5;
		private double minGain = 0.01;

		public DecisionTree(int pruningMethod)
		{
			this.takenIndexes = new HashSet<int>() { classIndex };
			this.pruningMethod = pruningMethod;
		}
		private IDictionary<string, int> CalculateTable(int index, string[][] set)
		{
			var table = set.Select(element => element[index])
				.GroupBy(item => item)
				.ToDictionary(item => item.Key, item => item.Count());
			return table;
		}

		private IDictionary<string, Dictionary<string, int>> CalculateTable2D(int index, int classIndex, string[][] set)
		{
			var table = new Dictionary<string, Dictionary<string, int>>();
			foreach (var item in set)
			{
				if (!table.ContainsKey(item[index]))
				{
					table.Add(item[index], new Dictionary<string, int>());
				}
				if (!table[item[index]].ContainsKey(item[classIndex]))
				{
					table[item[index]].Add(item[classIndex], 0);
				}
				table[item[index]][item[classIndex]]++;
			}

			return table;
		}

		private double Entropy(IDictionary<string, int> occurances, int setLength)
		{
			double sum = 0;
			foreach (double item in occurances.Values)
			{
				double probability = item / setLength;
				sum -= probability * Math.Log(probability, 2);
			}
			return sum;
		}
		private double OneAttribteEntropy(int index, string[][] set)
		{
			IDictionary<string, int> occurances = this.CalculateTable(index, set);
			return this.Entropy(occurances, set.Length);
		}

		private double TwoAttribteEntropy(int index, string[][] set)
		{
			double sum = 0;

			IDictionary<string, int> attributeOccurances = this.CalculateTable(index, set);
			if (attributeOccurances.Count == set.Length)
				return 2;
			IDictionary<string, Dictionary<string, int>> occurances = this.CalculateTable2D(index, classIndex, set);
			foreach (var item in occurances)
			{
				double probability = (double)attributeOccurances[item.Key] / set.Length;
				double entropy = this.Entropy(item.Value, attributeOccurances[item.Key]);
				sum += probability * entropy;
			}
			return sum;
		}

		private int InformationGain(string[][] set)
		{
			var classEntropy = this.OneAttribteEntropy(classIndex, set);
			if (classEntropy == 0)
			{
				return -1;
			}
			double maxGain = 0;
			var bestIndex = -1;
			for (int i = 0; i < set[0].Length; i++)
			{
				if (takenIndexes.Contains(i)) continue;

				var attribteEntropy = this.TwoAttribteEntropy(i, set);
				if ((classEntropy - attribteEntropy) > maxGain)
				{
					maxGain = classEntropy - attribteEntropy;
					bestIndex = i;
				}
			}

			return bestIndex;
		}
		// Същите методи за изграждане на дърво и пресмятане на ентропията

		private Node BuildTree(string[][] set, int depth = 0)
		{
			if (set.Length <= K || depth >= maxDepth)  // Pre-pruning based on maxDepth and K
			{
				string best = set.Select(element => element[classIndex])
					.GroupBy(item => item)
					.Select(item => (item.Key, item.Count()))
					.OrderBy(item => item.Item2).First().Key;
				return new Node(-1, null, true, best);
			}

			var index = this.InformationGain(set);
			if (index == -1)
			{
				return new Node(-1, null, true, set[0][classIndex]);
			}
			this.takenIndexes.Add(index);

			var childs = new Dictionary<string, Node>();
			var sets = set.GroupBy(item => item[index]);

			foreach (var group in sets)
			{
				var nextNodeSet = group.ToArray();
				var nextNode = BuildTree(nextNodeSet, depth + 1);
				childs.Add(group.Key, nextNode);
			}

			this.takenIndexes.Remove(index);
			var node = new Node(index, childs);

			// Приложение на post-pruning ако е избрано
			if (pruningMethod == 1 || pruningMethod == 2)
			{
				node = PostPruning(node, set);
			}

			return node;
		}

		public class Node
		{
			public bool isLeaf;
			public string value;
			public int index;
			public IDictionary<string, Node> childs;

			public Node(int index, IDictionary<string, Node> childs, bool leaf = false, string val = default)
			{
				this.value = val;
				this.isLeaf = leaf;
				this.index = index;
				this.childs = childs;
			}
		}

		private double TestModel(Node model, string[][] set)
		{
			double accuracy = 0;
			foreach (var item in set)
			{
				Node node = model;

				while (!node.isLeaf)
				{
					if (node.childs.ContainsKey(item[node.index]))
					{
						node = node.childs[item[node.index]];
					}
					else
					{
						node = node.childs.First().Value;
					}
				}

				if (node.value == item[classIndex]) accuracy++;
			}

			return accuracy;
		}

		// Метод за post-pruning: Reduced Error Pruning
		private Node PostPruning(Node node, string[][] set)
		{
			if (node.isLeaf)
				return node;

			// Изчисляване на точността преди подрязване
			double accuracyBeforePruning = TestModel(node, set);

			// Пробвайте да подрежете възела
			string mostCommonClass = set.Select(item => item[classIndex])
										.GroupBy(item => item)
										.OrderByDescending(group => group.Count())
										.First().Key;

			// Създайте подрязано дърво
			var prunedNode = new Node(-1, null, true, mostCommonClass);

			// Изчисляване на точността след подрязване
			double accuracyAfterPruning = TestModel(prunedNode, set);

			// Подрязване, ако подрязания възел е по-добър или със същата точност
			if (accuracyAfterPruning >= accuracyBeforePruning)
			{
				return prunedNode;
			}

			// Рекурсивно подрязване на децата
			foreach (var child in node.childs)
			{
				node.childs[child.Key] = PostPruning(child.Value, set);
			}

			return node;
		}

		public void TrainAndTest(string[][] set)
		{
			double accuracySum = 0;
			double accuracySumTest = 0;
			List<double> foldAccuracies = new List<double>();

			// Шафлиране на данни за 10-кратна кръстосана валидация
			set = this.Shuffle(set);

			for (int i = 0; i < 10; i++)
			{
				// Разделяне на данни на тренировъчни и тестови
				var trainSet = set.Take(i * 28).Union(set.Skip(i * 28 + 28)).ToArray();
				var model = this.BuildTree(trainSet);

				var testSet = set.Skip(i * 28).Take(28).ToArray();
				double accuracy = this.TestModel(model, testSet);
				foldAccuracies.Add(accuracy);
				accuracySum += accuracy;

				// Тестова точност (тренировъчната е направена по-горе)
				var testModel = this.BuildTree(set); // Обучение върху целия набор
				accuracySumTest += this.TestModel(testModel, testSet);
			}

			double averageAccuracy = accuracySum / 10;
			double standardDeviation = CalculateStandardDeviation(foldAccuracies, averageAccuracy);

			// Резултати
			Console.WriteLine("Train Set Accuracy:");
			Console.WriteLine($"Accuracy: {averageAccuracy / 28:P2}");

			Console.WriteLine("\n10-Fold Cross-Validation Results:");
			for (int i = 0; i < foldAccuracies.Count; i++)
			{
				Console.WriteLine($"Accuracy Fold {i + 1}: {foldAccuracies[i] / 28:P2}");
			}

			Console.WriteLine($"\nAverage Accuracy: {averageAccuracy / 28:P2}");
			Console.WriteLine($"Standard Deviation: {standardDeviation:P2}");

			double testSetAccuracy = accuracySumTest / 10;
			Console.WriteLine($"\nTest Set Accuracy:\nAccuracy: {testSetAccuracy / 28:P2}");
		}

		private string[][] Shuffle(string[][] set)
		{
			Random rnd = new Random();
			return set.OrderBy(x => rnd.Next()).ToArray();
		}

		private double CalculateStandardDeviation(List<double> accuracies, double averageAccuracy)
		{
			double sumOfSquares = accuracies.Sum(a => Math.Pow(a / 28 - averageAccuracy / 28, 2));
			return Math.Sqrt(sumOfSquares / accuracies.Count);
		}
	}
}
