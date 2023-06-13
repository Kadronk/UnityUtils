using System;
using UnityEngine;

namespace Kadronk.SpriteAnimation
{
    [Serializable]
    public struct SpriteFrame
    {
        public Sprite Sprite => _sprite;
        public float Delay => _delay;
        
        [SerializeField] private Sprite _sprite;
        [SerializeField] private float _delay;
    }
}
