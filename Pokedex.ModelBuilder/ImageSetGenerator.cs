using System.Drawing;

namespace Pokedex.ModelBuilder
{
    public class ImageSetGenerator
    {
        private const int ITERATIONS = 5;
        private static readonly string[] EXTENSIONS = { "jpg", "jpeg", "png" };
        private const int MAX_SIZE = 224;

        private Random _random = new Random();

        public void GenerateImages(string sourcePath, string outputPath)
        {
            if (Directory.Exists(outputPath)) Directory.Delete(outputPath, true);
            Directory.CreateDirectory(outputPath);

            Task[] tasks = Directory.EnumerateDirectories(sourcePath)
                .Select(d => Task.Run(() => CreateImagesDirectory(outputPath, d)))
                .ToArray();

            Task.WaitAll(tasks);
        }

        private void CreateImagesDirectory(string outputPath, string directory)
        {
            string directoryPath = $"{outputPath}/{Path.GetFileName(directory)}";

            Directory.CreateDirectory(directoryPath);
            string[] imagesFiles = Directory.EnumerateFiles(directory)
                .Where(f => EXTENSIONS.Contains(Path.GetExtension(f).Substring(1)))
                .ToArray();

            foreach (string imagePath in imagesFiles)
            {
                string filename = Path.GetFileNameWithoutExtension(imagePath);
                string extension = Path.GetExtension(imagePath);

                using Bitmap original = Resize(new Bitmap(imagePath), MAX_SIZE);
                string destinationFile = $"{directoryPath}/{Path.GetFileName(imagePath)}";
                original.Save(destinationFile);

                for (int i = 0; i < ITERATIONS; i++)
                {
                    Bitmap image = original;
                    image = Rotate(image, _random.NextDouble(0, 360));
                    image = MakeZoom(image, _random.NextDouble(0.8, 1.5));

                    string imageFileName = $"{directoryPath}/{filename}_{i}.{extension}";
                    //ImageFormat format = extension.Contains("png") ? ImageFormat.Png : ImageFormat.Jpeg;
                    image.Save(imageFileName);
                    image.Dispose();
                }
            }
        }

        private Bitmap Rotate(Bitmap image, double angle)
        {
            Bitmap result = new Bitmap(image.Width, image.Height);

            using Graphics graphics = Graphics.FromImage(result);
            graphics.TranslateTransform((float)image.Width / 2, (float)image.Height / 2);
            graphics.RotateTransform((float)angle);
            graphics.TranslateTransform(-(float)image.Width / 2, -(float)image.Height / 2);
            graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height));

            if (_random.NextDouble() > 0.5) result.RotateFlip(RotateFlipType.RotateNoneFlipX);

            return result;
        }

        private Bitmap MakeZoom(Bitmap image, double zoom)
        {
            Bitmap result = new Bitmap(image.Width, image.Height);

            using Graphics graphics = Graphics.FromImage(result);
            Size size = new Size((int)(image.Width * zoom), (int)(image.Height * zoom));
            Point location = new Point(image.Width / 2 - size.Width / 2, image.Height / 2 - size.Height / 2);
            Rectangle imageRectangle = new Rectangle(location, size);
            graphics.DrawImage(image, imageRectangle);

            return result;
        }

        private Bitmap Resize(Bitmap image, int maxSize)
        {
            int width;
            int height;

            if (image.Width > image.Height)
            {
                width = maxSize;
                height = (int)((double)maxSize / image.Width * image.Height);
            }
            else
            {
                width = (int)((double)maxSize / image.Height * image.Width);
                height = maxSize;
            }

            return new Bitmap(image, width, height);
        }
    }
}
