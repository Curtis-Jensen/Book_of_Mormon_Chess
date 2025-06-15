using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoardBoard : Board
{
    public GameObject pawn;
    public SpriteSet spriteSet;
    public ColorSet[] colorSets;

    protected override IEnumerator PhysicallyMovePiece(GameObject piece, Vector2 destination, Piece selectedPiece)
    {
        float time = 0;
        Vector3 startPosition = piece.transform.position;
        Vector3 endPosition = new Vector3(destination.x, destination.y, startPosition.z); // Preserve z-axis position
        var destinationTile = tiles[(int)destination.x, (int)destination.y];

        while (time < moveTime)
        {
            piece.transform.position = Vector3.Lerp(startPosition, endPosition, time / moveTime);
            time += Time.deltaTime;
            yield return null;
        }

        DestroyEnemyPiece(destinationTile);

        piece.transform.position = endPosition;

        AssignNewParent(destinationTile, selectedPiece);

        audioSource.Play();
        var spawnPoint = ChooseSpawn();
        SpawnPiece(spawnPoint);
        ChangeTurn();
    }

    private Vector2 ChooseSpawn()
    {
        int randomX = Random.Range(0, boardSize - 1);
        // Choose random x
        return new Vector2(randomX, boardSize - 1);
        
        
    }

    /// <summary>
    /// Changing things here?  Check Pawn.QueenPromotion() too.
    /// 
    /// 🧑🏻Designate the player for later
    /// 
    /// 🎨 Color the piece.  If it's a king, use the special king color
    /// 
    /// 🏗️ Instantiate the piece prefab at the specified tile 
    /// 
    /// 🔍 Retrieve the Piece component for configuration
    /// 
    /// 📛 Assign a descriptive name to the piece GameObject 
    /// 
    /// ⚖️ Set piece properties for team and player ownership  
    /// 
    /// 🤖 Register the piece with AI manager if player is AI 
    /// </summary>
    private void SpawnPiece(Vector2 spawnPoint)
    {
        var aiIndex = 1;
        var player = players[aiIndex]; //🧑🏻
        var pieceInstance =
        Instantiate(pawn, tiles[(int)spawnPoint.x, (int)spawnPoint.y].transform); //🏗️
        var spriteRenderer = pieceInstance.GetComponent<SpriteRenderer>();
        var pieceScript = pieceInstance.GetComponent<Pawn>(); //🔍
        pieceScript.queenSprite = spriteSet.GetType().GetField("Queen").GetValue(spriteSet) as Sprite;
        tiles[(int)spawnPoint.x, (int)spawnPoint.y].GetComponent<Tile>().piece = pieceScript;


        spriteRenderer.sprite =
            spriteSet.GetType().GetField(pawn.name).GetValue(spriteSet) as Sprite;
        pieceInstance.transform.localScale
            = new Vector3(spriteSet.transformScale, spriteSet.transformScale, 1);

        var colorSelection = PlayerPrefs.GetInt(player.name + "color");//🎨
        Debug.Log(colorSelection);

        spriteRenderer.color = colorSets[colorSelection].baseColor;

        pieceInstance.name = $"{pieceInstance.name} {player.name}";//📛

        pieceScript.teamOne = player.teamOne;//⚖️
        pieceScript.playerIndex = aiIndex;

        aiManager.aiPieces.Add(pieceScript);
    }
}
