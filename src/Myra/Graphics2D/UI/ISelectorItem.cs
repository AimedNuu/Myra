﻿namespace Myra.Graphics2D.UI
{
	public interface ISelectorItem: IItemWithId
	{
		bool IsSelected
		{
			get; set;
		}
	}
}
