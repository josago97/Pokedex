using System.Diagnostics;
using Microsoft.ML;
using Microsoft.ML.Data;
using Pokedex.Common.MachineLearning;

namespace Pokedex.ModelBuilder
{
    public class Program
    {
        private const string RESOURCES_FOLDER = "Resources";
        private const string IMAGES_FOLDER = $"{RESOURCES_FOLDER}/Images";
        private const string IMAGES_TO_TRAIN_FOLDER = $"{RESOURCES_FOLDER}/ImagesTraining";
        private const string RESULT_FOLDER = "Model";
        private const string MODEL_NAME = $"{RESULT_FOLDER}/PokedexModel";
        private const string LABELS_FILENAME = $"{RESULT_FOLDER}/labels.txt";

        public static void Main()
        {
            //GenerateImageSet();

            Directory.CreateDirectory(RESULT_FOLDER);
            GenerateTrainedModel();
            GenerateLabelsFile();
            //GenerateTFLiteModel();
        }

        private static void GenerateImageSet()
        {
            ImageSetGenerator imageSetGenerator = new ImageSetGenerator();
            imageSetGenerator.GenerateImages(IMAGES_FOLDER, IMAGES_TO_TRAIN_FOLDER);
        }

        private static void GenerateTrainedModel()
        {
            ModelTrainer modelTrainer = new ModelTrainer();
            modelTrainer.Train(IMAGES_TO_TRAIN_FOLDER, MODEL_NAME);
        }

        private static void GenerateLabelsFile()
        {
            MLContext mlContext = new MLContext();
            ITransformer model = mlContext.Model.Load($"{MODEL_NAME}.zip", out DataViewSchema schema);

            PredictionEngine<InputModel, OutputModel> predictionEngine = mlContext.Model.CreatePredictionEngine<InputModel, OutputModel>(model);
            DataViewSchema.Column? column = predictionEngine.OutputSchema.GetColumnOrNull("Score");

            VBuffer<ReadOnlyMemory<char>> slotNames = new VBuffer<ReadOnlyMemory<char>>();
            column.Value.GetSlotNames(ref slotNames);
            string[] names = new string[slotNames.Length];
            int num = 0;

            foreach (var denseValue in slotNames.DenseValues())
            {
                names[num++] = denseValue.ToString();
            }

            File.WriteAllLines(LABELS_FILENAME, names);
        }

        private static void GenerateTFLiteModel()
        {
            ProcessStartInfo info = new ProcessStartInfo()
            {
                FileName = "tflite_convert",
                UseShellExecute = false,
                CreateNoWindow = false,
            };

            info.ArgumentList.Add("--model_dir=");
            info.ArgumentList.Add("--output_model=");

            using Process process = Process.Start(info);

            process.WaitForExit();
        }
    }
}