// Copyright Gamelogic (c) http://www.gamelogic.co.za

using UnityEngine;
using UnityEditor;

namespace Gamelogic.Extensions.Editor.Internal
{
	/// <summary>
	/// Class with static functions for menu options.
	/// </summary>
	// ReSharper disable once PartialTypeWithSinglePart (Other parts are defined in other plugins)
	public static partial class GLMenu
	{
		public static void OpenUrl(string url) => Application.OpenURL(url);

		[MenuItem(Constants.HelpEmailMenuItem)]
		public static void OpenSupportEmail()
		{
			OpenUrl(Constants.SupportEmail);
		}

		[MenuItem(Constants.HelpExtensionsDocumentationMenuItem)]
		public static void OpenExtensionsAPI()
		{
			OpenUrl(Constants.ExtensionsDocumentationURL);
		}
	}
}
