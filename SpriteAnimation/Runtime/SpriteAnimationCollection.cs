using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kadronk.SpriteAnimation
{
    [CreateAssetMenu(fileName = "New Animation Collection", menuName = "2D/Frame-by-frame Animation Collection", order = 41)]
    public class SpriteAnimationCollection : ScriptableObject, ISerializationCallbackReceiver
    {
        public SpriteAnimation this[int i] => _animations[i];
        public SpriteAnimation DefaultAnimation
        {
            get
            {
                if (_assets.Length <= 0)
                    throw new IndexOutOfRangeException();
                return _assets[0];
            }
        }

        #if UNITY_EDITOR
        [SerializeField] private string[] _IDs;
        #endif
        [SerializeField] [HideInInspector] private int[] _hashes;
        [SerializeField] private SpriteAnimation[] _assets;
        
        private Dictionary<int, SpriteAnimation> _animations = new Dictionary<int, SpriteAnimation>();

        public void OnBeforeSerialize()
        {
            #if UNITY_EDITOR
            if (_IDs == null)
                return;
            
            _hashes = new int[_IDs.Length];
            for (int i = 0; i < _IDs.Length; i++)
            {
                _hashes[i] = _IDs[i].GetHashCode();
            }
            #endif
        }

        public void OnAfterDeserialize()
        {
            _animations = new Dictionary<int, SpriteAnimation>(_hashes.Length);
            for (int i = 0; i < _hashes.Length; ++i) 
            {
                _animations.Add(_hashes[i], _assets[i]);
            }
        }
    }
}

