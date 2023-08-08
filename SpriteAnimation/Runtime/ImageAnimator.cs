using UnityEngine;
using UnityEngine.UI;

namespace Kadronk.SpriteAnimation
{
    public class ImageAnimator : MonoBehaviour
    {
        public bool IsPlaying { get => _isPlaying; set => _isPlaying = value; }
        
        [SerializeField] private Image _image;
        [Header("Runtime playback")]
        [SerializeField] private bool _isPlaying = true;
        [Header("Animations")]
        [SerializeField] private SpriteAnimationCollection _animationCollection;
        // Playback info
        private SpriteAnimation _currentAnimation;
        private int _currentFrameIndex;
        private float _frameTimer;
        private bool _pingPongForward = true;
        // "Cache"
        private float _cacheDelay = 0.0f;
        
        private void Awake()
        {
            SetCurrentAnimation(_animationCollection.DefaultAnimation);
        }

        private void Update()
        {
            if (_isPlaying == false) return;
            
            if (_frameTimer >= _cacheDelay)
            {
                _frameTimer -= _cacheDelay;
                if ((_pingPongForward && _currentFrameIndex + 1 >= _currentAnimation.Frames.Length) || (_pingPongForward == false && _currentFrameIndex - 1 < 0))
                {
                    switch (_currentAnimation.Wrap)
                    {
                        case WrapAction.Stop:
                            _isPlaying = false;
                            break;
                        case WrapAction.Loop:
                            SetCurrentFrame(0);
                            break;
                        case WrapAction.PingPong:
                            _pingPongForward = !_pingPongForward;
                            NextFrame();
                            break;
                        case WrapAction.SetAnimation:
                            SetCurrentAnimation(_currentAnimation.NextAnimation);
                            break;
                        case WrapAction.Deactivate:
                            SetCurrentFrame(0);
                            gameObject.SetActive(false);
                            break;
                        case WrapAction.Destroy:
                            Destroy(gameObject);
                            break;
                    }
                }
                else
                    NextFrame();
            }
            _frameTimer += Time.deltaTime;
        }

        public void SetCurrentAnimation(SpriteAnimation animation)
        {
            _currentAnimation = animation;
            SetCurrentFrame(0);
        }

        public void SetCurrentAnimation(int hashcode)
        {
            SetCurrentAnimation(_animationCollection[hashcode]);
        }

        private void SetCurrentFrame(int frameIndex)
        {
            _currentFrameIndex = frameIndex;
            _image.sprite = _currentAnimation.Frames[frameIndex].Sprite;
            _cacheDelay = _currentAnimation.Frames[frameIndex].Delay;
        }

        void NextFrame()
        {
            SetCurrentFrame(_currentFrameIndex + (_pingPongForward ? 1 : -1));
        }
    }
}
