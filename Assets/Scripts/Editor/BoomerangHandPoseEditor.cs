using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using Managers;
using UnityEditor;
using UnityEngine;

namespace BoomerangEditor
{
    public class BoomerangHandPoseEditor : EditorWindow
    {
        #region Self Variables

        #region Private Variables
        private const string BoomerangsAssetPath = "Assets/Resources/Data/CD_Boomerangs.asset";
        private const string PlayerAssetPath = "Assets/Resources/Data/CD_Player.asset";
        private const string PreviewObjectName = "__BoomerangHandPreview__";

        private CD_Boomerangs _cdBoomerangs;
        private CD_Player _cdPlayer;
        private int _selectedIndex = 0;
        private bool _autoPreviewOnSwitch = true;
        private GameObject _previewInstance;
        #endregion

        #endregion

        [MenuItem("Tools/Boomerang/Hand Grip Pose Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<BoomerangHandPoseEditor>("Hand Grip Pose");
            window.minSize = new Vector2(380f, 340f);
            window.Show();
        }

        [InitializeOnLoadMethod]
        private static void EnsureDefaultHandPosesInitialized()
        {
            EditorApplication.delayCall += () =>
            {
                var cdBoomerangs = AssetDatabase.LoadAssetAtPath<CD_Boomerangs>(BoomerangsAssetPath);
                if (cdBoomerangs == null || cdBoomerangs.Boomerangs == null)
                {
                    return;
                }

                var cdPlayer = AssetDatabase.LoadAssetAtPath<CD_Player>(PlayerAssetPath);
                Vector3 defaultPos = cdPlayer != null && cdPlayer.Data != null
                    ? cdPlayer.Data.BoomerangHandLocalPosition
                    : new Vector3(0.106f, 0.425f, -0.144f);
                Vector3 defaultRot = cdPlayer != null && cdPlayer.Data != null
                    ? cdPlayer.Data.BoomerangHandLocalEulerAngles
                    : new Vector3(19.138f, 52.601f, -105.943f);

                bool modified = false;
                for (int i = 0; i < cdBoomerangs.Boomerangs.Count; i++)
                {
                    var item = cdBoomerangs.Boomerangs[i];
                    if (item != null && item.HandLocalPosition == Vector3.zero && item.HandLocalEulerAngles == Vector3.zero)
                    {
                        item.HandLocalPosition = defaultPos;
                        item.HandLocalEulerAngles = defaultRot;
                        modified = true;
                    }
                }

                if (modified)
                {
                    EditorUtility.SetDirty(cdBoomerangs);
                    AssetDatabase.SaveAssets();
                }
            };
        }

        private void OnEnable()
        {
            LoadAssets();
        }

        private void OnDestroy()
        {
            CleanupPreviewInstance();
        }

        private void LoadAssets()
        {
            if (_cdBoomerangs == null)
            {
                _cdBoomerangs = AssetDatabase.LoadAssetAtPath<CD_Boomerangs>(BoomerangsAssetPath);
            }

            if (_cdPlayer == null)
            {
                _cdPlayer = AssetDatabase.LoadAssetAtPath<CD_Player>(PlayerAssetPath);
            }
        }

        private void OnGUI()
        {
            LoadAssets();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Boomerang Hand Grip Pose Editor", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. Pick a Boomerang and click 'Preview in BoomerangHand'.\n" +
                "2. Adjust its Position & Rotation inside BoomerangHand in the Scene View.\n" +
                "3. Click 'Capture Current Pose to CD_Boomerangs' to save.",
                MessageType.Info);

            if (_cdBoomerangs == null || _cdBoomerangs.Boomerangs == null || _cdBoomerangs.Boomerangs.Count == 0)
            {
                EditorGUILayout.HelpBox("CD_Boomerangs.asset not found or empty.", MessageType.Error);
                return;
            }

            EditorGUILayout.Space(6f);

            // Build dropdown labels
            int count = _cdBoomerangs.Boomerangs.Count;
            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, count - 1);

            string[] options = new string[count];
            for (int i = 0; i < count; i++)
            {
                var b = _cdBoomerangs.Boomerangs[i];
                options[i] = b != null ? $"[{b.Id}] {b.Title}" : $"[{i}] Null";
            }

            // Navigation row
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("<< Prev", GUILayout.Width(70f)))
            {
                _selectedIndex = (_selectedIndex - 1 + count) % count;
                if (_autoPreviewOnSwitch)
                {
                    SpawnPreviewInHand(_selectedIndex);
                }
            }

            int newIndex = EditorGUILayout.Popup(_selectedIndex, options);
            if (newIndex != _selectedIndex)
            {
                _selectedIndex = newIndex;
                if (_autoPreviewOnSwitch)
                {
                    SpawnPreviewInHand(_selectedIndex);
                }
            }

            if (GUILayout.Button("Next >>", GUILayout.Width(70f)))
            {
                _selectedIndex = (_selectedIndex + 1) % count;
                if (_autoPreviewOnSwitch)
                {
                    SpawnPreviewInHand(_selectedIndex);
                }
            }
            EditorGUILayout.EndHorizontal();

            _autoPreviewOnSwitch = EditorGUILayout.ToggleLeft("Auto-spawn preview when switching boomerang", _autoPreviewOnSwitch);

            EditorGUILayout.Space(8f);

            BoomerangItemData currentItem = _cdBoomerangs.Boomerangs[_selectedIndex];
            if (currentItem == null)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();
            Vector3 editedPos = EditorGUILayout.Vector3Field("Saved Hand Local Pos", currentItem.HandLocalPosition);
            Vector3 editedRot = EditorGUILayout.Vector3Field("Saved Hand Local Euler", currentItem.HandLocalEulerAngles);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_cdBoomerangs, "Edit Boomerang Hand Pose");
                currentItem.HandLocalPosition = editedPos;
                currentItem.HandLocalEulerAngles = editedRot;
                EditorUtility.SetDirty(_cdBoomerangs);

                Transform activeTarget = GetActiveHandBoomerangTransform();
                if (activeTarget != null)
                {
                    Undo.RecordObject(activeTarget, "Update Preview Hand Pose");
                    activeTarget.localPosition = editedPos;
                    activeTarget.localEulerAngles = editedRot;
                }
            }

            EditorGUILayout.Space(10f);

            GUI.backgroundColor = new Color(0.35f, 0.85f, 1f);
            if (GUILayout.Button("1. Preview Selected Boomerang in BoomerangHand", GUILayout.Height(32f)))
            {
                SpawnPreviewInHand(_selectedIndex);
            }

            EditorGUILayout.Space(4f);

            GUI.backgroundColor = new Color(0.4f, 1f, 0.55f);
            if (GUILayout.Button("2. Capture Current Scene Pose to CD_Boomerangs", GUILayout.Height(36f)))
            {
                CaptureActiveHandPose(_selectedIndex);
            }

            EditorGUILayout.Space(4f);
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Pose to Default (CD_Player)"))
            {
                ResetPoseToDefault(currentItem);
            }

            if (GUILayout.Button("Remove Preview Object"))
            {
                CleanupPreviewInstance();
            }
            EditorGUILayout.EndHorizontal();
        }

        public static Transform FindBoomerangHandTransform()
        {
            GameObject taggedObj = GameObject.FindWithTag("BoomerangHand");
            if (taggedObj != null)
            {
                return taggedObj.transform;
            }

            Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < allTransforms.Length; i++)
            {
                if (allTransforms[i].CompareTag("BoomerangHand") || allTransforms[i].name == "BoomerangHand")
                {
                    return allTransforms[i];
                }
            }

            return null;
        }

        private Transform GetActiveHandBoomerangTransform()
        {
            if (_previewInstance != null)
            {
                return _previewInstance.transform;
            }

            Transform hand = FindBoomerangHandTransform();
            if (hand != null && hand.childCount > 0)
            {
                for (int i = 0; i < hand.childCount; i++)
                {
                    Transform child = hand.GetChild(i);
                    if (child.GetComponent<BoomerangManager>() != null || child.name == PreviewObjectName)
                    {
                        return child;
                    }
                }
            }

            if (Selection.activeTransform != null && Selection.activeTransform.parent != null &&
                Selection.activeTransform.parent.CompareTag("BoomerangHand"))
            {
                return Selection.activeTransform;
            }

            return null;
        }

        public void SpawnPreviewInHand(int index)
        {
            LoadAssets();
            if (_cdBoomerangs == null || index < 0 || index >= _cdBoomerangs.Boomerangs.Count)
            {
                return;
            }

            BoomerangItemData data = _cdBoomerangs.Boomerangs[index];
            if (data == null || data.Prefab == null)
            {
                Debug.LogWarning($"[BoomerangHandPoseEditor] Prefab missing for boomerang index {index}.");
                return;
            }

            Transform handTransform = FindBoomerangHandTransform();
            if (handTransform == null)
            {
                Debug.LogWarning("[BoomerangHandPoseEditor] Could not find 'BoomerangHand' in the current scene. Make sure MainScene is open.");
                return;
            }

            CleanupPreviewInstance();

            if (Application.isPlaying)
            {
                var existingMgr = FindFirstObjectByType<BoomerangManager>();
                if (existingMgr != null)
                {
                    _previewInstance = existingMgr.gameObject;
                }
            }

            if (_previewInstance == null)
            {
                _previewInstance = (GameObject)PrefabUtility.InstantiatePrefab(data.Prefab);
                _previewInstance.name = PreviewObjectName;
            }

            _previewInstance.transform.SetParent(handTransform, false);
            _previewInstance.transform.localPosition = data.HandLocalPosition;
            _previewInstance.transform.localEulerAngles = data.HandLocalEulerAngles;

            Transform parentChild = _previewInstance.transform.Find("Parent");
            if (parentChild != null)
            {
                parentChild.gameObject.SetActive(true);
                parentChild.localScale = Vector3.one * data.SizeMultiplier;
            }

            Selection.activeGameObject = _previewInstance;
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }
        }

        public void CaptureActiveHandPose(int index)
        {
            LoadAssets();
            if (_cdBoomerangs == null || index < 0 || index >= _cdBoomerangs.Boomerangs.Count)
            {
                return;
            }

            Transform targetTransform = GetActiveHandBoomerangTransform();
            if (targetTransform == null)
            {
                Debug.LogWarning("[BoomerangHandPoseEditor] No boomerang found inside BoomerangHand to capture. Click 'Preview Selected Boomerang in BoomerangHand' first.");
                return;
            }

            BoomerangItemData data = _cdBoomerangs.Boomerangs[index];
            CaptureTransformToData(_cdBoomerangs, data, targetTransform);
        }

        public static void CaptureTransformToData(CD_Boomerangs asset, BoomerangItemData data, Transform boomerangRoot)
        {
            if (asset == null || data == null || boomerangRoot == null)
            {
                return;
            }

            Undo.RecordObject(asset, $"Capture Hand Pose for {data.Title}");
            data.HandLocalPosition = boomerangRoot.localPosition;
            data.HandLocalEulerAngles = boomerangRoot.localEulerAngles;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();

            Debug.Log($"<color=#00FF99><b>[BoomerangHandPoseEditor]</b> Saved Hand Pose for <b>[{data.Id}] {data.Title}</b> -> Pos: {data.HandLocalPosition}, Rot: {data.HandLocalEulerAngles}</color>");
        }

        private void ResetPoseToDefault(BoomerangItemData item)
        {
            Vector3 defaultPos = _cdPlayer != null && _cdPlayer.Data != null
                ? _cdPlayer.Data.BoomerangHandLocalPosition
                : new Vector3(0.106f, 0.425f, -0.144f);
            Vector3 defaultRot = _cdPlayer != null && _cdPlayer.Data != null
                ? _cdPlayer.Data.BoomerangHandLocalEulerAngles
                : new Vector3(19.138f, 52.601f, -105.943f);

            Undo.RecordObject(_cdBoomerangs, "Reset Boomerang Hand Pose");
            item.HandLocalPosition = defaultPos;
            item.HandLocalEulerAngles = defaultRot;
            EditorUtility.SetDirty(_cdBoomerangs);
            AssetDatabase.SaveAssets();

            Transform activeTarget = GetActiveHandBoomerangTransform();
            if (activeTarget != null)
            {
                activeTarget.localPosition = defaultPos;
                activeTarget.localEulerAngles = defaultRot;
            }
        }

        private void CleanupPreviewInstance()
        {
            if (_previewInstance != null && !Application.isPlaying)
            {
                DestroyImmediate(_previewInstance);
                _previewInstance = null;
            }

            Transform hand = FindBoomerangHandTransform();
            if (hand != null && !Application.isPlaying)
            {
                List<GameObject> toDestroy = new List<GameObject>();
                for (int i = 0; i < hand.childCount; i++)
                {
                    Transform child = hand.GetChild(i);
                    if (child.name == PreviewObjectName)
                    {
                        toDestroy.Add(child.gameObject);
                    }
                }

                for (int i = 0; i < toDestroy.Count; i++)
                {
                    DestroyImmediate(toDestroy[i]);
                }
            }
        }
    }

    [CustomEditor(typeof(BoomerangManager))]
    public class BoomerangManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            BoomerangManager manager = (BoomerangManager)target;
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Hand Grip Pose Tools", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Hand Pose Editor"))
            {
                BoomerangHandPoseEditor.ShowWindow();
            }

            if (GUILayout.Button("Capture Current Hand Pose"))
            {
                CaptureFromManager(manager);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void CaptureFromManager(BoomerangManager manager)
        {
            var cdBoomerangs = AssetDatabase.LoadAssetAtPath<CD_Boomerangs>("Assets/Resources/Data/CD_Boomerangs.asset");
            if (cdBoomerangs == null || cdBoomerangs.Boomerangs == null)
            {
                Debug.LogWarning("[BoomerangManagerEditor] Could not load CD_Boomerangs.asset.");
                return;
            }

            BoomerangItemData item = null;

            // 1. Try matching by Prefab source (works in Edit Mode for Prefab instances)
            GameObject prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(manager.gameObject);
            if (prefabSource != null)
            {
                for (int i = 0; i < cdBoomerangs.Boomerangs.Count; i++)
                {
                    if (cdBoomerangs.Boomerangs[i] != null && cdBoomerangs.Boomerangs[i].Prefab == prefabSource)
                    {
                        item = cdBoomerangs.Boomerangs[i];
                        break;
                    }
                }
            }

            // 2. Fallback to active BoomerangStoreManager in Play Mode
            if (item == null && Application.isPlaying)
            {
                var storeMgr = Object.FindFirstObjectByType<BoomerangStoreManager>();
                int equippedId = storeMgr != null ? storeMgr.SelectedItemId : 0;
                item = cdBoomerangs.GetBoomerang(equippedId);
            }

            // 3. Fallback to ID 0
            if (item == null)
            {
                item = cdBoomerangs.GetBoomerang(0);
            }

            if (item == null)
            {
                Debug.LogWarning("[BoomerangManagerEditor] Could not resolve BoomerangItemData for this Boomerang.");
                return;
            }

            BoomerangHandPoseEditor.CaptureTransformToData(cdBoomerangs, item, manager.transform);
        }
    }
}
