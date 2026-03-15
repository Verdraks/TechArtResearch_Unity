using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using System;
using System.Collections.Generic;
using MVsToolkit.Preferences.Editor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace MVsToolkit.Favorites.Editor
{
    [InitializeOnLoad]
    public static class FavoritesWindowOverlay
    {
        #region Constants - Layout
        
        private const string k_OverlayName = "FavoritesOverlay";
        private const string k_FolderCapsuleName = "folder-capsule";
        private const string k_CrossButtonName = "cross-button";
        private const string k_DeleteConfirmPopupName = "delete-confirm-popup";
        private const string k_FolderEditorPopupName = "folder-editor-popup";

        private const float k_RowHeight = 50f;
        private const float k_IconSize = 45f;
        private const float k_CrossButtonOffsetFromRight = 23f;
        private const float k_CrossButtonSize = 18f;
        private const int k_AnimationDurationMs = 150;
        private const float k_BottomNavigationHeight = 20.5f;
        private const float k_SmallButtonSize = 20f;
        private const float k_NavButtonSize = 18f;
        private const float k_CapsuleBorderRadius = 10f;
        private const float k_PopupBorderRadius = 8f;
        private const float k_PopupWidth = 220f;
        private const float k_PopupPadding = 12f;

        #endregion

        #region Constants - Colors

        private static readonly Color s_OverlayBackgroundColor = new(0.2f, 0.2f, 0.2f, 0.98f);
        private static readonly Color s_OverlayDragHighlightColor = new(0.25f, 0.25f, 0.3f, 0.98f);
        private static readonly Color s_HighlightedRowColor = new(0.335f, 0.335f, 0.335f);
        private static readonly Color s_SmallButtonColor = new(0.35f, 0.35f, 0.35f);
        private static readonly Color s_NavButtonHoverColor = new(1f, 1f, 1f, 0.1f);
        private static readonly Color s_TextColor = new(0.78f, 0.78f, 0.78f);
        private static readonly Color s_TextHoverColor = new(0.91f, 0.91f, 0.91f);
        private static readonly Color s_TextMutedColor = new(0.75f, 0.75f, 0.75f);
        private static readonly Color s_TextDimColor = new(0.5f, 0.5f, 0.5f);
        private static readonly Color s_EmptyCapsuleColor = new(0.3f, 0.3f, 0.3f, 0.6f);
        private static readonly Color s_PopupBackgroundColor = new(0.22f, 0.22f, 0.22f, 0.98f);
        private static readonly Color s_PopupBorderColor = new(0.4f, 0.4f, 0.4f);
        private static readonly Color s_PopupButtonDeleteColor = new(0.7f, 0.3f, 0.3f);
        private static readonly Color s_PopupButtonDeleteHoverColor = new(0.8f, 0.35f, 0.35f);
        private static readonly Color s_PopupButtonCancelColor = new(0.35f, 0.35f, 0.35f);
        private static readonly Color s_PopupButtonCancelHoverColor = new(0.45f, 0.45f, 0.45f);


        #endregion

        #region Static Fields - State

        private static FavoritesService s_FavoritesService;
        private static bool s_IsVisible;
        private static int s_SelectedFolderIndex;

        // TODO: This will be managed via preferences later
        private static bool s_UseToggleMode = true;
        private static bool s_IsHolding;

        #endregion

        #region Static Fields - UI Elements

        private static VisualElement s_OverlayRoot;
        private static VisualElement s_ContentGrid;
        private static ScrollView s_ScrollView;
        private static VisualElement s_BottomNavigation;
        private static Label s_FolderNameLabel;
        private static VisualElement s_DeleteConfirmPopup;
        private static VisualElement s_FolderEditorPopup;

        #endregion

        #region Static Fields - Drag Reorder

        private static VisualElement s_DraggedElement;
        private static VisualElement s_DragPlaceholder;
        private static int s_DraggedIndex = -1;

        #endregion

        #region Properties
        
        private static List<FavoritesGroup> Groups => s_FavoritesService?.Storage?.FavoritesGroups;

        private static FavoritesGroup CurrentGroup =>
            Groups != null && s_SelectedFolderIndex >= 0 && s_SelectedFolderIndex < Groups.Count
                ? Groups[s_SelectedFolderIndex]
                : null;

        /// <summary>
        /// Toggle mode: true = toggle on/off, false = hold to show (release to hide).
        /// TODO: This will be managed via preferences later.
        /// </summary>
        public static bool UseToggleMode
        {
            get => s_UseToggleMode;
            set => s_UseToggleMode = value;
        }

        #endregion

        #region Initialization

        static FavoritesWindowOverlay()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        #endregion

        #region Public API

        [MenuItem("Tools/MVsToolkit/Favorites/Open-Close &f")]
        public static void ToggleOverlay()
        {
            if (!ValidateTargetWindow()) return;
            
            if (s_UseToggleMode)
            {
                // Toggle mode: simply toggle visibility
                if (s_IsVisible)
                    Hide();
                else
                    Show();
            }
            else
            {
                // Hold mode: show on key down
                if (!s_IsVisible)
                {
                    Show();
                    s_IsHolding = true;
                }
            }
        }

        /// <summary>
        /// Called when the shortcut key is released (for hold mode).
        /// Must be called from external key event handling.
        /// </summary>
        public static void OnShortcutKeyReleased()
        {
            if (!s_UseToggleMode && s_IsHolding)
            {
                Hide();
                s_IsHolding = false;
            }
        }
        
        #endregion

        #region Show / Hide

        private static void Show()
        {
            InitializeService();
            CreateOverlay();

            
            // Fade in animation
            if (s_OverlayRoot != null)
            {
                s_OverlayRoot.style.opacity = 0;
                s_OverlayRoot.schedule.Execute(() => { s_OverlayRoot.style.opacity = 1; }).StartingIn(k_AnimationDurationMs);
            }
            
            s_IsVisible = true;
        }

        private static void Hide()
        {
            CloseAllPopups();

            if (s_OverlayRoot != null)
            {
                s_OverlayRoot.style.opacity = 0;
                s_OverlayRoot.schedule.Execute(() =>
                {
                    ProjectBrowserExtension.ProjectBrowserWindow?.rootVisualElement?.Remove(s_OverlayRoot);
                    s_OverlayRoot = null;
                }).StartingIn(k_AnimationDurationMs);
            }

            ShutdownService();
            s_IsVisible = false;
            s_IsHolding = false;
        }

        #endregion

        #region Service Management

        private static void InitializeService()
        {
            s_FavoritesService = new FavoritesService();
            s_FavoritesService.Init();

            if (Groups is { Count: > 0 } && s_SelectedFolderIndex < 0)
                s_SelectedFolderIndex = 0;
        }

        private static void ShutdownService()
        {
            s_FavoritesService?.Shutdown();
            s_FavoritesService = null;
        }

        #endregion

        #region Editor Update

        private static void OnEditorUpdate()
        {
            if (!s_IsVisible) return;

            if (!ValidateTargetWindow())
            {
                Hide();
                return;
            }

            if (s_OverlayRoot?.parent == null)
                CreateOverlay();

            UpdateOverlayPosition();
        }

        private static bool ValidateTargetWindow()
        {
            return ProjectBrowserExtension.ProjectBrowserOpen;
        }

        private static void UpdateOverlayPosition()
        {
            if (s_OverlayRoot == null || !ProjectBrowserExtension.ProjectBrowserOpen) return;

            Rect viewRect = ProjectBrowserExtension.GetViewRect();
            s_OverlayRoot.style.left = viewRect.x;
            s_OverlayRoot.style.top = viewRect.y;
            s_OverlayRoot.style.width = viewRect.width;
            s_OverlayRoot.style.height = viewRect.height;
        }

        #endregion

        #region Overlay Creation

        private static void CreateOverlay()
        {
            RemoveExistingOverlay();

            s_OverlayRoot = CreateRootContainer();
            s_ScrollView = CreateScrollView();
            s_ContentGrid = CreateContentGrid();
            s_BottomNavigation = CreateBottomNavigation();

            s_ScrollView.Add(s_ContentGrid);
            s_OverlayRoot.Add(s_ScrollView);
            s_OverlayRoot.Add(s_BottomNavigation);

            SetupDragAndDrop();

            ProjectBrowserExtension.ProjectBrowserWindow.rootVisualElement.Add(s_OverlayRoot);
            
            RefreshContent();
            UpdateOverlayPosition();
        }

        private static void RemoveExistingOverlay()
        {
            VisualElement existing = ProjectBrowserExtension.ProjectBrowserWindow.rootVisualElement.Q<VisualElement>(k_OverlayName);
            existing?.RemoveFromHierarchy();
        }

        private static VisualElement CreateRootContainer()
        {
            return new VisualElement
            {
                name = k_OverlayName,
                style =
                {
                    position = Position.Absolute,
                    overflow = Overflow.Hidden,
                    backgroundColor = s_OverlayBackgroundColor,
                    flexDirection = FlexDirection.Column,
                    transitionProperty = new List<StylePropertyName> { new("opacity") },
                    transitionDuration = new List<TimeValue> { new(k_AnimationDurationMs, TimeUnit.Millisecond) }
                }
            };
        }

        private static ScrollView CreateScrollView()
        {
            ScrollView scrollView = new(ScrollViewMode.Vertical)
            {
                style = { flexGrow = 1 },
                horizontalScrollerVisibility = ScrollerVisibility.Hidden
            };

            return scrollView;
            
        }

        private static VisualElement CreateContentGrid()
        {
            return new VisualElement
            {
                style =
                {
                    flexGrow = 1
                }
            };
        }

        #endregion

        #region Bottom Navigation

        private static VisualElement CreateBottomNavigation()
        {
            VisualElement container = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.Center,
                    alignItems = Align.Center,
                    height = k_BottomNavigationHeight,
                    minHeight = k_BottomNavigationHeight,
                    flexShrink = 1
                },
            };

            Button deleteButton = CreateSmallButton("−", ShowDeleteConfirmPopup);
            deleteButton.style.marginRight = 8;
            container.Add(deleteButton);

            VisualElement folderLayout = CreateGroupFolderLayout();
            container.Add(folderLayout);

            Button addButton = CreateSmallButton("+", CreateNewGroup);
            addButton.style.marginLeft = 8;
            container.Add(addButton);

            return container;
        }

        private static Button CreateSmallButton(string text, Action onClick)
        {
            Button button = new(onClick)
            {
                text = text,
                style =
                {
                    width = k_SmallButtonSize,
                    height = k_SmallButtonSize,
                    borderTopLeftRadius = 3,
                    borderTopRightRadius = 3,
                    borderBottomLeftRadius = 3,
                    borderBottomRightRadius = 3,
                    backgroundColor = s_SmallButtonColor,
                    color = s_TextMutedColor,
                    fontSize = 14,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    flexShrink = 1,
                }
            };

            button.RegisterCallback<MouseEnterEvent, Button>((_, btn) =>
            {
                btn.style.backgroundColor = s_HighlightedRowColor;
            }, button);

            button.RegisterCallback<MouseLeaveEvent, Button>((_, btn) =>
            {
                btn.style.backgroundColor = s_SmallButtonColor;
            }, button);

            return button;
        }

        private static Button CreateNavButton(string text, Action onClick)
        {
            Button button = new(onClick)
            {
                text = text,
                style =
                {
                    width = k_NavButtonSize,
                    height = k_NavButtonSize,
                    borderTopLeftRadius = 2,
                    borderTopRightRadius = 2,
                    borderBottomLeftRadius = 2,
                    borderBottomRightRadius = 2,
                    backgroundColor = Color.clear,
                    color = new Color(0.8f, 0.8f, 0.8f),
                    fontSize = 10,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    flexShrink = 1,
                }
            };

            button.RegisterCallback<MouseEnterEvent, Button>((_, btn) =>
            {
                btn.style.backgroundColor = s_NavButtonHoverColor;
            }, button);

            button.RegisterCallback<MouseLeaveEvent, Button>((_, btn) =>
            {
                btn.style.backgroundColor = Color.clear;
            }, button);

            return button;
        }

        private static VisualElement CreateGroupFolderLayout()
        {
            VisualElement capsule = new()
            {
                name = k_FolderCapsuleName,
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    paddingLeft = 4,
                    paddingRight = 4,
                    paddingTop = 2,
                    paddingBottom = 2,
                    borderTopLeftRadius = k_CapsuleBorderRadius,
                    borderTopRightRadius = k_CapsuleBorderRadius,
                    borderBottomLeftRadius = k_CapsuleBorderRadius,
                    borderBottomRightRadius = k_CapsuleBorderRadius,
                    justifyContent = Justify.Center,
                    overflow = Overflow.Hidden,
                    flexShrink = 1,
                }
            };

            Button prevButton = CreateNavButton("◀", () => NavigateGroup(-1));
            capsule.Add(prevButton);

            s_FolderNameLabel = new Label
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    fontSize = 11,
                    color = Color.white,
                    minWidth = 50,
                    marginLeft = 4,
                    marginRight = 4,
                    flexShrink = 1,
                    whiteSpace = WhiteSpace.Normal
                }
            };
            capsule.Add(s_FolderNameLabel);

            Button nextButton = CreateNavButton("▶", () => NavigateGroup(1));
            capsule.Add(nextButton);

            capsule.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 2)
                    ShowFolderEditorPopup();
            });

            UpdateFolderLayoutGroup();
            return capsule;
        }

        private static void UpdateFolderLayoutGroup()
        {
            VisualElement capsule = s_OverlayRoot?.Q<VisualElement>(k_FolderCapsuleName);
            if (capsule == null || s_FolderNameLabel == null) return;

            FavoritesGroup group = CurrentGroup;
            if (group != null)
            {
                s_FolderNameLabel.text = group.Name;
                capsule.style.backgroundColor = new Color(group.Color.r, group.Color.g, group.Color.b, 0.6f);
            }
            else
            {
                s_FolderNameLabel.text = "Empty";
                capsule.style.backgroundColor = s_EmptyCapsuleColor;
            }
        }

        #endregion

        #region Group Management

        private static void NavigateGroup(int direction)
        {
            if (Groups == null || Groups.Count == 0) return;

            int lastIndex = s_SelectedFolderIndex;

            s_SelectedFolderIndex += direction;

            if (s_SelectedFolderIndex < 0)
                s_SelectedFolderIndex = Groups.Count - 1;
            else if (s_SelectedFolderIndex >= Groups.Count)
                s_SelectedFolderIndex = 0;

            if (lastIndex == s_SelectedFolderIndex) return;

            AnimateContentChange();
        }

        private static void CreateNewGroup()
        {
            s_FavoritesService.CreateGroup();
            s_SelectedFolderIndex = Groups.Count - 1;
            AnimateContentChange();
        }

        private static void DeleteCurrentGroup()
        {
            if (CurrentGroup == null) return;

            s_FavoritesService.DeleteGroup(CurrentGroup);

            if (s_SelectedFolderIndex >= Groups.Count)
                s_SelectedFolderIndex = Groups.Count - 1;

            RefreshContent();
            UpdateFolderLayoutGroup();
        }

        private static void EnsureFolderExists()
        {
            if (Groups == null || Groups.Count == 0)
            {
                s_FavoritesService.CreateGroup();
                s_SelectedFolderIndex = 0;
            }
            else if (s_SelectedFolderIndex < 0)
            {
                s_SelectedFolderIndex = 0;
            }
        }

        #endregion

        #region Content Display

        private static void RefreshContent()
        {
            if (s_ContentGrid == null) return;

            s_ContentGrid.Clear();
            UpdateFolderLayoutGroup();

            FavoritesGroup group = CurrentGroup;
            if (group == null)
            {
                ShowEmptyMessage();
                return;
            }

            s_FavoritesService.LoadGroup(CurrentGroup);

            for (int i = 0; i < group.Elements.Count; i++)
            {
                VisualElement itemElement = CreateItemElement(group, group.Elements[i], i);
                s_ContentGrid.Add(itemElement);
            }
        }

        private static void ShowEmptyMessage()
        {
            Label label = new("Drop assets here or create a folder with +")
            {
                style =
                {
                    color = s_TextDimColor,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    fontSize = 12,
                    flexGrow = 1,
                    paddingTop = 40,
                    flexWrap = Wrap.Wrap,
                    whiteSpace = WhiteSpace.Normal,
                }
            };
            s_ContentGrid.Add(label);
        }

        private static void AnimateContentChange()
        {
            if (s_ContentGrid == null) return;

            s_ContentGrid.style.opacity = 0;
            s_ContentGrid.style.transitionProperty = new List<StylePropertyName> { new("opacity") };
            s_ContentGrid.style.transitionDuration =
                new List<TimeValue> { new(k_AnimationDurationMs / 2f, TimeUnit.Millisecond) };

            s_ContentGrid.schedule.Execute(() =>
            {
                RefreshContent();
                s_ContentGrid.style.opacity = 1;
            }).StartingIn((long)(k_AnimationDurationMs / 2f));
        }

        #endregion

        #region Item Element Creation

        private static VisualElement CreateItemElement(FavoritesGroup group, IFavoritesElement item, int index)
        {
            IFavoritesCacheElement cache = s_FavoritesService.Storage.Resolve(item);
            Texture2D previewTexture = cache.Preview;
            string itemName = cache.Name;

           
            Color rowColor =  MVsPrefs<MVsHierarchyValues>.Values.BackgroundColor(index % 2 == 0);

            VisualElement rootElement = new()
            {
                userData = index,
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    height = k_RowHeight,
                    backgroundColor = rowColor
                }
            };

            Image iconContainer = new()
            {
                image = previewTexture,
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    width = k_IconSize,
                    height = k_IconSize,
                    marginLeft = 0,
                    marginTop = (k_RowHeight - k_IconSize) / 2,
                    marginBottom = (k_RowHeight - k_IconSize) / 2,
                    flexShrink = 0
                }
            };

            rootElement.Add(iconContainer);

            Label nameLabel = new(itemName)
            {
                style =
                {
                    flexGrow = 1,
                    marginLeft = 0,
                    fontSize = 12,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    color = s_TextColor,
                    overflow = Overflow.Hidden,
                    textOverflow = TextOverflow.Ellipsis,
                    whiteSpace = WhiteSpace.NoWrap
                }
            };
            rootElement.Add(nameLabel);

            Button crossButton = CreateCrossButton(group, item);
            rootElement.Add(crossButton);

            SetupItemHoverCallbacks(rootElement, crossButton, nameLabel, rowColor);
            SetupItemClickCallback(rootElement, item);
            SetupItemDragReorder(rootElement, group, index);

            return rootElement;
        }

        private static Button CreateCrossButton(FavoritesGroup group, IFavoritesElement item)
        {
            Button crossButton = new()
            {
                name = k_CrossButtonName,
                style =
                {
                    width = k_CrossButtonSize,
                    height = k_CrossButtonSize,
                    marginRight = k_CrossButtonOffsetFromRight - k_CrossButtonSize / 2,
                    flexShrink = 0,
                    display = DisplayStyle.None,
                    backgroundColor = Color.clear,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0
                }
            };

            Image crossIcon = new()
            {
                image = EditorGUIUtility.IconContent("CrossIcon").image,
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    width = k_CrossButtonSize,
                    height = k_CrossButtonSize
                }
            };
            crossButton.Add(crossIcon);

            crossButton.RegisterCallback<ClickEvent, Tuple<FavoritesGroup, IFavoritesElement>>((evt, t) =>
            {
                evt.StopPropagation();
                s_FavoritesService.DeleteItem(t.Item1, t.Item2);
                RefreshContent();
            }, new Tuple<FavoritesGroup, IFavoritesElement>(group, item));

            crossButton.RegisterCallback<MouseEnterEvent, Image>((_, img) => { img.tintColor = Color.gray; },
                crossIcon);

            crossButton.RegisterCallback<MouseLeaveEvent, Image>((_, img) => { img.tintColor = Color.white; },
                crossIcon);

            return crossButton;
        }

        private static void SetupItemHoverCallbacks(VisualElement rootElement, Button crossButton, Label nameLabel,
            Color rowColor)
        {
            rootElement.RegisterCallback<MouseEnterEvent, Tuple<VisualElement, VisualElement, VisualElement>>((_, t) =>
            {
                t.Item1.style.backgroundColor = s_HighlightedRowColor;
                t.Item2.style.display = DisplayStyle.Flex;
                t.Item3.style.color = s_TextHoverColor;
            }, new Tuple<VisualElement, VisualElement, VisualElement>(rootElement, crossButton, nameLabel));

            rootElement.RegisterCallback<MouseLeaveEvent, Tuple<VisualElement, VisualElement, VisualElement, Color>>(
                (_, t) =>
                {
                    t.Item1.style.backgroundColor = t.Item4;
                    t.Item2.style.display = DisplayStyle.None;
                    t.Item3.style.color = s_TextColor;
                },
                new Tuple<VisualElement, VisualElement, VisualElement, Color>(rootElement, crossButton, nameLabel,
                    rowColor));
        }

        private static void SetupItemClickCallback(VisualElement rootElement, IFavoritesElement item)
        {
            rootElement.RegisterCallback<ClickEvent, Tuple<VisualElement, IFavoritesElement>>((evt, t) =>
            {
                if (t.Item1.Contains(evt.target as VisualElement)) return;
                s_FavoritesService.FocusElement(t.Item2);
            }, new Tuple<VisualElement, IFavoritesElement>(rootElement, item));
        }

        #endregion

        #region Drag Reorder

        private static void SetupItemDragReorder(VisualElement element, FavoritesGroup group, int index)
        {
            element.RegisterCallback<MouseDownEvent, Tuple<VisualElement, int>>((evt, t) =>
            {
                if (evt.button != 0) return;

                s_DraggedElement = t.Item1;
                s_DraggedIndex = t.Item2;

                t.Item1.style.opacity = 0.5f;
                t.Item1.CaptureMouse();
            }, new Tuple<VisualElement, int>(element, index));

            element.RegisterCallback<MouseMoveEvent, Tuple<VisualElement, FavoritesGroup>>((evt, t) =>
            {
                if (s_DraggedElement != t.Item1) return;

                int targetIndex = GetTargetIndexFromPosition(evt.mousePosition);
                if (targetIndex != -1 && targetIndex != s_DraggedIndex)
                {
                    ReorderElement(t.Item2, s_DraggedIndex, targetIndex);
                    s_DraggedIndex = targetIndex;
                    t.Item1.userData = targetIndex;
                }
            }, new Tuple<VisualElement, FavoritesGroup>(element, group));

            element.RegisterCallback<MouseUpEvent, VisualElement>((_, elem) =>
            {
                if (s_DraggedElement != elem) return;

                elem.style.opacity = 1f;
                elem.ReleaseMouse();

                s_DraggedElement = null;
                s_DraggedIndex = -1;
            }, element);
        }

        private static int GetTargetIndexFromPosition(Vector2 mousePos)
        {
            if (s_ContentGrid == null) return -1;

            for (int i = 0; i < s_ContentGrid.childCount; i++)
            {
                VisualElement child = s_ContentGrid[i];
                if (child.worldBound.Contains(mousePos))
                    return i;
            }

            return -1;
        }

        private static void ReorderElement(FavoritesGroup group, int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= group.Elements.Count) return;
            if (toIndex < 0 || toIndex >= group.Elements.Count) return;
            if (fromIndex == toIndex) return;

            IFavoritesElement element = group.Elements[fromIndex];
            group.Elements.RemoveAt(fromIndex);
            group.Elements.Insert(toIndex, element);

            RefreshContent();
        }

        #endregion

        #region Drag and Drop (External)

        private static void SetupDragAndDrop()
        {
            if (s_OverlayRoot == null) return;

            s_OverlayRoot.RegisterCallback<DragEnterEvent>(_ =>
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                s_OverlayRoot.style.backgroundColor = s_OverlayDragHighlightColor;
            });

            s_OverlayRoot.RegisterCallback<DragLeaveEvent>(_ =>
            {
                s_OverlayRoot.style.backgroundColor = s_OverlayBackgroundColor;
            });

            s_OverlayRoot.RegisterCallback<DragUpdatedEvent>(_ =>
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            });

            s_OverlayRoot.RegisterCallback<DragPerformEvent>(_ =>
            {
                DragAndDrop.AcceptDrag();
                s_OverlayRoot.style.backgroundColor = s_OverlayBackgroundColor;

                EnsureFolderExists();

                foreach (Object obj in DragAndDrop.objectReferences)
                {
                    s_FavoritesService.AddItem(CurrentGroup, obj);
                }

                AnimateContentChange();
            });
        }

        #endregion

        #region Popup - Delete Confirmation

        private static void ShowDeleteConfirmPopup()
        {
            if (CurrentGroup == null) return;

            CloseAllPopups();

            s_DeleteConfirmPopup = CreatePopupContainer(k_DeleteConfirmPopupName);

            Label titleLabel = new($"Delete '{CurrentGroup.Name}'?")
            {
                style =
                {
                    fontSize = 13,
                    color = Color.white,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    marginBottom = 12,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
            s_DeleteConfirmPopup.Add(titleLabel);

            VisualElement buttonRow = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.Center
                }
            };

            Button cancelButton = CreatePopupButton("Cancel", s_PopupButtonCancelColor, s_PopupButtonCancelHoverColor,
                CloseDeleteConfirmPopup);
            cancelButton.style.marginRight = 8;
            buttonRow.Add(cancelButton);

            Button deleteButton = CreatePopupButton("Delete", s_PopupButtonDeleteColor, s_PopupButtonDeleteHoverColor,
                () =>
                {
                    CloseDeleteConfirmPopup();
                    DeleteCurrentGroup();
                });
            buttonRow.Add(deleteButton);

            s_DeleteConfirmPopup.Add(buttonRow);
            s_OverlayRoot.Add(s_DeleteConfirmPopup);

            s_DeleteConfirmPopup.schedule.Execute(() =>
            {
                PositionPopupBottomRootOverlay(s_DeleteConfirmPopup);
                AnimatePopupIn(s_DeleteConfirmPopup);
            });
            
            
        }

        private static void CloseDeleteConfirmPopup()
        {
            if (s_DeleteConfirmPopup == null) return;

            AnimatePopupOut(s_DeleteConfirmPopup, () =>
            {
                s_DeleteConfirmPopup?.RemoveFromHierarchy();
                s_DeleteConfirmPopup = null;
            });
        }

        #endregion

        #region Popup - Folder Editor

        private static void ShowFolderEditorPopup()
        {
            FavoritesGroup group = CurrentGroup;
            if (group == null) return;

            CloseAllPopups();

            s_FolderEditorPopup = CreatePopupContainer(k_FolderEditorPopupName);

            Label titleLabel = new("Edit Folder")
            {
                style =
                {
                    fontSize = 13,
                    color = Color.white,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    marginBottom = 12,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAutoSize = new TextAutoSize(TextAutoSizeMode.BestFit,1,13),
                }
            };
            s_FolderEditorPopup.Add(titleLabel);

            // Name field
            VisualElement nameRow = CreateLabeledRow("Name");
            TextField nameField = new()
            {
                value = group.Name,
                style =
                {
                    flexGrow = 1
                }
            };
            nameField.RegisterValueChangedCallback(evt =>
            {
                group.Name = evt.newValue;
                s_FavoritesService.Storage.Save();
                UpdateFolderLayoutGroup();
            });
            nameRow.Add(nameField);
            s_FolderEditorPopup.Add(nameRow);

            // Color field
            VisualElement colorRow = CreateLabeledRow("Color");
            ColorField colorField = new()
            {
                value = group.Color,
                showAlpha = false,
                style =
                {
                    flexGrow = 1
                }
            };
            colorField.RegisterValueChangedCallback(evt =>
            {
                group.Color = evt.newValue;
                s_FavoritesService.Storage.Save();
                UpdateFolderLayoutGroup();
            });
            colorRow.Add(colorField);
            s_FolderEditorPopup.Add(colorRow);

            // Close button
            Button closeButton = CreatePopupButton("Close", s_PopupButtonCancelColor, s_PopupButtonCancelHoverColor,
                CloseFolderEditorPopup);
            closeButton.style.marginTop = 12;
            closeButton.style.alignSelf = Align.Center;
            s_FolderEditorPopup.Add(closeButton);

            s_OverlayRoot.Add(s_FolderEditorPopup);
            s_FolderEditorPopup.schedule.Execute(() =>
            {
                PositionPopupBottomRootOverlay(s_FolderEditorPopup);
                AnimatePopupIn(s_FolderEditorPopup);
            });

        }

        private static void CloseFolderEditorPopup()
        {
            if (s_FolderEditorPopup == null) return;

            AnimatePopupOut(s_FolderEditorPopup, () =>
            {
                s_FolderEditorPopup?.RemoveFromHierarchy();
                s_FolderEditorPopup = null;
            });
        }

        #endregion

        #region Popup Utilities

        private static VisualElement CreatePopupContainer(string name)
        {
            return new VisualElement
            {
                name = name,
                style =
                {
                    position = Position.Absolute,
                    backgroundColor = s_PopupBackgroundColor,
                    borderTopLeftRadius = k_PopupBorderRadius,
                    borderTopRightRadius = k_PopupBorderRadius,
                    borderBottomLeftRadius = k_PopupBorderRadius,
                    borderBottomRightRadius = k_PopupBorderRadius,
                    borderTopColor = s_PopupBorderColor,
                    borderBottomColor = s_PopupBorderColor,
                    borderLeftColor = s_PopupBorderColor,
                    borderRightColor = s_PopupBorderColor,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    paddingTop = k_PopupPadding,
                    paddingBottom = k_PopupPadding,
                    paddingLeft = k_PopupPadding,
                    paddingRight = k_PopupPadding,
                    width =  Length.Percent(100),
                    transitionProperty = new List<StylePropertyName> { new("opacity"), new("translate") },
                    transitionDuration = new List<TimeValue>
                        { new(k_AnimationDurationMs, TimeUnit.Millisecond), new(k_AnimationDurationMs, TimeUnit.Millisecond) }
                }
            };
        }

        private static Button CreatePopupButton(string text, Color bgColor, Color hoverColor, Action onClick)
        {
            Button button = new(onClick)
            {
                text = text,
                style =
                {
                    backgroundColor = bgColor,
                    color = Color.white,
                    paddingLeft = 16,
                    paddingRight = 16,
                    paddingTop = 6,
                    paddingBottom = 6,
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
                    borderLeftWidth = 0,
                    borderRightWidth = 0
                }
            };

            button.RegisterCallback<MouseEnterEvent, Tuple<Button, Color>>((_, t) =>
            {
                t.Item1.style.backgroundColor = t.Item2;
            }, new Tuple<Button, Color>(button, hoverColor));

            button.RegisterCallback<MouseLeaveEvent, Tuple<Button, Color>>((_, t) =>
            {
                t.Item1.style.backgroundColor = t.Item2;
            }, new Tuple<Button, Color>(button, bgColor));

            return button;
        }

        private static VisualElement CreateLabeledRow(string labelText)
        {
            VisualElement row = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 8,
                    flexShrink = 1,
                }
            };

            Label label = new(labelText)
            {
                style =
                {
                    width = 50,
                    color = s_TextColor,
                    fontSize = 11,
                    unityTextAutoSize = new TextAutoSize()
                }
            };
            row.Add(label);

            return row;
        }


        // Need to be called after popup created (Schedule)
        private static void PositionPopupBottomRootOverlay(VisualElement popup)
        {
            VisualElement capsule = s_OverlayRoot?.Q<VisualElement>(k_FolderCapsuleName);
            if (capsule == null || s_OverlayRoot == null) return;

            // Position above the capsule, centered
            popup.schedule.Execute(() =>
            {
                Rect overlayBounds = s_OverlayRoot.contentRect;

                // float popupX = (overlayBounds.width - k_PopupWidth) / 2;
                float popupY = (overlayBounds.height - popup.worldBound.height);
                // popup.style.left = popupX;
                popup.style.top = popupY;
            });
        }

        private static void AnimatePopupIn(VisualElement popup)
        {
            popup.style.opacity = 0;
            popup.style.translate = new Translate(0, 10);
            popup.schedule.Execute(() =>
            {
                popup.style.opacity = 1;
                popup.style.translate = new Translate(0, 0);
            });
        }

        private static void AnimatePopupOut(VisualElement popup, Action onComplete)
        {
            popup.style.opacity = 0;
            popup.style.translate = new Translate(0, 10);
            popup.schedule.Execute(onComplete).StartingIn(k_AnimationDurationMs);
        }

        private static void CloseAllPopups()
        {
            if (s_DeleteConfirmPopup != null)
            {
                s_DeleteConfirmPopup.RemoveFromHierarchy();
                s_DeleteConfirmPopup = null;
            }

            if (s_FolderEditorPopup != null)
            {
                s_FolderEditorPopup.RemoveFromHierarchy();
                s_FolderEditorPopup = null;
            }
        }

        #endregion
    }
}
