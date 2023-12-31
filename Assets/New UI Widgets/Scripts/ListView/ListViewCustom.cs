namespace UIWidgets
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Threading;
	using UIWidgets.Attributes;
	using UIWidgets.Extensions;
	using UIWidgets.l10n;
	using UIWidgets.Styles;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;
	using UnityEngine.Serialization;
	using UnityEngine.UI;

	/// <summary>
	/// Base class for custom ListViews.
	/// </summary>
	/// <typeparam name="TComponent">Type of DefaultItem component.</typeparam>
	/// <typeparam name="TItem">Type of item.</typeparam>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Reviewed.")]
	[DataBindSupport]
	public partial class ListViewCustom<TComponent, TItem> : ListViewCustomBase
		where TComponent : ListViewItem
	{
		/// <summary>
		/// DataSourceEvent.
		/// </summary>
		public class DataSourceEvent : UnityEvent<ListViewCustom<TComponent, TItem>>
		{
		}

		/// <inheritdoc/>
		public override ListViewType ListType
		{
			get
			{
				return listType;
			}

			set
			{
				listType = value;

				if (listRenderer != null)
				{
					listRenderer.Disable();
					listRenderer = null;
				}

				if (isListViewCustomInited)
				{
					SetDefaultItem(defaultItem);
				}
			}
		}

		/// <summary>
		/// The items.
		/// </summary>
		[SerializeField]
		protected List<TItem> customItems = new List<TItem>();

		/// <summary>
		/// Data source.
		/// </summary>
		#if UNITY_2020_1_OR_NEWER
		[NonSerialized]
		#endif
		protected ObservableList<TItem> dataSource;

		/// <summary>
		/// Gets or sets the data source.
		/// </summary>
		/// <value>The data source.</value>
		[DataBindField]
		public virtual ObservableList<TItem> DataSource
		{
			get
			{
				if (dataSource == null)
				{
					#pragma warning disable 0618
					dataSource = new ObservableList<TItem>(customItems);
					dataSource.OnChange += UpdateItems;
					#pragma warning restore 0618
				}

				if (!isListViewCustomInited)
				{
					Init();
				}

				return dataSource;
			}

			set
			{
				if (!isListViewCustomInited)
				{
					Init();
				}

				SetNewItems(value, IsMainThread);

				if (IsMainThread)
				{
					ListRenderer.SetPosition(0f);
				}
				else
				{
					DataSourceSetted = true;
				}
			}
		}

		[SerializeField]
		[FormerlySerializedAs("DefaultItem")]
		TComponent defaultItem;

		/// <summary>
		/// The default item template.
		/// </summary>
		public TComponent DefaultItem
		{
			get
			{
				return defaultItem;
			}

			set
			{
				SetDefaultItem(value);
			}
		}

		#region ComponentPool fields

		/// <summary>
		/// The components list.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<TComponent> Components = new List<TComponent>();

		/// <summary>
		/// The components cache list.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<TComponent> ComponentsCache = new List<TComponent>();

		/// <summary>
		/// The components displayed indices.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<int> ComponentsDisplayedIndices = new List<int>();

		/// <inheritdoc/>
		public override bool DestroyDefaultItemsCache
		{
			get
			{
				return ComponentsPool.DestroyComponents;
			}

			set
			{
				ComponentsPool.DestroyComponents = value;
			}
		}

		ListViewComponentPool<TComponent, TItem> componentsPool;

		/// <summary>
		/// The components pool.
		/// Constructor with lists needed to avoid lost connections when instantiated copy of the inited ListView.
		/// </summary>
		protected virtual ListViewComponentPool<TComponent, TItem> ComponentsPool
		{
			get
			{
				if (componentsPool == null)
				{
					componentsPool = new ListViewComponentPool<TComponent, TItem>(Components, ComponentsCache, ComponentsDisplayedIndices)
					{
						Owner = this,
						Container = Container,
						CallbackAdd = AddCallback,
						CallbackRemove = RemoveCallback,
						Template = DefaultItem,
						DestroyComponents = destroyDefaultItemsCache,
					};
				}

				return componentsPool;
			}
		}

		#endregion

		/// <summary>
		/// Gets the selected item.
		/// </summary>
		/// <value>The selected item.</value>
		[DataBindField]
		public TItem SelectedItem
		{
			get
			{
				if (SelectedIndex == -1)
				{
					return default(TItem);
				}

				return DataSource[SelectedIndex];
			}
		}

		/// <summary>
		/// Gets the selected items.
		/// </summary>
		/// <value>The selected items.</value>
		[DataBindField]
		public List<TItem> SelectedItems
		{
			get
			{
				return SelectedIndices.Convert<int, TItem>(GetDataItem);
			}
		}

		[Obsolete("Replaced with DataSource.Comparison.")]
		Func<IEnumerable<TItem>, IEnumerable<TItem>> sortFunc;

		/// <summary>
		/// Sort function.
		/// Deprecated. Replaced with DataSource.Comparison.
		/// </summary>
		[Obsolete("Replaced with DataSource.Comparison.")]
		public Func<IEnumerable<TItem>, IEnumerable<TItem>> SortFunc
		{
			get
			{
				return sortFunc;
			}

			set
			{
				sortFunc = value;
				if (Sort && isListViewCustomInited)
				{
					UpdateItems();
				}
			}
		}

		/// <summary>
		/// What to do when the object selected.
		/// </summary>
		[DataBindEvent("SelectedItem", "SelectedItems")]
		[SerializeField]
		public ListViewCustomEvent OnSelectObject = new ListViewCustomEvent();

		/// <summary>
		/// What to do when the object deselected.
		/// </summary>
		[DataBindEvent("SelectedItem", "SelectedItems")]
		[SerializeField]
		public ListViewCustomEvent OnDeselectObject = new ListViewCustomEvent();

		#region ListRenderer fields

		/// <summary>
		/// The DefaultItem layout group.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected LayoutGroup DefaultItemLayoutGroup;

		/// <summary>
		/// The DefaultItem layout group.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected LayoutGroup DefaultItemLayout;

		/// <summary>
		/// The layout elements of the DefaultItem.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		protected List<ILayoutElement> LayoutElements = new List<ILayoutElement>();

		[SerializeField]
		[HideInInspector]
		TComponent defaultItemCopy;

		/// <summary>
		/// Gets the default item copy.
		/// </summary>
		/// <value>The default item copy.</value>
		protected TComponent DefaultItemCopy
		{
			get
			{
				if (defaultItemCopy == null)
				{
					defaultItemCopy = Compatibility.Instantiate(DefaultItem);
					defaultItemCopy.transform.SetParent(DefaultItem.transform.parent, false);
					defaultItemCopy.gameObject.name = "DefaultItemCopy";
					defaultItemCopy.gameObject.SetActive(false);

					Utilities.FixInstantiated(DefaultItem, defaultItemCopy);
				}

				return defaultItemCopy;
			}
		}

		RectTransform defaultItemCopyRect;

		/// <summary>
		/// Gets the RectTransform of DefaultItemCopy.
		/// </summary>
		/// <value>RectTransform.</value>
		protected RectTransform DefaultItemCopyRect
		{
			get
			{
				if (defaultItemCopyRect == null)
				{
					defaultItemCopyRect = defaultItemCopy.transform as RectTransform;
				}

				return defaultItemCopyRect;
			}
		}
		#endregion

		[SerializeField]
		[HideInInspector]
		ListViewTypeBase listRenderer;

		/// <summary>
		/// ListView renderer.
		/// </summary>
		protected ListViewTypeBase ListRenderer
		{
			get
			{
				if (listRenderer == null)
				{
					listRenderer = GetRenderer(ListType);
				}

				return listRenderer;
			}

			set
			{
				listRenderer = value;
			}
		}

		/// <inheritdoc/>
		public override int MaxVisibleItems
		{
			get
			{
				Init();

				return ListRenderer.MaxVisibleItems;
			}
		}

		/// <summary>
		/// Selected items cache (to keep valid selected indices with updates).
		/// </summary>
		protected HashSet<TItem> SelectedItemsCache = new HashSet<TItem>();

		/// <inheritdoc/>
		protected override ILayoutBridge LayoutBridge
		{
			get
			{
				if (layoutBridge == null)
				{
					if (Layout != null)
					{
						layoutBridge = new EasyLayoutBridge(Layout, DefaultItem.transform as RectTransform, setContentSizeFitter && ListRenderer.AllowSetContentSizeFitter, ListRenderer.AllowControlRectTransform)
						{
							IsHorizontal = IsHorizontal(),
						};
						ListRenderer.DirectionChanged();
					}
					else
					{
						var hv_layout = Container.GetComponent<HorizontalOrVerticalLayoutGroup>();
						if (hv_layout != null)
						{
							layoutBridge = new StandardLayoutBridge(hv_layout, DefaultItem.transform as RectTransform, setContentSizeFitter && ListRenderer.AllowSetContentSizeFitter);
						}
					}
				}

				return layoutBridge;
			}
		}

		/// <inheritdoc/>
		public override bool LoopedListAvailable
		{
			get
			{
				return LoopedList && Virtualization && ListRenderer.IsVirtualizationSupported() && ListRenderer.AllowLoopedList;
			}
		}

		/// <summary>
		/// Raised when DataSource changed.
		/// </summary>
		public DataSourceEvent OnDataSourceChanged = new DataSourceEvent();

		/// <summary>
		/// Init this instance.
		/// </summary>
		public override void Init()
		{
			if (isListViewCustomInited)
			{
				return;
			}

			isListViewCustomInited = true;

			MainThread = Thread.CurrentThread;

			base.Init();
			Items = new List<ListViewItem>();

			SelectedItemsCache.Clear();
			for (int i = 0; i < SelectedIndices.Count; i++)
			{
				var index = SelectedIndices[i];
				SelectedItemsCache.Add(DataSource[index]);
			}

			DestroyGameObjects = false;

			CanSetData = DefaultItem is IViewData<TItem>;

			ComponentsPool.Template = defaultItem;

			DefaultItem.gameObject.SetActive(true);
			DefaultItem.FindSelectableObjects();

			if (ListRenderer.IsVirtualizationSupported())
			{
				ScrollRect = scrollRect;
				CalculateItemSize();
			}

			SetContentSizeFitter = setContentSizeFitter;

			DefaultItem.gameObject.SetActive(false);

			SetDirection(direction);

			UpdateItems();

			if (Layout != null)
			{
				Layout.SettingsChanged.AddListener(SetNeedResize);
			}

			Localization.OnLocaleChanged += LocaleChanged;
		}

		/// <inheritdoc/>
		protected override void UpdateLayoutBridgeContentSizeFitter()
		{
			if (LayoutBridge != null)
			{
				LayoutBridge.UpdateContentSizeFitter = SetContentSizeFitter && ListRenderer.AllowSetContentSizeFitter;
			}
		}

		/// <inheritdoc/>
		protected override void SetScrollRect(ScrollRect newScrollRect)
		{
			if (scrollRect != null)
			{
				var old_resize_listener = scrollRect.GetComponent<ResizeListener>();
				if (old_resize_listener != null)
				{
					old_resize_listener.OnResize.RemoveListener(SetNeedResize);
				}

				scrollRect.onValueChanged.RemoveListener(SelectableCheck);
				ListRenderer.Disable();
				scrollRect.onValueChanged.RemoveListener(SelectableSet);
				scrollRect.onValueChanged.RemoveListener(OnScrollRectUpdate);
			}

			scrollRect = newScrollRect;

			if (scrollRect != null)
			{
				var resize_listener = Utilities.GetOrAddComponent<ResizeListener>(scrollRect);
				resize_listener.OnResize.AddListener(SetNeedResize);

				scrollRect.onValueChanged.AddListener(SelectableCheck);
				ListRenderer.Enable();
				scrollRect.onValueChanged.AddListener(SelectableSet);
				scrollRect.onValueChanged.AddListener(OnScrollRectUpdate);

				UpdateScrollRectSize();
			}
		}

		/// <summary>
		/// Process locale changes.
		/// </summary>
		public virtual void LocaleChanged()
		{
			ComponentsPool.LocaleChanged();
		}

		/// <summary>
		/// Get the rendered of the specified ListView type.
		/// </summary>
		/// <param name="type">ListView type</param>
		/// <returns>Renderer.</returns>
		protected virtual ListViewTypeBase GetRenderer(ListViewType type)
		{
			ListViewTypeBase renderer;
			switch (type)
			{
				case ListViewType.ListViewWithFixedSize:
					renderer = new ListViewTypeFixed(this);
					break;
				case ListViewType.ListViewWithVariableSize:
					renderer = new ListViewTypeSize(this);
					break;
				case ListViewType.TileViewWithFixedSize:
					renderer = new TileViewTypeFixed(this);
					break;
				case ListViewType.TileViewWithVariableSize:
					renderer = new TileViewTypeSize(this);
					break;
				case ListViewType.TileViewStaggered:
					renderer = new TileViewStaggered(this);
					break;
				case ListViewType.ListViewEllipse:
					renderer = new ListViewTypeEllipse(this);
					break;
				default:
					throw new NotSupportedException("Unknown ListView type: " + type);
			}

			renderer.Enable();

			return renderer;
		}

		/// <summary>
		/// Sets the default item.
		/// </summary>
		/// <param name="newDefaultItem">New default item.</param>
		protected virtual void SetDefaultItem(TComponent newDefaultItem)
		{
			if (newDefaultItem == null)
			{
				throw new ArgumentNullException("newDefaultItem");
			}

			if (defaultItemCopy != null)
			{
				Destroy(defaultItemCopy.gameObject);
				defaultItemCopy = null;
				defaultItemCopyRect = null;
			}

			defaultItem = newDefaultItem;

			if (!isListViewCustomInited)
			{
				return;
			}

			defaultItem.gameObject.SetActive(true);
			defaultItem.FindSelectableObjects();
			CalculateItemSize(true);

			CanSetData = defaultItem is IViewData<TItem>;

			ComponentsPool.Template = defaultItem;

			CalculateMaxVisibleItems();

			UpdateView();

			if (scrollRect != null)
			{
				var resizeListener = scrollRect.GetComponent<ResizeListener>();
				if (resizeListener != null)
				{
					resizeListener.OnResize.Invoke();
				}
			}
		}

		/// <inheritdoc/>
		protected override void SetDirection(ListViewDirection newDirection)
		{
			direction = newDirection;

			ListRenderer.ResetPosition();

			(Container as RectTransform).anchoredPosition = Vector2.zero;

			if (ListRenderer.IsVirtualizationSupported())
			{
				LayoutBridge.IsHorizontal = IsHorizontal();
				ListRenderer.DirectionChanged();

				CalculateMaxVisibleItems();
			}

			UpdateView();
		}

		/// <inheritdoc/>
		public override bool IsSortEnabled()
		{
			if (DataSource.Comparison != null)
			{
				return true;
			}

#pragma warning disable 0618
			return Sort && SortFunc != null;
#pragma warning restore 0618
		}

		/// <summary>
		/// Gets the index of the nearest item.
		/// </summary>
		/// <returns>The nearest index.</returns>
		/// <param name="eventData">Event data.</param>
		/// <param name="type">Preferable nearest index.</param>
		public override int GetNearestIndex(PointerEventData eventData, NearestType type)
		{
			if (IsSortEnabled())
			{
				return -1;
			}

			Vector2 point;
			var rectTransform = Container as RectTransform;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out point))
			{
				return DataSource.Count;
			}

			var rect = rectTransform.rect;
			if (!rect.Contains(point))
			{
				return DataSource.Count;
			}

			return GetNearestIndex(point, type);
		}

		/// <summary>
		/// Gets the index of the nearest item.
		/// </summary>
		/// <returns>The nearest item index.</returns>
		/// <param name="point">Point.</param>
		/// <param name="type">Preferable nearest index.</param>
		public override int GetNearestIndex(Vector2 point, NearestType type)
		{
			var index = ListRenderer.GetNearestIndex(point, type);
			if (index == DataSource.Count)
			{
				return index;
			}

			if (ListRenderer.AllowLoopedList)
			{
				index = ListRenderer.VisibleIndex2ItemIndex(index);
			}

			return index;
		}

		/// <summary>
		/// Gets the item.
		/// </summary>
		/// <returns>The item.</returns>
		/// <param name="index">Index.</param>
		protected TItem GetDataItem(int index)
		{
			return DataSource[index];
		}

		/// <summary>
		/// Calculates the size of the item.
		/// </summary>
		/// <param name="reset">Reset item size.</param>
		protected virtual void CalculateItemSize(bool reset = false)
		{
			ItemSize = ListRenderer.GetItemSize(reset);
		}

		/// <summary>
		/// Calculates the max count of visible items.
		/// </summary>
		protected virtual void CalculateMaxVisibleItems()
		{
			if (!isListViewCustomInited)
			{
				return;
			}

			ListRenderer.CalculateMaxVisibleItems();

			ListRenderer.ValidateContentSize();
		}

		/// <summary>
		/// Resize this instance.
		/// </summary>
		public virtual void Resize()
		{
			ListRenderer.CalculateItemsSizes(DataSource, false);

			NeedResize = false;

			UpdateScrollRectSize();

			CalculateItemSize(true);
			CalculateMaxVisibleItems();
			UpdateView();
		}

		/// <inheritdoc/>
		protected override void SelectItem(int index)
		{
			var component = GetComponent(index);
			SelectColoring(component);

			if (component != null)
			{
				component.StateSelected();
			}
		}

		/// <inheritdoc/>
		protected override void DeselectItem(int index)
		{
			var component = GetComponent(index);
			DefaultColoring(component);

			if (component != null)
			{
				component.StateDefault();
			}
		}

		/// <inheritdoc/>
		protected override void InvokeSelect(int index, bool raiseEvents)
		{
			if (!IsValid(index))
			{
				Debug.LogWarning("Incorrect index: " + index, this);
			}

			SelectedItemsCache.Add(DataSource[index]);

			base.InvokeSelect(index, raiseEvents);

			if (raiseEvents)
			{
				OnSelectObject.Invoke(index);
			}
		}

		/// <inheritdoc/>
		protected override void InvokeDeselect(int index, bool raiseEvents)
		{
			if (!IsValid(index))
			{
				Debug.LogWarning("Incorrect index: " + index, this);
			}

			SelectedItemsCache.Remove(DataSource[index]);

			base.InvokeDeselect(index, raiseEvents);

			if (raiseEvents)
			{
				OnDeselectObject.Invoke(index);
			}
		}

		/// <summary>
		/// Set flag to update view when data source changed.
		/// </summary>
		public override void UpdateItems()
		{
			SetNewItems(DataSource, IsMainThread);
			IsDataSourceChanged = !IsMainThread;
		}

		/// <summary>
		/// Clear items of this instance.
		/// </summary>
		public override void Clear()
		{
			DataSource.Clear();
			ListRenderer.SetPosition(0f);
		}

		/// <summary>
		/// Add the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <returns>Index of added item.</returns>
		public virtual int Add(TItem item)
		{
			DataSource.Add(item);

			return DataSource.IndexOf(item);
		}

		/// <summary>
		/// Remove the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <returns>Index of removed TItem.</returns>
		public virtual int Remove(TItem item)
		{
			var index = DataSource.IndexOf(item);
			if (index == -1)
			{
				return index;
			}

			DataSource.RemoveAt(index);

			return index;
		}

		/// <summary>
		/// Remove item by the specified index.
		/// </summary>
		/// <param name="index">Index.</param>
		public virtual void Remove(int index)
		{
			DataSource.RemoveAt(index);
		}

		/// <summary>
		/// Scrolls to specified item immediately.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void ScrollTo(TItem item)
		{
			var index = DataSource.IndexOf(item);
			if (index > -1)
			{
				ScrollTo(index);
			}
		}

		/// <summary>
		/// Scroll to the specified item with animation.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void ScrollToAnimated(TItem item)
		{
			var index = DataSource.IndexOf(item);
			if (index > -1)
			{
				ScrollToAnimated(index);
			}
		}

		/// <inheritdoc/>
		public override void ScrollTo(int index)
		{
			if (!ListRenderer.IsVirtualizationPossible())
			{
				return;
			}

			ListRenderer.SetPosition(ListRenderer.GetPosition(index));
		}

		/// <summary>
		/// Get scroll position.
		/// </summary>
		/// <returns>Position.</returns>
		public override float GetScrollPosition()
		{
			if (!ListRenderer.IsVirtualizationPossible())
			{
				return 0f;
			}

			return ListRenderer.GetPosition();
		}

		/// <summary>
		/// Scrolls to specified position.
		/// </summary>
		/// <param name="position">Position.</param>
		public override void ScrollToPosition(float position)
		{
			if (!ListRenderer.IsVirtualizationPossible())
			{
				return;
			}

			ListRenderer.SetPosition(position);
		}

		/// <summary>
		/// Scrolls to specified position.
		/// </summary>
		/// <param name="position">Position.</param>
		public override void ScrollToPosition(Vector2 position)
		{
			if (!ListRenderer.IsVirtualizationPossible())
			{
				return;
			}

			ListRenderer.SetPosition(position);
		}

		/// <summary>
		/// Is visible item with specified index.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="minVisiblePart">The minimal visible part of the item to consider item visible.</param>
		/// <returns>true if item visible; false otherwise.</returns>
		public override bool IsVisible(int index, float minVisiblePart = 0f)
		{
			if (!ListRenderer.IsVirtualizationSupported())
			{
				return false;
			}

			return ListRenderer.IsVisible(index, minVisiblePart);
		}

		/// <summary>
		/// Starts the scroll coroutine.
		/// </summary>
		/// <param name="coroutine">Coroutine.</param>
		protected virtual void StartScrollCoroutine(IEnumerator coroutine)
		{
			StopScrollCoroutine();
			ScrollCoroutine = coroutine;
			StartCoroutine(ScrollCoroutine);
		}

		/// <summary>
		/// Stops the scroll coroutine.
		/// </summary>
		protected virtual void StopScrollCoroutine()
		{
			if (ScrollCoroutine != null)
			{
				StopCoroutine(ScrollCoroutine);
			}
		}

		/// <summary>
		/// Stop scrolling.
		/// </summary>
		public override void ScrollStop()
		{
			StopScrollCoroutine();
		}

		/// <summary>
		/// Scroll to specified index with time.
		/// </summary>
		/// <param name="index">Index.</param>
		public override void ScrollToAnimated(int index)
		{
			StartScrollCoroutine(ScrollToAnimatedCoroutine(index, ScrollUnscaledTime));
		}

		/// <summary>
		/// Scrolls to specified position with time.
		/// </summary>
		/// <param name="target">Position.</param>
		public override void ScrollToPositionAnimated(float target)
		{
#if CSHARP_7_3_OR_NEWER
			Vector2 Position()
#else
			Func<Vector2> Position = () =>
#endif
			{
				var current_position = ListRenderer.GetPositionVector();
				var target_position = IsHorizontal()
					? new Vector2(ListRenderer.ValidatePosition(-target), current_position.y)
					: new Vector2(current_position.x, ListRenderer.ValidatePosition(target));

				return target_position;
			}
#if !CSHARP_7_3_OR_NEWER
			;
#endif

			StartScrollCoroutine(ScrollToAnimatedCoroutine(Position, ScrollUnscaledTime));
		}

		/// <summary>
		/// Scrolls to specified position with time.
		/// </summary>
		/// <param name="target">Position.</param>
		public override void ScrollToPositionAnimated(Vector2 target)
		{
#if CSHARP_7_3_OR_NEWER
			Vector2 Position()
#else
			Func<Vector2> Position = () =>
#endif
			{
				return ListRenderer.ValidatePosition(target);
			}
#if !CSHARP_7_3_OR_NEWER
			;
#endif

			StartScrollCoroutine(ScrollToAnimatedCoroutine(Position, ScrollUnscaledTime));
		}

		/// <summary>
		/// Scroll to specified index with time coroutine.
		/// </summary>
		/// <returns>The scroll to index with time coroutine.</returns>
		/// <param name="index">Index.</param>
		/// <param name="unscaledTime">Use unscaled time.</param>
		protected virtual IEnumerator ScrollToAnimatedCoroutine(int index, bool unscaledTime)
		{
#if CSHARP_7_3_OR_NEWER
			Vector2 Position()
#else
			Func<Vector2> Position = () =>
#endif
			{
				return ListRenderer.GetPosition(index);
			}
#if !CSHARP_7_3_OR_NEWER
			;
#endif

			return ScrollToAnimatedCoroutine(Position, unscaledTime);
		}

		/// <summary>
		/// Get start position for the animated scroll.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <returns>Start position.</returns>
		protected virtual Vector2 GetScrollStartPosition(Vector2 target)
		{
			var start = ListRenderer.GetPositionVector();
			if (IsHorizontal())
			{
				start.x = -start.x;
			}

			if (ListRenderer.AllowLoopedList)
			{
				// find shortest distance to target for the looped list
				var list_size = ListRenderer.ListSize() + LayoutBridge.GetSpacing();
				var distance_straight = IsHorizontal()
					? (target.x - start.x)
					: (target.y - start.y);
				var distance_reverse_1 = IsHorizontal()
					? (target.x - (start.x + list_size))
					: (target.y - start.y + list_size);
				var distance_reverse_2 = IsHorizontal()
					? (target.x - (start.x - list_size))
					: (target.y - start.y - list_size);

				if (Mathf.Abs(distance_reverse_1) < Mathf.Abs(distance_straight))
				{
					if (IsHorizontal())
					{
						start.x += list_size;
					}
					else
					{
						start.y += list_size;
					}
				}

				if (Mathf.Abs(distance_reverse_2) < Mathf.Abs(distance_straight))
				{
					if (IsHorizontal())
					{
						start.x -= list_size;
					}
					else
					{
						start.y -= list_size;
					}
				}
			}

			return start;
		}

		/// <summary>
		/// Scroll to specified position with time coroutine.
		/// </summary>
		/// <returns>The scroll to index with time coroutine.</returns>
		/// <param name="targetPosition">Target position.</param>
		/// <param name="unscaledTime">Use unscaled time.</param>
		protected virtual IEnumerator ScrollToAnimatedCoroutine(Func<Vector2> targetPosition, bool unscaledTime)
		{
			var start = GetScrollStartPosition(targetPosition());

			float delta;
			var animationLength = ScrollMovement.keys[ScrollMovement.keys.Length - 1].time;
			var startTime = UtilitiesTime.GetTime(unscaledTime);

			do
			{
				delta = UtilitiesTime.GetTime(unscaledTime) - startTime;
				var value = ScrollMovement.Evaluate(delta);

				var target = targetPosition();
				var pos = start + ((target - start) * value);

				ListRenderer.SetPosition(pos);

				yield return null;
			}
			while (delta < animationLength);

			ListRenderer.SetPosition(targetPosition());

			yield return null;

			ListRenderer.SetPosition(targetPosition());
		}

		/// <summary>
		/// Gets the item position by index.
		/// </summary>
		/// <returns>The item position.</returns>
		/// <param name="index">Index.</param>
		public override float GetItemPosition(int index)
		{
			return ListRenderer.GetItemPosition(index);
		}

		/// <summary>
		/// Gets the item position by index.
		/// </summary>
		/// <returns>The item position.</returns>
		/// <param name="index">Index.</param>
		public override float GetItemPositionBorderEnd(int index)
		{
			return ListRenderer.GetItemPositionBorderEnd(index);
		}

		/// <summary>
		/// Gets the item middle position by index.
		/// </summary>
		/// <returns>The item middle position.</returns>
		/// <param name="index">Index.</param>
		public override float GetItemPositionMiddle(int index)
		{
			return ListRenderer.GetItemPositionMiddle(index);
		}

		/// <summary>
		/// Gets the item bottom position by index.
		/// </summary>
		/// <returns>The item bottom position.</returns>
		/// <param name="index">Index.</param>
		public override float GetItemPositionBottom(int index)
		{
			return ListRenderer.GetItemPositionBottom(index);
		}

		/// <summary>
		/// Adds the callback.
		/// </summary>
		/// <param name="item">Item.</param>
		protected virtual void AddCallback(ListViewItem item)
		{
			ListRenderer.AddCallback(item);
		}

		/// <summary>
		/// Removes the callback.
		/// </summary>
		/// <param name="item">Item.</param>
		protected virtual void RemoveCallback(ListViewItem item)
		{
			if (item == null)
			{
				return;
			}

			ListRenderer.RemoveCallback(item);
		}

		/// <summary>
		/// Set the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <param name="allowDuplicate">If set to <c>true</c> allow duplicate.</param>
		/// <returns>Index of item.</returns>
		public int Set(TItem item, bool allowDuplicate = true)
		{
			int index;

			if (!allowDuplicate)
			{
				index = DataSource.IndexOf(item);
				if (index == -1)
				{
					index = Add(item);
				}
			}
			else
			{
				index = Add(item);
			}

			Select(index);

			return index;
		}

		/// <summary>
		/// Sets component data with specified item.
		/// </summary>
		/// <param name="component">Component.</param>
		/// <param name="item">Item.</param>
		protected virtual void SetData(TComponent component, TItem item)
		{
			if (CanSetData)
			{
				(component as IViewData<TItem>).SetData(item);
			}
		}

		/// <summary>
		/// Gets the default width of the item.
		/// </summary>
		/// <returns>The default item width.</returns>
		public override float GetDefaultItemWidth()
		{
			return ItemSize.x;
		}

		/// <summary>
		/// Gets the default height of the item.
		/// </summary>
		/// <returns>The default item height.</returns>
		public override float GetDefaultItemHeight()
		{
			return ItemSize.y;
		}

		/// <summary>
		/// Sets the displayed indices.
		/// </summary>
		/// <param name="isNewData">Is new data?</param>
		protected virtual void SetDisplayedIndices(bool isNewData = true)
		{
			if (isNewData)
			{
				ComponentsPool.DisplayedIndicesSet(DisplayedIndices, ComponentSetData);
			}
			else
			{
				ComponentsPool.DisplayedIndicesUpdate(DisplayedIndices, ComponentSetData);
			}

			ListRenderer.UpdateLayout();
			ComponentsHighlightedColoring();
		}

		/// <summary>
		/// Process the ScrollRect update event.
		/// </summary>
		/// <param name="position">Position.</param>
		protected virtual void OnScrollRectUpdate(Vector2 position)
		{
			StartScrolling();
		}

		/// <summary>
		/// Set data to component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected virtual void ComponentSetData(TComponent component)
		{
			SetData(component, DataSource[component.Index]);
			Coloring(component as ListViewItem);

			if (IsSelected(component.Index))
			{
				component.StateSelected();
			}
			else
			{
				component.StateDefault();
			}
		}

		/// <inheritdoc/>
		public override void UpdateView()
		{
			if (!isListViewCustomInited)
			{
				return;
			}

			ListRenderer.UpdateDisplayedIndices();

			SetDisplayedIndices();

			OnUpdateView.Invoke();
		}

		/// <summary>
		/// Keep selected items on items update.
		/// </summary>
		[SerializeField]
		protected bool KeepSelection = true;

		/// <summary>
		/// Updates the items.
		/// </summary>
		/// <param name="newItems">New items.</param>
		/// <param name="updateView">Update view.</param>
		protected virtual void SetNewItems(ObservableList<TItem> newItems, bool updateView = true)
		{
			ListRenderer.CalculateItemsSizes(newItems, false);

			lock (DataSource)
			{
				DataSource.OnChange -= UpdateItems;

#pragma warning disable 0618
				if (Sort && SortFunc != null)
				{
					newItems.BeginUpdate();

					var sorted = new List<TItem>(SortFunc(newItems));

					newItems.Clear();
					newItems.AddRange(sorted);

					newItems.EndUpdate();
				}
#pragma warning restore 0618

				SilentDeselect(SelectedIndices);
				var new_selected_indices = RecalculateSelectedIndices(newItems);

				dataSource = newItems;

				CalculateMaxVisibleItems();

				if (KeepSelection)
				{
					SilentSelect(new_selected_indices);
				}

				SelectedItemsCache.Clear();
				for (int i = 0; i < SelectedIndices.Count; i++)
				{
					var index = SelectedIndices[i];
					SelectedItemsCache.Add(DataSource[index]);
				}

				DataSource.OnChange += UpdateItems;

				if (updateView)
				{
					UpdateView();
				}
			}
		}

		/// <summary>
		/// Recalculates the selected indices.
		/// </summary>
		/// <returns>The selected indices.</returns>
		/// <param name="newItems">New items.</param>
		protected virtual List<int> RecalculateSelectedIndices(ObservableList<TItem> newItems)
		{
			var new_selected_indices = new List<int>();

			foreach (var item in SelectedItemsCache)
			{
				var new_index = newItems.IndexOf(item);
				if (new_index != -1)
				{
					new_selected_indices.Add(new_index);
				}
			}

			return new_selected_indices;
		}

		/// <summary>
		/// Determines if item exists with the specified index.
		/// </summary>
		/// <returns><c>true</c> if item exists with the specified index; otherwise, <c>false</c>.</returns>
		/// <param name="index">Index.</param>
		public override bool IsValid(int index)
		{
			return (index >= 0) && (index < DataSource.Count);
		}

		/// <summary>
		/// Process the item move event.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="item">Item.</param>
		/// <param name="eventData">Event data.</param>
		protected override void OnItemMove(int index, ListViewItem item, AxisEventData eventData)
		{
			if (ListRenderer.OnItemMove(eventData, item))
			{
				return;
			}

			switch (eventData.moveDir)
			{
				case MoveDirection.Left:
					Navigate(eventData, FindSelectableOnLeft());
					return;
				case MoveDirection.Right:
					Navigate(eventData, FindSelectableOnRight());
					return;
				case MoveDirection.Up:
					Navigate(eventData, FindSelectableOnUp());
					return;
				case MoveDirection.Down:
					Navigate(eventData, FindSelectableOnDown());
					return;
				default:
					return;
			}
		}

		/// <summary>
		/// Coloring the specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected override void Coloring(ListViewItem component)
		{
			if (component == null)
			{
				return;
			}

			if (IsSelected(component.Index))
			{
				SelectColoring(component);
			}
			else
			{
				DefaultColoring(component);
			}
		}

		/// <summary>
		/// Set highlights colors of specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected override void HighlightColoring(ListViewItem component)
		{
			if (component == null)
			{
				return;
			}

			if (IsSelected(component.Index))
			{
				return;
			}

			HighlightColoring(component as TComponent);
		}

		/// <summary>
		/// Set highlights colors of specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected virtual void HighlightColoring(TComponent component)
		{
			if (component == null)
			{
				return;
			}

			if (!allowColoring)
			{
				return;
			}

			if (!CanSelect(component.Index))
			{
				return;
			}

			if (IsSelected(component.Index))
			{
				return;
			}

			component.GraphicsColoring(HighlightedColor, HighlightedBackgroundColor, FadeDuration);
		}

		/// <summary>
		/// Set select colors of specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected virtual void SelectColoring(ListViewItem component)
		{
			if (component == null)
			{
				return;
			}

			SelectColoring(component as TComponent);
		}

		/// <summary>
		/// Set select colors of specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected virtual void SelectColoring(TComponent component)
		{
			if (component == null)
			{
				return;
			}

			if (!allowColoring)
			{
				return;
			}

			if (IsInteractable())
			{
				component.GraphicsColoring(SelectedColor, SelectedBackgroundColor, FadeDuration);
			}
			else
			{
				component.GraphicsColoring(SelectedColor * DisabledColor, SelectedBackgroundColor * DisabledColor, FadeDuration);
			}
		}

		/// <summary>
		/// Set default colors of specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected override void DefaultColoring(ListViewItem component)
		{
			if (component == null)
			{
				return;
			}

			DefaultColoring(component as TComponent);
		}

		/// <summary>
		/// Set default colors of specified component.
		/// </summary>
		/// <param name="component">Component.</param>
		protected virtual void DefaultColoring(TComponent component)
		{
			if (component == null)
			{
				return;
			}

			if (!allowColoring)
			{
				return;
			}

			if (IsInteractable())
			{
				component.GraphicsColoring(DefaultColor, DefaultBackgroundColor, FadeDuration);
			}
			else
			{
				component.GraphicsColoring(DefaultColor * DisabledColor, DefaultBackgroundColor * DisabledColor, FadeDuration);
			}
		}

		/// <summary>
		/// Updates the colors.
		/// </summary>
		/// <param name="instant">Is should be instant color update?</param>
		public override void ComponentsColoring(bool instant = false)
		{
			if (!isListViewCustomInited)
			{
				return;
			}

			if (!allowColoring && instant)
			{
				ComponentsPool.ForEach(DefaultColoring);
				return;
			}

			if (instant)
			{
				var old_duration = FadeDuration;
				FadeDuration = 0f;
				ComponentsPool.ForEach(Coloring);
				FadeDuration = old_duration;
			}
			else
			{
				ComponentsPool.ForEach(Coloring);
			}

			ComponentsHighlightedColoring();
		}

		/// <summary>
		/// This function is called when the MonoBehaviour will be destroyed.
		/// </summary>
		protected override void OnDestroy()
		{
			Localization.OnLocaleChanged -= LocaleChanged;

			if (dataSource != null)
			{
				dataSource.OnChange -= UpdateItems;
			}

			if (layout != null)
			{
				layout.SettingsChanged.RemoveListener(SetNeedResize);
				layout = null;
			}

			layoutBridge = null;

			ScrollRect = null;

			if (componentsPool != null)
			{
				componentsPool.Template = null;
				componentsPool = null;
			}

			if (defaultItem != null)
			{
				defaultItem.gameObject.SetActive(true);
			}

			base.OnDestroy();
		}

		/// <summary>
		/// Calls the specified action for each component.
		/// </summary>
		/// <param name="func">Action.</param>
		public override void ForEachComponent(Action<ListViewItem> func)
		{
			base.ForEachComponent(func);

			func(DefaultItem);

			if (defaultItemCopy != null)
			{
				func(DefaultItemCopy);
			}

			if (componentsPool != null)
			{
				componentsPool.ForEachCache(func);
			}
		}

		/// <summary>
		/// Calls the specified action for each component.
		/// </summary>
		/// <param name="func">Action.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods", Justification = "Reviewed.")]
		public virtual void ForEachComponent(Action<TComponent> func)
		{
			base.ForEachComponent<TComponent>(func);
			func(DefaultItem);
			ComponentsPool.ForEachCache(func);
		}

		/// <summary>
		/// Determines whether item visible.
		/// </summary>
		/// <returns><c>true</c> if item visible; otherwise, <c>false</c>.</returns>
		/// <param name="index">Index.</param>
		public override bool IsItemVisible(int index)
		{
			return DisplayedIndices.Contains(index);
		}

		/// <summary>
		/// Gets the visible indices.
		/// </summary>
		/// <returns>The visible indices.</returns>
		public List<int> GetVisibleIndices()
		{
			return new List<int>(DisplayedIndices);
		}

		/// <summary>
		/// Gets the visible components.
		/// </summary>
		/// <returns>The visible components.</returns>
		public List<TComponent> GetVisibleComponents()
		{
			return ComponentsPool.List();
		}

		/// <summary>
		/// Gets the item component.
		/// </summary>
		/// <returns>The item component.</returns>
		/// <param name="index">Index.</param>
		public TComponent GetItemComponent(int index)
		{
			return GetComponent(index) as TComponent;
		}

		/// <summary>
		/// OnStartScrolling event.
		/// </summary>
		public UnityEvent OnStartScrolling = new UnityEvent();

		/// <summary>
		/// OnEndScrolling event.
		/// </summary>
		public UnityEvent OnEndScrolling = new UnityEvent();

		/// <summary>
		/// Time before raise OnEndScrolling event since last OnScrollRectUpdate event raised.
		/// </summary>
		public float EndScrollDelay = 0.3f;

		/// <summary>
		/// Is ScrollRect now on scrolling state.
		/// </summary>
		protected bool Scrolling;

		/// <summary>
		/// When last scroll event happen?
		/// </summary>
		protected float LastScrollingTime;

		/// <summary>
		/// Update this instance.
		/// </summary>
		protected virtual void Update()
		{
			if (DataSourceSetted || IsDataSourceChanged)
			{
				var reset_scroll = DataSourceSetted;

				DataSourceSetted = false;
				IsDataSourceChanged = false;

				lock (DataSource)
				{
					CalculateMaxVisibleItems();
					UpdateView();
				}

				if (reset_scroll)
				{
					ListRenderer.SetPosition(0f);
				}
			}

			if (NeedResize)
			{
				Resize();
			}

			if (IsStopScrolling())
			{
				EndScrolling();
			}

			SelectableSet();
		}

		/// <summary>
		/// LateUpdate.
		/// </summary>
		protected virtual void LateUpdate()
		{
			SelectableSet();
		}

		/// <summary>
		/// This function is called when the object becomes enabled and active.
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();

			StartCoroutine(ForceRebuild());

			var old = FadeDuration;
			FadeDuration = 0f;
			ComponentsColoring(true);
			FadeDuration = old;

			Resize();
		}

		IEnumerator ForceRebuild()
		{
			yield return null;
			ForEachComponent(MarkLayoutForRebuild);
		}

		void MarkLayoutForRebuild(ListViewItem item)
		{
			if (item != null)
			{
				LayoutRebuilder.MarkLayoutForRebuild(item.transform as RectTransform);
			}
		}

		/// <summary>
		/// Start to track scrolling event.
		/// </summary>
		protected virtual void StartScrolling()
		{
			LastScrollingTime = UtilitiesTime.GetTime(true);
			if (Scrolling)
			{
				return;
			}

			Scrolling = true;
			OnStartScrolling.Invoke();
		}

		/// <summary>
		/// Determines whether ScrollRect is stop scrolling.
		/// </summary>
		/// <returns><c>true</c> if ScrollRect is stop scrolling; otherwise, <c>false</c>.</returns>
		protected virtual bool IsStopScrolling()
		{
			if (!Scrolling)
			{
				return false;
			}

			return (LastScrollingTime + EndScrollDelay) <= UtilitiesTime.GetTime(true);
		}

		/// <summary>
		/// Raise OnEndScrolling event.
		/// </summary>
		protected virtual void EndScrolling()
		{
			Scrolling = false;
			OnEndScrolling.Invoke();
		}

		/// <summary>
		/// Is need to handle resize event?
		/// </summary>
		protected bool NeedResize;

		/// <summary>
		/// Sets the need resize.
		/// </summary>
		protected virtual void SetNeedResize()
		{
			if (!ListRenderer.IsVirtualizationSupported())
			{
				return;
			}

			NeedResize = true;
		}

		/// <summary>
		/// Change DefaultItem size.
		/// </summary>
		/// <param name="size">Size.</param>
		public virtual void ChangeDefaultItemSize(Vector2 size)
		{
			if (defaultItemCopy != null)
			{
				var rt = defaultItemCopy.transform as RectTransform;
				rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
				rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
			}

			ComponentsPool.SetSize(size);

			CalculateItemSize(true);
			CalculateMaxVisibleItems();
			UpdateView();
		}

		/// <summary>
		/// Select first item.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <returns>true if item was found and selected; otherwise false.</returns>
		public bool SelectFirstItem(TItem item)
		{
			var index = DataSource.IndexOf(item);
			if (IsValid(index))
			{
				Select(index);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Select all items.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <returns>true if item was found and selected; otherwise false.</returns>
		public bool SelectAllItems(TItem item)
		{
			var selected = false;
			var start = 0;
			bool is_valid;
			do
			{
				var index = DataSource.IndexOf(item, start);
				is_valid = IsValid(index);
				if (is_valid)
				{
					start = index + 1;
					Select(index);
					selected = true;
				}
			}
			while (is_valid);

			return selected;
		}

		#region ListViewPaginator support

		/// <summary>
		/// Gets the ScrollRect.
		/// </summary>
		/// <returns>The ScrollRect.</returns>
		public override ScrollRect GetScrollRect()
		{
			return ScrollRect;
		}

		/// <summary>
		/// Gets the items count.
		/// </summary>
		/// <returns>The items count.</returns>
		public override int GetItemsCount()
		{
			return DataSource.Count;
		}

		/// <summary>
		/// Gets the items per block count.
		/// </summary>
		/// <returns>The items per block.</returns>
		public override int GetItemsPerBlock()
		{
			return ListRenderer.GetItemsPerBlock();
		}

		/// <summary>
		/// Gets the index of the nearest item.
		/// </summary>
		/// <returns>The nearest item index.</returns>
		public override int GetNearestItemIndex()
		{
			return ListRenderer.GetNearestItemIndex();
		}

		/// <summary>
		/// Gets the size of the DefaultItem.
		/// </summary>
		/// <returns>Size.</returns>
		public override Vector2 GetDefaultItemSize()
		{
			return ItemSize;
		}
		#endregion

		#region Obsolete

		/// <summary>
		/// Gets the visible indices.
		/// </summary>
		/// <returns>The visible indices.</returns>
		[Obsolete("Use GetVisibleIndices()")]
		public List<int> GetVisibleIndicies()
		{
			return GetVisibleIndices();
		}
#endregion

#region IStylable implementation

		/// <summary>
		/// Set the specified style.
		/// </summary>
		/// <param name="style">Style data.</param>
		protected virtual void SetStyleDefaultItem(Style style)
		{
			if (defaultItemCopy != null)
			{
				defaultItemCopy.Owner = this;
				defaultItemCopy.SetStyle(style.Collections.DefaultItemBackground, style.Collections.DefaultItemText, style);
			}

			if (componentsPool != null)
			{
				componentsPool.SetStyle(style.Collections.DefaultItemBackground, style.Collections.DefaultItemText, style);
			}
			else if (defaultItem != null)
			{
				defaultItem.Owner = this;
				defaultItem.SetStyle(style.Collections.DefaultItemBackground, style.Collections.DefaultItemText, style);
			}
		}

		/// <summary>
		/// Sets the style colors.
		/// </summary>
		/// <param name="style">Style.</param>
		protected virtual void SetStyleColors(Style style)
		{
			defaultBackgroundColor = style.Collections.DefaultBackgroundColor;
			defaultColor = style.Collections.DefaultColor;
			highlightedBackgroundColor = style.Collections.HighlightedBackgroundColor;
			highlightedColor = style.Collections.HighlightedColor;
			selectedBackgroundColor = style.Collections.SelectedBackgroundColor;
			selectedColor = style.Collections.SelectedColor;
		}

		/// <summary>
		/// Sets the ScrollRect style.
		/// </summary>
		/// <param name="style">Style.</param>
		protected virtual void SetStyleScrollRect(Style style)
		{
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			var viewport = scrollRect.viewport != null ? scrollRect.viewport : Container.parent;
#else
			var viewport = Container.parent;
#endif
			style.Collections.Viewport.ApplyTo(viewport.GetComponent<Image>());

			style.HorizontalScrollbar.ApplyTo(scrollRect.horizontalScrollbar);
			style.VerticalScrollbar.ApplyTo(scrollRect.verticalScrollbar);
		}

		/// <inheritdoc/>
		public override bool SetStyle(Style style)
		{
			SetStyleDefaultItem(style);

			SetStyleColors(style);

			SetStyleScrollRect(style);

			style.Collections.MainBackground.ApplyTo(GetComponent<Image>());

			if (StyleTable)
			{
				var image = Utilities.GetOrAddComponent<Image>(Container);
				image.sprite = null;
				image.color = DefaultColor;

				var mask_image = Utilities.GetOrAddComponent<Image>(Container.parent);
				mask_image.sprite = null;

				var mask = Utilities.GetOrAddComponent<Mask>(Container.parent);
				mask.showMaskGraphic = false;

				defaultBackgroundColor = style.Table.Background.Color;
			}

			if (componentsPool != null)
			{
				ComponentsColoring(true);
			}
			else if (defaultItem != null)
			{
				defaultItem.SetColors(DefaultColor, DefaultBackgroundColor);
			}

			if (header != null)
			{
				header.SetStyle(style);
			}
			else
			{
				style.ApplyTo(transform.Find("Header"));
			}

			return true;
		}

		/// <summary>
		/// Set style options from the DefaultItem.
		/// </summary>
		/// <param name="style">Style data.</param>
		protected virtual void GetStyleDefaultItem(Style style)
		{
			if (defaultItem != null)
			{
				defaultItem.Owner = this;
				defaultItem.GetStyle(style.Collections.DefaultItemBackground, style.Collections.DefaultItemText, style);
			}
		}

		/// <summary>
		/// Get style colors.
		/// </summary>
		/// <param name="style">Style.</param>
		protected virtual void GetStyleColors(Style style)
		{
			style.Collections.DefaultBackgroundColor = defaultBackgroundColor;
			style.Collections.DefaultColor = defaultColor;
			style.Collections.HighlightedBackgroundColor = highlightedBackgroundColor;
			style.Collections.HighlightedColor = highlightedColor;
			style.Collections.SelectedBackgroundColor = selectedBackgroundColor;
			style.Collections.SelectedColor = selectedColor;
		}

		/// <summary>
		/// Get style options from the ScrollRect.
		/// </summary>
		/// <param name="style">Style.</param>
		protected virtual void GetStyleScrollRect(Style style)
		{
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			var viewport = scrollRect.viewport != null ? scrollRect.viewport : Container.parent;
#else
			var viewport = Container.parent;
#endif
			style.Collections.Viewport.GetFrom(viewport.GetComponent<Image>());

			style.HorizontalScrollbar.GetFrom(scrollRect.horizontalScrollbar);
			style.VerticalScrollbar.GetFrom(scrollRect.verticalScrollbar);
		}

		/// <inheritdoc/>
		public override bool GetStyle(Style style)
		{
			GetStyleDefaultItem(style);

			GetStyleColors(style);

			GetStyleScrollRect(style);

			style.Collections.MainBackground.GetFrom(GetComponent<Image>());

			if (StyleTable)
			{
				style.Table.Background.Color = defaultBackgroundColor;
			}

			if (header != null)
			{
				header.GetStyle(style);
			}
			else
			{
				style.GetFrom(transform.Find("Header"));
			}

			return true;
		}
		#endregion

		#region Selectable

		/// <summary>
		/// Selectable data.
		/// </summary>
		protected struct SelectableData : IEquatable<SelectableData>
		{
			/// <summary>
			/// Is need to update EventSystem.currentSelectedGameObject?
			/// </summary>
			public bool Update;

			/// <summary>
			/// Index of the item with selectable GameObject.
			/// </summary>
			public int Item
			{
				get;
				private set;
			}

			/// <summary>
			/// Index of the selectable GameObject of the item.
			/// </summary>
			public int SelectableGameObject
			{
				get;
				private set;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="SelectableData"/> struct.
			/// </summary>
			/// <param name="item">Index of the item with selectable GameObject.</param>
			/// <param name="selectableGameObject">Index of the selectable GameObject of the item.</param>
			public SelectableData(int item, int selectableGameObject)
			{
				Update = true;
				Item = item;
				SelectableGameObject = selectableGameObject;
			}

			/// <summary>
			/// Hash function.
			/// </summary>
			/// <returns>A hash code for the current object.</returns>
			public override int GetHashCode()
			{
				return Item ^ SelectableGameObject;
			}

			/// <summary>
			/// Determines whether the specified object is equal to the current object.
			/// </summary>
			/// <param name="obj">The object to compare with the current object.</param>
			/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
			public override bool Equals(object obj)
			{
				if (!(obj is SelectableData))
				{
					return false;
				}

				return Equals((SelectableData)obj);
			}

			/// <summary>
			/// Determines whether the specified object is equal to the current object.
			/// </summary>
			/// <param name="other">The object to compare with the current object.</param>
			/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
			public bool Equals(SelectableData other)
			{
				if (Update != other.Update)
				{
					return false;
				}

				if (Item != other.Item)
				{
					return false;
				}

				return SelectableGameObject == other.SelectableGameObject;
			}

			/// <summary>
			/// Compare specified objects.
			/// </summary>
			/// <param name="data1">First object.</param>
			/// <param name="data2">Second object.</param>
			/// <returns>true if the objects are equal; otherwise, false.</returns>
			public static bool operator ==(SelectableData data1, SelectableData data2)
			{
				return data1.Equals(data2);
			}

			/// <summary>
			/// Compare specified objects.
			/// </summary>
			/// <param name="data1">First object.</param>
			/// <param name="data2">Second object.</param>
			/// <returns>true if the objects not equal; otherwise, false.</returns>
			public static bool operator !=(SelectableData data1, SelectableData data2)
			{
				return !data1.Equals(data2);
			}
		}

		/// <summary>
		/// Selectable data.
		/// </summary>
		protected SelectableData NewSelectableData;

		/// <summary>
		/// Get current selected GameObject.
		/// </summary>
		/// <returns>Selected GameObject.</returns>
		protected GameObject GetCurrentSelectedGameObject()
		{
			var es = EventSystem.current;
			if (es == null)
			{
				return null;
			}

			var go = es.currentSelectedGameObject;
			if (go == null)
			{
				return null;
			}

			if (!go.transform.IsChildOf(Container))
			{
				return null;
			}

			return go;
		}

		/// <summary>
		/// Get item component with selected GameObject.
		/// </summary>
		/// <param name="go">Selected GameObject.</param>
		/// <returns>Item component.</returns>
		protected TComponent SelectedGameObject2Component(GameObject go)
		{
			if (go == null)
			{
				return null;
			}

			var t = go.transform;
			for (int i = 0; i < Items.Count; i++)
			{
				var item_transform = Items[i].transform;
				if (t.IsChildOf(item_transform) && (t.GetInstanceID() != item_transform.GetInstanceID()))
				{
					return Items[i] as TComponent;
				}
			}

			return null;
		}

		/// <summary>
		/// Find index of the next item.
		/// </summary>
		/// <param name="index">Index of the current item with selected GameObject.</param>
		/// <returns>Index of the next item</returns>
		protected virtual int SelectableFindNextObjectIndex(int index)
		{
			for (int i = 0; i < DataSource.Count; i++)
			{
				var prev_index = index - i;
				var next_index = index + i;
				var prev_valid = IsValid(prev_index);
				var next_valid = IsValid(next_index);
				if (!prev_valid && !next_valid)
				{
					return -1;
				}

				if (IsVisible(next_index))
				{
					return next_index;
				}

				if (IsVisible(prev_index))
				{
					return prev_index;
				}
			}

			return -1;
		}

		/// <inheritdoc/>
		protected override void SelectableCheck()
		{
			var go = GetCurrentSelectedGameObject();
			if (go == null)
			{
				return;
			}

			var component = SelectedGameObject2Component(go);
			if (component == null)
			{
				return;
			}

			if (IsVisible(component.Index))
			{
				return;
			}

			var item_index = SelectableFindNextObjectIndex(component.Index);
			if (!IsValid(item_index))
			{
				return;
			}

			NewSelectableData = new SelectableData(item_index, component.GetSelectableIndex(go));
		}

		/// <inheritdoc/>
		protected override void SelectableSet()
		{
			if (!NewSelectableData.Update)
			{
				return;
			}

			var es = EventSystem.current;
			if ((es == null) || es.alreadySelecting)
			{
				return;
			}

			var component = GetItemComponent(NewSelectableData.Item);
			if (component == null)
			{
				return;
			}

			var go = component.GetSelectableObject(NewSelectableData.SelectableGameObject);
			NewSelectableData.Update = false;

			if (go != null)
			{
				es.SetSelectedGameObject(go);
			}
		}
		#endregion

		#region AutoScroll

		/// <summary>
		/// Auto scroll.
		/// </summary>
		/// <returns>Coroutine.</returns>
		protected override IEnumerator AutoScroll()
		{
			while (true)
			{
				var delta = AutoScrollSpeed * UtilitiesTime.GetDeltaTime(ScrollUnscaledTime) * AutoScrollDirection;
				var max = GetItemPositionBottom(DataSource.Count - 1);
				var pos = Mathf.Clamp(GetScrollPosition() + delta, 0f, max);

				ScrollToPosition(pos);

				yield return null;

				if (AutoScrollCallback != null)
				{
					AutoScrollCallback(AutoScrollEventData);
				}
			}
		}
		#endregion
	}
}