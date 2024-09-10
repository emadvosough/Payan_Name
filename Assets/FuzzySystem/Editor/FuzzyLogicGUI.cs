using UnityEditor;
using UnityEngine;

namespace FuzzySystem.Editor
{
    public class FuzzyLogicGUI : IGUI
    {
        private HighlightGUI _highlight = null;
        public HighlightGUI highlight
        {
            get
            {
                if (_highlight == null)
                {
                    _highlight = new HighlightGUI();
                }
                return _highlight;
            }
        }

        // Which editorWindow is drawing gui of this fuzzyLogic.
        // If a fuzzyLogic is not opened in editorWindow, this property should be null.
        private FuzzyLogicEditor _editorWindow = null;
        public FuzzyLogicEditor editorWindow
        {
            set
            {
                _editorWindow = value;
            }
            get
            {
                return _editorWindow;
            }
        }

        private string _popupMenuItemPath = null;
        public string popupMenuItemPath
        {
            set
            {
                _popupMenuItemPath = value;
            }
            get
            {
                return _popupMenuItemPath;
            }
        }

        private bool _isChanged = false;
        public bool isChanged
        {
            set
            {
                _isChanged = value;
            }
            get
            {
                return _isChanged;
            }
        }

        private FuzzySystem _fuzzySystem = null;

        private readonly float fuzzificationsScrollerSize = 15;

        public FuzzyLogicGUI(FuzzySystem fuzzySystem)
        {
            this._fuzzySystem = fuzzySystem;
        }

        public void ShowNotification(string msg)
        {
            if (editorWindow != null)
            {
                editorWindow.ShowNotification(new GUIContent(msg));
            }
        }

        public void Draw()
        {
            var maximizeTargetGUID = GUIUtils.Get(_fuzzySystem).editorWindow.focusedTargetGUID;
            if (maximizeTargetGUID == null)
            {
                EditorGUILayout.BeginVertical();
                {
                    GUIUtils.Get(_fuzzySystem).editorWindow.scrollFuzzifications = EditorGUILayout.BeginScrollView(GUIUtils.Get(_fuzzySystem).editorWindow.scrollFuzzifications, GUILayout.Height(GUIUtils.Get(_fuzzySystem).editorWindow.fuzzificationGUIHeight + fuzzificationsScrollerSize));
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            int numFuzzifications = _fuzzySystem.NumberFuzzifications();
                            for (int i = 0; i < numFuzzifications; i++)
                            {
                                GUIUtils.Get(_fuzzySystem.GetFuzzification(i)).Draw();

                                // A fuzzification was deleted. Iterator changed, so break it.
                                if (numFuzzifications != _fuzzySystem.NumberFuzzifications())
                                {
                                    break;
                                }
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndScrollView();

                    // overlay on gui of fuzzifications
                    if (Event.current.type == EventType.Repaint)
                    {
                        Rect rect = GUILayoutUtility.GetLastRect();
                        GUI.color = new Color(56 / 255.0f, 56 / 255.0f, 56 / 255.0f, 1);
                        GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height, 4000, 4000), Texture2D.whiteTexture, ScaleMode.StretchToFill);
                        GUI.color = Color.white;
                    }

                    DrawHSeparator();

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUIUtils.Get(_fuzzySystem.defuzzification).Draw();

                        DrawVSeparator();

                        DrawInferences();
                    }
                    EditorGUILayout.EndHorizontal();

                }
                
                EditorGUILayout.EndVertical();
            }
            else
            {
                if (_fuzzySystem.IsFuzzificationGUID(maximizeTargetGUID))
                {
                    GUIUtils.Get(_fuzzySystem).editorWindow.fuzzificationGUIHeight = FuzzificationGUI.MIN_GUI_HEIGHT;
                    var fuzzification = _fuzzySystem.GetFuzzification(maximizeTargetGUID);
                    GUIUtils.Get(fuzzification).Draw();
                }
                else if(_fuzzySystem.IsDefuzzificationGUID(maximizeTargetGUID))
                {
                    GUIUtils.Get(_fuzzySystem.defuzzification).Draw();
                }
                else if(_fuzzySystem.IsInferenceGUID(maximizeTargetGUID))
                {
                    DrawInferences();
                }
                else
                {
                    // Do nothing
                }
            }

            _fuzzySystem.Update();
        }

        private void DrawInferences()
        {
            EditorGUILayout.BeginVertical();
            {
                DrawFocusOnButton();

                GUIUtils.Get(_fuzzySystem).editorWindow.scrollInferences = GUILayout.BeginScrollView(GUIUtils.Get(_fuzzySystem).editorWindow.scrollInferences);
                {
                    int numInferences = _fuzzySystem.NumberInferences();
                    for (int i = 0; i < numInferences; i++)
                    {
                        GUIUtils.Get(_fuzzySystem.GetInference(i)).Draw();

                        // An inference was deleted. Iterator changed, so break it.
                        if (numInferences != _fuzzySystem.NumberInferences())
                        {
                            break;
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawFocusOnButton()
        {
            if (_fuzzySystem.NumberInferences() > 0)
            {
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.FindTexture("d_ScaleTool On"), "Focus on this and Hide others"), GUILayout.Width(20), GUILayout.Height(20)))
                {
                    var focusedTargetGUID = GUIUtils.Get(_fuzzySystem).editorWindow.focusedTargetGUID;
                    if (focusedTargetGUID == null)
                    {
                        focusedTargetGUID = _fuzzySystem.GetInference(0).guid;
                    }
                    else
                    {
                        focusedTargetGUID = null;
                    }
                    GUIUtils.Get(_fuzzySystem).editorWindow.SetFocusedTargetGUID(focusedTargetGUID, InferenceGUI.FORCUS_ON_DEFAULT_WIDTH, InferenceGUI.FORCUS_ON_DEFAULT_HEIGHT);
                }
            }
        }

        private void SetFuzzificationsGUIHeight(float height)
        {
            int numFuzzifications = _fuzzySystem.NumberFuzzifications();
            for (int i = 0; i < numFuzzifications; i++)
            {
                GUIUtils.Get(_fuzzySystem).editorWindow.fuzzificationGUIHeight = height;
            }
        }

        #region V Separator

        private Rect defuzzificationRect;
        private Rect vSeparatorRect;
        private bool vIsDragging = false;

        private void DrawVSeparator()
        {
            if (Event.current.type == EventType.Repaint)
            {
                defuzzificationRect = GUILayoutUtility.GetLastRect();
            }

            GUILayout.BeginVertical(GUILayout.Width(18));
            {
                GUILayout.FlexibleSpace();
                GUI.color = Color.gray;
                GUILayout.Label("|");
                GUI.color = Color.white;
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();

            if (EditorWindow.mouseOverWindow == GUIUtils.Get(_fuzzySystem).editorWindow)
            {
                var mousePos = Event.current.mousePosition;
                var mouseArea = GUIUtils.Get(_fuzzySystem).editorWindow.position;
                mouseArea.x = 20;
                mouseArea.y = 20;
                mouseArea.width -= 20;
                mouseArea.height -= 20;

                if (Event.current.type == EventType.Repaint)
                {
                    vSeparatorRect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(vSeparatorRect, MouseCursor.ResizeHorizontal);
                }
                if (vSeparatorRect.Contains(mousePos))
                {
                    if (Event.current.type == EventType.MouseDrag)
                    {
                        vIsDragging = true;
                    }
                }
                if (Event.current.type == EventType.MouseUp || mouseArea.Contains(mousePos) == false)
                {
                    vIsDragging = false;
                }
                if (vIsDragging)
                {
                    float mousePosX = Mathf.Clamp(mousePos.x, 100, mouseArea.x + mouseArea.width - 100);
                    GUIUtils.Get(_fuzzySystem).editorWindow.defuzzificationGUIWidth = Mathf.Max(mousePosX - defuzzificationRect.x - vSeparatorRect.width * 0.5f, FuzzificationGUI.MIN_GUI_WIDTH);
                }
            }
        }

        #endregion

        #region H Separator

        private Rect fuzzificationsRect;
        private Rect hSeparatorRect;
        private bool hIsDragging = false;

        private void DrawHSeparator()
        {
            if (Event.current.type == EventType.Repaint)
            {
                fuzzificationsRect = GUILayoutUtility.GetLastRect();
            }

            GUILayout.BeginHorizontal(GUILayout.Height(18));
            {
                GUILayout.FlexibleSpace();
                GUI.color = Color.gray;
                GUILayout.Label("——");
                GUI.color = Color.white;
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            if (EditorWindow.mouseOverWindow == GUIUtils.Get(_fuzzySystem).editorWindow)
            {
                var mousePos = Event.current.mousePosition;
                var mouseArea = GUIUtils.Get(_fuzzySystem).editorWindow.position;
                mouseArea.x = 20;
                mouseArea.y = 20;
                mouseArea.width -= 20;
                mouseArea.height -= 20;

                if (Event.current.type == EventType.Repaint)
                {
                    hSeparatorRect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(hSeparatorRect, MouseCursor.ResizeVertical);
                }
                if (hSeparatorRect.Contains(mousePos))
                {
                    if (Event.current.type == EventType.MouseDrag)
                    {
                        hIsDragging = true;
                    }
                }
                if (Event.current.type == EventType.MouseUp || mouseArea.Contains(mousePos) == false)
                {
                    hIsDragging = false;
                }
                if (hIsDragging)
                {
                    float mousePosY = Mathf.Clamp(mousePos.y, 100, mouseArea.y + mouseArea.height - 100);
                    SetFuzzificationsGUIHeight(mousePosY - fuzzificationsRect.y - fuzzificationsScrollerSize - hSeparatorRect.height * 0.5f);
                }
            }
        }

        #endregion
    }
}