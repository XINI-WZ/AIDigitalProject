using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UniVRM10;

namespace DigitalHuman.Animation.Editor
{
    public class ProceduralDanceSetup : EditorWindow
    {
        [MenuItem("DigitalHuman/Setup Procedural Dance Scene")]
        public static void ShowWindow()
        {
            GetWindow<ProceduralDanceSetup>("Procedural Dance Setup");
        }

        private Vrm10Instance _vrmInstance;
        private Animator _animator;
        private ProceduralDanceGenerator _danceGenerator;

        void OnGUI()
        {
            GUILayout.Label("程序化舞蹈场景设置", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // 查找场景中的 DigitalHuman
            GameObject digitalHuman = GameObject.Find("DigitalHuman");
            
            if (digitalHuman != null)
            {
                GUILayout.Label("✓ 已找到 DigitalHuman 对象");
                
                _animator = digitalHuman.GetComponent<Animator>();
                if (_animator == null)
                {
                    _animator = digitalHuman.GetComponentInChildren<Animator>();
                    if (_animator != null)
                    {
                        GUILayout.Label("  - 找到 Animator: " + _animator.name);
                    }
                }
                
                _danceGenerator = digitalHuman.GetComponent<ProceduralDanceGenerator>();
            }
            else
            {
                GUILayout.Label("✗ 未找到 DigitalHuman 对象");
                GUILayout.Label("请先加载 VRM 模型到场景中");
            }

            GUILayout.Space(10);
            
            // 绑定骨骼引用
            if (_animator != null && _danceGenerator != null)
            {
                GUILayout.Label("绑定骨骼引用", EditorStyles.boldLabel);
                
                if (GUILayout.Button("自动绑定骨骼"))
                {
                    BindBones();
                }
            }

            GUILayout.Space(20);
            GUILayout.Label("手动设置说明:", EditorStyles.boldLabel);
            GUILayout.Label("1. 拖入 VRM 模型到场景");
            GUILayout.Label("2. 将模型命名为 'DigitalHuman'");
            GUILayout.Label("3. 添加 ProceduralDanceGenerator 组件");
            GUILayout.Label("4. 在 Inspector 中绑定骨骼引用");
            GUILayout.Label("5. 点击 Play 测试");
        }

        private void BindBones()
        {
            if (_animator == null || _danceGenerator == null) return;

            // 使用 SerializedObject 修改组件
            SerializedObject serializedObj = new SerializedObject(_danceGenerator);
            
            // 骨骼映射表
            var boneMappings = new System.Collections.Generic.Dictionary<string, string>
            {
                {"_hips", "Hips"},
                {"_spine", "Spine"},
                {"_chest", "Chest"},
                {"_neck", "Neck"},
                {"_head", "Head"},
                {"_leftShoulder", "LeftUpperArm"},
                {"_leftUpperArm", "LeftUpperArm"},
                {"_leftLowerArm", "LeftLowerArm"},
                {"_leftHand", "LeftHand"},
                {"_rightShoulder", "RightUpperArm"},
                {"_rightUpperArm", "RightUpperArm"},
                {"_rightLowerArm", "RightLowerArm"},
                {"_rightHand", "RightHand"},
                {"_leftUpperLeg", "LeftUpperLeg"},
                {"_leftLowerLeg", "LeftLowerLeg"},
                {"_leftFoot", "LeftFoot"},
                {"_rightUpperLeg", "RightUpperLeg"},
                {"_rightLowerLeg", "RightLowerLeg"},
                {"_rightFoot", "RightFoot"}
            };

            bool foundAny = false;
            
            // 尝试查找并绑定骨骼
            Transform rootTransform = _animator.transform;
            
            foreach (var mapping in boneMappings)
            {
                SerializedProperty property = serializedObj.FindProperty(mapping.Key);
                
                if (property != null)
                {
                    // 递归查找骨骼
                    Transform bone = FindBoneRecursive(rootTransform, mapping.Value);
                    
                    if (bone != null)
                    {
                        property.objectReferenceValue = bone;
                        foundAny = true;
                        Debug.Log($"[ProceduralDanceSetup] 已绑定: {mapping.Key} -> {bone.name}");
                    }
                }
            }
            
            if (foundAny)
            {
                serializedObj.ApplyModifiedProperties();
                EditorUtility.SetDirty(_danceGenerator);
                EditorUtility.DisplayDialog("绑定成功", "骨骼绑定完成！\n请在 Inspector 中检查并补充未绑定的骨骼。", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("绑定失败", "未能找到匹配的骨骼。\n请确保 VRM 模型已正确加载并包含标准骨骼名称。", "确定");
            }
        }

        private Transform FindBoneRecursive(Transform parent, string boneName)
        {
            // 直接匹配
            if (parent.name == boneName)
            {
                return parent;
            }

            // 模糊匹配（处理可能的命名差异）
            if (parent.name.Contains(boneName) || boneName.Contains(parent.name))
            {
                return parent;
            }

            // 递归查找子对象
            foreach (Transform child in parent)
            {
                Transform found = FindBoneRecursive(child, boneName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
