using UnityEngine;
using System.Collections.Generic;

namespace DigitalHuman.Animation
{
    /// <summary>
    /// 智能骨骼自动查找器 - 自动识别并绑定所有骨骼
    /// </summary>
    public class SmartBoneFinder : MonoBehaviour
    {
        [Header("参考对象")]
        [Tooltip("VRM 模型的根对象或 Hips 骨骼")]
        public Transform vrmRoot;

        [Header("目标组件")]
        [Tooltip("程序化舞蹈生成器组件")]
        public ProceduralDanceGenerator danceGenerator;

        [Header("操作")]
        [Tooltip("点击此按钮自动查找并绑定所有骨骼")]
        public bool autoFindAndBind = false;

        private bool _lastState = false;

        void Update()
        {
            if (autoFindAndBind != _lastState && autoFindAndBind)
            {
                if (Application.isPlaying)
                {
                    FindAndBindBones();
                }
                _lastState = autoFindAndBind;
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (autoFindAndBind != _lastState && autoFindAndBind)
            {
                FindAndBindBones();
                _lastState = autoFindAndBind;
                autoFindAndBind = false;
            }
        }
#endif

        void FindAndBindBones()
        {
            if (vrmRoot == null || danceGenerator == null)
            {
                Debug.LogError("[SmartBoneFinder] 请先设置 VRM Root 和 Dance Generator！");
                return;
            }

            Debug.Log("═══════════════════════════════════════");
            Debug.Log("[SmartBoneFinder] 开始智能骨骼查找...");
            Debug.Log("═══════════════════════════════════════");

            // 收集所有骨骼
            List<Transform> allBones = new List<Transform>();
            CollectAllBones(vrmRoot, allBones);

            Debug.Log($"[SmartBoneFinder] 找到 {allBones.Count} 个骨骼");
            Debug.Log("");

            // 骨骼查找配置（包含多种可能的名称）
            var boneConfigs = new List<BoneConfig>
            {
                new BoneConfig { Target = "_hips", Keywords = new[] { "hips", "hip", "pelvis", "root" } },
                new BoneConfig { Target = "_spine", Keywords = new[] { "spine", "upperchest" } },
                new BoneConfig { Target = "_chest", Keywords = new[] { "chest", "breast", "upper_body" } },
                new BoneConfig { Target = "_neck", Keywords = new[] { "neck", "head_top" } },
                new BoneConfig { Target = "_head", Keywords = new[] { "head", "face" } },
                
                new BoneConfig { Target = "_leftShoulder", Keywords = new[] { "leftshoulder", "l_shoulder", "shoulder_l", "left arm", "left_shoulder_arm" } },
                new BoneConfig { Target = "_leftUpperArm", Keywords = new[] { "leftupperarm", "l_upperarm", "upper_arm_l", "left arm", "arm_upper_l", "leftUpperArm" } },
                new BoneConfig { Target = "_leftLowerArm", Keywords = new[] { "leftlowerarm", "l_lowerarm", "lower_arm_l", "left forearm", "left_forearm", "lower_arm_l", "leftLowerArm" } },
                new BoneConfig { Target = "_leftHand", Keywords = new[] { "lefthand", "l_hand", "hand_l", "left hand", "hand_l" } },
                
                new BoneConfig { Target = "_rightShoulder", Keywords = new[] { "rightshoulder", "r_shoulder", "shoulder_r", "right arm", "right_shoulder_arm" } },
                new BoneConfig { Target = "_rightUpperArm", Keywords = new[] { "rightupperarm", "r_upperarm", "upper_arm_r", "right arm", "arm_upper_r", "rightUpperArm" } },
                new BoneConfig { Target = "_rightLowerArm", Keywords = new[] { "rightlowerarm", "r_lowerarm", "lower_arm_r", "right forearm", "right_forearm", "lower_arm_r", "rightLowerArm" } },
                new BoneConfig { Target = "_rightHand", Keywords = new[] { "righthand", "r_hand", "hand_r", "right hand", "hand_r" } },
                
                new BoneConfig { Target = "_leftUpperLeg", Keywords = new[] { "leftupperleg", "l_upperleg", "upper_leg_l", "left leg", "leg_upper_l", "left leg 1", "leftUpLeg" } },
                new BoneConfig { Target = "_leftLowerLeg", Keywords = new[] { "leftlowerleg", "l_lowerleg", "lower_leg_l", "left shin", "left calf", "lower_leg_l", "leftLowerLeg", "left leg 2" } },
                new BoneConfig { Target = "_leftFoot", Keywords = new[] { "leftfoot", "l_foot", "foot_l", "left foot", "foot_l" } },
                
                new BoneConfig { Target = "_rightUpperLeg", Keywords = new[] { "rightupperleg", "r_upperleg", "upper_leg_r", "right leg", "leg_upper_r", "right leg 1", "rightUpLeg" } },
                new BoneConfig { Target = "_rightLowerLeg", Keywords = new[] { "rightlowerleg", "r_lowerleg", "lower_leg_r", "right shin", "right calf", "lower_leg_r", "rightLowerLeg", "right leg 2" } },
                new BoneConfig { Target = "_rightFoot", Keywords = new[] { "rightfoot", "r_foot", "foot_r", "right foot", "foot_r" } }
            };

            int foundCount = 0;
            int notFoundCount = 0;

            foreach (var config in boneConfigs)
            {
                Transform foundBone = FindBestMatch(allBones, config.Keywords, config.Target);
                
                if (foundBone != null)
                {
                    SetBoneValue(config.Target, foundBone);
                    foundCount++;
                    Debug.Log($"✓ 已找到: {config.Target} → {foundBone.name} (路径: {GetBonePath(foundBone)})");
                }
                else
                {
                    notFoundCount++;
                    Debug.LogWarning($"✗ 未找到: {config.Target} (关键词: {string.Join(", ", config.Keywords)})");
                }
            }

            Debug.Log("");
            Debug.Log("═══════════════════════════════════════");
            Debug.Log($"[SmartBoneFinder] 查找完成！成功: {foundCount}/{boneConfigs.Count}");
            if (notFoundCount > 0)
            {
                Debug.LogWarning($"[SmartBoneFinder] 有 {notFoundCount} 个骨骼未找到，请手动检查");
            }
            Debug.Log("═══════════════════════════════════════");
        }

        void CollectAllBones(Transform parent, List<Transform> boneList)
        {
            boneList.Add(parent);
            
            foreach (Transform child in parent)
            {
                CollectAllBones(child, boneList);
            }
        }

        Transform FindBestMatch(List<Transform> bones, string[] keywords, string targetField)
        {
            Transform bestMatch = null;
            int bestScore = -1;

            foreach (Transform bone in bones)
            {
                int score = CalculateMatchScore(bone.name, keywords);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMatch = bone;
                }
            }

            return bestScore > 0 ? bestMatch : null;
        }

        int CalculateMatchScore(string boneName, string[] keywords)
        {
            string boneLower = boneName.ToLower();
            int score = 0;

            foreach (string keyword in keywords)
            {
                string keywordLower = keyword.ToLower();
                
                // 完全匹配（最高分）
                if (boneLower == keywordLower)
                {
                    score += 100;
                }
                // 包含关键词
                else if (boneLower.Contains(keywordLower))
                {
                    score += 50;
                }
                // 关键词包含骨骼名
                else if (keywordLower.Contains(boneLower))
                {
                    score += 30;
                }
            }

            return score;
        }

        void SetBoneValue(string fieldName, Transform value)
        {
#if UNITY_EDITOR
            UnityEditor.SerializedObject serializedObj = new UnityEditor.SerializedObject(danceGenerator);
            UnityEditor.SerializedProperty property = serializedObj.FindProperty(fieldName);
            
            if (property != null)
            {
                property.objectReferenceValue = value;
                serializedObj.ApplyModifiedProperties();
                UnityEditor.EditorUtility.SetDirty(danceGenerator);
            }
#else
            var field = typeof(ProceduralDanceGenerator).GetField(fieldName, 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
            
            if (field != null)
            {
                field.SetValue(danceGenerator, value);
            }
#endif
        }

        string GetBonePath(Transform bone)
        {
            string path = bone.name;
            Transform current = bone.parent;
            
            while (current != null && current != vrmRoot)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            
            return path;
        }
    }

    [System.Serializable]
    public class BoneConfig
    {
        public string Target;
        public string[] Keywords;
    }
}
