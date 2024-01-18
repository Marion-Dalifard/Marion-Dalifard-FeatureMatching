using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;

namespace Marion.Dalifard.FeatureMatching.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class FeatureMatchingController : ControllerBase
{
    private readonly ObjectDetection _objectDetection;
    public FeatureMatchingController( ObjectDetection objectDetection)
    {
        _objectDetection = objectDetection;
    }

    [HttpPost]
    public async Task<IActionResult> MatchFeatures([FromForm] IFormFileCollection files)
    {
        if (files.Count < 2)
            return BadRequest("Au moins deux fichiers sont requis.");

        var objectImageData = await ReadImageDataAsync(files[0]);

        var sceneImagesData = new List<byte[]>();
        for (int i = 1; i < files.Count; i++)
        {
            var sceneImageData = await ReadImageDataAsync(files[i]);
            sceneImagesData.Add(sceneImageData);
        }

        var detectionResults = await _objectDetection.DetectObjectInScenesAsync(objectImageData, sceneImagesData);
        
        return File(detectionResults[0].ImageData, "image/png");
        
        // Pour retourner un zip avec tout les fichiers et ne pas renvoyer que la première image
        
        /*using (var memoryStream = new MemoryStream())
        {
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                for (int i = 0; i < detectionResults.Count; i++)
                {
                    var result = detectionResults[i];
                    var zipEntry = archive.CreateEntry($"Scene{i + 1}.png", CompressionLevel.Fastest);

                    using (var zipEntryStream = zipEntry.Open())
                    {
                        await zipEntryStream.WriteAsync(result.ImageData, 0, result.ImageData.Length);
                    }
                }
            }
            
            memoryStream.Position = 0;
            return File(memoryStream.ToArray(), "application/zip", "MatchedScenes.zip");
        }*/
    }

    private async Task<byte[]> ReadImageDataAsync(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
}