using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingManagerTests
{
    private BoardTestFixture fixture;
    private EndingManager endingManager;
    private GameObject endingManagerObject;
    private Slider[] materialSliders;
    private TextMeshProUGUI materialCountText;

    private static readonly Color TieColor = Color.white;
    private static readonly Color NephiteWinColor = Color.blue;
    private static readonly Color LamaniteWinColor = Color.red;

    [SetUp]
    public void SetUp()
    {
        fixture = new BoardTestFixture();

        endingManagerObject = new GameObject("TestEndingManager");
        endingManager = endingManagerObject.AddComponent<EndingManager>();

        materialSliders = new[]
        {
            new GameObject("NephiteSlider").AddComponent<Slider>(),
            new GameObject("LamaniteSlider").AddComponent<Slider>(),
        };
        materialCountText = new GameObject("MaterialCountText").AddComponent<TextMeshProUGUI>();

        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var type = typeof(EndingManager);
        // pieceSpawner is declared (protected) on the base HoardEndingManager, not here.
        typeof(HoardEndingManager).GetField("pieceSpawner", flags).SetValue(endingManager, fixture.PieceSpawner);
        type.GetField("factionCounts", flags).SetValue(endingManager, new[] { 0, 0 });
        type.GetField("materialSliders", flags).SetValue(endingManager, materialSliders);
        type.GetField("materialCountText", flags).SetValue(endingManager, materialCountText);
        type.GetField("tieColor", flags).SetValue(endingManager, TieColor);
        type.GetField("nephiteWinColor", flags).SetValue(endingManager, NephiteWinColor);
        type.GetField("lamaniteWinColor", flags).SetValue(endingManager, LamaniteWinColor);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(materialSliders[0].gameObject);
        Object.DestroyImmediate(materialSliders[1].gameObject);
        Object.DestroyImmediate(materialCountText.gameObject);
        Object.DestroyImmediate(endingManagerObject);
        fixture.Teardown();
    }

    [Test]
    public void UpdateMaterial_ShowsTieColorWhenMaterialIsEven()
    {
        endingManager.UpdateMaterial(playerIndex: 0, materialValue: 3);
        endingManager.UpdateMaterial(playerIndex: 1, materialValue: 3);

        Assert.AreEqual(TieColor, materialCountText.color);
        Assert.AreEqual("0", materialCountText.text);
    }

    [Test]
    public void UpdateMaterial_ShowsNephiteColorWhenNephitesAreAhead()
    {
        endingManager.UpdateMaterial(playerIndex: 0, materialValue: 5);

        Assert.AreEqual(NephiteWinColor, materialCountText.color);
        Assert.AreEqual("5", materialCountText.text);
    }

    [Test]
    public void UpdateMaterial_ShowsLamaniteColorWhenLamanitesAreAhead()
    {
        endingManager.UpdateMaterial(playerIndex: 1, materialValue: 4);

        Assert.AreEqual(LamaniteWinColor, materialCountText.color);
        Assert.AreEqual("-4", materialCountText.text);
    }

    [Test]
    public void UpdateMaterial_NegativeValueFromACaptureLowersThatFactionsSlider()
    {
        endingManager.UpdateMaterial(playerIndex: 0, materialValue: 5);
        endingManager.UpdateMaterial(playerIndex: 0, materialValue: -2);

        Assert.AreEqual(3, materialSliders[0].value);
    }

    [Test]
    public void UpdateMaterial_RaisesTheSlidersMaxWhenMaterialExceedsIt()
    {
        materialSliders[0].maxValue = 1;

        endingManager.UpdateMaterial(playerIndex: 0, materialValue: 9);

        Assert.AreEqual(9, materialSliders[0].maxValue);
        Assert.AreEqual(9, materialSliders[0].value);
    }

    [Test]
    public void UpdateMaterial_FourPlayerFactionsShareASliderByModulo()
    {
        // playerIndex 2 is a second Nephite-faction player in 4-player mode;
        // its material should fold into the same (index 0) faction count/slider.
        endingManager.UpdateMaterial(playerIndex: 0, materialValue: 3);
        endingManager.UpdateMaterial(playerIndex: 2, materialValue: 2);

        Assert.AreEqual(5, materialSliders[0].value);
    }
}
