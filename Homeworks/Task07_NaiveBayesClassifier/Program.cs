﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NaiveBayesClassifier
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Enter 0 for 'abstained' or 1 for replacement on '?':");
			int choice = int.Parse(Console.ReadLine());

			string filePath = "D:\\programing\\Artificial-Intelligence\\Homeworks\\Task07_NaiveBayesClassifier\\house-votes-84.data";
			var rawData = LoadData(filePath);

			var (labels, features) = PreprocessData(rawData, choice);

			var (trainFeatures, trainLabels, testFeatures, testLabels) = SplitData(features, labels, 0.8);

			var (model, classCounts, valueToIndex) = TrainNaiveBayes(trainFeatures, trainLabels);

			double trainAccuracy = Evaluate(trainFeatures, trainLabels, model, classCounts, valueToIndex);
			Console.WriteLine($"Train Set Accuracy: {trainAccuracy * 100:F2}%");

			var (cvAccuracies, cvMean, cvStdDev) = CrossValidate(features, labels, 10, classCounts, valueToIndex);
			Console.WriteLine("\n10-Fold Cross-Validation Results:");
			for (int i = 0; i < cvAccuracies.Count; i++)
			{
				Console.WriteLine($"    Accuracy Fold {i + 1}: {cvAccuracies[i] * 100:F2}%");
			}
			Console.WriteLine($"\n    Average Accuracy: {cvMean * 100:F2}%");
			Console.WriteLine($"    Standard Deviation: {cvStdDev * 100:F2}%");

			double testAccuracy = Evaluate(testFeatures, testLabels, model, classCounts, valueToIndex);
			Console.WriteLine($"\nTest Set Accuracy: {testAccuracy * 100:F2}%");
		}

		static List<string[]> LoadData(string filePath)
		{
			return System.IO.File.ReadAllLines(filePath).Select(line => line.Split(',')).ToList();
		}

		static (List<int>, List<List<string>>) PreprocessData(List<string[]> rawData, int choice)
		{
			var labels = rawData.Select(row => row[0] == "democrat" ? 0 : 1).ToList();
			var features = rawData.Select(row => row.Skip(1).ToList()).ToList();

			if (choice == 0)
			{
				for (int i = 0; i < features.Count; i++)
				{
					for (int j = 0; j < features[i].Count; j++)
					{
						if (features[i][j] == "?")
							features[i][j] = "2";
					}
				}
			}
			else
			{
				for (int j = 0; j < features[0].Count; j++)
				{
					var columnValues = features.Select(row => row[j]).Where(value => value != "?").ToList();
					string mostCommon = columnValues.GroupBy(x => x).OrderByDescending(g => g.Count()).First().Key;

					for (int i = 0; i < features.Count; i++)
					{
						if (features[i][j] == "?")
							features[i][j] = mostCommon;
					}
				}
			}

			return (labels, features);
		}

		static (List<List<string>>, List<int>, List<List<string>>, List<int>) SplitData(
			List<List<string>> features, List<int> labels, double trainRatio)
		{
			int trainSize = (int)(features.Count * trainRatio);
			var indices = Enumerable.Range(0, features.Count).ToList();
			var random = new Random();
			indices = indices.OrderBy(x => random.Next()).ToList();

			var trainIndices = indices.Take(trainSize).ToList();
			var testIndices = indices.Skip(trainSize).ToList();

			var trainFeatures = trainIndices.Select(i => features[i]).ToList();
			var trainLabels = trainIndices.Select(i => labels[i]).ToList();
			var testFeatures = testIndices.Select(i => features[i]).ToList();
			var testLabels = testIndices.Select(i => labels[i]).ToList();

			return (trainFeatures, trainLabels, testFeatures, testLabels);
		}

		static (Dictionary<int, Dictionary<int, Dictionary<int, int>>>, Dictionary<int, int>, Dictionary<string, int>) TrainNaiveBayes(
			List<List<string>> trainFeatures, List<int> trainLabels)
		{
			var uniqueValues = trainFeatures.SelectMany(row => row).Distinct().ToList();
			var valueToIndex = uniqueValues.Select((value, index) => new { value, index }).ToDictionary(x => x.value, x => x.index);
			var numericalFeatures = trainFeatures.Select(row => row.Select(value => valueToIndex[value]).ToList()).ToList();

			var classCounts = trainLabels.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
			var featureCounts = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();

			for (int label = 0; label <= 1; label++)
			{
				featureCounts[label] = new Dictionary<int, Dictionary<int, int>>();

				for (int j = 0; j < numericalFeatures[0].Count; j++)
				{
					featureCounts[label][j] = new Dictionary<int, int>();
					foreach (var featureValue in uniqueValues.Select(v => valueToIndex[v]))
					{
						featureCounts[label][j][featureValue] = 1;
					}
				}
			}

			foreach (var (featuresRow, label) in numericalFeatures.Zip(trainLabels, (f, l) => (f, l)))
			{
				for (int j = 0; j < featuresRow.Count; j++)
				{
					featureCounts[label][j][featuresRow[j]]++;
				}
			}

			return (featureCounts, classCounts, valueToIndex);
		}

		static double Predict(List<string> featureRow,
			Dictionary<int, Dictionary<int, Dictionary<int, int>>> model,
			Dictionary<int, int> classCounts, Dictionary<string, int> valueToIndex)
		{
			var numericalRow = featureRow.Select(value => valueToIndex[value]).ToList();
			double[] probabilities = new double[2];

			for (int label = 0; label <= 1; label++)
			{
				probabilities[label] = Math.Log(classCounts[label] / (double)classCounts.Values.Sum());

				for (int j = 0; j < numericalRow.Count; j++)
				{
					probabilities[label] += Math.Log(model[label][j][numericalRow[j]] / (double)classCounts[label]);
				}
			}

			return probabilities[0] > probabilities[1] ? 0 : 1;
		}

		static double Evaluate(List<List<string>> featuresSet, List<int> labelsSet,
			Dictionary<int, Dictionary<int, Dictionary<int, int>>> model,
			Dictionary<int, int> classCounts, Dictionary<string, int> valueToIndex)
		{
			int correct = 0;

			for (int i = 0; i < featuresSet.Count; i++)
			{
				if (Predict(featuresSet[i], model, classCounts, valueToIndex) == labelsSet[i])
					correct++;
			}

			return correct / (double)featuresSet.Count;
		}
		static (List<double>, double, double) CrossValidate(List<List<string>> features,
		List<int> labels, int folds, Dictionary<int, int> classCounts, Dictionary<string, int> valueToIndex)
		{
			int foldSize = features.Count / folds;
			var indices = Enumerable.Range(0, features.Count).ToList();
			var random = new Random();
			indices = indices.OrderBy(x => random.Next()).ToList();

			List<double> accuracies = new List<double>();

			for (int fold = 0; fold < folds; fold++)
			{
				var testIndices = indices.Skip(fold * foldSize).Take(foldSize).ToList();
				var trainIndices = indices.Except(testIndices).ToList();

				var trainFeatures = trainIndices.Select(i => features[i]).ToList();
				var trainLabels = trainIndices.Select(i => labels[i]).ToList();
				var testFeatures = testIndices.Select(i => features[i]).ToList();
				var testLabels = testIndices.Select(i => labels[i]).ToList();

				var currentModel = TrainNaiveBayes(trainFeatures, trainLabels, classCounts, valueToIndex);

				double accuracy = Evaluate(testFeatures, testLabels, currentModel, classCounts, valueToIndex);
				accuracies.Add(accuracy);
			}

			double meanAccuracy = accuracies.Average();
			double stdDevAccuracy = Math.Sqrt(accuracies.Select(a => Math.Pow(a - meanAccuracy, 2)).Sum() / (accuracies.Count - 1));

			return (accuracies, meanAccuracy, stdDevAccuracy);
		}

		static Dictionary<int, Dictionary<int, Dictionary<int, int>>> TrainNaiveBayes(
			List<List<string>> trainFeatures, List<int> trainLabels, Dictionary<int, int> classCounts,
			Dictionary<string, int> valueToIndex)
		{
			var uniqueValues = trainFeatures.SelectMany(row => row).Distinct().ToList();
			var numericalFeatures = trainFeatures.Select(row => row.Select(value => valueToIndex[value]).ToList()).ToList();

			var featureCounts = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();

			for (int label = 0; label <= 1; label++)
			{
				featureCounts[label] = new Dictionary<int, Dictionary<int, int>>();

				for (int j = 0; j < numericalFeatures[0].Count; j++)
				{
					featureCounts[label][j] = new Dictionary<int, int>();
					foreach (var featureValue in uniqueValues.Select(v => valueToIndex[v]))
					{
						featureCounts[label][j][featureValue] = 1;
					}
				}
			}

			foreach (var (featuresRow, label) in numericalFeatures.Zip(trainLabels, (f, l) => (f, l)))
			{
				for (int j = 0; j < featuresRow.Count; j++)
				{
					featureCounts[label][j][featuresRow[j]]++;
				}
			}

			return featureCounts;
		}

	}
}
