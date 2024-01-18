using System.Text.Json;
using Marion.Dalifard.FeatureMatching;

if (args.Length < 2)
{
    Console.WriteLine("<path_to_object_image> <path_to_scene_image>");
    return;
}

string objectImagePath = args[0];
byte[] objectImageData = await File.ReadAllBytesAsync(objectImagePath);

List<byte[]> scenesData = new List<byte[]>();
for (int i = 1; i < args.Length; i++)
{
    scenesData.Add(await File.ReadAllBytesAsync(args[i]));
}

var objectDetection = new ObjectDetection();

var detectObjectInScenesResults = await objectDetection.DetectObjectInScenesAsync(objectImageData, scenesData);

foreach (var objectDetectionResult in detectObjectInScenesResults)
{
    Console.WriteLine($"Points: {JsonSerializer.Serialize(objectDetectionResult.Points)}");
}