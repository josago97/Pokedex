using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.ML;
using Microsoft.ML.Vision;
using Pokedex.Common.MachineLearning;
using Tensorflow.Keras.Engine;
using static Microsoft.ML.Transforms.ValueToKeyMappingEstimator;

namespace Pokedex.ModelBuilder
{
    public class ModelTrainer
    {
        private static readonly string[] EXTENSIONS = { "jpg", "jpeg", "png" };

        public void Train(string imagesPath, string modelPath)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            MLContext mlContext = new MLContext();

            // 1. Load the initial full image-set into an IDataView and shuffle so it'll be better balanced
            Console.WriteLine("Loading images...");
            InputModel[] images = LoadImages(imagesPath);
            IDataView fullImagesDataset = mlContext.Data.LoadFromEnumerable(images);
            IDataView shuffledFullImageFilePathsDataset = fullImagesDataset; // mlContext.Data.ShuffleRows(fullImagesDataset);

            // 2. Load Images with in-memory type within the IDataView and Transform Labels to Keys (Categorical)
            IDataView shuffledFullImagesDataset = mlContext.Transforms.Conversion
                .MapValueToKey("Label", keyOrdinality: KeyOrdinality.ByOccurrence)
                .Fit(shuffledFullImageFilePathsDataset)
                .Transform(shuffledFullImageFilePathsDataset);

            // 3. Split the data 80:20 into train and test sets, train and evaluate.
            var trainTestData = mlContext.Data.TrainTestSplit(shuffledFullImagesDataset, testFraction: 0.2);
            IDataView trainDataView = trainTestData.TrainSet;
            IDataView testDataView = trainTestData.TestSet;

            // 5. Define the model's training pipeline using DNN default values
            var options = new ImageClassificationTrainer.Options()
            {
                FeatureColumnName = "ImageSource",
                LabelColumnName = "Label",
                ScoreColumnName = "Score",
                PredictedLabelColumnName = "PredictedLabel",
                // Just by changing/selecting InceptionV3/MobilenetV2/ResnetV250
                // you can try a different DNN architecture (TensorFlow pre-trained model).
                Arch = ImageClassificationTrainer.Architecture.ResnetV250,
                Epoch = 200,
                BatchSize = 10,
                LearningRate = 0.01f,
                MetricsCallback = (metrics) => { if (metrics.Train != null) Console.WriteLine(metrics); },
                ValidationSet = testDataView,
                FinalModelPrefix = "PokedexModel",
                WorkspacePath = "Workspace"
            };

            var pipeline = mlContext.MulticlassClassification.Trainers.ImageClassification(options)
                    .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            // 6. Train/create the ML model
            Console.WriteLine("Training model...");
            ITransformer trainedModel = pipeline.Fit(trainDataView);

            // 7. Get the quality metrics (accuracy, etc.)
            IDataView predictionsDataView = trainedModel.Transform(testDataView);
            var metrics = mlContext.MulticlassClassification.Evaluate(predictionsDataView, "Label", "Score", "PredictedLabel");

            Console.WriteLine();
            Console.WriteLine($"Training time: {stopwatch.Elapsed}");
            Console.WriteLine($"TensorFlow DNN Transfer Learning | MacroAccuracy: {metrics.MacroAccuracy}, MicroAccuracy: {metrics.MicroAccuracy}");
            Console.WriteLine("Training successfuly");

            mlContext.Model.Save(trainedModel, fullImagesDataset.Schema, $"{modelPath}.zip");
            /*using FileStream stream = File.Create("PokedexModel.onnx");
            mlContext.Model.ConvertToOnnx(trainedModel, trainDataView, stream);*/
        }

        /*private InputModel[] LoadImages(string imagesPath)
        {
            var tasks = Directory.EnumerateDirectories(imagesPath)
                .Select(d => Task.Run(() =>
                {
                    string folderName = Path.GetFileName(d);
                    int separatorIndex = folderName.IndexOf(' ');
                    int pokemonId = int.Parse(folderName.Substring(0, separatorIndex));
                    string pokemonName = folderName.Substring(separatorIndex + 1);

                    return Directory.EnumerateFiles(d)
                        .Where(f => EXTENSIONS.Contains(Path.GetExtension(f).Substring(1)))
                        .Select(f => new { Id = pokemonId, Model = GetInputModel(pokemonName, f) });
                }));

            return Task.WhenAll(tasks)
                .ContinueWith(t => t.Result.SelectMany(x => x)
                    .OrderBy(x => x.Id)
                    .Select(x => x.Model)
                    .ToArray())
                .Result;
        }*/

        private InputModel[] LoadImages(string imagesPath)
        {
            string[] directories = Directory.EnumerateDirectories(imagesPath).ToArray();
            (int Id, InputModel[] Models)[] data = new (int Id, InputModel[] Models)[directories.Length];

            Parallel.For(0, directories.Length, i =>
            {
                string folder = directories[i];
                string folderName = Path.GetFileName(folder);
                int separatorIndex = folderName.IndexOf(' ');
                int pokemonId = int.Parse(folderName.Substring(0, separatorIndex));
                string pokemonName = folderName.Substring(separatorIndex + 1);
                string[] images = Directory.EnumerateFiles(folder)
                    .Where(f => EXTENSIONS.Contains(Path.GetExtension(f).Substring(1)))
                    .ToArray();
                InputModel[] models = new InputModel[images.Length];

                Parallel.For(0, images.Length, j =>
                {
                    models[j] = GetInputModel(pokemonName, images[j]);
                });

                data[i] = (pokemonId, models);
            });

            return data.OrderBy(x => x.Id)
                .SelectMany(x => x.Models)
                .ToArray();
        }

        private InputModel GetInputModel(string pokemonName, string file)
        {
            return new InputModel
            {
                ImageSource = File.ReadAllBytes(file),
                Label = pokemonName
            };
        }
    }
}
