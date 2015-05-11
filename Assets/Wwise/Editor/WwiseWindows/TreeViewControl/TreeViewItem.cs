using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public class TreeViewItem
{
    public string Header = string.Empty;
    public bool IsDraggable = false;
    public bool IsExpanded = false;
    public bool IsCheckBox = false;
    public bool IsChecked = false;
    public bool IsHover = false;
    public bool IsSelected = false;
	public bool IsHidden = false;
    public List<TreeViewItem> Items = new List<TreeViewItem>();

    public TreeViewControl ParentControl = null;
    public TreeViewItem Parent = null;
    public object DataContext = null;

	static int s_clickCount = 0;

    public class ClickEventArgs : System.EventArgs
    {
		public uint m_clickCount = 0;

		public ClickEventArgs(uint in_clickCount)
		{
			m_clickCount = in_clickCount;
		}
    }
    public EventHandler Click = null;


	public class CheckedEventArgs : System.EventArgs
    {
    }
    public EventHandler Checked = null;

    public class UncheckedEventArgs : System.EventArgs
    {
    }
    public EventHandler Unchecked = null;

    public class SelectedEventArgs : System.EventArgs
    {
    }
    public EventHandler Selected = null;

    public class UnselectedEventArgs : System.EventArgs
    {
    }
    public EventHandler Unselected = null;

    public class DragEventArgs : System.EventArgs
    {
    }
    public EventHandler Dragged = null;

    public class CustomIconEventArgs : System.EventArgs
    {
    }
    public EventHandler CustomIconBuilder = null;

    /// <summary>
    /// The distance to the hover item
    /// </summary>
    float m_hoverTime = 0f;

    public TreeViewItem(TreeViewControl parentControl, TreeViewItem parent)
    {
        ParentControl = parentControl;
        Parent = parent;

        if (null == parentControl)
        {
            return;
        }
    }

    public TreeViewItem AddItem(string header)
    {
        TreeViewItem item = new TreeViewItem(ParentControl, this) { Header = header };
        Items.Add(item);
        return item;
    }

    public TreeViewItem AddItem(string header, object context)
    {
        TreeViewItem item = new TreeViewItem(ParentControl, this) { Header = header, DataContext = context };
        Items.Add(item);
        return item;
    }

	public TreeViewItem AddItem(string header, object context, bool in_isExpended)
	{
		TreeViewItem item = new TreeViewItem(ParentControl, this) { Header = header, DataContext = context, IsExpanded = in_isExpended};
		Items.Add(item);
		return item;
	}
	
	public TreeViewItem AddItem(string header, bool isExpanded)
    {
        TreeViewItem item = new TreeViewItem(ParentControl, this) { Header = header, IsExpanded = isExpanded };
        Items.Add(item);
        return item;
    }

    public TreeViewItem AddItem(string header, bool isDraggable, bool isExpanded, object context)
    {
        TreeViewItem item = new TreeViewItem(ParentControl, this) { Header = header, IsDraggable = isDraggable, IsExpanded = isExpanded, DataContext = context };
        Items.Add(item);
        return item;
    }

    public TreeViewItem AddItem(string header, bool isExpanded, bool isChecked)
    {
        TreeViewItem item = new TreeViewItem(ParentControl, this) { Header = header, IsExpanded = isExpanded, IsCheckBox = true, IsChecked = isChecked };
        Items.Add(item);
        return item;
    }

    public TreeViewItem FindItemByName(string name)
    {
        foreach (TreeViewItem item in Items)
        {
            if (item.Header == name)
            {
                return item;
            }
        }

        return null;
    }

    public bool HasChildItems()
    {
        if (null == Items ||
            Items.Count == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public enum SiblingOrder
    {
        FIRST_CHILD,
        MIDDLE_CHILD,
        LAST_CHILD,
    }

    public enum TextureIcons
    {
        BLANK,
        GUIDE,
        LAST_SIBLING_COLLAPSED,
        LAST_SIBLING_EXPANDED,
        LAST_SIBLING_NO_CHILD,
        MIDDLE_SIBLING_COLLAPSED,
        MIDDLE_SIBLING_EXPANDED,
        MIDDLE_SIBLING_NO_CHILD,
        NORMAL_CHECKED,
        NORMAL_UNCHECKED,
    }

    float CalculateHoverTime(Rect rect, Vector3 mousePos)
    {
        if (rect.Contains(mousePos))
        {
            return 0f;
        }
        float midPoint = (rect.yMin + rect.yMax) * 0.5f;
        float pointA = mousePos.y;
        return Mathf.Abs(midPoint - pointA) / 50f;
    }

    public void DisplayItem(int levels, SiblingOrder siblingOrder)
    {
        if (null == ParentControl || IsHidden)
        {
            return;
        }

        GUILayout.BeginHorizontal();

        for (int index = 0; index < levels; ++index)
        {
            ParentControl.Button(TextureIcons.GUIDE);
        }

        if (!HasChildItems())
        {
            bool result;
            switch (siblingOrder)
            {
                case SiblingOrder.FIRST_CHILD:
                    result = ParentControl.Button(TextureIcons.MIDDLE_SIBLING_NO_CHILD);
                    break;
                case SiblingOrder.MIDDLE_CHILD:
                    result = ParentControl.Button(TextureIcons.MIDDLE_SIBLING_NO_CHILD);
                    break;
                case SiblingOrder.LAST_CHILD:
                    result = ParentControl.Button(TextureIcons.LAST_SIBLING_NO_CHILD);
                    break;
                default:
                    result = false;
                    break;
            }
            if (result)
            {
                IsExpanded = !IsExpanded;
            }
        }
        else
        {
            if (IsExpanded)
            {
                bool result;
                switch (siblingOrder)
                {
                    case SiblingOrder.FIRST_CHILD:
                        result = ParentControl.Button(TextureIcons.MIDDLE_SIBLING_EXPANDED);
                        break;
                    case SiblingOrder.MIDDLE_CHILD:
                        result = ParentControl.Button(TextureIcons.MIDDLE_SIBLING_EXPANDED);
                        break;
                    case SiblingOrder.LAST_CHILD:
                        result = ParentControl.Button(TextureIcons.LAST_SIBLING_EXPANDED);
                        break;
                    default:
                        result = false;
                        break;
                }
                if (result)
                {
                    IsExpanded = !IsExpanded;
                }
            }
            else
            {
                bool result;
                switch (siblingOrder)
                {
                    case SiblingOrder.FIRST_CHILD:
                        result = ParentControl.Button(TextureIcons.MIDDLE_SIBLING_COLLAPSED);
                        break;
                    case SiblingOrder.MIDDLE_CHILD:
                        result = ParentControl.Button(TextureIcons.MIDDLE_SIBLING_COLLAPSED);
                        break;
                    case SiblingOrder.LAST_CHILD:
                        result = ParentControl.Button(TextureIcons.LAST_SIBLING_COLLAPSED);
                        break;
                    default:
                        result = false;
                        break;
                }
                if (result)
                {
                    IsExpanded = !IsExpanded;
                }
            }
        }


		bool clicked = false;

        // display the text for the tree view
        if (!string.IsNullOrEmpty(Header))
        {
            bool isSelected;
            if (ParentControl.SelectedItem == this &&
                !ParentControl.m_forceDefaultSkin)
            {
                //use selected skin
                GUI.skin = ParentControl.m_skinSelected;
                isSelected = true;
            }
            else
            {
                isSelected = false;
            }

            if (IsCheckBox)
            {
                if (IsChecked &&
                    ParentControl.Button(TextureIcons.NORMAL_CHECKED))
                {
                    IsChecked = false;
                    if (ParentControl.SelectedItem != this)
                    {
                        ParentControl.SelectedItem = this;
                        this.IsSelected = true;
                        if (null != Selected)
                        {
                            Selected.Invoke(this, new SelectedEventArgs());
                        }
                    }
                    if (null != Unchecked)
                    {
                        Unchecked.Invoke(this, new UncheckedEventArgs());
                    }
                }
                else if (!IsChecked &&
                    ParentControl.Button(TextureIcons.NORMAL_UNCHECKED))
                {
                    IsChecked = true;
                    if (ParentControl.SelectedItem != this)
                    {
                        ParentControl.SelectedItem = this;
                        this.IsSelected = true;
                        if (null != Selected)
                        {
                            Selected.Invoke(this, new SelectedEventArgs());
                        }
                    }
                    if (null != Checked)
                    {
                        Checked.Invoke(this, new CheckedEventArgs());
                    }
                }

                ParentControl.Button(TextureIcons.BLANK);
            }

            // Add a custom icon, if any
            if (null != CustomIconBuilder)
            {
                CustomIconBuilder.Invoke(this, new CustomIconEventArgs());
                ParentControl.Button(TextureIcons.BLANK);
            }

			if(Event.current.isMouse)
				s_clickCount = Event.current.clickCount;

			
            if (ParentControl.IsHoverEnabled)
            {
                GUISkin oldSkin = GUI.skin;
                if (isSelected)
                {
                    GUI.skin = ParentControl.m_skinSelected;
                }
                else if (IsHover)
                {
                    GUI.skin = ParentControl.m_skinHover;
                }
                else
                {
                    GUI.skin = ParentControl.m_skinUnselected;
                }
                if (ParentControl.IsHoverAnimationEnabled)
                {
                    GUI.skin.button.fontSize = (int)Mathf.Lerp(20f, 12f, m_hoverTime);
                }
                GUI.SetNextControlName("toggleButton"); //workaround to dirty GUI
                if (GUILayout.Button(Header))
                {
                    GUI.FocusControl("toggleButton"); //workaround to dirty GUI
                    if (ParentControl.SelectedItem != this)
                    {
                        ParentControl.SelectedItem = this;
                        this.IsSelected = true;
                        if (null != Selected)
                        {
                            Selected.Invoke(this, new SelectedEventArgs());
                        }
                    }
					if (null != Click && (uint)s_clickCount <= 2)
					{
					
						clicked = true;
                    }
                }
                GUI.skin = oldSkin;
            }
            else
            {				
				GUI.SetNextControlName("toggleButton"); //workaround to dirty GUI
                if (GUILayout.RepeatButton(Header))
                {
					GUI.FocusControl("toggleButton"); //workaround to dirty GUI
                    if (ParentControl.SelectedItem != this)
                    {
                        ParentControl.SelectedItem = this;
                        this.IsSelected = true;
                        if (null != Selected)
                        {
                            Selected.Invoke(this, new SelectedEventArgs());
                        }
					}
					if (null != Click && (uint)s_clickCount <= 2) 
					{
						
						clicked = true;;
					}
				}
			}
			
			if (isSelected &&
			    !ParentControl.m_forceDefaultSkin)
            {
                //return to default skin
                GUI.skin = ParentControl.m_skinUnselected;
            }
        }

        GUILayout.EndHorizontal();

        if (ParentControl.IsHoverEnabled)
        {
            if (null != Event.current &&
                Event.current.type == EventType.Repaint)
            {
                Vector2 mousePos = Event.current.mousePosition;
                if (ParentControl.HasFocus(mousePos))
                {
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    if (lastRect.Contains(mousePos))
                    {
                        IsHover = true;
                        ParentControl.HoverItem = this;
                    }
                    else
                    {
                        IsHover = false;
                    }
                    if (ParentControl.IsHoverEnabled &&
                        ParentControl.IsHoverAnimationEnabled)
                    {
                        m_hoverTime = CalculateHoverTime(lastRect, Event.current.mousePosition);
                    }
                }
            }
        }

        if (HasChildItems() &&
            IsExpanded)
        {
            for (int index = 0; index < Items.Count; ++index)
            {
                TreeViewItem child = Items[index];
                child.Parent = this;
                if ((index + 1) == Items.Count)
                {
                    child.DisplayItem(levels + 1, SiblingOrder.LAST_CHILD);
                }
                else if (index == 0)
                {
                    child.DisplayItem(levels + 1, SiblingOrder.FIRST_CHILD);
                }
                else
                {
                    child.DisplayItem(levels + 1, SiblingOrder.MIDDLE_CHILD);
                }
            }
        }

		if(clicked)
		{
			Click.Invoke(this, new ClickEventArgs((uint)s_clickCount)); 
		}

        if (IsSelected &&
            ParentControl.SelectedItem != this)
        {
            if (null != Unselected)
            {
                Unselected.Invoke(this, new UnselectedEventArgs());
            }
        }
        IsSelected = ParentControl.SelectedItem == this;


        if (IsDraggable)
        {
            HandleGUIEvents();
        }
    }

    void HandleGUIEvents()
    {
        // Handle events
        Event evt = Event.current;
        EventType currentEventType = evt.type;


        if (currentEventType == EventType.DragExited) DragAndDrop.PrepareStartDrag();

        if (currentEventType == EventType.MouseDown)
        {
            DragAndDrop.PrepareStartDrag(); // reset data
        }
        else if (currentEventType == EventType.MouseDrag)
        {
            if (null != Dragged)
            {
                Dragged.Invoke(ParentControl.SelectedItem, new DragEventArgs());
                evt.Use();
            }
        }

    }



}