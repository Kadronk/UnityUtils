using System;
using UnityEditor;
using UnityEngine;

namespace Kadronk.SpriteAnimation.Editor
{
    [CustomEditor(typeof(SpriteAnimation))]
    public class SpriteAnimationEditor : UnityEditor.Editor
    {
        // Serialized Properties
        private SerializedProperty _propFrames;
        private SerializedProperty _propWrap;
        // GUI styles and contents
        private GUIStyle _styleCenteredLabel;
        private GUIContent _contActionOnWrap = new GUIContent("Action on Wrap");
        // Playback animation
        private Sprite _currentFrame;
        private int _currentFrameIndex;
        private float _frameTimer;
        private bool _isPlaying;
        private double _lastTime;
        private bool _pingPongForward = true;
        private float _cacheDelay;
        // Playback controls
        private bool _previewLoop;
        private bool _previewPingPong;
        // Tools
        private int _fps;
        
        private void OnEnable()
        {
            // Serialized Properties
            _propFrames = serializedObject.FindProperty("_frames");
            _propWrap = serializedObject.FindProperty("_wrap");
            // GUI styles
            _styleCenteredLabel = new GUIStyle(EditorStyles.boldLabel);
            _styleCenteredLabel.alignment = TextAnchor.MiddleCenter;
            // Playback
            SetCurrentFrame(0);
            _pingPongForward = true;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.ApplyModifiedProperties();
            
            DrawSeparator("Animation asset");

            EditorGUILayout.PropertyField(_propFrames);
            EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_propWrap, _contActionOnWrap);
            if (_propWrap.enumValueIndex == (int)WrapAction.SetAnimation)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_nextAnimation"));
            
            if (_propFrames.arraySize <= 0) return;
            
            if (_currentFrame == null)
                SetCurrentFrame(0);
            
            // ---- TOOLS ----
            
            EditorGUILayout.Space();
            DrawSeparator("Tools");

            EditorGUILayout.BeginHorizontal();
            _fps = EditorGUILayout.IntField("Set FPS on all frames :", _fps);
            if (_fps <= 0)
                _fps = 1;
            if (GUILayout.Button("Set FPS"))
            {
                for (int i = 0; i < _propFrames.arraySize; i++)
                {
                    _propFrames.GetArrayElementAtIndex(i).FindPropertyRelative("_delay").floatValue = 1.0f / _fps;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // ---- PLAYBACK PREVIEW ----
            
            EditorGUILayout.Space();
            DrawSeparator("Preview");
            
            GUILayoutOption[] layoutOptions = new GUILayoutOption[1];
            if (Screen.width < Screen.height)
                layoutOptions[0] = GUILayout.Width(Screen.width);
            else
                layoutOptions[0] = GUILayout.Height(Screen.height);
            Rect rectFrame = GUILayoutUtility.GetAspectRect(_currentFrame.rect.width / _currentFrame.rect.height, layoutOptions);
            Rect rectFrameTexCoords = _currentFrame.textureRect;
            rectFrameTexCoords.x /= _currentFrame.textureRect.width;
            rectFrameTexCoords.width /= _currentFrame.textureRect.width;
            rectFrameTexCoords.y /= _currentFrame.textureRect.height;
            rectFrameTexCoords.height /= _currentFrame.textureRect.height;
            GUI.DrawTextureWithTexCoords(rectFrame, _currentFrame.texture, rectFrameTexCoords);

            // ---- PLAYBACK CONTROLS ----

            Rect rectTemp = GUILayoutUtility.GetRect(Screen.width, Screen.height, EditorGUIUtility.singleLineHeight * 2.0f, EditorGUIUtility.singleLineHeight * 2.0f);
            rectTemp.x += rectTemp.width * 0.33f;
            rectTemp.width *= 0.33f;
            if (GUI.Button(new Rect(rectTemp.x, rectTemp.y, rectTemp.width * 0.33f, rectTemp.height), "←") && IsFrameOffsetWithinRange(-1))
                SetCurrentFrame(_currentFrameIndex-1);
            if (GUI.Button(new Rect(rectTemp.x + rectTemp.width * 0.33f, rectTemp.y, rectTemp.width * 0.33f, rectTemp.height), _isPlaying ? "▌▌" : "▶"))
            {
                _isPlaying = !_isPlaying;
                if (_isPlaying)
                {
                    if (_currentFrameIndex >= _propFrames.arraySize - 1)
                        SetCurrentFrame(0);
                    RefreshLastTime();
                }
            }
            if (GUI.Button(new Rect(rectTemp.x + rectTemp.width * 0.66f, rectTemp.y, rectTemp.width * 0.33f, rectTemp.height), "→") && IsFrameOffsetWithinRange(1))
                SetCurrentFrame(_currentFrameIndex+1);
            int frameIndexFromSlider = EditorGUILayout.IntSlider(_currentFrameIndex, 0, _propFrames.arraySize - 1);
            if (frameIndexFromSlider != _currentFrameIndex)
                SetCurrentFrame(frameIndexFromSlider);

            EditorGUILayout.BeginHorizontal();
            _previewLoop = EditorGUILayout.ToggleLeft("Loop", _previewLoop);
            if (_previewLoop)
                _previewPingPong = EditorGUILayout.ToggleLeft("Ping pong", _previewPingPong);
            EditorGUILayout.EndHorizontal();

            // ---- PLAYBACK ANIMATION (no gui code) ----
            
            if (_isPlaying == false) return;
            
            if ((_previewLoop == false || _previewPingPong == false) && _pingPongForward == false)
                _pingPongForward = true;

            if (Event.current.type.Equals(EventType.Repaint))
            {
                if (_frameTimer > _cacheDelay)
                {
                    _frameTimer -= _cacheDelay;
                    if ((_pingPongForward && IsFrameOffsetWithinRange(1) == false) || (_pingPongForward == false && IsFrameOffsetWithinRange(-1) == false))
                    {
                        if (_previewLoop)
                        {
                            if (_previewPingPong)
                            {
                                _pingPongForward = !_pingPongForward;
                                NextFrame();
                            }
                            else
                                SetCurrentFrame(0);
                        }
                        else
                            _isPlaying = false;
                    }
                    else
                        NextFrame();
                }
                _frameTimer += (float)(EditorApplication.timeSinceStartup - _lastTime);
                RefreshLastTime();
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return _isPlaying;
        }

        void SetCurrentFrame(int frameIndex)
        {
            if (frameIndex >= _propFrames.arraySize)
            {
                _currentFrameIndex = 0;
                _currentFrame = null;
                _cacheDelay = 0.0f;
                return;
            }

            SerializedProperty propFrameElement = _propFrames.GetArrayElementAtIndex(frameIndex);
            
            _currentFrameIndex = frameIndex;
            _currentFrame = propFrameElement.FindPropertyRelative("_sprite").objectReferenceValue as Sprite;
            _cacheDelay = propFrameElement.FindPropertyRelative("_delay").floatValue;
        }

        void RefreshLastTime()
        {
            _lastTime = EditorApplication.timeSinceStartup;
        }
        
        void NextFrame()
        {
            SetCurrentFrame(_currentFrameIndex + (_pingPongForward ? 1 : -1));
        }

        bool IsFrameOffsetWithinRange(int offset)
        {
            if (offset < 0)
                return _currentFrameIndex + offset >= 0;
            if (offset > 0)
                return _currentFrameIndex + offset < _propFrames.arraySize;
            return true;
        }

        void DrawSeparator(string label)
        {
            Vector2 textSize = _styleCenteredLabel.CalcSize(new GUIContent(label));
            Rect rect = GUILayoutUtility.GetRect(Screen.width, Screen.width, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);
            Rect line = new Rect(rect);
            line.width -= (Screen.width + textSize.x) * 0.5f + 3.0f;
            EditorGUI.DrawRect(new Rect(rect.x, rect.center.y, line.width, 1.0f), Color.white);
            EditorGUI.LabelField(rect, label, _styleCenteredLabel);
            EditorGUI.DrawRect(new Rect(Screen.width, rect.center.y, -line.width, 1.0f), Color.white);
        }
    }
}
