using UnityEngine;

namespace DigitalHuman.Animation
{
    /// <summary>
    /// 快速设置工具 - 在场景中自动绑定骨骼
    /// </summary>
    public class QuickBoneBinder : MonoBehaviour
    {
        [Header("参考骨骼列表")]
        [Tooltip("拖入 VRM 模型的根对象")]
        public Transform vrmRoot;

        [Header("目标组件")]
        [Tooltip("程序化舞蹈生成器组件")]
        public ProceduralDanceGenerator danceGenerator;

        [Header("操作")]
        [Tooltip("在运行时或编辑器中点击此按钮绑定骨骼")]
        [UnityEngine.Serialization.FormerlySerializedAs("bindInEditor")]
        public bool bindBones = false;

        private bool _lastBindState = false;

        void Update()
        {
            // 检测按钮状态变化
            if (bindBones != _lastBindState && bindBones)
            {
                if (Application.isPlaying)
                {
                    BindBonesRuntime();
                }
                _lastBindState = bindBones;
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            // 在编辑器模式下绑定
            if (bindBones != _lastBindState && bindBones)
            {
                BindBonesEditor();
                _lastBindState = bindBones;
                bindBones = false; // 重置按钮
            }
        }
#endif

        private void BindBonesRuntime()
        {
            if (vrmRoot == null || danceGenerator == null)
            {
                Debug.LogError("[QuickBoneBinder] 请先设置 VRM Root 和 Dance Generator！");
                return;
            }

            Debug.Log("[QuickBoneBinder] 开始自动绑定骨骼...");

            var boneMappings = new System.Collections.Generic.Dictionary<string, string>
            {
                {"_hips", "Hips"},
                {"_spine", "Spine"},
                {"_chest", "Chest"},
                {"_neck", "Neck"},
                {"_head", "Head"},
                {"_leftShoulder", "LeftShoulder"},
                {"_leftUpperArm", "LeftUpperArm"},
                {"_leftLowerArm", "LeftLowerArm"},
                {"_leftHand", "LeftHand"},
                {"_rightShoulder", "RightShoulder"},
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

            int foundCount = 0;
            int totalCount = boneMappings.Count;

            // 使用反射设置私有字段
            var generatorType = typeof(ProceduralDanceGenerator);
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;

            foreach (var mapping in boneMappings)
            {
                Transform bone = FindBoneRecursive(vrmRoot, mapping.Value);
                
                if (bone != null)
                {
                    // 查找私有字段
                    var field = generatorType.GetField(mapping.Key, flags);
                    
                    if (field != null)
                    {
                        field.SetValue(danceGenerator, bone);
                        foundCount++;
                        Debug.Log($"[QuickBoneBinder] ✓ 已绑定: {mapping.Key} -> {bone.name}");
                    }
                }
            }

            Debug.Log($"[QuickBoneBinder] 绑定完成: {foundCount}/{totalCount}");
        }

#if UNITY_EDITOR
        private void BindBonesEditor()
        {
            if (vrmRoot == null || danceGenerator == null)
            {
                UnityEngine.Debug.LogError("[QuickBoneBinder] 请先设置 VRM Root 和 Dance Generator！");
                return;
            }

            Debug.Log("[QuickBoneBinder] 开始编辑器模式绑定骨骼...");

            var boneMappings = new System.Collections.Generic.Dictionary<string, string>
            {
                {"_hips", "Hips"},
                {"_spine", "Spine"},
                {"_chest", "Chest"},
                {"_neck", "Neck"},
                {"_head", "Head"},
                {"_leftShoulder", "LeftShoulder"},
                {"_leftUpperArm", "LeftUpperArm"},
                {"_leftLowerArm", "LeftLowerArm"},
                {"_leftHand", "LeftHand"},
                {"_rightShoulder", "RightShoulder"},
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

            int foundCount = 0;
            int totalCount = boneMappings.Count;

            // 使用 SerializedObject 编辑组件
            UnityEditor.SerializedObject serializedObj = new UnityEditor.SerializedObject(danceGenerator);
            
            foreach (var mapping in boneMappings)
            {
                UnityEditor.SerializedProperty property = serializedObj.FindProperty(mapping.Key);
                
                if (property != null)
                {
                    Transform bone = FindBoneRecursive(vrmRoot, mapping.Value);
                    
                    if (bone != null)
                    {
                        property.objectReferenceValue = bone;
                        foundCount++;
                        Debug.Log($"[QuickBoneBinder] ✓ 已绑定: {mapping.Key} -> {bone.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"[QuickBoneBinder] ✗ 未找到骨骼: {mapping.Value}");
                    }
                }
            }

            if (foundCount > 0)
            {
                serializedObj.ApplyModifiedProperties();
                UnityEditor.EditorUtility.SetDirty(danceGenerator);
                Debug.Log($"[QuickBoneBinder] 绑定完成: {foundCount}/{totalCount}");
            }
        }
#endif

        private Transform FindBoneRecursive(Transform parent, string boneName)
        {
            if (parent == null) return null;

            // 直接匹配
            if (parent.name == boneName)
            {
                return parent;
            }

            // 模糊匹配
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
