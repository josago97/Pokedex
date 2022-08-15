using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pokedex.Common.MachineLearning;
using Pokedex.WebAPI.Logic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Pokedex.WebAPI.Controllers
{
    [Route("classify")]
    [ApiController]
    [Authorize]
    public class ClassifyController : ControllerBase
    {
        private readonly Classifier _classifier;

        public ClassifyController(Classifier classifier)
        {
            _classifier = classifier;
        }

        [HttpPost]
        public async Task<OutputModel> PostAsync()
        {
            using MemoryStream imageStream = new MemoryStream();
            await Request.Body.CopyToAsync(imageStream);
            return _classifier.Classify(imageStream.ToArray());
        }
    }
}
