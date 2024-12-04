using Microsoft.ML;
using Microsoft.ML.Data;

namespace Task07_NaiveBayesClassifier
{
	class Program
	{
		// Define the data structure
		public class VotingData
		{
			[LoadColumn(0)]
			public string Party { get; set; }

			[LoadColumn(1, 16)]
			public string[] Votes { get; set; }
		}

		public class TransformedData
		{
			public bool Label { get; set; } // Democrat = false, Republican = true

			[VectorType(16)]
			public float[] Features { get; set; }
		}

		static void Main(string[] args)
		{
			Console.WriteLine("Choose handling mode for missing values ('?' = abstained):");
			Console.WriteLine("0: Treat '?' as a third category (Abstained)");
			Console.WriteLine("1: Fill '?' with the most frequent value in each column");
			int mode = int.Parse(Console.ReadLine());

			string dataPath = "D:\\programing\\Artificial-Intelligence\\Homeworks\\Task07_NaiveBayesClassifier\\house-votes-84.data";

			// Load data
			var rawData = File.ReadAllLines(dataPath)
							  .Select(line => line.Split(','))
							  .Select(fields => new VotingData
							  {
								  Party = fields[0],
								  Votes = fields.Skip(1).ToArray()
							  })
							  .ToList();

			// Transform data based on the chosen mode
			var transformedData = TransformData(rawData, mode);

			// Split data into train and test sets
			var context = new MLContext();
			var (trainData, testData) = SplitData(context, transformedData, 0.8);

			// Define pipeline and train model
			var pipeline = context.Transforms.Conversion.MapValueToKey("Label")
						  .Append(context.Transforms.Concatenate("Features", "Features"))
						  .Append(context.MulticlassClassification.Trainers.NaiveBayes())
						  .Append(context.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

			var model = pipeline.Fit(trainData);

			// Evaluate the model
			Console.WriteLine("\nTraining Accuracy:");
			var trainMetrics = EvaluateModel(context, model, trainData);
			Console.WriteLine($"Accuracy: {trainMetrics.MacroAccuracy:P2}");

			Console.WriteLine("\nCross-Validation Results:");
			var cvMetrics = CrossValidate(context, transformedData);
			Console.WriteLine($"Average Accuracy: {cvMetrics.Average():P2}");
			Console.WriteLine($"Standard Deviation: {CalculateStandardDeviation(cvMetrics):P2}");

			Console.WriteLine("\nTest Set Accuracy:");
			var testMetrics = EvaluateModel(context, model, testData);
			Console.WriteLine($"Accuracy: {testMetrics.MacroAccuracy:P2}");
		}

		static List<TransformedData> TransformData(List<VotingData> rawData, int mode)
		{
			return rawData.Select(item =>
			{
				var features = item.Votes.Select(v => mode == 0
					? (v == "y" ? 1f : v == "n" ? 0f : 0.5f) // Treat "?" as abstained (0.5)
					: v == "y" ? 1f : v == "n" ? 0f : float.NaN) // Fill NaN for missing values
					.ToArray();

				if (mode == 1) // Fill missing values with column means
				{
					for (int i = 0; i < features.Length; i++)
					{
						if (float.IsNaN(features[i]))
						{
							var columnMean = rawData
								.Where(x => x.Votes[i] != "?")
								.Select(x => x.Votes[i] == "y" ? 1f : 0f)
								.Average();
							features[i] = columnMean;
						}
					}
				}

				return new TransformedData
				{
					Label = item.Party == "republican",
					Features = features
				};
			}).ToList();
		}

		static (IDataView TrainSet, IDataView TestSet) SplitData(MLContext context, List<TransformedData> data, double trainRatio)
		{
			var random = new Random();
			var grouped = data.GroupBy(x => x.Label)
							  .SelectMany(g => g.OrderBy(_ => random.Next()))
							  .ToList();

			int trainSize = (int)(data.Count * trainRatio);
			var trainSet = grouped.Take(trainSize).ToList();
			var testSet = grouped.Skip(trainSize).ToList();

			return (context.Data.LoadFromEnumerable(trainSet), context.Data.LoadFromEnumerable(testSet));
		}

		static MulticlassClassificationMetrics EvaluateModel(MLContext context, ITransformer model, IDataView data)
		{
			var predictions = model.Transform(data);
			return context.MulticlassClassification.Evaluate(predictions);
		}

		static List<double> CrossValidate(MLContext context, List<TransformedData> data)
		{
			const int folds = 10;
			int foldSize = data.Count / folds;
			var accuracies = new List<double>();

			for (int i = 0; i < folds; i++)
			{
				var testFold = data.Skip(i * foldSize).Take(foldSize).ToList();
				var trainFold = data.Except(testFold).ToList();

				var trainData = context.Data.LoadFromEnumerable(trainFold);
				var testData = context.Data.LoadFromEnumerable(testFold);

				var pipeline = context.Transforms.Conversion.MapValueToKey("Label")
							  .Append(context.Transforms.Concatenate("Features", "Features"))
							  .Append(context.MulticlassClassification.Trainers.NaiveBayes())
							  .Append(context.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

				var model = pipeline.Fit(trainData);
				var metrics = EvaluateModel(context, model, testData);
				accuracies.Add(metrics.MacroAccuracy);
			}

			return accuracies;
		}

		static double CalculateStandardDeviation(List<double> accuracies)
		{
			double mean = accuracies.Average();
			return Math.Sqrt(accuracies.Sum(a => Math.Pow(a - mean, 2)) / accuracies.Count);
		}
	}
}
