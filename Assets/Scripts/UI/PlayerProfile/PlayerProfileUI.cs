using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerProfileUI : MonoBehaviour
    {
        [Header("Profile 显示区")]
        public TMP_InputField nameInputField;
        public TMP_Text nameDisplayText;
        public TMP_Text courseModeLevelText;
        public TMP_Text displayedTitleText;
        public Image displayedAvatarImage;

        [Header("称号滚动列表")]
        public Transform titleContent;           // Content Transform of ScrollView
        public GameObject titleItemPrefab;       // Prefab 挂载 TitleItemUI

        [Header("头像滚动列表")]
        public Transform avatarContent;
        public GameObject avatarItemPrefab;      // Prefab 挂载 AvatarItemUI

        [Header("所有可用数据（在 Inspector 填好）")]
        public List<TitleData> allTitles;
        public List<AvatarData> allAvatars;

        private global::Archive archive;

        void Start()
        {
            InitUI();
        }

        private void InitUI()
        {
            // 1. 载入存档
            archive = GameUtilities.Archive.LoadLocalArchive();
            if (archive.playerName == null || archive.playerName == "")
            {
                GameUtilities.Archive.ChangePlayerName("Player");
                archive = GameUtilities.Archive.LoadLocalArchive();
            }
            // 2. 显示玩家信息
            nameInputField.text = archive.playerName;
            nameDisplayText.text = archive.playerName;
            
            if (archive.courseModeLevel != 0) courseModeLevelText.text = archive.courseModeLevel.ToString();
            else courseModeLevelText.text = "";
            //displayedTitleText.text = archive.displayedTitle;
            var tit = allTitles.Find(a => a.id == archive.displayedTitle);
            if (tit != null) displayedTitleText.text = tit.enText; 
            else displayedTitleText.text = "";
            // 如果存档里有对应头像 id，则找到 sprite
            var ava = allAvatars.Find(a => a.id == archive.displayedAvatar);
            if (ava != null) displayedAvatarImage.sprite = ava.sprite;

            // 3. 侦听改名
            nameInputField.onValueChanged.AddListener(OnNameChanged);

            // 4. 填充称号列表
            foreach (var data in allTitles)
            {
                bool unlocked = archive.titles.Contains(data.id);
                var go = Instantiate(titleItemPrefab, titleContent);
                var ui = go.GetComponent<TitleItemUI>();
                ui.Setup(data, unlocked, OnTitleSelected);
            }

            // 5. 填充头像列表
            foreach (var data in allAvatars)
            {
                bool unlocked = archive.avatars.Contains(data.id);
                var go = Instantiate(avatarItemPrefab, avatarContent);
                var ui = go.GetComponent<AvatarItemUI>();
                ui.Setup(data, unlocked, OnAvatarSelected);
            }
            
            /* --- 6. 生成完毕后立即自动排布 --- */
            AutoLayoutContent(titleContent);
            AutoLayoutContent(avatarContent);
        }

        private void OnNameChanged(string newName)
        {
            // 存档改名并刷新展示
            GameUtilities.Archive.ChangePlayerName(newName);
            nameDisplayText.text = newName;
        }

        private void OnTitleSelected(string titleId)
        {
            if (GameUtilities.Archive.ChangeDisplayedTitle(titleId))
            {
                displayedTitleText.text = titleId;
            }
        }

        private void OnAvatarSelected(string avatarId)
        {
            if (GameUtilities.Archive.ChangeDisplayedAvatar(avatarId))
            {
                var data = allAvatars.Find(a => a.id == avatarId);
                if (data != null)
                    displayedAvatarImage.sprite = data.sprite;
            }
        }
        
        /// <summary>
        /// 让 Content 内所有子物体从左上开始紧贴排布，
        /// 按行优先，无空隙，行高 = 该行子物体最大高度。
        /// </summary>
        private void AutoLayoutContent(Transform content)
        {
            var contentRT = content as RectTransform;
            if (contentRT == null || content.childCount == 0) return;

            // 强制刷新所有带 ContentSizeFitter / Layout 的子物体尺寸
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);

            float viewW = ((RectTransform)content.parent).rect.width;

            float curX = 0f;            // 当前行已占宽度
            float curY = 0f;            // 累计行高（负值，因左上原点）
            float rowMaxH = 0f;         // 当前行最高子物体

            for (int i = 0; i < content.childCount; ++i)
            {
                var rt = content.GetChild(i) as RectTransform;
                if (rt == null) continue;

                // ---- 统一锚点 / Pivot 为左上 ----
                rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
                rt.pivot     = new Vector2(0, 1);

                float w = rt.rect.width;
                float h = rt.rect.height;

                // 换行判断（留 0.01 容错）
                if (curX + w > viewW + 0.01f)
                {
                    // 进入下一行
                    curX = 0f;
                    curY -= rowMaxH;    // 行距 = 上一行最高高度
                    rowMaxH = 0f;
                }

                // 定位
                rt.anchoredPosition = new Vector2(curX, curY);

                // 更新游标
                curX   += w;
                rowMaxH = Mathf.Max(rowMaxH, h);
            }

            // 最后一行高度也要计入 Content 总高
            curY -= rowMaxH;
            contentRT.sizeDelta = new Vector2(contentRT.sizeDelta.x, -curY);
        }
    }