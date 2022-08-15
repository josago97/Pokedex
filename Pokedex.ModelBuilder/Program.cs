using Microsoft.ML;
using Microsoft.ML.Data;
using Pokedex.Common.MachineLearning;

namespace Pokedex.ModelBuilder
{
    public class Program
    {
        private const string IMAGES_FOLDER = "Resources/Images";
        private const string IMAGES_TO_TRAIN_FOLDER = "Resources/ImagesTraining";

        public static void Main()
        {
            //GenerateImageSet();
            GenerateTrainedModel();
            //GenerateLabelsFile();
        }

        private static void GenerateImageSet()
        {
            ImageSetGenerator imageSetGenerator = new ImageSetGenerator();
            imageSetGenerator.GenerateImages(IMAGES_FOLDER, IMAGES_TO_TRAIN_FOLDER);
        }

        private static void GenerateTrainedModel()
        {
            ModelTrainer modelTrainer = new ModelTrainer();
            modelTrainer.Train(IMAGES_TO_TRAIN_FOLDER, "PokedexModel");
        }

        private static void GenerateLabelsFile()
        {
            MLContext mlContext = new MLContext();
            ITransformer model = mlContext.Model.Load("PokedexModel.zip", out DataViewSchema schema);

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
        }
    }
}