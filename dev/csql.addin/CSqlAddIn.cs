using System;
using System.Diagnostics;
using System.Drawing;
using csql.addin.Commands;
using csql.addin.Globals;
using csql.addin.Gui.Views;
using csql.addin.Settings;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.VisualStudio.CommandBars;
using Sqt.VisualStudio;

namespace csql.addin
{
	/// <summary>
	/// The main object for implementing an add-in.
	/// </summary>
	/// <seealso class='IDTExtensibility2' />
	public class CSqlAddIn : VsAddIn
	{
		internal static SettingsPanelViewModel GetSettingsCollection( DTE2 application )
		{
			string key = typeof( CSqlAddIn ).FullName.Replace('.', '_') + "_Settings";
			SettingsPanelViewModel settingsViewModel = null;
			if ( application.Globals.get_VariableExists( key ) ) {
				settingsViewModel = (SettingsPanelViewModel)application.Globals[key];
			} else {
				// Konfiguration laden und ViewModel vorbereiten
				ISettingsPersistensProvider persistensProvider = new SettingsFilePersistensProvider();
				settingsViewModel = new csql.addin.Gui.Views.SettingsPanelViewModel( persistensProvider );
				application.Globals[key] = settingsViewModel;
			}
			return settingsViewModel; 
		}

		internal static SettingsItemViewModel GetCurrentSettings( DTE2 application )
		{
			SettingsPanelViewModel settingsCollection = GetSettingsCollection( application );
			if ( settingsCollection == null )
				return null;
			object settings = settingsCollection.ItemsSourceView.CurrentItem;
			return (SettingsItemViewModel)settings;
		}

		/// <summary>
		/// Implements the constructor for the Add-in object. Place your initialization code within this method.
		/// </summary>
		public CSqlAddIn()
		{
			String imageResourceRoot = "csql.addin.Resources.Images.Commands.";
			VsCommand command;
			int position = 1;

			command = new SettingsCommand();
			command.CommandBarName = "csql";
			command.Icon = new VsCommandIcon( imageResourceRoot + "Settings.bmp" );
			command.Position = position++;
			RegisterCommand( command );

			command = new ExecuteScriptCommand();
			command.CommandBarName = "csql";
			command.Icon = new VsCommandIcon( imageResourceRoot + "ExecuteScript.bmp" );
			command.Position = position++;
			RegisterCommand( command );

			command = new LocateFileCommand();
			command.CommandBarName = "csql";
			command.Icon = new VsCommandIcon( imageResourceRoot + "LocateFile.bmp" );
			command.Position = position++;
			RegisterCommand( command );

			command = new GroupFilesCommand();
			command.CommandBarName = "csql";
			command.Icon = new VsCommandIcon( imageResourceRoot + "GroupFiles.bmp" );
			command.Position = position++;
			RegisterCommand( command );

			command = new AboutDialogCommand();
			command.CommandBarName = "csql";
			command.Icon = new VsCommandIcon( imageResourceRoot + "AboutDialog.bmp" );
			command.Position = position++;
			RegisterCommand( command );
		}

		protected override CommandBar GetCommandBar( string name )
		{
			CommandBars commandBars = (CommandBars)Application.CommandBars;
			foreach ( Object item in commandBars ) {
				CommandBar commandBar = item as CommandBar;
				if ( commandBar == null )
					continue;

				if ( commandBar.Name.Equals( name ) )
					return commandBar;
			}
			CommandBar newCommandBar = (CommandBar)Application.Commands.AddCommandBar( name, vsCommandBarType.vsCommandBarTypeToolbar, Type.Missing, 1 );
			return newCommandBar;

		}
	}


}