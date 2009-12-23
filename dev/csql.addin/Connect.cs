using System;
using System.Diagnostics;
using System.Drawing;
using csql.addin.Globals;
using csql.addin.Settings;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.VisualStudio.CommandBars;
using csql.addin.Commands;
using csql.addin.Gui.Views;

namespace csql.addin
{
	/// <summary>
	/// The main object for implementing an add-in.
	/// </summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{
		private DTE2 application;
		private AddIn addInInstance;
		private Window toolWindow;
		private SettingsPanelViewModel settingsViewModel;
		private OutputWindowPane outputWindowPane;

		OutputWindowPane OutputWindowPane
		{
			get
			{
				if ( outputWindowPane == null )
					outputWindowPane = application.ToolWindows.OutputWindow.OutputWindowPanes.Add( "csql" );

				return outputWindowPane;
			}
		}

		/// <summary>
		/// Gets the command prefix that the studio uses for commands added by this adding.
		/// The command prefix is just the qualified name of this type.
		/// </summary>
		/// <value>The full name of this type plus a single dot.</value>
		private string CommandNamePrefix
		{
			get
			{
				Type thisType = GetType();
				return thisType.FullName + '.';
			}
		}

		/// <summary>
		/// Implements the constructor for the Add-in object. Place your initialization code within this method.
		/// </summary>
		public Connect()
		{
			// Konfiguration laden und ViewModel vorbereiten
			ISettingsPersistensProvider persistensProvider = new SettingsFilePersistensProvider();
			settingsViewModel = new csql.addin.Gui.Views.SettingsPanelViewModel( persistensProvider );
		}

		#region IDTExtensibility2 implementation

		/// <summary>
		/// Implements the OnConnection method of the IDTExtensibility2 interface. 
		/// Receives notification that the Add-in is being loaded.
		/// </summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection( object application, ext_ConnectMode connectMode, object addInInst, ref Array custom )
		{
			this.application = (DTE2)application;
			this.addInInstance = (AddIn)addInInst;

			//Add-Trace-Listener
			Trace.Listeners.Clear();
			Trace.Listeners.Add( new AddInTraceListener( this.application ) );

			if ( connectMode == ext_ConnectMode.ext_cm_UISetup ) {
				object[] contextGUIDS = new object[] { };
				Commands2 commands = (Commands2)this.application.Commands;


				// This try/catch block can be duplicated if you wish to add multiple commands 
				// to be handled by your Add-in, just make sure you also update the QueryStatus/Exec 
				// method to include the new command names.
				try {

					// Kontextmenu des Editors ermitteln
					CommandBars commandBars = (CommandBars)this.application.CommandBars;
					CommandBar editorCommandBar = (CommandBar)commandBars["Editor Context Menus"];
					CommandBarPopup editorCommandBarPopup = (CommandBarPopup)editorCommandBar.Controls["Code Window"];
					CommandBar editorContextMenueCommandBar = editorCommandBarPopup.CommandBar;


					// Eigene Command Bar hinzufügen 

					CommandBarButton commandBarButton;
					Color transparencyKey = Color.Magenta;
					String imageResourceRoot = "csql.addin.Resources.Images.Commands.";
					Image image;


					CommandBar commandBar = (CommandBar)this.application.Commands.AddCommandBar( "csql", vsCommandBarType.vsCommandBarTypeToolbar, Type.Missing, 1 );

					Command commandExecuteAll = commands.AddNamedCommand2( addInInstance, "ExecuteAll", "Execute", "Execute csql with the script currently open in the editor", true, 1, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton );
					commandBarButton = (CommandBarButton)commandExecuteAll.AddControl( commandBar, 1 );

					//image = ImageHelper.LoadImageResource( "csql.Addin.Resources.Images.Commands.Options3.bmp" );
					image = ImageHelper.LoadImageResource( imageResourceRoot + "ExecuteAll.bmp" );
					commandBarButton.Picture = ImageTranslator.GetIPictureDisp( image );
					commandBarButton.Mask = ImageTranslator.GetIPictureDispMask( image, System.Drawing.Color.White, System.Drawing.Color.Black, transparencyKey );
					commandBarButton.Style = MsoButtonStyle.msoButtonIconAndCaption;



					// Settings
					Command commandShowSettings = commands.AddNamedCommand2( addInInstance, "ViewSettingsPanel", "Settings", "Open the csql settings dialog", true, 1, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton );
					commandBarButton = (CommandBarButton)commandShowSettings.AddControl( commandBar, 2 );

					image = ImageHelper.LoadImageResource( imageResourceRoot + "ViewSettingsPanel.bmp" );
					commandBarButton.Picture = ImageTranslator.GetIPictureDisp( image );
					commandBarButton.Mask = ImageTranslator.GetIPictureDispMask( image, System.Drawing.Color.White, System.Drawing.Color.Black, transparencyKey );
					commandBarButton.Style = MsoButtonStyle.msoButtonIconAndCaption;

					// Locate current file.
					Command commandEditLocateFile = commands.AddNamedCommand2( addInInstance, "EditLocateFile", "Locate file", "Selects the file currently open in the editor in the Solution Explorer", true, 1, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton );
					commandBarButton = (CommandBarButton)commandEditLocateFile.AddControl( commandBar, 3 );
					commandBarButton.Style = MsoButtonStyle.msoButtonCaption;

					// Group files.
					Command commandEditGroupFiles = commands.AddNamedCommand2( addInInstance, "EditGroupFiles", "Group files", "Changes the hierarchy of .csql so that all files beginning with the same name are gathered in one project item.", true, 1, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton );
					commandBarButton = (CommandBarButton)commandEditGroupFiles.AddControl( commandBar, 4 );
					commandBarButton.Style = MsoButtonStyle.msoButtonCaption;

					// Info
					Command commandShowInfo = commands.AddNamedCommand2( addInInstance, "ViewAboutDialog", "About", "Show the csql about dialog", true, 1, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton );
					commandBarButton = (CommandBarButton)commandShowInfo.AddControl( commandBar, 5 );
					commandBarButton.Style = MsoButtonStyle.msoButtonIcon;

					image = ImageHelper.LoadImageResource( imageResourceRoot + "ViewAboutDialog.bmp" );
					commandBarButton.Picture = ImageTranslator.GetIPictureDisp( image );
					commandBarButton.Mask = ImageTranslator.GetIPictureDispMask( image, System.Drawing.Color.White, System.Drawing.Color.Black, transparencyKey );

					commandBar.Visible = true;

					//Contextmenu
					//commandBarButton = (CommandBarButton)commandSqlExecuteQuery.AddControl(editorContextMenueCommandBar, 1);
					//image = "csql.Addin.Images.DatabaseRefresh.bmp".ToImage();
					//commandBarButton.Picture = ImageTranslator.GetIPictureDisp(image);
					//commandBarButton.Mask = ImageTranslator.GetIPictureDispMask(image, System.Drawing.Color.White, System.Drawing.Color.Black, transparencyKey);
					//commandBarButton.Style = MsoButtonStyle.msoButtonIcon;
				}
				catch ( System.ArgumentException ex ) {
					// If we are here, then the exception is probably because a command with that name
					// already exists. If so there is no need to recreate the command and we can 
					// safely ignore the exception.
					System.Diagnostics.Debug.WriteLine( "Exception: " + ex.Message );
					System.Windows.Forms.MessageBox.Show( ex.Message, "Error adding csql commands: " + ex.Message );
				}
			}
		}

		/// <summary>
		/// Implements the OnDisconnection method of the IDTExtensibility2 interface. 
		/// Receives notification that the Add-in is being unloaded.
		/// </summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection( ext_DisconnectMode disconnectMode, ref Array custom )
		{
		}

		/// <summary>
		/// Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. 
		/// Receives notification when the collection of Add-ins has changed.
		/// </summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate( ref Array custom )
		{
		}

		/// <summary>
		/// Implements the OnStartupComplete method of the IDTExtensibility2 interface. 
		/// Receives notification that the host application has completed loading.
		/// </summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete( ref Array custom )
		{
		}

		/// <summary>
		/// Implements the OnBeginShutdown method of the IDTExtensibility2 interface. 
		/// Receives notification that the host application is being unloaded.
		/// </summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown( ref Array custom )
		{
		}

		#endregion

		#region IDTCommandTarget implementation

		/// <summary>
		/// Implements the QueryStatus method of the IDTCommandTarget interface. 
		/// This is called when the command's availability is updated.
		/// </summary>
		/// <param term='commandName'>The name of the command to determine state for.</param>
		/// <param term='neededText'>Text that is needed for the command.</param>
		/// <param term='status'>The state of the command in the user interface.</param>
		/// <param term='commandText'>Text requested by the neededText parameter.</param>
		/// <seealso class='Exec' />
		public void QueryStatus( string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText )
		{
			/*
			if ( neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone ) {
				if ( commandName == "csql.Addin.Connect.csql.Addin" ) {
					status = vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
					return;
				}
			}
			*/

			status = vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
		}




		/// <summary>
		/// Implements the Exec method of the IDTCommandTarget interface. 
		/// This is called when the command is invoked.
		/// </summary>
		/// <param term='commandName'>The name of the command to execute.</param>
		/// <param term='executeOption'>Describes how the command should be run.</param>
		/// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
		/// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
		/// <param term='handled'>Informs the caller if the command was handled or not.</param>
		/// <seealso class='Exec' />
		public void Exec( string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled )
		{
			if ( !commandName.StartsWith( this.CommandNamePrefix ) ) {
				Trace.TraceWarning( "Exec: Recieved unexpected command: " + commandName +  "\r\n" );
				return;
			} else {
				Debug.WriteLine( "Executing Command: " + commandName );
			}

			commandName = commandName.Remove( 0, CommandNamePrefix.Length );

			if ( executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault ) {

				#region Informationen anzeigen

				if ( commandName == "ViewAboutDialog" ) {
					//Informationsfenster anzeigen
					var view = new Gui.Views.AboutDialog();
					view.ShowDialog();
				}

				#endregion


				#region Einstellungen anzeigen

				if ( commandName == "ViewSettingsPanel" ) {
					if ( toolWindow == null ) {
						WpfHost wpfHost = null;

						try {
							object programmableObject = null;

							String guidstr = "{858C3FCD-3333-4540-A592-F31C1520B174}";
							Windows2 windows2 = (EnvDTE80.Windows2)application.Windows;
							System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
							Type wpfHostType = typeof( WpfHost );
							toolWindow = windows2.CreateToolWindow2( addInInstance, asm.Location, wpfHostType.FullName, "csql Settings", guidstr, ref programmableObject );

							toolWindow.Visible = true;


							//Das Wpf-Control setzten
							wpfHost = (WpfHost)toolWindow.Object;

							//Content laden
							wpfHost.LoadContent( settingsViewModel );
						}
						catch ( Exception ex ) {
							System.Windows.MessageBox.Show( ex.Message, "csql addin error" );
						}

					} else {
						toolWindow.Visible = true;
					}
				}

				#endregion


				#region Execute Command

				if ( commandName == "ExecuteAll" ) {
					//Prüfen ob ein Dokument selektiert ist, sonst Fehlermeldung
					if ( application.ActiveDocument == null ) {
						Globals.Messages.ShowError( "Ausführen von CSQL nicht möglich", "Aktuell ist kein gültiges Dokument selektiert" );
						return;
					}

					//Aktuelle Einstellungen ermitteln
					SettingsItemViewModel currentItem = settingsViewModel.ItemsSourceView.CurrentItem as SettingsItemViewModel;

					// Überprüfen ob die aktuellen Einstellungen gültig sind
					if ( currentItem == null ) {
						Globals.Messages.ShowError( "Ausführen von CSQL nicht möglich", "Es ist keine Konfiguration erstellt / selektiert" );
						return;
					}

					//Zum ExecutionHelper (für Csql) delegieren

					string documentPath = application.ActiveDocument.FullName;

					ScriptExecutor executionHelper = new ScriptExecutor( documentPath, currentItem );

					executionHelper.Execute();

					// Dokument anzeigen, wenn dieses erstellt wurde und das Flag zur Erzeugung des Distributionfile gesetzt wurde
					if ( System.IO.File.Exists( currentItem.Distributionfile.Value ) && currentItem.IsDistributionfileEnabled.Value ) {
						application.ItemOperations.OpenFile( currentItem.Distributionfile.Value, Constants.vsViewKindCode );
					}
				}

				#endregion

				if ( commandName == "EditLocateFile" ) {
					SolutionExplorerFileLocator locator = new SolutionExplorerFileLocator( this.application );
					locator.LocateAndSelectCurrentFile();
				}

				if ( commandName == "EditGroupFiles" ) {
					FileGrouper grouper = new FileGrouper( this.application );
					grouper.GroupProjectItems();
				}
			}
		}
		#endregion
	}
}