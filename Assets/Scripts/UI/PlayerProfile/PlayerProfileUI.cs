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
            // 2. 显示玩家信息
            nameInputField.text = archive.playerName;
            nameDisplayText.text = archive.playerName;
            courseModeLevelText.text = archive.courseModeLevel.ToString();
            displayedTitleText.text = archive.displayedTitle;
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
    }