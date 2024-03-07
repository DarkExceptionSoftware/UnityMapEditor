using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PMA1980.MapGenerator
{
    public enum TILEID
    {
        VACUUM = 0,
        FLOOR = 1,
        WALL = 2,
    }

    public enum DIRECTION
    { NORTH = 0, EAST = 90, SOUTH = 180, WEST = 270 }

    public class TileInfo
    {
        public string name;
        public TILEID tile;
        public TILEID[] tileid;
        public int instance;
        public DIRECTION direction;

        public TileInfo()
        {
            this.tile = TILEID.VACUUM;
            this.name = "vacuum";
            this.tileid = new TILEID[] { TILEID.VACUUM, TILEID.VACUUM, TILEID.VACUUM, TILEID.VACUUM };
            this.instance = 0;
            this.direction = DIRECTION.NORTH;
        }

        public TileInfo(string _name, TILEID _tile, TILEID[] _tileID, int _instance, DIRECTION _direction)
        {
            this.name = _name;
            this.tile = _tile;
            this.tileid = _tileID;
            this.instance = _instance;
            this.direction = _direction;
        }
    }

    // Collection of Person objects. This class
    // implements IEnumerable so that it can be used
    // with ForEach syntax.
    public class Tiles : IEnumerable
    {
        private TileInfo[] _tiles;
        public Tiles(TileInfo[] pArray)
        {
            _tiles = new TileInfo[pArray.Length];

            for (int i = 0; i < pArray.Length; i++)
            {
                _tiles[i] = pArray[i];
            }
        }

        // Implementation for the GetEnumerator method.
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public TileEnum GetEnumerator()
        {
            return new TileEnum(_tiles);
        }
    }

    // When you implement IEnumerable, you must also implement IEnumerator.
    public class TileEnum : IEnumerator
    {
        public TileInfo[] _tiles;

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int position = -1;

        public TileEnum(TileInfo[] list)
        {
            _tiles = list;
        }

        public bool MoveNext()
        {
            position++;
            return (position < _tiles.Length);
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public TileInfo Current
        {
            get
            {
                try
                {
                    return _tiles[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}