using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MorulabTools.Launcher
{
    public class MorulabLauncher : EditorWindow
    {
        [MenuItem("MorulabLauncher/Launcher")]
        public static void ShowWindow()
        {
            MorulabLauncher wnd = GetWindow<MorulabLauncher>();
            wnd.titleContent = new GUIContent("Morulab Launcher");
        }

        // Data
        private List<ToolCommandData> _allCommands = new List<ToolCommandData>();
        private List<ToolCommandData> _filteredCommands = new List<ToolCommandData>();

        // UI Elements
        private ScrollView _commandScrollView;
        private TextField _searchField;

        // State
        private ToolCommandData _selectedCommand;
        private Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();
        private Dictionary<ToolCommandData, VisualElement> _commandElements = new Dictionary<ToolCommandData, VisualElement>();

        public void CreateGUI()
        {
            // Load UXML
            var visualTree = Resources.Load<VisualTreeAsset>("MorulabLauncher");
            visualTree.CloneTree(rootVisualElement);

            // Load USS
            var styleSheet = Resources.Load<StyleSheet>("MorulabLauncher");
            rootVisualElement.styleSheets.Add(styleSheet);

            // Find Elements
            _commandScrollView = rootVisualElement.Q<ScrollView>("CommandScrollContainer");
            _searchField = rootVisualElement.Q<TextField>("SearchField");

            // Setup Search
            _searchField.RegisterValueChangedCallback(evt => FilterCommands(evt.newValue));

            // Load Data
            RefreshCommands();

            // Initial Render
            RefreshUI();
        }

        private void OnEnable()
        {
            if (_commandScrollView != null)
            {
                RefreshCommands();
                RefreshUI();
            }
        }

        private void RefreshUI()
        {
            _commandScrollView.Clear();
            _commandElements.Clear();

            // Group by Category
            var groupedCommands = _filteredCommands
                .GroupBy(c => c.Category)
                .OrderBy(g => g.Key);

            foreach (var group in groupedCommands)
            {
                var categoryInfo = group.Key;

                // Create Foldout
                var foldout = new Foldout();
                foldout.text = categoryInfo;
                foldout.AddToClassList("command-foldout");

                // Restore open/close state
                if (_foldoutStates.TryGetValue(categoryInfo, out bool isOpen))
                {
                    foldout.value = isOpen;
                }
                else
                {
                    foldout.value = true; // Default open
                }

                // Save state on change
                foldout.RegisterValueChangedCallback(evt =>
                {
                    if (_foldoutStates.ContainsKey(categoryInfo))
                        _foldoutStates[categoryInfo] = evt.newValue;
                    else
                        _foldoutStates.Add(categoryInfo, evt.newValue);
                });

                _commandScrollView.Add(foldout);

                foreach (var command in group)
                {
                    var item = new VisualElement();
                    item.AddToClassList("command-item");

                    // Highlight if previously selected
                    if (_selectedCommand == command)
                    {
                        item.AddToClassList("selected");
                    }

                    item.RegisterCallback<ClickEvent>(evt => SelectCommand(command, item));

                    var label = new Label(command.Title);
                    label.AddToClassList("command-item-label");
                    item.Add(label);

                    foldout.Add(item);
                    _commandElements[command] = item;
                }
            }
        }

        private void SelectCommand(ToolCommandData command, VisualElement itemElement)
        {
            // Deselect previous
            if (_selectedCommand != null && _commandElements.TryGetValue(_selectedCommand, out var prevElement))
            {
                prevElement.RemoveFromClassList("selected");
            }

            _selectedCommand = command;

            // Select new
            if (itemElement != null)
            {
                itemElement.AddToClassList("selected");
            }

            UpdateDetailPanel(command);
        }

        private void UpdateDetailPanel(ToolCommandData command)
        {
            var detailPanel = rootVisualElement.Q<VisualElement>("DetailPanel");
            var btnLaunch = detailPanel.Q<Button>("LaunchButton");

            // Clean up old listener
            if (btnLaunch.userData is System.Action oldAction)
            {
                btnLaunch.clicked -= oldAction;
            }

            if (command == null)
            {
                detailPanel.Q<Label>("DetailTitle").text = "Select a command";
                detailPanel.Q<Label>("DetailCategory").text = "";
                detailPanel.Q<Label>("DetailDesc").text = "";
                btnLaunch.SetEnabled(false);
                btnLaunch.userData = null;
                detailPanel.Q<VisualElement>("EmbeddedToolView")?.Clear();
                return;
            }

            detailPanel.Q<Label>("DetailTitle").text = command.Title;
            detailPanel.Q<Label>("DetailCategory").text = $"Category: {command.Category}";
            detailPanel.Q<Label>("DetailDesc").text = command.Description;

            btnLaunch.SetEnabled(true);

            // Launch Action
            System.Action newAction = () =>
            {
                ExecuteCommand(command);
            };
            btnLaunch.userData = newAction;
            btnLaunch.clicked += newAction;

            // Load Documentation (Markdown)
            LoadDocumentation(command, detailPanel.Q<Label>("DocumentationLabel"));
        }

        private void LoadDocumentation(ToolCommandData command, Label label)
        {
            if (label == null) return;
            label.text = "No documentation found.";

            if (command.TargetMethod == null) return;

            var type = command.TargetMethod.DeclaringType;
            if (type == null) return;

            // Find the script asset
            var guids = AssetDatabase.FindAssets($"t:MonoScript {type.Name}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() == type)
                {
                    // Look for .md file with same name
                    var dir = System.IO.Path.GetDirectoryName(path);
                    var mdPath = System.IO.Path.Combine(dir, type.Name + ".md");

                    if (System.IO.File.Exists(mdPath))
                    {
                        var text = System.IO.File.ReadAllText(mdPath);
                        label.text = text;
                        return;
                    }

                    // Fallback: look for "ReadMe.md" in the same folder
                    var readmePath = System.IO.Path.Combine(dir, "ReadMe.md");
                    if (System.IO.File.Exists(readmePath))
                    {
                        var text = System.IO.File.ReadAllText(readmePath);
                        label.text = text;
                        return;
                    }
                }
            }
        }

        private void OnDisable() { }

        private void ExecuteCommand(ToolCommandData command)
        {
            if (command.TargetMethod != null)
            {
                command.TargetMethod.Invoke(null, null);
            }
            else
            {
                EditorApplication.ExecuteMenuItem(command.Path);
            }
            Debug.Log($"Executed: {command.Title}");
        }

        private void RefreshCommands()
        {
            try
            {
                var commands = ReflectionUtils.FindCommands("Morulab") ?? new List<ToolCommandData>();
                _allCommands = commands.Where(c => !c.Title.Contains("Launcher")).ToList();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MorulabLauncher] Failed to refresh commands: {e}");
                _allCommands = new List<ToolCommandData>();
            }
            _filteredCommands = new List<ToolCommandData>(_allCommands);

            if (_searchField != null)
            {
                FilterCommands(_searchField.value);
            }
        }

        private void FilterCommands(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                _filteredCommands = new List<ToolCommandData>(_allCommands);
            }
            else
            {
                var lowerSearch = searchText.ToLower();
                _filteredCommands = _allCommands.Where(c =>
                    c.Title.ToLower().Contains(lowerSearch) ||
                    c.Description.ToLower().Contains(lowerSearch) ||
                    c.Category.ToLower().Contains(lowerSearch)
                ).ToList();
            }

            if (_commandScrollView != null)
            {
                RefreshUI();
            }
        }
    }
}
