

# Amazon Captcha Solver

Amazon Captcha Solver is a simple library that uses image manipulation along with ML.net AI image classification to solve amazon captcha images. The solver has a high-accuracy at this time, however, you can easily add new images to the training set to make it even more accurate. 

## Disclaimer

This project is for educational use only. It is not intended for any malicious or unauthorized use. I do not endorse or condone the use of this library for any activities that violate the terms of service of any website or service, including Amazon's.

## Usage

The solving library is pretty easy to use out of the box. Simply add a reference to the assembly and then use the following code:

```C#
    var solver = new Solver(<Image Path>)
    var result = solver.Solve()
    if (result.Success == true)
	    Console.WriteLine($"Captcha solved: {result.GetResult()}");
	else
	    Console.WriteLine($"Failed to solve captcha: {result.Message}");
```

## Training

To add training data to the model:

 1. Extract the training data set included to your training data path of choice. I usually would extract it to the Training Data folder in the project ("Tranining Data/A", "Tranining Data/B", etc).
 2. Get your training characters. You must split the images from a respective captcha image prior to adding them to their respective training set directory. You will want to use the solver's method of doing this to maintain consistency in the data set. This can easily be accomplished by: 

```C#
        var result = solver.GetCharacterImages();
        if (result.Success == true)
        {
            foreach (var image in iresult.GetResult())
            {
                File.WriteAllBytes(<Path>, image.ToByteArray());
            }
        }
```
    
3. Place your captcha character images into their corresponding training directory at the path you decided in step 1.
4. Open the model in ML.net in Visual Studio, and follow the steps to rebuild the model. Note, you will need to download the ML.net Model Builder that corresponds with your version of Visual Studio (project was made in VS 2022). 

And thats it. To ensure the model is still working, try running the test I've included in the project.