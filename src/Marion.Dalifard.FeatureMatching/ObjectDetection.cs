using OpenCvSharp;

namespace Marion.Dalifard.FeatureMatching;

public class ObjectDetection
{
    public async Task<IList<ObjectDetectionResult>> DetectObjectInScenesAsync(byte[]
        objectImageData, IList<byte[]> imagesSceneData)
    {
        List<ObjectDetectionResult> results = new List<ObjectDetectionResult>();
        
        var tasks = imagesSceneData.Select(sceneData =>
         Task.Run(() => DetectObjectInScene(objectImageData, sceneData))).ToList();
        
        foreach (var task in tasks)
        {
            var result = await task;
            results.Add(result);
        }
        
        return results;
    }
    
    private ObjectDetectionResult DetectObjectInScene(byte[] imageObjectData, byte[]
        imageSceneData)
    {
        using var imgobject = Mat.FromImageData(imageObjectData, ImreadModes.Color);
        using var imgScene = Mat.FromImageData(imageSceneData, ImreadModes.Color);
        using var orb = ORB.Create(10000);
        using var descriptors1 = new Mat();
        using var descriptors2 = new Mat();
        orb.DetectAndCompute(imgobject, null, out var keyPoints1, descriptors1);
        orb.DetectAndCompute(imgScene, null, out var keyPoints2, descriptors2);
        using var bf = new BFMatcher(NormTypes.Hamming, crossCheck: true);
        var matches = bf.Match(descriptors1, descriptors2);
        var goodMatches = matches
            .OrderBy(x => x.Distance)
            .Take(10)
            .ToArray();
        var srcPts = goodMatches.Select(m => keyPoints1[m.QueryIdx].Pt).Select(p => new
            Point2d(p.X, p.Y));
        var dstPts = goodMatches.Select(m => keyPoints2[m.TrainIdx].Pt).Select(p => new
            Point2d(p.X, p.Y));
        using var homography = Cv2.FindHomography(srcPts, dstPts, HomographyMethods.Ransac,
            5, null);
        int h = imgobject.Height, w = imgobject.Width;
        var img2Bounds = new[]
        {
            new Point2d(0, 0),
            new Point2d(0, h-1),
            new Point2d(w-1, h-1),
            new Point2d(w-1, 0),
        };
        var img2BoundsTransformed = Cv2.PerspectiveTransform(img2Bounds, homography);
        using var view = imgScene.Clone();
        var drawingPoints = img2BoundsTransformed.Select(p => (Point) p).ToArray();
        Cv2.Polylines(view, new []{drawingPoints}, true, Scalar.Red, 3);
        var imageResult = view.ToBytes(".png");
        
        return new ObjectDetectionResult()
        {
            ImageData = imageResult,
            Points = drawingPoints.Select(point => new ObjectDetectionPoint(){X=point.X,
                Y=point.Y} ).ToList()
        };
    }
}

