﻿using SaveSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TileMap
{
    public static class MapReader
    {
        public static Transform tileParent;

        public static bool lodeing = false;

        public static int sizeX;
        public static int sizeY;
        public static Vector2Int Size => new Vector2Int(sizeX, sizeY);
        public static Vector2 mapHalfSize => new Vector2(sizeX / 2f, sizeY / 2f);

        private static Tile[] tiles;
        /// <summary>
        /// Number of tiles in a given grid position
        /// </summary>
        private static int[] numTile;
        public static List<Unit> implements = new List<Unit>();

        public static string mapName;
        public static string mapQuest;
        public static int mapModPathIndex;

        public static Texture2D[] spritePallate;

        public static Map Map => new Map(tiles, numTile, sizeX, sizeY, implements.ToArray());

        public static int numTiles
        {
            get
            {
                int size = 0;
                foreach (int i in numTile)
                {
                    size += i;
                }
                return size;
            }
        }

        public static Action MapGeneratedEvent;

        public static void GeneratePhysicalMap(Map map)
        {
            GenerateVirtualMap(map);
            GeneratePhysicalMap();
        }

        private static void GeneratePhysicalMap()
        {
            ResetTileParent();

            spritePallate = mapName != "" && mapQuest != "" ? SaveSystem.Tile.LoadPallate(mapModPathIndex, mapQuest) : SaveSystem.Tile.GetDebugPallate();

            foreach (Tile tile in tiles)
            {
                if (tile != null && tile.topHeight > 0)
                {
                    TileBehaviour.GetTileBehaviour(tile, tileParent);
                }
            }
            TileBehaviour.ClearCachedTiles();
            MapGeneratedEvent?.Invoke();
        }

        public static void GenerateVirtualMap(Map map)
        {
            ResetTileParent();

            mapName = map.name;
            mapQuest = map.quest;
            mapModPathIndex = map.modPathIndex;

            sizeX = map.sizeX;
            sizeY = map.sizeY;
            tiles = new Tile[map.sizeX * map.sizeY];
            numTile = map.numTile;
            implements.Clear();

            foreach (MapTilePair mapTilePair in map)
            {
                tiles[mapTilePair.index] = MapTile.ConvertTile(mapTilePair.mapTile, mapTilePair.tilePos.x, mapTilePair.tilePos.y);
                Unit.GetUnit(mapTilePair.mapTile.unit, mapModPathIndex);
            }
        }

        private static void ResetTileParent()
        {
            if (tileParent == null)
            {
                tileParent = new GameObject("Tile Parent").transform;
            }
            else
            {
                foreach (Transform transform in tileParent)
                {
                    if (transform.TryGetComponent(out TileBehaviour t) && !t.cached)
                    {
                        t.CacheTile();
                    }
                }
                tileParent.position = Vector3.zero;
                tileParent.localScale = Vector3.one;
                tileParent.rotation = Quaternion.identity;
            }
        }

        public static Tile[] GetTiles(int x, int y)
        {
            if (x >= sizeX || y >= sizeY || x < 0 || y < 0)
            {
                return new Tile[0];
            }
            int startIndex = 0;
            for (int i = 0; i < y * sizeX + x; i++)
            {
                startIndex += numTile[i];
            }

            int length = numTile[y * sizeX + x];
            Tile[] output = new Tile[length];
            for (int i = 0; i < length; i++)
            {
                output[i] = tiles[startIndex + i];
            }

            return output;
        }

        public static Tile GetTile(int x, int y, float minHeight, float MaxHeight)
        {
            Tile[] tiles = GetTiles(x, y);
            if (tiles.Length > 0)
            {
                foreach (Tile tile in tiles)
                {
                    if (tile != null && tile.bottomHeight >= minHeight && tile.topHeight <= MaxHeight)
                    {
                        return tile;
                    }
                }
            }
            return null;
        }

        public static Tile GetTile(TilePos pos) => GetTile(pos.x, pos.y, pos.z - 1, pos.z + 1);

        public static Vector3 GridToWorldSpace(TilePos posInGrid)
        {
            float xRealitive = (int)(posInGrid.x + .5f - mapHalfSize.x);
            float yRealitive = (int)(posInGrid.y + .5f - mapHalfSize.y);
            float x = xRealitive + tileParent.position.x;
            float y = yRealitive + tileParent.position.y;
            return new Vector3(x, y, posInGrid.z + tileParent.position.z);
        }

        public static Vector3 GridToWorldSpace(int x, int y, float z) => GridToWorldSpace(new TilePos(x, y, z));

        public static TilePos WorldToGridSpace(Vector3 posInWorld)
        {
            float xRealitive = posInWorld.x - tileParent.position.x;
            float yRealitive = posInWorld.y - tileParent.position.y;
            int x = (int)(xRealitive - .5f + mapHalfSize.x);
            int y = (int)(yRealitive - .5f + mapHalfSize.y);
            return new TilePos(x, y, posInWorld.z + tileParent.position.z);
        }

        public static TilePos WorldToGridSpace(float x, float y, float z) => WorldToGridSpace(new Vector3(x, y, z));

        public static Vector3 AlignWorldPosToGrid(Vector3 posInWorld) => new Vector3(Mathf.RoundToInt(posInWorld.x), Mathf.RoundToInt(posInWorld.y), posInWorld.z);

        public static TilePos ConstrainToMap(TilePos posInGrid)
        {
            posInGrid.x = Mathf.Clamp(posInGrid.x, 0, sizeX - 1);
            posInGrid.y = Mathf.Clamp(posInGrid.y, 0, sizeY - 1);
            return posInGrid;
        }

        public static Vector3 ConstrainToMap(Vector3 posInWorld) => GridToWorldSpace(ConstrainToMap(WorldToGridSpace(posInWorld)));

    }
}
