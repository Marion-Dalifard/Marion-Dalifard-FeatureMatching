using System.Reflection;
using System.Text.Json;
using Xunit;

namespace Marion.Dalifard.FeatureMatching.Tests;

public class FeatureMatchingUnitTest
{
    [Fact]
    public async Task ObjectShouldBeDetectedCorrectly()
    {
        var executingPath = GetExecutingPath();
        var imageScenesData = new List<byte[]>();
        foreach (var imagePath in Directory.EnumerateFiles(Path.Combine(executingPath,
                     "Scenes")))
        {
            var imageBytes = await File.ReadAllBytesAsync(imagePath);
            imageScenesData.Add(imageBytes);
        }
        var objectImageData = await File.ReadAllBytesAsync(Path.Combine(executingPath,
            "DALIFARD-Marion-object.jpg"));
        var detectObjectInScenesResults = await new
            ObjectDetection().DetectObjectInScenesAsync(objectImageData, imageScenesData);
        Assert.Equal("[{\"X\":1856,\"Y\":812},{\"X\":408,\"Y\":1170},{\"X\":759,\"Y\":2245},{\"X\":2092,\"Y\":1857}]",JsonSerializer.Serialize(detectObjectInScenesResults[0].Points));
        Assert.Equal("[{\"X\":730,\"Y\":2341},{\"X\":1555,\"Y\":3543},{\"X\":2457,\"Y\":2628},{\"X\":1431,\"Y\":1615}]",JsonSerializer.Serialize(detectObjectInScenesResults[1].Points));
    }
    private static string GetExecutingPath()
    {
        var executingAssemblyPath = Assembly.GetExecutingAssembly().Location;
        var executingPath = Path.GetDirectoryName(executingAssemblyPath);
        return executingPath;
    }
}