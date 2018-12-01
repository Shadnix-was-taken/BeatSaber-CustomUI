﻿using CustomUI.BeatSaber;
using CustomUI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CustomUI.GameplaySettings
{
    public class GameplaySettingsUI : MonoBehaviour
    {
        public bool initialized = false;
        private Button _pageDownButton = null;
        private Button _pageUpButton = null;
        private int _listIndex = 0;
        private string _currentSubmenu = "MainMenu";
        private GameplaySettingsPanels panel;
        private Dictionary<string, List<GameOption>> _customMenus = new Dictionary<string, List<GameOption>>();
        private List<Transform> _defaultSeparators = new List<Transform>();
        private List<GameOption> _currentOptions = null;
        private RectTransform _panelContainer = null;
        private Transform[] _defaultOptions = new Transform[4];

        private static Dictionary<GameplaySettingsPanels, GameplaySettingsUI> _instances = new Dictionary<GameplaySettingsPanels, GameplaySettingsUI>();
        public static Dictionary<GameplaySettingsPanels, GameplaySettingsUI> Instance
        {
            get
            {
                for (int i = 0; i < 4; i++)
                    CreateInstance((GameplaySettingsPanels)i);

                return _instances;
            }
            private set
            {
                _instances = value;
            }
        }

        private static void CreateInstance(GameplaySettingsPanels panel)
        {
            if (_instances.ContainsKey(panel) && _instances[panel] != null) return;

            _instances[panel] = new GameObject("GameplaySettingsUI").AddComponent<GameplaySettingsUI>();
            _instances[panel].panel = panel;
        }
        
        void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            _customMenus[_currentSubmenu] = new List<GameOption>();
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        void OnDestroy()
        {
            SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        }

        private void SceneManager_activeSceneChanged(Scene from, Scene to)
        {
            if (to.name == "EmptyTransition")
            {
                Destroy(this.gameObject);
                initialized = false;
            }
            else if (to.name == "Menu")
            {
                Build();
            }
        }

        //Returns a list of options for the current page index
        private List<GameOption> GetOptionsForPage(int page)
        {
            //Default options
            if (page == 0 && _currentSubmenu == "MainMenu") return null;

            page--; //If the page isn't 0, we should pick from the 0th pagination of our list
            
            //Get 4 custom options and return them
            return _customMenus[_currentSubmenu].Skip(4 * page).Take(4).ToList();
        }

        //Sets the active value for our game options depending on the active page
        private void ChangePage(int page, Transform container, params Transform[] defaults)
        {
            _currentOptions = Instance[panel].GetOptionsForPage(Instance[panel]._listIndex);
            bool defaultsActive = _currentOptions == null;
            defaults?.ToList().ForEach(x => x.gameObject.SetActive(defaultsActive));
            
            if (defaultsActive)
            {
                for (int i = 0; i < _defaultSeparators.Count; i++)
                    _defaultSeparators[i].gameObject.SetActive(true);

                _customMenus.Values.ToList().ForEach(m => m.ForEach(o => o.separator.SetActive(false)));
            }
            else
            {
                foreach (List<GameOption> subMenu in _customMenus.Values)
                    foreach (GameOption g in subMenu)
                        g.separator.SetActive(false);

                _currentOptions[_currentOptions.Count-1].separator.SetActive(false);
                for (int i = 0; i < _currentOptions.Count-1; i++)
                    _currentOptions[i].separator.SetActive(true);

                _defaultSeparators.ForEach(s => s.gameObject.SetActive(false));
            }
            
            //Custom options
            Instance[panel]._customMenus.Values.ToList().ForEach(m => m.ForEach(x => x.gameObject.SetActive(false)));
            if (!defaultsActive) _currentOptions?.ToList().ForEach(x => x.gameObject.SetActive(true));
        }

        public static void EnterSubmenu(GameplaySettingsPanels panel, string menuName)
        {
            lock (Instance[panel])
            {
                var instance = Instance[panel];
                if (instance._customMenus.ContainsKey(menuName))
                {
                    instance._currentSubmenu = menuName;
                    instance._listIndex = 0;
                    instance.ChangePage(0, instance._panelContainer, instance._defaultOptions[0], instance._defaultOptions[1], instance._defaultOptions[2], instance._defaultOptions[3]);

                    if (instance._listIndex <= 0) instance._pageUpButton.gameObject.SetActive(false);
                    else instance._pageUpButton.gameObject.SetActive(true);

                    int index = instance._listIndex * 4 + (instance._currentSubmenu == "MainMenu" ? -4 : 0);
                    if (index + 4 < instance._customMenus[instance._currentSubmenu].Count) instance._pageDownButton.gameObject.SetActive(true);
                    else instance._pageDownButton.gameObject.SetActive(false);
                }
            }
        }

        public static MultiSelectOption CreateListOption(GameplaySettingsPanels panel, string optionName, string submenuName, string hintText = "")
        {
            lock (Instance[panel])
            {
                MultiSelectOption ret = new MultiSelectOption(panel, optionName, hintText);
                if (!Instance[panel]._customMenus.ContainsKey(submenuName))
                    Instance[panel]._customMenus[submenuName] = new List<GameOption>();

                Instance[panel]._customMenus[submenuName].Add(ret);
                return ret;
            }
        }

        public static MultiSelectOption CreateListOption(GameplaySettingsPanels panel, string optionName, string hintText = "")
        {
            return CreateListOption(panel, optionName, "MainMenu", hintText);
        }

        public static MultiSelectOption CreateListOption(string optionName, string hintText)
        {
            return CreateListOption(GameplaySettingsPanels.ModifiersRight, optionName, hintText);
        }

        public static MultiSelectOption CreateListOption(string optionName)
        {
            return CreateListOption(optionName, "");
        }

        public static ToggleOption CreateToggleOption(GameplaySettingsPanels panel, string optionName, string submenuName, string hintText = "", Sprite optionIcon = null, float multiplier = 0f)
        {
            lock (Instance[panel])
            {
                ToggleOption ret = new ToggleOption(panel, optionName, hintText, optionIcon, multiplier);
                if(!Instance[panel]._customMenus.ContainsKey(submenuName))
                    Instance[panel]._customMenus[submenuName] = new List<GameOption>();

                Instance[panel]._customMenus[submenuName].Add(ret);
                return ret;
            }
        }

        public static ToggleOption CreateToggleOption(GameplaySettingsPanels panel, string optionName, string hintText = "", Sprite optionIcon = null, float multiplier = 0f)
        {
            return CreateToggleOption(panel, optionName, "MainMenu", hintText, optionIcon, multiplier);
        }

        public static ToggleOption CreateToggleOption(string optionName, string hintText, Sprite optionIcon, float multiplier)
        {
            return CreateToggleOption(GameplaySettingsPanels.ModifiersRight, optionName, hintText, optionIcon, multiplier);
        }

        public static ToggleOption CreateToggleOption(string optionName, string hintText, Sprite optionIcon)
        {
            return CreateToggleOption(optionName, hintText, optionIcon, 0f);
        }

        public static ToggleOption CreateToggleOption(string optionName, string hintText)
        {
            return CreateToggleOption(optionName, hintText, null, 0f);
        }

        public static ToggleOption CreateToggleOption(string optionName)
        {
            return CreateToggleOption(optionName, "", null, 0f);
        }

        public static ToggleOption CreateSubmenuOption(GameplaySettingsPanels panel, string optionName, string submenuName, string submenuToEnter, string hintText = "", Sprite optionIcon = null)
        {
            lock (Instance[panel])
            {
                SubmenuOption ret = new SubmenuOption(panel, optionName, hintText, optionIcon);
                if (!Instance[panel]._customMenus.ContainsKey(submenuName))
                    Instance[panel]._customMenus[submenuName] = new List<GameOption>();

                ret.OnToggle += (e) => GameplaySettingsUI.EnterSubmenu(panel, submenuToEnter);

                Instance[panel]._customMenus[submenuName].Add(ret);
                return ret;
            }
        }

        public void Build()
        {
            string pageName = String.Empty, panelName = String.Empty;
            GameOption.GetPanelNames(panel, ref pageName, ref panelName);

            //Grab necessary references
            SoloFreePlayFlowCoordinator sfpfc = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().First();
            GameplaySetupViewController gsvc = sfpfc.GetField<GameplaySetupViewController>("_gameplaySetupViewController");

            //Get reference to the switch container
            RectTransform page = (RectTransform)gsvc.transform.Find(pageName);
            Destroy(page.gameObject.GetComponent<HorizontalLayoutGroup>());
            Destroy(page.gameObject.GetComponent<ContentSizeFitter>());
            _panelContainer = (RectTransform)page.Find(panelName);

            if (!_panelContainer.gameObject.GetComponent<ContentSizeFitter>())
            {
                var fitter = _panelContainer.gameObject.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            var height = _panelContainer.Find("PlayerHeight");
            if (height)
            {
                var staticLights = _panelContainer.Find("StaticLights");
                (height as RectTransform).sizeDelta = (staticLights as RectTransform).sizeDelta;
                var title = height.Find("Title");
                if (title)
                {
                    (title as RectTransform).sizeDelta = new Vector2((title as RectTransform).sizeDelta.x, 1);
                    title.localPosition += new Vector3(0, 0.5f);
                    var text = title.gameObject.GetComponentInChildren<TextMeshProUGUI>();
                    text.alignment = TextAlignmentOptions.MidlineLeft;
                    text.enableWordWrapping = false;
                }

                var measure = height.Find("MeassureButton");
                measure.localScale *= 0.6f;
                measure.localPosition += new Vector3(5f, 0);
                var reset = height.Find("ResetButton");
                reset.localScale *= 0.6f;
            }

            if (!initialized)
            {
                //Get references to the original switches, so we can later duplicate then destroy them
                GameOption.GetOptionTransforms(panel, _panelContainer, ref _defaultOptions[0], ref _defaultOptions[1], ref _defaultOptions[2], ref _defaultOptions[3]);
                
                foreach (Transform t in _panelContainer)
                {
                    if (t.name.StartsWith("Separator"))
                    {
                        _defaultSeparators.Add(t);
                    }
                }
                _defaultSeparators.Reverse();
                

                //Create up button
                _pageUpButton = Instantiate(Resources.FindObjectsOfTypeAll<Button>().First(x => (x.name == "PageUpButton")), _panelContainer);

                _pageUpButton.transform.SetParent(_panelContainer.parent);
                _pageUpButton.transform.localScale = Vector3.one / 2;
                _pageUpButton.transform.localPosition = new Vector3(_pageUpButton.transform.localPosition.x,  -2.2f, _pageUpButton.transform.localPosition.z);
                _pageUpButton.interactable = true;
                //(_pageUpButton.transform as RectTransform).sizeDelta = new Vector2((_pageUpButton.transform.parent as RectTransform).sizeDelta.x, 3.5f);
                _pageUpButton.onClick.RemoveAllListeners();
                _pageUpButton.onClick.AddListener(delegate ()
                {
                    Instance[panel].ChangePage(--Instance[panel]._listIndex, _panelContainer, Instance[panel]._defaultOptions[0], Instance[panel]._defaultOptions[1], Instance[panel]._defaultOptions[2], Instance[panel]._defaultOptions[3]);

                    //Nice responsive scroll buttons
                    if (Instance[panel]._listIndex <= 0) _pageUpButton.gameObject.SetActive(false);
                    if (Instance[panel]._customMenus[_currentSubmenu].Count > 0) _pageDownButton.gameObject.SetActive(true);
                });

                //Create down button
                _pageDownButton = Instantiate(Resources.FindObjectsOfTypeAll<Button>().First(x => (x.name == "PageDownButton")), _panelContainer);
                
                _pageDownButton.transform.SetParent(_panelContainer.parent);
                _pageDownButton.transform.localScale = Vector3.one / 2;
                _pageDownButton.transform.localPosition = new Vector3(_pageDownButton.transform.localPosition.x, -34.3f, _pageDownButton.transform.localPosition.z);
                _pageDownButton.interactable = true;
                //(_pageDownButton.transform as RectTransform).sizeDelta = new Vector2((_pageDownButton.transform.parent as RectTransform).sizeDelta.x, (_pageDownButton.transform as RectTransform).sizeDelta.y);
                _pageDownButton.onClick.RemoveAllListeners();
                _pageDownButton.onClick.AddListener(delegate ()
                {
                    Instance[panel].ChangePage(++Instance[panel]._listIndex, _panelContainer, Instance[panel]._defaultOptions[0], Instance[panel]._defaultOptions[1], Instance[panel]._defaultOptions[2], Instance[panel]._defaultOptions[3]);

                    //Nice responsive scroll buttons
                    if (Instance[panel]._listIndex >= 0) _pageUpButton.gameObject.SetActive(true);
                    if (((Instance[panel]._customMenus[_currentSubmenu].Count + 4 - 1) / 4) - Instance[panel]._listIndex <= 0) _pageDownButton.gameObject.SetActive(false);
                });

                _pageUpButton.gameObject.SetActive(false);
                _pageDownButton.gameObject.SetActive(Instance[panel]._customMenus[_currentSubmenu].Count > 0);

                //Unfortunately, due to weird object creation for versioning, this doesn't always
                //happen when the scene changes
                Instance[panel]._listIndex = 0;

                initialized = true;
            }

            //Create custom options
            foreach (List<GameOption> menu in Instance[panel]._customMenus.Values)
            {
                foreach (GameOption option in menu)
                {
                    if (!option.initialized)
                        option.Instantiate();
                }
            }
        }
    }
}
