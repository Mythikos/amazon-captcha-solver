using ImageMagick;
using System.Text;

namespace AmazonCaptchaSolver
{
    public class Solver
    {
        #region Instance Fields
        public MagickImage Image { get; private set; }
        private const int MAXIMUM_CHARACTER_WIDTH = 33;
        private const int MINIMUM_CHARACTER_LENGTH = 14;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of magic image, applying any necessary settings.
        /// </summary>
        /// <param name="captchaBytes"></param>
        /// <returns></returns>
        public Solver(string captchaPath)
        {
            var createImageResult = Solver.CreateImage(captchaPath);
            if (createImageResult.Success == false)
                throw new ArgumentException(createImageResult.Message);

            var image = createImageResult.GetResult();
            if (image == null)
                throw new ArgumentException("Unable to create the captcha image.");

            this.Image = image;
        }

        /// <summary>
        /// Creates a new instance of magic image, applying any necessary settings.
        /// </summary>
        /// <param name="captchaBytes"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public Solver(byte[] captchaBytes)
        {
            var createImageResult = Solver.CreateImage(captchaBytes);
            if (createImageResult.Success == false)
                throw new ArgumentException(createImageResult.Message);

            var image = createImageResult.GetResult();
            if (image == null)
                throw new ArgumentException("Unable to create the captcha image.");

            this.Image = image;
        }
        #endregion

        /// <summary>
        /// Attempts to solve the captcha. Returns method result with the characters as a string.
        /// </summary>
        /// <param name="captchaImage"></param>
        /// <param name="confidenceThreshold"></param>
        /// <returns>A method result that contains a string code of the solved captcha</returns>
        public MethodResultSingle<string> Solve(double confidenceThreshold = 0.95)
        {
            try
            {
                if (confidenceThreshold < 0 || confidenceThreshold > 1)
                    return MethodResult.CreateFailureSingle<string>("Confidence threshold must be between 0 and 1.");

                // Find characters
                var findCharactersMethodResult = this.GetCharacterImages();
                if (findCharactersMethodResult.Success == false)
                    return MethodResult.CreateFailureSingle<string>("No characters found in the captcha.", findCharactersMethodResult);

                var characterImages = findCharactersMethodResult.GetResult();
                if (characterImages == null)
                    return MethodResult.CreateFailureSingle<string>("No characters found in the captcha.");

                // Solve the captcha
                var captchaCode = new StringBuilder();
                foreach (var characterImage in characterImages)
                {
                    var solveCharacterResult = Solver.SolveCharacter(characterImage.ToByteArray(), confidenceThreshold);
                    if (solveCharacterResult.Success == false)
                    {
                        captchaCode.Append('_');
                    }
                    else
                    {
                        var solvedCharacter = solveCharacterResult.GetResult();
                        if (solvedCharacter == null)
                        {
                            captchaCode.Append('_');
                        }
                        else
                        {
                            captchaCode.Append(solvedCharacter.ToUpper());
                        }
                    }
                }

                return MethodResult.CreateSuccessSingle(captchaCode.ToString());
            }
            catch (Exception ex)
            {
                return MethodResult.CreateFailureSingle<string>(ex.Message);
            }
        }

        /// <summary>
        /// Finds and trims each character of the captcha.
        /// </summary>
        /// <returns>A method result that contains a list of MagickImage</returns>
        public MethodResultSingle<List<MagickImage>> GetCharacterImages()
        {
            try
            {
                var characterColumnsResults = this.FindCharacterColumns(this.Image);
                if (characterColumnsResults.Success == false)
                    return MethodResult.CreateFailureSingle<List<MagickImage>>("Unable to find character columns in the captcha image.", characterColumnsResults);

                var characterColumns = characterColumnsResults.GetResult() ?? new List<(int, int)>();
                if (characterColumns.Count <= 0)
                    return MethodResult.CreateFailureSingle<List<MagickImage>>("Unable to find character columns in the captcha image.");

                var characterImages = new List<MagickImage>();
                foreach (var characterColumn in characterColumns)
                {
                    var copy = (MagickImage)this.Image.Clone();
                    copy.Crop(new MagickGeometry(characterColumn.Item1, 0, characterColumn.Item2 - characterColumn.Item1, copy.Height)); // Crop down to the character column
                    characterImages.Add(copy);
                }

                // Handle edge cases
                if ((characterImages.Count == 6 && characterImages[0].Width < MINIMUM_CHARACTER_LENGTH) || (characterImages.Count != 6 && characterImages.Count != 7))
                    characterImages = Enumerable.Range(0, 6).Select(x => Solver.CreateImage(MagickColors.White, 200, 700).GetResult()).ToList();

                // Handle instances where the first character is split into two and wraps around
                if (characterImages.Count == 7)
                {
                    var mergeResult = this.MergeVertical(characterImages[6], characterImages[0]);
                    if (mergeResult.Success == false)
                        return MethodResult.CreateFailureSingle<List<MagickImage>>("Unable to merge the first and last characters.", mergeResult);

                    var mergedImage = mergeResult.GetResult();
                    if (mergedImage == null)
                        return MethodResult.CreateFailureSingle<List<MagickImage>>("Unable to merge the first and last characters.");

                    characterImages[6] = mergedImage;
                    characterImages.RemoveAt(0);
                }

                // Apply final corrections
                foreach (var characterImage in characterImages)
                {
                    characterImage.Trim(); // Trims the whitespace around the character
                    characterImage.Resize(new MagickGeometry(MAXIMUM_CHARACTER_WIDTH, MAXIMUM_CHARACTER_WIDTH) { IgnoreAspectRatio = true }); // Resize the image to a consistent size
                }

                return MethodResult.CreateSuccessSingle(characterImages);
            }
            catch (Exception ex)
            {
                return MethodResult.CreateFailureSingle<List<MagickImage>>(ex.Message);
            }
        }

        #region Private Helper Methods
        /// <summary>
        /// Merges two vertical slices into one image and returns the result.
        /// </summary>
        /// <param name="image1"></param>
        /// <param name="image2"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private MethodResultSingle<MagickImage> MergeVertical(MagickImage image1, MagickImage image2)
        {
            try
            {
                if (image1.Format != image2.Format)
                    return MethodResult.CreateFailureSingle<MagickImage>("The images must be of the same format.");

                if (image1.Format != MagickFormat.Png)
                    return MethodResult.CreateFailureSingle<MagickImage>("The images must be in png format.");

                var createImageResult = Solver.CreateImage(MagickColors.White, image1.Width + image2.Width, image1.Height, false);
                if (createImageResult.Success == false)
                    return MethodResult.CreateFailureSingle<MagickImage>(createImageResult.Message);

                var image = createImageResult.GetResult();
                if (image == null)
                    return MethodResult.CreateFailureSingle<MagickImage>("Unable to create the merged image.");

                image.Composite(image1, 0, 0);
                image.Composite(image2, image1.Width, 0);

                return MethodResult.CreateSuccessSingle(image);
            }
            catch (Exception ex)
            {
                return MethodResult.CreateFailureSingle<MagickImage>(ex.Message);
            }
        }

        /// <summary>
        /// Finds the character boxes in the captcha image and returns a list of x coordinates for the start and end of each character.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        private MethodResultSingle<List<(int, int)>> FindCharacterColumns(MagickImage image)
        {
            try
            {
                var pixels = image.GetPixels();
                var imageColumns = Enumerable.Range(0, image.Width).Select(x => Enumerable.Range(0, image.Height).Select(y => pixels.GetPixel(x, y).ToColor().R).ToList()).ToList();
                var imageCode = imageColumns.Select(column => column.Any(pixel => pixel == 0) ? 1 : 0).ToList();
                var xPoints = imageCode.Select((s, d) => new { s, d }).Where(item => item.s == 1).Select(item => item.d).ToList();
                var xCoords = xPoints.Where(x => !xPoints.Contains(x - 1) || !xPoints.Contains(x + 1)).ToList();
                if (xCoords.Count % 2 != 0)
                    xCoords.Insert(1, xCoords[0]);

                var characterBoxes = new List<(int, int)>();
                for (var i = 0; i < xCoords.Count; i += 2)
                {
                    var start = xCoords[i];
                    var end = Math.Min(xCoords[i + 1] + 1, image.Width - 1);
                    if (end - start <= MAXIMUM_CHARACTER_WIDTH)
                    {
                        characterBoxes.Add((start, end));
                    }
                    else
                    {
                        var twoCharacters = Enumerable.Range(start + 5, end - start - 10).ToDictionary(k => k, k => imageColumns[k].Count(v => v == 0));
                        var divider = twoCharacters.OrderBy(item => item.Value).First().Key + 5;
                        characterBoxes.AddRange(new[] { (start, start + divider), (start + divider + 1, end) });
                    }
                }

                return MethodResult.CreateSuccessSingle(characterBoxes);
            }
            catch (Exception ex)
            {
                return MethodResult.CreateFailureSingle<List<(int, int)>>(ex.Message);
            }
        }
        #endregion

        #region Static Helper Methods
        /// <summary>
        /// Attempts to solve a single letter. Returns method result with the character as a string.
        /// </summary>
        /// <param name="confidenceThreshold"></param>
        /// <returns>A method result that contains a string value of the solved character, or ? if unknown</returns>
        public static MethodResultSingle<string> SolveCharacter(byte[] characterImageBytes, double confidenceThreshold = 0.95)
        {
            try
            {
                if (characterImageBytes == null || characterImageBytes.Length <= 0)
                    return MethodResult.CreateFailureSingle<string>("Character image bytes are empty.");

                if (confidenceThreshold < 0 || confidenceThreshold > 1)
                    return MethodResult.CreateFailureSingle<string>("Confidence threshold must be between 0 and 1.");

                var predictionResults = SolverModel.PredictAllLabels(new SolverModel.ModelInput { ImageSource = characterImageBytes });
                var highestScore = 0.0;
                var highestScoreLabel = (char?)null;
                foreach (var predictionResult in predictionResults)
                {
                    if (predictionResult.Value > highestScore)
                    {
                        highestScore = predictionResult.Value;
                        highestScoreLabel = predictionResult.Key.ToUpper()?.FirstOrDefault() ?? '_';
                    }
                }

                return MethodResult.CreateSuccessSingle<string>((highestScore >= confidenceThreshold ? highestScoreLabel : '_').ToString() ?? "_");
            }
            catch (Exception ex)
            {
                return MethodResult.CreateFailureSingle<string>(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new instance of magic image, applying any necessary settings.
        /// </summary>
        /// <param name="captchaBytes"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static MethodResultSingle<MagickImage> CreateImage(string captchaPath, bool applyThreshold = true)
        {
            var image = new MagickImage(captchaPath);
            image.ColorSpace = ColorSpace.Gray;
            image.Format = MagickFormat.Png;
            if (applyThreshold)
                image.ColorThreshold(MagickColor.FromRgb(2, 2, 2), MagickColor.FromRgb(255, 255, 255));
            return MethodResult.CreateSuccessSingle<MagickImage>(image);
        }

        /// <summary>
        /// Creates a new instance of magic image, applying any necessary settings.
        /// </summary>
        /// <param name="captchaBytes"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static MethodResultSingle<MagickImage> CreateImage(byte[] captchaBytes, bool applyThreshold = true)
        {
            var image = new MagickImage(captchaBytes);
            image.ColorSpace = ColorSpace.Gray;
            image.Format = MagickFormat.Png;
            if (applyThreshold)
                image.ColorThreshold(MagickColor.FromRgb(2, 2, 2), MagickColor.FromRgb(255, 255, 255));
            return MethodResult.CreateSuccessSingle<MagickImage>(image);
        }

        /// <summary>
        /// Creates a new instance of magic image, applying any necessary settings.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static MethodResultSingle<MagickImage> CreateImage(IMagickColor<ushort> color, int width, int height, bool applyThreshold = true)
        {
            var image = new MagickImage(color, width, height);
            image.ColorSpace = ColorSpace.Gray;
            image.Format = MagickFormat.Png;
            if (applyThreshold)
                image.ColorThreshold(MagickColor.FromRgb(2, 2, 2), MagickColor.FromRgb(255, 255, 255));
            return MethodResult.CreateSuccessSingle<MagickImage>(image);
        }
        #endregion
    }
}
