using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Hand-rolled mapping between GameDoc and Firestore's REST "typed value" document
// format (https://firebase.google.com/docs/firestore/use-rest-api), since the
// native Firebase SDK's [FirestoreData] attributes don't apply once we're talking
// to Firestore over plain HTTP (see CorrespondenceGameRepository).
public static class FirestoreJson
{
    public static string ToDocumentJson(GameDoc doc)
    {
        var fields = new JObject
        {
            ["players"] = EncodeStringArray(doc.players),
            ["playerNames"] = EncodeStringArray(doc.playerNames),
            ["boardSize"] = EncodeInt(doc.boardSize),
            ["backRowPrefabNames"] = EncodeStringArray(doc.backRowPrefabNames),
            ["currentTurnIndex"] = EncodeInt(doc.currentTurnIndex),
            ["status"] = EncodeString(doc.status),
            ["winnerIndex"] = EncodeInt(doc.winnerIndex),
            ["pieces"] = EncodePieces(doc.pieces),
            ["lastMove"] = EncodeLastMove(doc.lastMove)
        };

        var body = new JObject { ["fields"] = fields };
        return body.ToString(Formatting.None);
    }

    // Parses a Firestore REST document response (top-level {name, fields, createTime, updateTime}).
    public static GameDoc FromDocumentJson(string rawJson)
    {
        var response = JObject.Parse(rawJson);
        var fields = response["fields"] as JObject ?? new JObject();

        return new GameDoc
        {
            players = DecodeStringArray(fields["players"]),
            playerNames = DecodeStringArray(fields["playerNames"]),
            boardSize = DecodeInt(fields["boardSize"]),
            backRowPrefabNames = DecodeStringArray(fields["backRowPrefabNames"]),
            currentTurnIndex = DecodeInt(fields["currentTurnIndex"]),
            status = DecodeString(fields["status"]),
            winnerIndex = DecodeInt(fields["winnerIndex"]),
            pieces = DecodePieces(fields["pieces"]),
            lastMove = DecodeLastMove(fields["lastMove"])
        };
    }

    // A create response's "name" is the full resource path; the game code is just the last segment.
    public static string ExtractDocumentId(string rawJson)
    {
        var response = JObject.Parse(rawJson);
        var fullName = response["name"]?.ToString() ?? "";
        return fullName.Substring(fullName.LastIndexOf('/') + 1);
    }

    #region Encoding

    static JObject EncodeInt(int value) => new() { ["integerValue"] = value.ToString() };
    static JObject EncodeString(string value) => new() { ["stringValue"] = value ?? "" };
    static JObject EncodeBool(bool value) => new() { ["booleanValue"] = value };

    static JObject EncodeStringArray(List<string> values) => new()
    {
        ["arrayValue"] = new JObject
        {
            ["values"] = new JArray(values.Select(v => (JToken)EncodeString(v)))
        }
    };

    static JObject EncodePieces(List<PieceStateDto> pieces) => new()
    {
        ["arrayValue"] = new JObject
        {
            ["values"] = new JArray(pieces.Select(p => (JToken)new JObject
            {
                ["mapValue"] = new JObject
                {
                    ["fields"] = new JObject
                    {
                        ["prefabName"] = EncodeString(p.prefabName),
                        ["playerIndex"] = EncodeInt(p.playerIndex),
                        ["x"] = EncodeInt(p.x),
                        ["y"] = EncodeInt(p.y),
                        ["firstTurnTaken"] = EncodeBool(p.firstTurnTaken)
                    }
                }
            }))
        }
    };

    static JObject EncodeLastMove(LastMoveDto move) => new()
    {
        ["mapValue"] = new JObject
        {
            ["fields"] = new JObject
            {
                ["fromX"] = EncodeInt(move.fromX),
                ["fromY"] = EncodeInt(move.fromY),
                ["toX"] = EncodeInt(move.toX),
                ["toY"] = EncodeInt(move.toY)
            }
        }
    };

    #endregion

    #region Decoding

    static int DecodeInt(JToken field) => field == null ? 0 : int.Parse(field["integerValue"]?.ToString() ?? "0");
    static string DecodeString(JToken field) => field?["stringValue"]?.ToString() ?? "";
    static bool DecodeBool(JToken field) => field?["booleanValue"]?.ToObject<bool>() ?? false;

    static List<string> DecodeStringArray(JToken field)
    {
        var result = new List<string>();
        if (field?["arrayValue"]?["values"] is not JArray values) return result;

        foreach (var value in values) result.Add(value["stringValue"]?.ToString() ?? "");
        return result;
    }

    static List<PieceStateDto> DecodePieces(JToken field)
    {
        var result = new List<PieceStateDto>();
        if (field?["arrayValue"]?["values"] is not JArray values) return result;

        foreach (var value in values)
        {
            var pieceFields = value["mapValue"]?["fields"];
            result.Add(new PieceStateDto
            {
                prefabName = DecodeString(pieceFields?["prefabName"]),
                playerIndex = DecodeInt(pieceFields?["playerIndex"]),
                x = DecodeInt(pieceFields?["x"]),
                y = DecodeInt(pieceFields?["y"]),
                firstTurnTaken = DecodeBool(pieceFields?["firstTurnTaken"])
            });
        }

        return result;
    }

    static LastMoveDto DecodeLastMove(JToken field)
    {
        var moveFields = field?["mapValue"]?["fields"];
        if (moveFields == null) return new LastMoveDto(); // defaults to -1s -- no move yet

        return new LastMoveDto
        {
            fromX = DecodeInt(moveFields["fromX"]),
            fromY = DecodeInt(moveFields["fromY"]),
            toX = DecodeInt(moveFields["toX"]),
            toY = DecodeInt(moveFields["toY"])
        };
    }

    #endregion
}
