using System;
using System.Collections.Generic;
using UnityEngine;

namespace MJ.Core
{
    [CreateAssetMenu(fileName = "TileSpriteLibrary", menuName = "Mahjong/Tile Sprite Library")]
    public class TileSpriteLibrarySO : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public Suit suit;
            public int rank;         // 1–9 for suited tiles, 0 for honors
            public Wind wind;       // only used if suit == Winds
            public Dragon dragon;   // only used if suit == Dragons
            public Sprite sprite;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        private Dictionary<TileType, Sprite> _lookup;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            _lookup = new Dictionary<TileType, Sprite>();
            foreach (var e in entries)
            {
                TileType type = e.suit switch
                {
                    Suit.Winds   => TileType.WindTile(e.wind),
                    Suit.Dragons => TileType.DragonTile(e.dragon),
                    _            => TileType.Suited(e.suit, e.rank),
                };
                
                if (!_lookup.ContainsKey(type))
                {
                    _lookup.Add(type, e.sprite);
                }
                else
                {
                    Debug.LogWarning($"[TileSpriteLibrary] Duplicate entry for {type}.");
                }
            }
        }

        public Sprite GetSpriteFor(TileType type)
        {
            if (_lookup == null || _lookup.Count == 0)
            {
                BuildLookup();
            }

            if (_lookup.TryGetValue(type, out var sprite))
            {
                return sprite;
            }

            Debug.LogWarning($"[TileSpriteLibrary] No sprite found for {type}.");
            return null;
        }
    }
}
