﻿using Myra.Attributes;
using Myra.Graphics2D.UI.Styles;
using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using static Myra.Graphics2D.UI.Grid;

namespace Myra.Graphics2D.UI
{
	public class TabControl : Selector<Grid, TabItem>
	{
		private Grid _gridButtons;
		private SingleItemContainer<Widget> _panelContent;

		[HiddenInEditor]
		[XmlIgnore]
		public TabControlStyle TabControlStyle
		{
			get; set;
		}

		[DefaultValue(HorizontalAlignment.Left)]
		public override HorizontalAlignment HorizontalAlignment
		{
			get
			{
				return base.HorizontalAlignment;
			}
			set
			{
				base.HorizontalAlignment = value;
			}
		}

		[DefaultValue(VerticalAlignment.Top)]
		public override VerticalAlignment VerticalAlignment
		{
			get
			{
				return base.VerticalAlignment;
			}
			set
			{
				base.VerticalAlignment = value;
			}
		}

		public TabControl(TabControlStyle style): base(new Grid())
		{
			// First row contains button
			InternalChild.RowsProportions.Add(new Proportion());

			_gridButtons = new Grid();
			InternalChild.Widgets.Add(_gridButtons);

			// Second row contains content
			InternalChild.RowsProportions.Add(new Proportion(ProportionType.Fill));

			_panelContent = new SingleItemContainer<Widget>()
			{
				GridRow = 1,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch
			};
			InternalChild.Widgets.Add(_panelContent);

			HorizontalAlignment = HorizontalAlignment.Left;
			VerticalAlignment = VerticalAlignment.Top;

			if (style != null)
			{
				ApplyTabControlStyle(style);
			}
		}

		public TabControl(string style)
			: this(Stylesheet.Current.TabControlStyles[style])
		{
		}

		public TabControl(): this(Stylesheet.Current.TabControlStyle)
		{
		}

		private void ItemOnChanged(object sender, EventArgs eventArgs)
		{
			var item = (TabItem)sender;

			var button = item.Button;
			button.Text = item.Text;
			button.TextColor = item.Color ?? TabControlStyle.TabItemStyle.LabelStyle.TextColor;

			InvalidateMeasure();
		}

		private void UpdateGridPositions()
		{
			for (var i = 0; i < Items.Count; ++i)
			{
				var widget = _gridButtons.Widgets[i];
				widget.GridColumn = i;
			}
		}

		protected override void InsertItem(TabItem item, int index)
		{
			item.Changed += ItemOnChanged;

			Button button = new ListButton(TabControlStyle.TabItemStyle, this)
			{
				Text = item.Text,
				TextColor = item.Color ?? TabControlStyle.TabItemStyle.LabelStyle.TextColor,
				Tag = item,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				Image = item.Image,
				ImageTextSpacing = item.ImageTextSpacing
			};

			button.Click += ButtonOnClick;

			_gridButtons.ColumnsProportions.Insert(index, new Proportion(ProportionType.Auto));
			_gridButtons.Widgets.Insert(index, button);

			item.Button = button;

			UpdateGridPositions();

			if (index == SelectedIndex)
			{
				SelectedItem = item;
			}
		}

		protected override void RemoveItem(TabItem item)
		{
			item.Changed -= ItemOnChanged;

			var index = _gridButtons.Widgets.IndexOf(item.Button);
			_gridButtons.ColumnsProportions.RemoveAt(index);
			_gridButtons.Widgets.RemoveAt(index);

			if (SelectedItem == item)
			{
				SelectedItem = null;
			}

			UpdateGridPositions();
		}

		protected override void OnSelectedItemChanged()
		{
			base.OnSelectedItemChanged();

			if (SelectedItem != null)
			{
				_panelContent.InternalChild = SelectedItem.Content;
			}
		}

		protected override void Reset()
		{
			while (_gridButtons.Widgets.Count > 0)
			{
				RemoveItem((TabItem)_gridButtons.Widgets[0].Tag);
			}
		}

		private void ButtonOnClick(object sender, EventArgs eventArgs)
		{
			var item = (Button)sender;
			var index = _gridButtons.Widgets.IndexOf(item);
			SelectedIndex = index;
		}

		public void ApplyTabControlStyle(TabControlStyle style)
		{
			ApplyWidgetStyle(style);

			TabControlStyle = style;

			InternalChild.RowSpacing = style.HeaderSpacing;
			_gridButtons.ColumnSpacing = style.ButtonSpacing;

			_panelContent.ApplyWidgetStyle(style.ContentStyle);

			foreach (var item in Items)
			{
				item.Button.ApplyButtonStyle(style.TabItemStyle);
			}
		}

		protected override void SetStyleByName(Stylesheet stylesheet, string name)
		{
			ApplyTabControlStyle(stylesheet.TabControlStyles[name]);
		}

		internal override string[] GetStyleNames(Stylesheet stylesheet)
		{
			return stylesheet.TabControlStyles.Keys.ToArray();
		}
	}
}