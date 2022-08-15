using Microsoft.ML;
using Pokedex.Common.MachineLearning;

namespace Pokedex.WebAPI.Logic
{
    public class Classifier
    {
        private static readonly string MODEL_PATH = Environment.GetEnvironmentVariable("MODEL_FILE_PATH");

        private PredictionEngine<InputModel, OutputModel> _predictor;

        public Classifier()
        {
            MLContext mlContext = new MLContext();
            ITransformer model = mlContext.Model.Load(MODEL_PATH, out var _);
            _predictor = mlContext.Model.CreatePredictionEngine<InputModel, OutputModel>(model);
        }

        public OutputModel Classify(byte[] image)
        {
            return _predictor.Predict(new InputModel { ImageSource = image });
        }
    }
}
