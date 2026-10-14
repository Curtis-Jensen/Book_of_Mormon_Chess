using System;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// One-shot layout pass over the main menu scene: vertical main menu, tabbed Settings
// page, gold-plate styling. Run it from Tools > Menu Rework. It leaves the scene
// unsaved so the result can be reviewed (and discarded) before committing to it.
public static class MenuReworkBuilder
{
    const string ScenePath = "Assets/Scenes/BOM Main Menu.unity";
    const string SpriteDir = "Assets/Prefabs/Menus/";
    const string OutDir = "Library/MenuRework";
    const float GoldBorderPx = 118f;
    const float DarkBorderPx = 12f;

    static readonly Vector2 TopLeft = new Vector2(0f, 1f);
    static readonly Vector2 MidLeft = new Vector2(0f, 0.5f);
    static readonly Vector2 MidRight = new Vector2(1f, 0.5f);
    static readonly Vector2 Mid = new Vector2(0.5f, 0.5f);

    static readonly Color Brown = new Color(0.23f, 0.145f, 0.016f);
    static readonly Color Gold = new Color(0.94f, 0.81f, 0.46f);
    static readonly Color Pale = new Color(0.965f, 0.906f, 0.706f);
    static readonly Color Gray = new Color(0.68f, 0.7f, 0.76f);
    static readonly Color Line = new Color(0.784f, 0.588f, 0.165f);

    static Sprite goldPlate, darkPlate, frame, diamond, goldFill, gem;
    static TMP_FontAsset cinzel;
    static StringBuilder log;

    [MenuItem("Tools/Menu Rework/Apply to Main Menu Scene")]
    static void Run()
    {
        log = new StringBuilder();
        Directory.CreateDirectory(OutDir);
        try
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Exit Play Mode first.");

            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                if (!EditorSceneManager.GetSceneAt(i).isDirty) continue;
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                break;
            }

            string backup = OutDir + "/BOM Main Menu.before-rework.unity";
            if (!File.Exists(backup)) File.Copy(ScenePath, backup);

            ImportSprites();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvas = scene.GetRootGameObjects().First(g => g.name == "Canvas").transform;
            if (FindChild(canvas.Find("Mode Select Panel"), "\"Play\"") != null) throw new Exception("The rework is already applied to this scene.");

            var main = Child(canvas, "Mode Select Panel");
            var settings = Child(canvas, "Settings Panel");
            var chapters = Child(canvas, "Chapters Panel");

            BuildFrame(canvas);
            BuildMain(main, settings);
            BuildSettings(settings, main);
            var play = BuildPlay(canvas, main, chapters, scene);
            Child(chapters, "\"Settings\"").gameObject.SetActive(false);

            // Last Stand starts from the Play menu now, so the chapter list is training only
            // and its section label becomes the screen title
            Child(chapters, "Nephites' Last Stand").gameObject.SetActive(false);
            var chaptersTitle = Child(chapters, "\"Training Chapters\"").GetComponent<TMP_Text>();
            chaptersTitle.text = "Training Chapters";
            Place(chaptersTitle, Mid, Mid, new Vector2(0f, 112f), new Vector2(520f, 40f));
            StyleText(chaptersTitle, 28f, Color.white, TextAlignmentOptions.Center, true, true);
            chaptersTitle.fontStyle |= FontStyles.UpperCase;
            chaptersTitle.characterSpacing = 4f;
            GoldGradient(chaptersTitle);

            // Direct property writes on prefab instances are dropped on the next scene
            // reload unless they are recorded as overrides
            foreach (var component in canvas.GetComponentsInChildren<Component>(true))
            {
                if (!PrefabUtility.IsPartOfPrefabInstance(component)) continue;
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                PrefabUtility.RecordPrefabInstancePropertyModifications(component.gameObject);
            }

            EditorSceneManager.MarkSceneDirty(scene);

            var canvasComponent = canvas.GetComponent<Canvas>();
            Capture(canvasComponent, OutDir + "/main.png");
            main.gameObject.SetActive(false);
            play.gameObject.SetActive(true);
            Capture(canvasComponent, OutDir + "/play.png");
            play.gameObject.SetActive(false);
            chapters.gameObject.SetActive(true);
            Capture(canvasComponent, OutDir + "/chapters.png");
            chapters.gameObject.SetActive(false);
            settings.gameObject.SetActive(true);
            Capture(canvasComponent, OutDir + "/settings-match.png");
            var tabs = settings.GetComponent<SettingsTabs>();
            tabs.Show(1);
            Capture(canvasComponent, OutDir + "/settings-audio.png");
            tabs.Show(0);
            settings.gameObject.SetActive(false);
            main.gameObject.SetActive(true);

            log.AppendLine("OK");
        }
        catch (Exception e)
        {
            log.AppendLine("ERROR: " + e);
            Debug.LogError("[MenuRework] " + e);
        }
        finally
        {
            File.WriteAllText(OutDir + "/log.txt", log.ToString());
        }
    }

    // ---------- assets ----------

    static void ImportSprites()
    {
        AssetDatabase.Refresh();
        SetSpriteImport("gold_plate.png", GoldBorderPx);
        SetSpriteImport("dark_plate.png", DarkBorderPx);
        SetSpriteImport("menu_frame.png", 48f);
        SetSpriteImport("gold_diamond.png", 0f);
        SetSpriteImport("gold_fill.png", 0f);

        goldPlate = LoadSprite("gold_plate.png");
        darkPlate = LoadSprite("dark_plate.png");
        frame = LoadSprite("menu_frame.png");
        diamond = LoadSprite("gold_diamond.png");
        goldFill = LoadSprite("gold_fill.png");
        gem = LoadSprite("Play button.png");
        cinzel = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Cinzel/Cinzel SDF.asset");
        if (cinzel == null) throw new Exception("Cinzel SDF font asset not found.");
    }

    static void SetSpriteImport(string file, float border)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(SpriteDir + file);
        if (importer == null) throw new Exception("Missing texture " + file);
        var wanted = new Vector4(border, border, border, border);
        if (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Single
            && importer.spriteBorder == wanted) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spriteBorder = wanted;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }

    static Sprite LoadSprite(string file)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteDir + file);
        if (sprite == null) throw new Exception("Missing sprite " + file);
        return sprite;
    }

    // ---------- screens ----------

    static void BuildFrame(Transform canvas)
    {
        var image = NewImage("Menu Frame", canvas, frame, Color.white);
        Stretch(image, 3f, 3f, 3f, 3f);
        Sliced(image, frame, 48f, 19f);

        // DefaultMenuPanel turns every Canvas child off on Awake unless it is listed
        var so = new SerializedObject(canvas.GetComponent<Ui.DefaultMenuPanel>());
        var list = so.FindProperty("initiallyActiveObjects");
        list.arraySize++;
        list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = image.gameObject;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void BuildMain(Transform main, Transform settings)
    {
        const float columnX = 179f;
        Stretch(main);

        var title = Child(main, "Title").GetComponent<TMP_Text>();
        Place(title, TopLeft, new Vector2(0.5f, 1f), new Vector2(columnX, -30f), new Vector2(320f, 96f));
        title.text = "<size=12.5>BOOK OF MORMON</size>\nCHESS";
        StyleText(title, 60f, Color.white, TextAlignmentOptions.Top, true, true);
        title.characterSpacing = 4f;
        GoldGradient(title);

        var ornament = NewRect("Title Ornament", main);
        Place(ornament, TopLeft, Mid, new Vector2(columnX, -128f), new Vector2(180f, 8f));
        Place(NewImage("Line Left", ornament, null, Line), Mid, Mid, new Vector2(-48f, 0f), new Vector2(80f, 1.25f));
        Place(NewImage("Diamond", ornament, diamond, Color.white), Mid, Mid, Vector2.zero, new Vector2(8f, 8f));
        Place(NewImage("Line Right", ornament, null, Line), Mid, Mid, new Vector2(48f, 0f), new Vector2(80f, 1.25f));

        var play = Child(main, "\"Single Player\"");
        var online = Child(main, "\"Multiplayer\"");

        // Clone before restyling so the copies start from the untouched button
        var settingsButton = Object.Instantiate(online.gameObject, main).transform;
        var quit = Object.Instantiate(online.gameObject, main).transform;

        StyleButton(play, "Play", true, new Vector2(columnX, -172f), new Vector2(238f, 60f), 20f);
        var gemImage = NewImage("Gem", play, gem, Color.white);
        Place(gemImage, MidLeft, Mid, new Vector2(46f, 0f), new Vector2(28f, 28f));
        gemImage.preserveAspect = true;

        StyleButton(online, "Play Online", false, new Vector2(columnX, -227.5f), new Vector2(212f, 36f), 12f);

        StyleButton(settingsButton, "Settings", false, new Vector2(columnX, -271f), new Vector2(212f, 36f), 12f);
        SetObject(settingsButton.GetComponent<PanelSwitcher>(), "newPanel", settings.gameObject);

        StyleButton(quit, "Quit", false, new Vector2(columnX, -314.5f), new Vector2(212f, 36f), 12f);
        var quitButton = quit.GetComponent<Button>();
        while (quitButton.onClick.GetPersistentEventCount() > 0) UnityEventTools.RemovePersistentListener(quitButton.onClick, 0);
        Object.DestroyImmediate(quit.GetComponent<PanelSwitcher>());
        UnityEventTools.AddPersistentListener(quitButton.onClick, quit.gameObject.AddComponent<QuitButton>().Quit);
    }

    static GameObject[] BuildSettings(Transform panel, Transform main)
    {
        Stretch(panel);
        var background = panel.GetComponent<Image>();
        background.enabled = true;
        background.sprite = null;
        background.type = Image.Type.Simple;
        background.color = new Color(0.027f, 0.035f, 0.063f, 0.78f);

        var title = Child(panel, "Title").GetComponent<TMP_Text>();
        Place(title, TopLeft, TopLeft, new Vector2(60f, -26f), new Vector2(320f, 36f));
        StyleText(title, 30f, Color.white, TextAlignmentOptions.TopLeft, true, true);
        title.fontStyle |= FontStyles.UpperCase;
        title.characterSpacing = 4f;
        GoldGradient(title);

        var back = Child(panel, "\"Back\"");
        StyleButton(back, "Back", false, Vector2.zero, Vector2.zero, 11f);
        Place(back, Vector2.zero, Vector2.zero, new Vector2(60f, 22f), new Vector2(90f, 28f));
        SetObject(back.GetComponent<PanelSwitcher>(), "newPanel", main.gameObject);

        var matchPage = NewRect("Match Page", panel);
        var audioPage = NewRect("Audio Page", panel);
        Stretch(matchPage);
        Stretch(audioPage);
        audioPage.SetSiblingIndex(0);
        matchPage.SetSiblingIndex(0);

        // --- Match page ---
        var board = Child(panel, "Board Size");
        Row(board, matchPage, 143f, 41f);
        var boardInner = Child(board, "Size Slider");
        Stretch(boardInner);
        var boardLabel = Child(boardInner, "Board Width / Height").GetComponent<TMP_Text>();
        boardLabel.text = "Board Size";
        Place(boardLabel, MidLeft, MidLeft, new Vector2(0f, 6f), new Vector2(300f, 16f));
        StyleText(boardLabel, 12.5f, Pale, TextAlignmentOptions.Left, true, true);
        Helper(board, "Width and height of the board, in squares.", 0f);
        var boardSlider = Child(boardInner, "Size Slider").GetComponent<Slider>();
        Place(boardSlider, MidRight, MidRight, new Vector2(-58f, 0f), new Vector2(190f, 20f));
        StyleSlider(boardSlider);
        var sizeInput = Child(board, "Size Input");
        Place(sizeInput, MidRight, MidRight, Vector2.zero, new Vector2(46f, 22f));
        Sliced(sizeInput.GetComponent<Image>(), darkPlate, DarkBorderPx, 4f);
        foreach (var text in sizeInput.GetComponentsInChildren<TMP_Text>(true))
        {
            text.fontSize = 12f;
            text.color = Gold;
            text.alignment = TextAlignmentOptions.Center;
        }
        Divider(board, 0.28f);

        var duos = Child(panel, "\"4 Players\"");
        Row(duos, matchPage, 184f, 41f);
        var duosLabel = duos.GetComponent<TMP_Text>();
        StyleText(duosLabel, 12.5f, Pale, TextAlignmentOptions.Left, true, true);
        duosLabel.margin = new Vector4(0f, 0f, 0f, 12f);
        Helper(duos, "Adds players 3 and 4 to the board.", 0f);
        var duosToggle = Child(duos, "Toggle");
        Place(duosToggle, MidRight, MidRight, Vector2.zero, new Vector2(40f, 24f));
        StyleSwitch(duosToggle.GetComponent<Toggle>());
        Divider(duos, 0.28f);

        var header = NewRect("Player Table Header", matchPage);
        Place(header, TopLeft, MidLeft, new Vector2(60f, -222f), new Vector2(680f, 22.5f));
        HeaderLabel(header, "Player", 0f);
        HeaderLabel(header, "Piece Style", 134f);
        HeaderLabel(header, "Color", 311f);
        HeaderLabel(header, "AI Player", 470f);
        Divider(header, 0.6f);

        PlayerRow(Child(panel, "\"Nephites\""), matchPage, "Nephites", 251.75f, null, 0);
        PlayerRow(Child(panel, "\"Lamanites\""), matchPage, "Lamanites", 289.25f, null, 0);
        PlayerRow(Child(panel, "\"Player 3\""), matchPage, "2 Nephi", 326.75f, "player3style", 0);
        PlayerRow(Child(panel, "\"Player 4\""), matchPage, "2 Laman", 364.25f, "player4style", 1);

        // --- Audio page ---
        AudioRow(Child(panel, "Master Volume"), audioPage, "Master Volume Slider", "Master Volume", 143f, 0f,
            "Scales music and sound effects together.");
        AudioRow(Child(panel, "Music"), audioPage, "Music Slider", "Music", 184f, 17.5f, null);
        AudioRow(Child(panel, "Sound Effects"), audioPage, "Sound Effects Slider", "Sound Effects", 225f, 17.5f, null);

        // --- Tabs ---
        var matchTab = NewButton("Match Tab", panel, "Match", 10.5f);
        var audioTab = NewButton("Audio Tab", panel, "Audio", 10.5f);
        Place(matchTab, TopLeft, TopLeft, new Vector2(60f, -66f), new Vector2(119f, 35f));
        Place(audioTab, TopLeft, TopLeft, new Vector2(186.5f, -66f), new Vector2(119f, 35f));

        var tabs = panel.gameObject.AddComponent<SettingsTabs>();
        tabs.tabs = new[] { matchTab, audioTab };
        tabs.pages = new[] { matchPage.gameObject, audioPage.gameObject };
        tabs.activeSprite = goldPlate;
        tabs.inactiveSprite = darkPlate;
        tabs.activePixelsPerUnitMultiplier = GoldBorderPx / 11f;
        tabs.inactivePixelsPerUnitMultiplier = DarkBorderPx / 4f;
        tabs.activeTextColor = Brown;
        tabs.inactiveTextColor = Gold;
        tabs.Show(0);

        return tabs.pages;
    }

    static Transform BuildPlay(Transform canvas, Transform main, Transform chapters, UnityEngine.SceneManagement.Scene scene)
    {
        var panel = NewRect("Play Panel", canvas);
        Stretch(panel);
        panel.SetSiblingIndex(main.GetSiblingIndex() + 1);
        panel.gameObject.AddComponent<Image>().color = new Color(0.027f, 0.035f, 0.063f, 0.55f);

        var title = NewText("Title", panel, "Play");
        Place(title, TopLeft, TopLeft, new Vector2(60f, -26f), new Vector2(320f, 36f));
        StyleText(title, 30f, Color.white, TextAlignmentOptions.TopLeft, true, true);
        title.fontStyle |= FontStyles.UpperCase;
        title.characterSpacing = 4f;
        GoldGradient(title);

        var detail = NewImage("Mode Detail", panel, darkPlate, Color.white);
        Place(detail, TopLeft, TopLeft, new Vector2(362.5f, -94f), new Vector2(377.5f, 202.5f));
        Sliced(detail, darkPlate, DarkBorderPx, 5f);

        var kind = NewText("Kind", detail.transform, "");
        Place(kind, TopLeft, TopLeft, new Vector2(25f, -20f), new Vector2(327f, 12f));
        StyleText(kind, 8f, Gold, TextAlignmentOptions.TopLeft, true, true);
        kind.fontStyle |= FontStyles.UpperCase;
        kind.characterSpacing = 10f;

        var modeTitle = NewText("Mode Title", detail.transform, "");
        Place(modeTitle, TopLeft, TopLeft, new Vector2(25f, -37f), new Vector2(327f, 28f));
        StyleText(modeTitle, 21f, Color.white, TextAlignmentOptions.TopLeft, true, true);
        GoldGradient(modeTitle);

        var description = NewText("Description", detail.transform, "");
        Place(description, TopLeft, TopLeft, new Vector2(25f, -74f), new Vector2(327f, 64f));
        StyleText(description, 11f, new Color(0.84f, 0.82f, 0.78f), TextAlignmentOptions.TopLeft, false, false);
        description.enableWordWrapping = true;

        var start = NewButton("Start", detail.transform, "Start", 13f);
        Place(start, Vector2.zero, Vector2.zero, new Vector2(25f, 20f), new Vector2(170f, 50f));
        Sliced(start.GetComponent<Image>(), goldPlate, GoldBorderPx, 14f);
        var startText = start.GetComponentInChildren<TMP_Text>();
        startText.color = Brown;
        startText.margin = new Vector4(26f, 0f, 0f, 0f);
        var gemImage = NewImage("Gem", start.transform, gem, Color.white);
        Place(gemImage, MidLeft, Mid, new Vector2(32f, 0f), new Vector2(22f, 22f));
        gemImage.preserveAspect = true;

        var back = NewButton("\"Back\"", panel, "Back", 11f);
        Place(back, Vector2.zero, Vector2.zero, new Vector2(60f, 22f), new Vector2(90f, 28f));
        Sliced(back.GetComponent<Image>(), darkPlate, DarkBorderPx, 4f);
        var backSwitcher = back.gameObject.AddComponent<PanelSwitcher>();
        SetObject(backSwitcher, "newPanel", main.gameObject);
        UnityEventTools.AddPersistentListener(back.onClick, backSwitcher.Switch);

        var menu = panel.gameObject.AddComponent<PlayMenu>();
        menu.sceneLoader = scene.GetRootGameObjects().Select(g => g.GetComponentInChildren<SceneLoader>(true)).First(l => l != null);
        menu.kindText = kind;
        menu.titleText = modeTitle;
        menu.descriptionText = description;
        menu.startText = startText;
        menu.startButton = start;
        menu.selectedSprite = goldPlate;
        menu.unselectedSprite = darkPlate;
        menu.selectedPixelsPerUnitMultiplier = GoldBorderPx / 13f;
        menu.unselectedPixelsPerUnitMultiplier = DarkBorderPx / 4f;
        menu.selectedTextColor = Brown;
        menu.unselectedTextColor = Gold;
        menu.modes = new[]
        {
            PlayMode(panel, 0, "Local Match", "Standard", "All Units", 0, null,
                "Nephites against Lamanites on one board. Set the board size and the AI players in Settings."),
            PlayMode(panel, 1, "Duos (2v2)", "Four players", "2v2 All Units", 1, null,
                "Two teams of two on one board. Players 3 and 4 use the colors and styles from Settings."),
            PlayMode(panel, 2, "Nephites' Last Stand", "Survival", "Nephites' Last Stand", -1, null,
                "Hold the board against the Lamanite horde. The game saves a high score for each board size."),
            PlayMode(panel, 3, "Training Chapters", "Learn the pieces", "", -1, chapters.gameObject,
                "Short chapters that teach one piece at a time. Win a chapter to unlock the next one."),
        };
        menu.modes[3].startLabel = "Chapters";
        menu.Select(0);

        // Play now opens this panel, and the chapter list returns to it
        SetObject(Child(main, "\"Play\"").GetComponent<PanelSwitcher>(), "newPanel", panel.gameObject);
        SetObject(Child(chapters, "\"Back\"").GetComponent<PanelSwitcher>(), "newPanel", panel.gameObject);

        panel.gameObject.SetActive(false);
        return panel;
    }

    static PlayMenu.Mode PlayMode(Transform panel, int index, string title, string kind, string sceneConfigName,
        int fourPlayerMode, GameObject panelToOpen, string description)
    {
        var button = NewButton(title, panel, title, 11.5f);
        Place(button, TopLeft, TopLeft, new Vector2(60f, -(94f + index * 52.5f)), new Vector2(250f, 45f));
        return new PlayMenu.Mode
        {
            listButton = button,
            kind = kind,
            title = title,
            description = description,
            sceneConfigName = sceneConfigName,
            fourPlayerMode = fourPlayerMode,
            panelToOpen = panelToOpen,
        };
    }

    // styleKey: the scene's Player 3 / Player 4 rows carry a copy of player 2's style
    // dropdown, so they need their own PlayerPrefs key. Null keeps the row's existing one.
    static void PlayerRow(Transform row, Transform page, string playerName, float y, string styleKey, int styleDefault)
    {
        Row(row, page, y, 37.5f);
        var label = row.GetComponent<TMP_Text>();
        label.text = playerName;
        StyleText(label, 12.5f, Pale, TextAlignmentOptions.Left, true, true);

        foreach (Transform child in row)
        {
            if (child.GetComponent<Toggle>() != null)
            {
                Place(child, MidLeft, MidLeft, new Vector2(470f, 0f), new Vector2(40f, 24f));
                StyleSwitch(child.GetComponent<Toggle>());
            }
            else if (child.name == "Color")
            {
                Place(child, MidLeft, MidLeft, new Vector2(311f, 0f), new Vector2(130f, 24f));
                StyleDropdownGroup(child);
            }
            else if (child.gameObject.activeSelf && child.GetComponentInChildren<DropdownPopulator>(true) != null)
            {
                Place(child, MidLeft, MidLeft, new Vector2(134f, 0f), new Vector2(150f, 24f));
                StyleDropdownGroup(child);
                if (styleKey == null) continue;

                var saver = child.GetComponentInChildren<DropdownSaver>(true);
                saver.toSave = styleKey;
                saver.defaultValue = styleDefault;
                saver.GetComponent<TMP_Dropdown>().SetValueWithoutNotify(styleDefault);
            }
        }
        Divider(row, 0.28f);
    }

    static void AudioRow(Transform row, Transform page, string sliderName, string labelName, float y, float indent, string helper)
    {
        Row(row, page, y, 41f);
        var label = Child(row, labelName).GetComponent<TMP_Text>();
        Place(label, MidLeft, MidLeft, new Vector2(indent, helper != null ? 6f : 0f), new Vector2(300f, 16f));
        StyleText(label, indent > 0f ? 11f : 12.5f, Pale, TextAlignmentOptions.Left, true, indent == 0f);
        if (helper != null) Helper(row, helper, indent);

        var slider = Child(row, sliderName).GetComponent<Slider>();
        Place(slider, MidRight, MidRight, new Vector2(-58f, 0f), new Vector2(190f, 20f));
        StyleSlider(slider);

        var value = NewText("Value", row, "100%");
        Place(value, MidRight, MidRight, Vector2.zero, new Vector2(46f, 20f));
        StyleText(value, 12.5f, Gold, TextAlignmentOptions.Right, true, true);
        var valueLabel = slider.gameObject.AddComponent<SliderValueLabel>();
        valueLabel.slider = slider;
        valueLabel.label = value;
        valueLabel.Show(slider.value);

        Divider(row, indent > 0f ? 0.28f : 0.6f);
    }

    // ---------- controls ----------

    static void StyleButton(Transform button, string label, bool primary, Vector2 position, Vector2 size, float fontSize)
    {
        button.name = $"\"{label}\"";
        Place(button, TopLeft, Mid, position, size);

        var image = button.GetComponent<Image>();
        if (primary) Sliced(image, goldPlate, GoldBorderPx, 17f);
        else Sliced(image, darkPlate, DarkBorderPx, 4f);

        var text = button.GetComponentInChildren<TMP_Text>(true);
        Stretch(text);
        text.text = label;
        StyleText(text, fontSize, primary ? Brown : Gold, TextAlignmentOptions.Center, true, true);
        text.fontStyle |= FontStyles.UpperCase;
        text.characterSpacing = 8f;

        HoverColors(button.GetComponent<Button>());
    }

    static void HoverColors(Selectable selectable)
    {
        var colors = selectable.colors;
        colors.normalColor = new Color(0.86f, 0.86f, 0.86f);
        colors.highlightedColor = Color.white;
        colors.selectedColor = colors.normalColor;
        colors.pressedColor = new Color(0.68f, 0.68f, 0.68f);
        selectable.colors = colors;
    }

    static void StyleSlider(Slider slider)
    {
        var background = (RectTransform)Child(slider.transform, "Background");
        background.anchorMin = MidLeft;
        background.anchorMax = MidRight;
        background.anchoredPosition = Vector2.zero;
        background.sizeDelta = new Vector2(0f, 8f);
        Sliced(background.GetComponent<Image>(), darkPlate, DarkBorderPx, 3f);

        var fillArea = (RectTransform)Child(slider.transform, "Fill Area");
        fillArea.anchorMin = MidLeft;
        fillArea.anchorMax = MidRight;
        fillArea.anchoredPosition = Vector2.zero;
        fillArea.sizeDelta = new Vector2(-4f, 5f);
        slider.fillRect.sizeDelta = Vector2.zero;
        var fill = slider.fillRect.GetComponent<Image>();
        fill.sprite = goldFill;
        fill.type = Image.Type.Simple;
        fill.color = Color.white;

        var handleArea = (RectTransform)Child(slider.transform, "Handle Slide Area");
        handleArea.anchorMin = MidLeft;
        handleArea.anchorMax = MidRight;
        handleArea.anchoredPosition = Vector2.zero;
        handleArea.sizeDelta = new Vector2(-14f, 14f);
        slider.handleRect.sizeDelta = new Vector2(14f, 0f);
        var handle = slider.handleRect.GetComponent<Image>();
        handle.sprite = diamond;
        handle.type = Image.Type.Simple;
        handle.color = Color.white;
        handle.preserveAspect = true;
        slider.targetGraphic = handle;
        HoverColors(slider);
    }

    static void StyleSwitch(Toggle toggle)
    {
        var track = Child(toggle.transform, "Background");
        Place(track, Mid, Mid, Vector2.zero, new Vector2(40f, 16f));
        var trackImage = track.GetComponent<Image>();
        Sliced(trackImage, darkPlate, DarkBorderPx, 3f);

        var knob = (RectTransform)Child(track, "Checkmark");
        knob.sizeDelta = new Vector2(13f, 13f);
        var knobImage = knob.GetComponent<Image>();
        knobImage.sprite = diamond;
        knobImage.type = Image.Type.Simple;

        // The knob stays visible in both states, so the Toggle must not fade it out
        toggle.graphic = null;
        toggle.targetGraphic = trackImage;
        HoverColors(toggle);

        var label = FindChild(toggle.transform, "Label");
        if (label != null) label.gameObject.SetActive(false);

        var visual = toggle.gameObject.AddComponent<ToggleSwitchVisual>();
        visual.toggle = toggle;
        visual.knob = knob;
        visual.knobGraphic = knobImage;
        visual.Apply(toggle.isOn);
    }

    static void StyleDropdownGroup(Transform group)
    {
        var headerText = FindChild(group, "Header Text");
        if (headerText != null) headerText.gameObject.SetActive(false);

        var dropdown = group.GetComponentInChildren<TMP_Dropdown>(true);
        Stretch(dropdown);
        Sliced(dropdown.GetComponent<Image>(), darkPlate, DarkBorderPx, 4f);
        HoverColors(dropdown);

        var caption = dropdown.captionText;
        Stretch(caption, 9f, 0f, 20f, 0f);
        caption.fontSize = 10.5f;
        caption.enableAutoSizing = false;
        caption.color = Gold;
        caption.alignment = TextAlignmentOptions.Left;
        caption.margin = Vector4.zero;

        var arrow = FindChild(dropdown.transform, "Arrow");
        if (arrow != null)
        {
            arrow.gameObject.SetActive(true);
            Place(arrow, MidRight, MidRight, new Vector2(-7f, 0f), new Vector2(10f, 10f));
            arrow.GetComponent<Image>().color = Gold;
        }

        if (dropdown.template != null)
        {
            dropdown.template.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.1f);
            if (dropdown.itemText != null)
            {
                dropdown.itemText.color = Pale;
                var item = dropdown.itemText.transform.parent;
                var itemBackground = FindChild(item, "Item Background");
                if (itemBackground != null) itemBackground.GetComponent<Image>().color = new Color(0.3f, 0.24f, 0.1f);
                var itemCheckmark = FindChild(item, "Item Checkmark");
                if (itemCheckmark != null) itemCheckmark.GetComponent<Image>().color = Gold;
                var itemToggle = item.GetComponent<Toggle>();
                if (itemToggle != null)
                {
                    var colors = itemToggle.colors;
                    colors.normalColor = new Color(0.25f, 0.25f, 0.25f);
                    colors.highlightedColor = Color.white;
                    colors.selectedColor = new Color(0.7f, 0.7f, 0.7f);
                    itemToggle.colors = colors;
                }
            }
        }
    }

    // ---------- small builders ----------

    static void Row(Transform row, Transform page, float y, float height)
    {
        row.SetParent(page, false);
        Place(row, TopLeft, MidLeft, new Vector2(60f, -y), new Vector2(680f, height));
    }

    static void Helper(Transform row, string text, float indent)
    {
        var helper = NewText("Helper", row, text);
        Place(helper, MidLeft, MidLeft, new Vector2(indent, -8f), new Vector2(420f, 12f));
        StyleText(helper, 9f, Gray, TextAlignmentOptions.Left, false, false);
    }

    static void HeaderLabel(Transform header, string text, float x)
    {
        var label = NewText(text, header, text);
        Place(label, MidLeft, MidLeft, new Vector2(x, 0f), new Vector2(160f, 14f));
        StyleText(label, 8f, Gold, TextAlignmentOptions.Left, true, true);
        label.fontStyle |= FontStyles.UpperCase;
        label.characterSpacing = 10f;
    }

    static void Divider(Transform row, float alpha)
    {
        var line = NewImage("Divider", row, null, new Color(Line.r, Line.g, Line.b, alpha));
        var rect = line.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, 0.6f);
    }

    static Button NewButton(string name, Transform parent, string label, float fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)) { layer = 5 };
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().type = Image.Type.Sliced;
        var text = NewText("Text (TMP)", go.transform, label);
        Stretch(text);
        StyleText(text, fontSize, Gold, TextAlignmentOptions.Center, true, true);
        text.fontStyle |= FontStyles.UpperCase;
        text.characterSpacing = 8f;
        var button = go.GetComponent<Button>();
        HoverColors(button);
        return button;
    }

    static Transform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform)) { layer = 5 };
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    static Image NewImage(string name, Transform parent, Sprite sprite, Color color)
    {
        var image = NewRect(name, parent).gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    static TMP_Text NewText(string name, Transform parent, string text)
    {
        var label = NewRect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        return label;
    }

    static void StyleText(TMP_Text text, float size, Color color, TextAlignmentOptions alignment, bool useCinzel, bool bold)
    {
        if (useCinzel) text.font = cinzel;
        text.enableAutoSizing = false;
        text.fontSize = size;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.color = color;
        text.alignment = alignment;
        text.margin = Vector4.zero;
        text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        text.raycastTarget = false;
    }

    static void GoldGradient(TMP_Text text)
    {
        var top = new Color(1f, 0.953f, 0.69f);
        var bottom = new Color(0.66f, 0.43f, 0.07f);
        text.enableVertexGradient = true;
        text.colorGradient = new VertexGradient(top, top, bottom, bottom);
    }

    static void Sliced(Image image, Sprite sprite, float borderPx, float borderUnits)
    {
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.fillCenter = true;
        image.pixelsPerUnitMultiplier = borderPx / borderUnits;
        image.color = Color.white;
    }

    static RectTransform Place(Component target, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        var rect = (RectTransform)target.transform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
        return rect;
    }

    static void Stretch(Component target, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
    {
        var rect = (RectTransform)target.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = Mid;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
        rect.localScale = Vector3.one;
    }

    static void SetObject(Object target, string property, Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(property).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Transform.Find treats '/' as a path separator, and some of these names contain one
    static Transform FindChild(Transform parent, string name)
    {
        if (parent == null) return null;
        foreach (Transform child in parent)
            if (child.name == name) return child;
        return null;
    }

    static Transform Child(Transform parent, string name)
    {
        var child = FindChild(parent, name);
        if (child == null) throw new Exception($"Missing '{name}' under '{parent.name}'");
        return child;
    }

    // ---------- preview capture ----------

    static void Capture(Canvas canvas, string file)
    {
        const int width = 1600, height = 900;
        var cameraObject = new GameObject("MenuReworkCaptureCamera") { hideFlags = HideFlags.HideAndDontSave };
        var texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        var image = new Texture2D(width, height, TextureFormat.RGB24, false);
        var scaler = canvas.GetComponent<CanvasScaler>();
        try
        {
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            camera.cullingMask = 1 << canvas.gameObject.layer;
            camera.targetTexture = texture;

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            scaler.enabled = false;
            scaler.enabled = true;
            Canvas.ForceUpdateCanvases();
            foreach (var text in canvas.GetComponentsInChildren<TMP_Text>()) text.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            camera.Render();

            var previous = RenderTexture.active;
            RenderTexture.active = texture;
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            RenderTexture.active = previous;
            File.WriteAllBytes(file, image.EncodeToPNG());
            log.AppendLine("captured " + file);
            camera.targetTexture = null;
        }
        finally
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = null;
            scaler.enabled = false;
            scaler.enabled = true;
            Object.DestroyImmediate(cameraObject);
            Object.DestroyImmediate(texture);
            Object.DestroyImmediate(image);
        }
    }
}
