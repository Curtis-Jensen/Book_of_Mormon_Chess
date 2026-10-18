using NUnit.Framework;
using UnityEngine;

public class AnnotationDrawerTests
{
    private AnnotationDrawer a;
    private AnnotationDrawer b;

    [SetUp]
    public void SetUp()
    {
        a = new GameObject("TileA").AddComponent<AnnotationDrawer>();
        b = new GameObject("TileB").AddComponent<AnnotationDrawer>();
        b.transform.position = new Vector3(3, 2, 0);
        // Clear any drag left over from another test
        AnnotationDrawer.Release(a);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(a.gameObject);
        Object.DestroyImmediate(b.gameObject);
    }

    [Test]
    public void DragToAnotherTile_PlacesArrowOnStartTile()
    {
        AnnotationDrawer.Press(a);
        AnnotationDrawer.Release(b);
        Assert.AreEqual(1, a.ArrowCount);
        Assert.AreEqual(0, b.ArrowCount);
    }

    [Test]
    public void SameDragTwice_ErasesArrow()
    {
        AnnotationDrawer.Press(a);
        AnnotationDrawer.Release(b);
        AnnotationDrawer.Press(a);
        AnnotationDrawer.Release(b);
        Assert.AreEqual(0, a.ArrowCount);
    }

    [Test]
    public void ReleaseWithoutPress_DoesNothing()
    {
        AnnotationDrawer.Release(b);
        Assert.AreEqual(0, a.ArrowCount);
        Assert.AreEqual(0, b.ArrowCount);
    }

    [Test]
    public void DragCanStartOnEitherTile_Repeatedly()
    {
        for (int i = 0; i < 5; i++)
        {
            AnnotationDrawer.Press(a);
            AnnotationDrawer.Release(b);
            AnnotationDrawer.Press(b);
            AnnotationDrawer.Release(a);
        }
        Assert.AreEqual(1, a.ArrowCount);
        Assert.AreEqual(1, b.ArrowCount);
    }
}
