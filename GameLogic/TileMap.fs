﻿namespace GameLogic

open Bearded.Utilities.SpaceTime
open System.Collections.Generic

type public Tile() =
    let mutable content : List<GameObject> = new List<GameObject>()

    member this.Clear() =
        content.Clear()

    member this.Add(value : GameObject) =
        content.Add(value)

    member this.Get() =
        content :> seq<GameObject>


type public TileMap(originX : Unit, originY : Unit, width : Unit, height : Unit, rows : int, columns : int) =
    let tiles : array<array<Tile>> = [| for _ in 1..rows -> [| for _ in 1..columns -> new Tile() |] |]
    let tileWidth = width / (single columns)
    let tileHeight = height / (single rows)
    let y2Row (y:Unit) = (y - originY) / height * (single rows) |> int
    let x2Column (x:Unit) = (x - originX) / width * (single columns) |> int
    let tileForPosition (pos : Position2) = tiles.[y2Row pos.Y].[x2Column pos.X]

    let isPointWithinRadius (origin:Position2) (radius:Unit) (point:Position2) = true
    let isPointWithinMap (point:Position2) =
        Unit.op_LessThan(originX, point.X) &&
        Unit.op_LessThan(point.X, originX + width) && 
        Unit.op_LessThan(originY, point.Y) &&
        Unit.op_LessThan(point.Y, originY + height)

    // Does this do boxing?
    let capBetween (lower:int) (upper:int) (value:int) : int = min upper (max lower value)

    member public this.Update (objects : seq<GameObject>) =
        for row in tiles do
            for tile in row do
                tile.Clear()

        for obj in objects do
            if isPointWithinMap obj.Position then
                (tileForPosition obj.Position).Add(obj)
            else ()

    member public this.GetNeighbors (from : Position2) (radius : Unit) =
        let columnBound = capBetween 0 (columns - 1)
        let rowBound = capBetween 0 (rows - 1)
        let firstColumn = from.X - radius |> x2Column |> columnBound
        let lastColumn = from.X + radius |> x2Column |> columnBound
        let firstRow = from.Y - radius |> y2Row |> rowBound
        let lastRow = from.Y + radius |> y2Row |> rowBound
        seq {
        for col = firstColumn to lastColumn do
            for row = firstRow to lastRow do
                // TODO: We could add another filter here by checking on the point in the tile that is closest to `from`
                for candidate in tiles.[row].[col].Get() do 
                    if isPointWithinRadius from radius candidate.Position then yield candidate else ()
        }

    interface ITileMap with
        member this.GetNeighbors (from : Position2) (radius : Unit) =
            this.GetNeighbors from radius |> Seq.cast

