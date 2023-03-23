using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Kadronk.Editor
{
    public class PrefabPlacer : EditorWindow
    {
        enum Axis
        {
            DontAlign,
            Forward,
            Back,
            Up,
            Down,
            Right,
            Left
        }
        
        private bool _enabled = false;
        [SerializeField] private GameObject[] _prefabs;
        // Position
        private Vector3 _positionOffset;
        // Rotation
        private Axis _alongNormal = Axis.DontAlign;
        private TransformComponentAxis[] _rotation = new TransformComponentAxis[3];
        // Scale
        private TransformComponentAxis[] _scale = new TransformComponentAxis[3];

        private bool[] _foldoutRot = new bool[3];
        private bool[] _foldoutScale = new bool[3];
        private Vector2 _scrollPos;
        private StringBuilder _sb = new StringBuilder();
        
        private SerializedObject _so;
        private SerializedProperty _prefabsProp; //ReorderableLists with target array did NOT work
                                                
        [MenuItem("Tools/Prefab placer")]
        public static void ShowWindow()
        {
            PrefabPlacer window = GetWindow<PrefabPlacer>("Prefab Placer");
            Rect pos = window.position;
            pos.height = 500.0f;
            window.position = pos;
        }
        
        private void OnEnable()
        {
            _so = new SerializedObject(this);
            _prefabsProp = _so.FindProperty("_prefabs");
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            _so.Dispose();
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            
            GUIStyle style = new GUIStyle(EditorStyles.miniButtonMid);
            style.normal.textColor = _enabled ? Color.green : Color.white;
            _sb.Clear();
            _sb.Append(_enabled ? "Enabled" : "Enable");
            if (EditorGUILayout.DropdownButton(new GUIContent(_sb.ToString()), FocusType.Keyboard, style))
                _enabled = !_enabled;
            EditorGUILayout.PropertyField(_prefabsProp, new GUIContent("Prefabs"));
            
            // Position
            EditorGUILayout.LabelField("Position", EditorStyles.boldLabel);
            _positionOffset = EditorGUILayout.Vector3Field("Offset", _positionOffset);
            
            // Rotation
            EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);
            _alongNormal = (Axis)EditorGUILayout.EnumPopup("Align along normal", _alongNormal);
            ThreeAxisPropertiesGUI(_foldoutRot, _rotation);
            
            // Scale
            EditorGUILayout.LabelField("Scale", EditorStyles.boldLabel);
            ThreeAxisPropertiesGUI(_foldoutScale, _scale);

            EditorGUILayout.EndScrollView();
                
            _so.ApplyModifiedProperties();
        }
        
        private void OnSceneGUI(SceneView sceneView)
        {
            if (_enabled == false || _prefabs.Length == 0) return;
            
            Event e = Event.current;
            
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Vector3 mousePos = e.mousePosition;
                float ppp = EditorGUIUtility.pixelsPerPoint;
                mousePos.y = sceneView.camera.pixelHeight - mousePos.y * ppp;
                mousePos.x *= ppp;
     
                Ray ray = sceneView.camera.ScreenPointToRay(mousePos);
                RaycastHit hit;
     
                if (Physics.Raycast(ray, out hit))
                {
                    int selectedIndex = Random.Range(0, _prefabs.Length);
                    GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(_prefabs[selectedIndex]);
                    Transform tf = go.transform;
                    
                     // Rotation
                     if (_alongNormal == Axis.Forward)
                         tf.forward = hit.normal;
                     else if (_alongNormal == Axis.Back)
                         tf.forward = -hit.normal;
                     else if (_alongNormal == Axis.Up)
                         tf.up = hit.normal;
                     else if (_alongNormal == Axis.Down)
                         tf.up = -hit.normal;
                     else if (_alongNormal == Axis.Right)
                         tf.right = hit.normal;
                     else if (_alongNormal == Axis.Left)
                         tf.right = -hit.normal;
                     else
                        tf.rotation = _prefabs[selectedIndex].transform.rotation;
                     ThreeAxisApplyProperties(_rotation, out float[] finalRot);
                     tf.Rotate(Vector3.up, finalRot[1]);
                     tf.Rotate(Vector3.right, finalRot[0]);
                     tf.Rotate(Vector3.forward, finalRot[2]);
                     
                     // Position
                     tf.position =  hit.point 
                                    + tf.right * (_prefabs[selectedIndex].transform.position.x + _positionOffset.x)
                                    + tf.up * (_prefabs[selectedIndex].transform.position.y + _positionOffset.y)
                                    + tf.forward * (_prefabs[selectedIndex].transform.position.z + _positionOffset.z);
                     
                     // Scale
                     ThreeAxisApplyProperties(_scale, out float[] finalScale);
                     tf.localScale = new Vector3(
                         _prefabs[selectedIndex].transform.localScale.x + finalScale[0],
                         _prefabs[selectedIndex].transform.localScale.y + finalScale[1],
                         _prefabs[selectedIndex].transform.localScale.z + finalScale[2]
                     );
                     
                    Undo.RegisterCreatedObjectUndo(go, "Placed prefab");
                }
                e.Use();
            }
        }

        void ThreeAxisPropertiesGUI(bool[] foldouts, TransformComponentAxis[] transformComponents)
        {
            for (int i = 0; i < 3; i++)
            {
                string axis = ((char)('X' + i)).ToString();
                foldouts[i] = EditorGUILayout.Foldout(foldouts[i], axis);
                if (foldouts[i])
                {
                    _sb.Clear();
                    transformComponents[i].Random = EditorGUILayout.Toggle("Randomized", transformComponents[i].Random);
                    _sb.Append("Offset");
                    if (transformComponents[i].Random)
                        _sb.Append(" min");
                    transformComponents[i].Value = EditorGUILayout.FloatField(_sb.ToString(), transformComponents[i].Value);
                    if (transformComponents[i].Random)
                    {
                        _sb.Remove(_sb.Length - 4, 4);
                        _sb.Append(" max");
                        transformComponents[i].ValueMax = EditorGUILayout.FloatField(_sb.ToString(), transformComponents[i].ValueMax);
                    }
                }
            }
        }

        void ThreeAxisApplyProperties(TransformComponentAxis[] transformComponents, out float[] finalComponent)
        {
            finalComponent = new float[3];
            for (int i = 0; i < 3; i++)
            {
                if (transformComponents[i].Random)
                    finalComponent[i] = Random.Range(transformComponents[i].Value, transformComponents[i].ValueMax);
                else
                    finalComponent[i] = transformComponents[i].Value;
            }
        }
    }

    [Serializable]
    public struct TransformComponentAxis
    {
        public bool Random;
        public float Value;
        public float ValueMax;
    }
}
