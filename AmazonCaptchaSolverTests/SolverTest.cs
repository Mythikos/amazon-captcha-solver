using AmazonCaptchaSolver;

namespace AmazonCaptchaSolverTests
{
    public class Tests
    {
        [Test]
        public void TestSolveLetters()
        {

            var captchaFiles = Directory.GetFiles("Test Data");
            Assert.That(captchaFiles.Length > 0, Is.True, "No captcha files found in test data.");

            foreach (var captchaFile in Directory.GetFiles("Test Data"))
            {
                if (File.Exists(captchaFile))
                {
                    var fileName = Path.GetFileNameWithoutExtension(captchaFile);
                    if (string.IsNullOrWhiteSpace(fileName) == false)
                    {
                        var solver = new Solver(captchaFile);
                        var result = solver.Solve();
                        if (result.Success == true)
                            Console.WriteLine($"Captcha solved: {result.GetResult()}");
                        else
                            Console.WriteLine($"Failed to solve captcha: {result.Message}");

                        Assert.That(result.Success == true, Is.True, $"Solver indicated failure: {result.Message}");
                        Assert.That(result.GetResult(), Is.EqualTo(fileName), "Solver failed to solve captcha correctly.");
                    }
                }
            }
        }
    }
}