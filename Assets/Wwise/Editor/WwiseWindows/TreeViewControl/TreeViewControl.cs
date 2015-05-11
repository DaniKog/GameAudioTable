using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TreeViewControl
{
    public int X = 0;
    public int Y = 0;
    public int Width = 400;
    public int Height = 400;
    public bool IsExpanded = false;
    public bool IsHoverEnabled = false;
    public bool IsHoverAnimationEnabled = false;

    void Start()
    {
        SelectedItem = null;
    }

    /// <summary>
    /// The root item
    /// </summary>
    public TreeViewItem m_roomItem = null;
    public TreeViewItem RootItem
    {
        get
        {
            if (null == m_roomItem)
            {
                m_roomItem = new TreeViewItem(this, null) { Header = "Root item" };
            }
            return m_roomItem;
        }
    }

    /// <summary>
    /// Accesses the root item header
    /// </summary>
    public string Header
    {
        get
        {
            return RootItem.Header;
        }
        set
        {
            RootItem.Header = value;
        }
    }

    /// <summary>
    /// Accesses the root data context
    /// </summary>
    public object DataContext
    {
        get
        {
            return RootItem.DataContext;
        }
        set
        {
            RootItem.DataContext = value;
        }
    }

    /// <summary>
    /// Accessor to the root items
    /// </summary>
    public List<TreeViewItem> Items
    {
        get
        {
            return RootItem.Items;
        }
        set
        {
            RootItem.Items = value;
        }
    }

    
    /// <summary>
    /// Skin used by the tree view
    /// </summary>
    public GUISkin m_skinHover = null;
    public GUISkin m_skinUnselected = null;
    public GUISkin m_skinSelected = null;

    /// <summary>
    /// Texture skin references
    /// </summary>
    public Texture2D m_textureBlank = null;
	public Texture2D m_textureGuide = null;
    public Texture2D m_textureLastSiblingCollapsed = null;
    public Texture2D m_textureLastSiblingExpanded = null;
    public Texture2D m_textureLastSiblingNoChild = null;
    public Texture2D m_textureMiddleSiblingCollapsed = null;
    public Texture2D m_textureMiddleSiblingExpanded = null;
    public Texture2D m_textureMiddleSiblingNoChild = null;
	public Texture2D m_textureNormalChecked = null;
	public Texture2D m_textureNormalUnchecked = null;
	public Texture2D m_textureSelectedBackground = null;

    /// <summary>
    /// Force to use the button text
    /// </summary>
    public bool m_forceButtonText = false;

    /// <summary>
    /// Use the default skin
    /// </summary>
    public bool m_forceDefaultSkin = false;

    /// <summary>
    /// The selected item
    /// </summary>
    public TreeViewItem HoverItem = null;
	public TreeViewItem SelectedItem = null;
	
	/// <summary>
	/// Show the button texture 
	/// </summary>
	/// <param name="texture">
	/// A <see cref="Texture2D"/>
	/// </param>
	/// <returns>
	/// A <see cref="System.Boolean"/>
	/// </returns>
	protected bool ShowButtonTexture(Texture2D texture)
	{
		return GUILayout.Button(texture, GUILayout.MaxWidth(texture.width), GUILayout.MaxHeight(texture.height));
	}

    /// <summary>
    /// Find the button texture/text by enum
    /// </summary>
    /// <param name="item"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public bool Button(TreeViewItem.TextureIcons item)
    {
        switch (item)
        {
            case TreeViewItem.TextureIcons.BLANK:
                if (null == m_textureGuide ||
                    m_forceButtonText)
                {
                    GUILayout.Label("", GUILayout.MaxWidth(4));
                }
                else
                {
                    GUILayout.Label(m_textureBlank, GUILayout.MaxWidth(4), GUILayout.MaxHeight(16));
                }
				return false;
            case TreeViewItem.TextureIcons.GUIDE:
                if (null == m_textureGuide ||
                    m_forceButtonText)
                {
                    GUILayout.Label("|", GUILayout.MaxWidth(16));
                }
                else
                {
                    GUILayout.Label(m_textureGuide, GUILayout.MaxWidth(16), GUILayout.MaxHeight(16));
                }
				return false;
            case TreeViewItem.TextureIcons.LAST_SIBLING_COLLAPSED:
                if (null == m_textureLastSiblingCollapsed ||
                    m_forceButtonText)
                {
                    return GUILayout.Button("<", GUILayout.MaxWidth(16));
                }
                else
                {
                    return ShowButtonTexture(m_textureLastSiblingCollapsed);
                }
            case TreeViewItem.TextureIcons.LAST_SIBLING_EXPANDED:
                if (null == m_textureLastSiblingExpanded ||
                    m_forceButtonText)
                {
                    return GUILayout.Button(">", GUILayout.MaxWidth(16));
                }
                else
                {
                    return ShowButtonTexture(m_textureLastSiblingExpanded);
                }
            case TreeViewItem.TextureIcons.LAST_SIBLING_NO_CHILD:
                if (null == m_textureLastSiblingNoChild ||
                    m_forceButtonText)
                {
                    return GUILayout.Button("-", GUILayout.MaxWidth(16));
                }
                else
                {
                    return GUILayout.Button(m_textureLastSiblingNoChild, GUILayout.MaxWidth(16));
                }
            case TreeViewItem.TextureIcons.MIDDLE_SIBLING_COLLAPSED:
                if (null == m_textureMiddleSiblingCollapsed ||
                    m_forceButtonText)
                {
                    return GUILayout.Button("<", GUILayout.MaxWidth(16));
                }
                else
                {
                    return ShowButtonTexture(m_textureMiddleSiblingCollapsed);
                }
            case TreeViewItem.TextureIcons.MIDDLE_SIBLING_EXPANDED:
                if (null == m_textureMiddleSiblingExpanded ||
                    m_forceButtonText)
                {
                    return GUILayout.Button(">", GUILayout.MaxWidth(16));
                }
                else
                {
                    return GUILayout.Button(m_textureMiddleSiblingExpanded, GUILayout.MaxWidth(16));
                }
            case TreeViewItem.TextureIcons.MIDDLE_SIBLING_NO_CHILD:
                if (null == m_textureMiddleSiblingNoChild ||
                    m_forceButtonText)
                {
                    return GUILayout.Button("-", GUILayout.MaxWidth(16));
                }
                else
                {
                    return ShowButtonTexture(m_textureMiddleSiblingNoChild);
                }
			case TreeViewItem.TextureIcons.NORMAL_CHECKED:
                if (null == m_textureNormalChecked ||
                    m_forceButtonText)
                {
                    return GUILayout.Button("x", GUILayout.MaxWidth(16));
                }
                else
                {
                    return GUILayout.Button(m_textureNormalChecked, GUILayout.MaxWidth(16));
                }
			case TreeViewItem.TextureIcons.NORMAL_UNCHECKED:
                if (null == m_textureNormalUnchecked ||
                    m_forceButtonText)
                {
                    return GUILayout.Button("o", GUILayout.MaxWidth(16));
                }
                else
                {
                    return ShowButtonTexture(m_textureNormalUnchecked);
                }
            default:
                return false;
        }
    }

    /// <summary>
    /// Handle the unity scrolling vector
    /// </summary>
    protected Vector2 m_scrollView = Vector2.zero;

    public enum DisplayTypes
    {
        NONE, //used by the inspector
        USE_SCROLL_VIEW, //used by panels
        USE_SCROLL_AREA, //used by gameview, sceneview
    }

    /// <summary>
    /// Called from OnGUI or EditorWindow.OnGUI
    /// </summary>
    public virtual void DisplayTreeView(TreeViewControl.DisplayTypes displayType)
    {
        GUILayout.BeginHorizontal("box");

        AssignDefaults(); 
        if (!m_forceDefaultSkin)
        {
            ApplySkinKeepingScrollbars();
        }

        switch (displayType)
        {
            case TreeViewControl.DisplayTypes.USE_SCROLL_VIEW:
                m_scrollView = GUILayout.BeginScrollView(m_scrollView);//, GUILayout.MaxWidth(Width), GUILayout.MaxHeight(Height));
                break;
            //case TreeViewControl.DisplayTypes.USE_SCROLL_AREA:
            //    GUILayout.BeginArea(new Rect(X, Y, Width, Height));
            //    m_scrollView = GUILayout.BeginScrollView(m_scrollView);//, GUILayout.MaxWidth(Width), GUILayout.MaxHeight(Height));
            //    break;
        }

        RootItem.DisplayItem(0, TreeViewItem.SiblingOrder.FIRST_CHILD);

        switch (displayType)
        {
            case TreeViewControl.DisplayTypes.USE_SCROLL_VIEW:
                GUILayout.EndScrollView();
                break;
            //case TreeViewControl.DisplayTypes.USE_SCROLL_AREA:
            //    GUILayout.EndScrollView();
            //    GUILayout.EndArea();
            //    break;
        }

        GUI.skin = null;

        GUILayout.EndHorizontal();
    }

    void ApplySkinKeepingScrollbars()
    {
        GUIStyle hScroll = GUI.skin.horizontalScrollbar;
        GUIStyle hScrollDButton = GUI.skin.horizontalScrollbarLeftButton;
        GUIStyle hScrollUButton = GUI.skin.horizontalScrollbarRightButton;
        GUIStyle hScrollThumb = GUI.skin.horizontalScrollbarThumb;
        GUIStyle vScroll = GUI.skin.verticalScrollbar;
        GUIStyle vScrollDButton = GUI.skin.verticalScrollbarDownButton;
        GUIStyle vScrollUButton = GUI.skin.verticalScrollbarUpButton;
        GUIStyle vScrollThumb = GUI.skin.verticalScrollbarThumb;

        GUI.skin = m_skinUnselected;

        GUI.skin.horizontalScrollbar = hScroll;
        GUI.skin.horizontalScrollbarLeftButton = hScrollDButton;
        GUI.skin.horizontalScrollbarRightButton = hScrollUButton;
        GUI.skin.horizontalScrollbarThumb = hScrollThumb;
        GUI.skin.verticalScrollbar = vScroll;
        GUI.skin.verticalScrollbarDownButton = vScrollDButton;
        GUI.skin.verticalScrollbarUpButton = vScrollUButton;
        GUI.skin.verticalScrollbarThumb = vScrollThumb;
    }

    public bool HasFocus(Vector2 mousePos)
    {
        Rect rect = new Rect(m_scrollView.x, m_scrollView.y,600,900);// Width, Height);
        return rect.Contains(mousePos);
    }
   
    public void ApplySkin()
    {
        // create new skin instance
        GUISkin skinHover = (GUISkin)Object.Instantiate(m_skinHover);
        GUISkin skinSelected = (GUISkin)Object.Instantiate(m_skinSelected);
        GUISkin skinUnselected = (GUISkin)Object.Instantiate(m_skinUnselected);

        // name the skins
        skinHover.name = "Hover";
        skinSelected.name = "Selected";
        skinUnselected.name = "Unselected";

        m_skinHover = skinHover;
        m_skinSelected = skinSelected;
        m_skinUnselected = skinUnselected;
    }

    public virtual void AssignDefaults()
    {
        // create new skin instance
        GUISkin skinHover = ScriptableObject.CreateInstance<GUISkin>();
        GUISkin skinSelected = ScriptableObject.CreateInstance<GUISkin>();
        GUISkin skinUnselected = ScriptableObject.CreateInstance<GUISkin>();
        skinHover.hideFlags = HideFlags.HideAndDontSave;
		skinSelected.hideFlags = HideFlags.HideAndDontSave;
		skinUnselected.hideFlags = HideFlags.HideAndDontSave;
		
		// name the skins
        skinHover.name = "Hover";
        skinSelected.name = "Selected";
        skinUnselected.name = "Unselected";

        string tempWwisePath = "Assets/Wwise/Editor/WwiseWindows/TreeViewControl/";

        m_textureBlank = GetTexture(tempWwisePath + "blank.png");
        m_textureGuide = GetTexture(tempWwisePath + "guide.png");
        m_textureLastSiblingCollapsed = GetTexture(tempWwisePath + "last_sibling_collapsed.png");
        m_textureLastSiblingExpanded = GetTexture(tempWwisePath + "last_sibling_expanded.png");
        m_textureLastSiblingNoChild = GetTexture(tempWwisePath + "last_sibling_nochild.png");
        m_textureMiddleSiblingCollapsed = GetTexture(tempWwisePath + "middle_sibling_collapsed.png");
        m_textureMiddleSiblingExpanded = GetTexture(tempWwisePath + "middle_sibling_expanded.png");
        m_textureMiddleSiblingNoChild = GetTexture(tempWwisePath + "middle_sibling_nochild.png");
        m_textureNormalChecked = GetTexture(tempWwisePath + "normal_checked.png");
        m_textureNormalUnchecked = GetTexture(tempWwisePath + "normal_unchecked.png");
        m_textureSelectedBackground = GetTexture(tempWwisePath + "selected_background_color.png");

        m_skinHover = skinHover;
        m_skinSelected = skinSelected;
        m_skinUnselected = skinUnselected;

        SetBackground(m_skinHover.button, null);
        SetBackground(m_skinHover.toggle, null);
        SetButtonFontSize(m_skinHover.button);
        SetButtonFontSize(m_skinHover.toggle);
        RemoveMargins(m_skinHover.button);
        RemoveMargins(m_skinHover.toggle);
        SetTextColor(m_skinHover.button, Color.yellow);
        SetTextColor(m_skinHover.toggle, Color.yellow);

        SetBackground(m_skinSelected.button,m_textureSelectedBackground);
        SetBackground(m_skinSelected.toggle,m_textureSelectedBackground);
        SetButtonFontSize(m_skinSelected.button);
        SetButtonFontSize(m_skinSelected.toggle);
        RemoveMargins(m_skinSelected.button);
        RemoveMargins(m_skinSelected.toggle);
        SetTextColor(m_skinSelected.button, Color.yellow);
        SetTextColor(m_skinSelected.toggle, Color.yellow);

        SetBackground(m_skinUnselected.button, null);
        SetBackground(m_skinUnselected.toggle, null);
        SetButtonFontSize(m_skinUnselected.button);
        SetButtonFontSize(m_skinUnselected.toggle);
        RemoveMargins(m_skinUnselected.button);
        RemoveMargins(m_skinUnselected.toggle);
		if( Application.HasProLicense() )
		{
	        SetTextColor(m_skinUnselected.button, Color.white);
	        SetTextColor(m_skinUnselected.toggle, Color.white);
	    }
	    else
	    {
			SetTextColor(m_skinUnselected.button, Color.black);
			SetTextColor(m_skinUnselected.toggle, Color.black);
		}
    }

    void SetBackground(GUIStyle style, Texture2D texture)
    {
        style.active.background = texture;
        style.focused.background = texture;
        style.hover.background = texture;
        style.normal.background = texture;
        style.onActive.background = texture;
        style.onFocused.background = texture;
        style.onHover.background = texture;
        style.onNormal.background = texture;
    }

    void SetTextColor(GUIStyle style, Color color)
    {
        style.active.textColor = color;
        style.focused.textColor = color;
        style.hover.textColor = color;
        style.normal.textColor = color;
        style.onActive.textColor = color;
        style.onFocused.textColor = color;
        style.onHover.textColor = color;
        style.onNormal.textColor = color;
    }

    void RemoveMargins(GUIStyle style)
    {
        style.margin.bottom = 0;
        style.margin.left = 0;
        style.margin.right = 0;
        style.margin.top = 0;
    }

    void SetButtonFontSize(GUIStyle style)
    {
        style.fontSize = 12;
    }

    protected Texture2D GetTexture(string texturePath)
    {
        try
        {
            Texture2D item = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
            return item;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(string.Format("Failed to find local texture: {0}", ex));
            return null;
        }
    }

}