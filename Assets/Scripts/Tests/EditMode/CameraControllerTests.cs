using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class CameraControllerTests
{
    private GameObject cameraObject;
    private CameraController controller;
    private Camera cam;
    private GameObject background;
    private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

    [SetUp]
    public void SetUp()
    {
        cameraObject = new GameObject("TestCamera");
        controller = cameraObject.AddComponent<CameraController>();
        cam = cameraObject.GetComponent<Camera>();

        background = new GameObject("TestBackground");
        controller.background = background;

        // Start() (which wires cam = GetComponent<Camera>()) never fires for
        // AddComponent in EditMode tests -- set it directly, same as every other
        // fixture in this suite.
        typeof(CameraController).GetField("cam", PrivateInstance).SetValue(controller, cam);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(background);
        Object.DestroyImmediate(cameraObject);
    }

    private void InvokeSetCameraSize(int boardWidth) =>
        typeof(CameraController).GetMethod("SetCameraSize", PrivateInstance).Invoke(controller, new object[] { boardWidth });

    private void InvokeCenterBoard(int boardWidth) =>
        typeof(CameraController).GetMethod("CenterBoard", PrivateInstance).Invoke(controller, new object[] { boardWidth });

    [Test]
    public void SetCameraSize_OrthographicSizeIsHalfTheBoardWidth()
    {
        InvokeSetCameraSize(8);
        Assert.AreEqual(4f, cam.orthographicSize);
    }

    [Test]
    public void SetCameraSize_MaxZoomIsOneAndAHalfTimesTheInitialSize()
    {
        InvokeSetCameraSize(8);

        // maxZoom is private; confirm it indirectly via the public effect it has on
        // ZoomCamera's clamp -- but since that also reads Input, just check it was
        // computed by reading it back via reflection instead.
        var maxZoom = (float)typeof(CameraController).GetField("maxZoom", PrivateInstance).GetValue(controller);
        Assert.AreEqual(6f, maxZoom);
    }

    [Test]
    public void CenterBoard_OnAnEvenBoard_CentersOnTheHalfTileBoundary()
    {
        InvokeCenterBoard(8);
        Assert.AreEqual(new Vector3(3.5f, 3.5f, -10f), cameraObject.transform.position);
    }

    [Test]
    public void CenterBoard_OnAnOddBoard_CentersOnAWholeTile()
    {
        // Integer division in CenterBoard's centerLength means an odd board doesn't
        // get the same "-0.5" treatment an even board does -- this pins down that
        // actual (if perhaps unintended) behavior rather than an idealized one.
        InvokeCenterBoard(7);
        Assert.AreEqual(new Vector3(3f, 3f, -10f), cameraObject.transform.position);
    }

    [Test]
    public void CenterBoard_ScalesTheBackgroundToCoverTheWholeBoard()
    {
        background.transform.localScale = new Vector3(1f, 1f, 1f);

        InvokeCenterBoard(8);

        Assert.AreEqual(new Vector3(8f, 8f, 1f), background.transform.localScale);
    }

    [Test]
    public void CenterBoard_OffsetsTheBackgroundByTheConfiguredAmountTimesBoardWidth()
    {
        controller.backGroundOffset = new Vector3(0f, 0f, 1f);

        InvokeCenterBoard(8);

        Assert.AreEqual(new Vector3(3.5f, 3.5f, -10f) + new Vector3(0f, 0f, 8f), background.transform.position);
    }
}
