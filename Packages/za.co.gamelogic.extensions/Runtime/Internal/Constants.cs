namespace Gamelogic.Extensions.Editor.Internal
{
	// Contains constants used to implement the tools, mostly UI, and locations (URLs, paths). 
	internal static class Constants
	{
		public const string Gamelogic = nameof(Gamelogic);
		
		public const string Extensions = nameof(Extensions);
		public const string Documentation = nameof(Documentation);

		public const string GamelogicFolder = Gamelogic + "/";
		
		public const string ToolsRoot = "Tools/" + GamelogicFolder;
		public const string HelpRoot = "Help/" + GamelogicFolder;
		public const string ComponentsRoot = GamelogicFolder;

		public const string HelpEmailMenuItem = HelpRoot + "Email Support";
		public const string HelpExtensionsDocumentationMenuItem = HelpRoot + Extensions + "/" + Documentation;
		
		
		public const string ExtensionsComponentsRoot = ComponentsRoot + Extensions;
		
		public const string SupportEmail = "mailto:support@gamelogic.co.za";
		public const string ExtensionsDocumentationURL = "http://www.gamelogic.co.za/documentation/extensions/";
		
		
	}
}
