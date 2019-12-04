using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using EnvDTE80;
using LaraSPQ.Tools;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Ookii.Dialogs.Wpf;

namespace OpenOutputFolder
{
	/// <summary>
	/// Interaction logic for SelectTool.xaml
	/// </summary>
	public partial class SelectTool : System.Windows.Window
	{
		// Settings' constants
		private const string	SS_Collection	= "OpenOutputFolder";
		private const string	SS_WindowTop	= "WindowTop";
		private const string	SS_WindowLeft	= "WindowLeft";
		private const string	SS_WindowHeight = "WindowHeight";
		private const string	SS_WindowWidth	= "WindowWidth";
		private const string	SS_WindowState	= "WindowState";
		private const string	SS_LeftPanelTC	= "LeftPanelTC";
		private const string	SS_RightPanelTC = "RightPanelTC";
		private const string	SS_SelectedItem = "SelectedItem";
		private const string	SS_TCPath		= "Total Commander";
		private const string	SS_ConEmuPath	= "ConEmu";

		// Properties
		private DTE2 Dte
		{
			get; set;
		}

		private AsyncPackage Package
		{
			get; set;
		}
		private WritableSettingsStore WritableSettingsStore
		{
			get; set;
		}

		private string _activePath;


		/// <summary>
		/// Constructor
		/// </summary>
		public SelectTool( DTE2 dte, AsyncPackage package )
		{
			InitializeComponent();

			Dte		= dte;
			Package = package;
		}




		/// <summary>
		/// Called when the window has loaded
		/// </summary>
		private void OnLoad( object sender, RoutedEventArgs e )
		{
			// Window icon
			var icon = WindowExtensions.ImageSourceFromIcon( Properties.Resources.oof );

			if( icon != null )
			{
				Icon = icon;
			}

			// Hide/disable minimize button
			WindowExtensions.HideMinimizeAndMaximizeButtons( this, hideMaximize: false );

			// Restore window size and position, if any
			try
			{
				var settingsManager = new ShellSettingsManager( this.Package );

				WritableSettingsStore = settingsManager.GetWritableSettingsStore( SettingsScope.UserSettings );

				if( WritableSettingsStore.CollectionExists( SS_Collection ) == true )
				{
					// These two are required from now on
					WindowStartupLocation	= WindowStartupLocation.Manual;
					SizeToContent			= SizeToContent.Manual;

					// Window size and position
					Top			= WritableSettingsStore.GetInt32( SS_Collection, SS_WindowTop );
					Left		= WritableSettingsStore.GetInt32( SS_Collection, SS_WindowLeft );
					Height		= WritableSettingsStore.GetInt32( SS_Collection, SS_WindowHeight );
					Width		= WritableSettingsStore.GetInt32( SS_Collection, SS_WindowWidth );
					WindowState = (WindowState)WritableSettingsStore.GetInt32( SS_Collection, SS_WindowState );

					// Other settings
					rbLeftPanelTC.IsChecked		= WritableSettingsStore.GetBoolean( SS_Collection, SS_LeftPanelTC );
					rbRightPanelTC.IsChecked	= WritableSettingsStore.GetBoolean( SS_Collection, SS_RightPanelTC );
					chSelectedItem.IsChecked	= WritableSettingsStore.GetBoolean( SS_Collection, SS_SelectedItem );
				}
				else
				{
					// Create collection now to be able to check for other settings the 1st time around
					WritableSettingsStore.CreateCollection( SS_Collection );

					// Must have a default
					rbLeftPanelTC.IsChecked = ( WritableSettingsStore.PropertyExists( SS_Collection, SS_LeftPanelTC ) == true )?
											  WritableSettingsStore.GetBoolean( SS_Collection, SS_LeftPanelTC ) : true;
					rbRightPanelTC.IsChecked = ( WritableSettingsStore.PropertyExists( SS_Collection, SS_RightPanelTC ) == true )?
											   WritableSettingsStore.GetBoolean( SS_Collection, SS_RightPanelTC ) : false;
				}
			}
			catch( Exception )
			{
				// Ignore quietly
			}

			// Load configurations and active path
#pragma warning disable VSTHRD010

			if( Paths.ListSolutionConfigurations( Dte, lbConfigurations, ref _activePath ) == false )
			{
				// Something went wrong so abort
				// Error reporting, if any, is to be done in the calling method above
				Close();
			}

#pragma warning restore VSTHRD010
		}




		/// <summary>
		/// Called when the window is about to close
		/// </summary>
		private void OnClosing( object sender, CancelEventArgs e )
		{
			try
			{
				// This check should be unnecessary unless there was an Exception in OnLoad
				if( WritableSettingsStore.CollectionExists( SS_Collection ) == false )
				{
					WritableSettingsStore.CreateCollection( SS_Collection );
				}

				// Window size and position
				WritableSettingsStore.SetInt32( SS_Collection, SS_WindowTop, (int)Top );
				WritableSettingsStore.SetInt32( SS_Collection, SS_WindowLeft, ( int )Left );
				WritableSettingsStore.SetInt32( SS_Collection, SS_WindowWidth, (int)Width );
				WritableSettingsStore.SetInt32( SS_Collection, SS_WindowHeight, (int)Height );
				WritableSettingsStore.SetInt32( SS_Collection, SS_WindowState, (int)WindowState );

				// Other settings
				WritableSettingsStore.SetBoolean( SS_Collection, SS_LeftPanelTC, rbLeftPanelTC.IsChecked ?? false );
				WritableSettingsStore.SetBoolean( SS_Collection, SS_RightPanelTC, rbRightPanelTC.IsChecked ?? false );
				WritableSettingsStore.SetBoolean( SS_Collection, SS_SelectedItem, chSelectedItem.IsChecked ?? false );
			}
			catch( Exception )
			{
				// Ignore quietly
			}
		}




		/// <summary>
		/// Overrides the ESC key to quickly dismiss the editor or the manager
		/// </summary>
		protected override void OnPreviewKeyDown( KeyEventArgs e )
		{
			base.OnPreviewKeyDown( e );

			if( e.Key == Key.Escape )
			{
				// Quit quietly
				Close();
			}
		}




		private void btTotalCommander_Click( object sender, RoutedEventArgs e )
		{
			var arguments = "/O /T /{0}=".FormatWith( ( rbLeftPanelTC.IsChecked == true )? 'L' : 'R' );

			FireTool( SS_TCPath, arguments );
		}




		private void btConEmu_Click( object sender, RoutedEventArgs e )
		{
			var arguments = "-Reuse /Dir ";

			FireTool( SS_ConEmuPath, arguments );
		}




		/// <summary>
		/// Fires up TC/ConEmu by means of a setting indicating which tool to use and appropriate arguments for each
		/// </summary>
		private void FireTool( string setting, string arguments )
		{
			( bool cancelled, string toolPath ) = GetExePath( setting );

			if( cancelled == true )
			{
				// Quit quietly
				return;
			}

			if( toolPath.IsNullOrWhitespace() == true
				|| File.Exists( toolPath ) == false )
			{
				Box.Error( "The path to {0} is invalid.".FormatWith( setting ) );
			}
			else
			{
				// Update settings if needed
				if( WritableSettingsStore.PropertyExists( SS_Collection, setting ) == false
					|| WritableSettingsStore.GetString( SS_Collection, setting ) != toolPath )
				{
					WritableSettingsStore.SetString( SS_Collection, setting, toolPath );
				}

				// Make arguments
				try
				{
					var filePath	= GetFilePath();
					var process		= System.Diagnostics.Process.Start( new ProcessStartInfo
					{
						FileName = toolPath,
						Arguments = "{0}\"{1}\"".FormatWith( arguments, filePath ),
					} );

					System.Threading.Thread.Sleep( 250 );

					if( process != null
						&& process.HasExited == false )
					{
						WindowExtensions.SetForegroundWindow( process.MainWindowHandle );
					}
				}
				catch( Exception ex )
				{
					Box.Error( "Unable to fire {0}, exception:".FormatWith( setting ), ex.Message );
				}

				Close();
			}
		}




		/// <summary>
		/// Gets path from configurations' listbox or, if nothing selected, the active's file path
		/// </summary>
		/// <exception cref="PathTooLongException">Ignore.</exception>
		private string GetFilePath()
		{
			var filePath = null as string;

			if( lbConfigurations.SelectedItem != null
				&& chSelectedItem.IsChecked != true )
			{
				filePath = lbConfigurations.SelectedItem.ToString();
			}
			else
			{
				if( Directory.Exists( _activePath ) == true )
				{
					filePath = _activePath;
				}
				else
				{
					filePath = Path.GetDirectoryName( _activePath );
				}
			}

			return filePath;
		}




		/// <summary>
		/// Gets path to TC/ConEmu from settings or prompts the user if that fails
		/// </summary>
		private ( bool, string )GetExePath( string setting )
		{
			var cancelled	= false;
			var path		= ( WritableSettingsStore.PropertyExists( SS_Collection, setting ) == true )?
							  WritableSettingsStore.GetString( SS_Collection, setting ) : null as string;


			if( path.IsNullOrWhitespace() == true
				|| File.Exists( path ) == false )
			{
				try
				{
					var dlg = new VistaOpenFileDialog
					{
						CheckFileExists		= true,
						FileName			= path,
						Filter				= "Program files (*.exe)|*.exe",
						InitialDirectory	= ( path.IsNullOrWhitespace() == true )? "" : System.IO.Path.GetDirectoryName( path ),
						Multiselect			= false,
						RestoreDirectory	= true,
						Title				= "Enter path to " + setting,
					};

					if( (bool)dlg.ShowDialog( this ) == true )
					{
						path = dlg.FileName;
					}
					else
					{
						cancelled = true;
					}
				}
				catch( PathTooLongException ex )
				{
					Box.Error( "Path too long, exception:", ex.Message );
					cancelled = true;
				}
			}

			return ( cancelled, path );
		}
	}
}
