using UnityEngine;

namespace Kadronk.SpriteAnimation
{
    public enum WrapAction
    {
        Stop,
        Loop,
        PingPong,
        SetAnimation,
        Deactivate,
        Destroy
    }

    [CreateAssetMenu(fileName = "New Animation", menuName = "2D/Frame-by-frame Animation", order = 41)]
    public class SpriteAnimation : ScriptableObject
    {
        public SpriteFrame[] Frames { get => _frames; set => _frames = value; }

        public WrapAction Wrap { get => _wrap; set => _wrap = value; }
        public SpriteAnimation NextAnimation { get => _nextAnimation; set => _nextAnimation = value; }

        [SerializeField] private SpriteFrame[] _frames;
        // [Header("Properties")]
        [SerializeField] private WrapAction _wrap;
        [SerializeField] private SpriteAnimation _nextAnimation;
    }
}
