using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using EnvDTE80;
using LaraSPQ.Tools;
using Microsoft.VisualStudio.Shell;
using Ookii.Dialogs.Wpf;

namespace OpenOutputFolder
{
	/// <summary>
	/// Interaction logic for SelectTool.xaml
	/// </summary>
	public partial class SelectTool : System.Windows.Window
	{
		private DTE2 Dte
		{
			get; set;
		}

		private string _activePath;


		/// <summary>
		/// Constructor
		/// </summary>
		public SelectTool( DTE2 dte )
		{
			InitializeComponent();

			Dte = dte;
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
			if( Properties.Settings.Default.WindowHeight != -1 )
			{
				WindowStartupLocation	= WindowStartupLocation.Manual;
				SizeToContent			= SizeToContent.Manual;
				Top						= Properties.Settings.Default.WindowTop;
				Left					= Properties.Settings.Default.WindowLeft;
				Width					= Properties.Settings.Default.WindowHeight;
				Height					= Properties.Settings.Default.WindowWidth;
				WindowState				= Properties.Settings.Default.WindowState;
			}

			// Restore other settings
			rbLeftPanelTC.IsChecked		= Properties.Settings.Default.LeftPanelTC;
			rbRightPanelTC.IsChecked	= Properties.Settings.Default.RightPanelTC;
			chActiveDocument.IsChecked	= Properties.Settings.Default.ActiveDocument;

			// Load configurations and active path
#pragma warning disable VSTHRD010
			if ( Paths.ListSolutionConfigurations( Dte, lbConfigurations, ref _activePath ) == false )
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
			Properties.Settings.Default.WindowTop		= Top;
			Properties.Settings.Default.WindowLeft		= Left;
			Properties.Settings.Default.WindowHeight	= Width;
			Properties.Settings.Default.WindowWidth		= Height;
			Properties.Settings.Default.WindowState		= WindowState;

			Properties.Settings.Default.LeftPanelTC		= rbLeftPanelTC.IsChecked ?? false;
			Properties.Settings.Default.RightPanelTC	= rbRightPanelTC.IsChecked ?? false;
			Properties.Settings.Default.ActiveDocument	= chActiveDocument.IsChecked ?? false;
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
			var toolPath = GetExePath( "Total Commander", Properties.Settings.Default.TCPath );

			if( toolPath.IsNullOrWhitespace() == true
				|| File.Exists( toolPath ) == false )
			{
				Box.Error( "The path to Total Commander is invalid." );
			}
			else
			{
				// Update settings if needed
				if( Properties.Settings.Default.TCPath != toolPath )
				{
					Properties.Settings.Default.TCPath = toolPath;
				}

				// Make arguments
				var filePath	= GetFilePath();
				var arguments	= "/O /T /{0}={1}".FormatWith( ( rbLeftPanelTC.IsChecked == true )? 'L' : 'R', filePath );

				// Fire up tool
				FireToolAndQuit( toolPath, arguments );
			}
		}




		private void btConEmu_Click( object sender, RoutedEventArgs e )
		{
			var toolPath = GetExePath( "Total Commander", Properties.Settings.Default.ConEmuPath );

			if( toolPath.IsNullOrWhitespace() == true
				|| File.Exists( toolPath ) == false )
			{
				Box.Error( "The path to Total Commander is invalid." );
			}
			else
			{
				// Update settings if needed
				if( Properties.Settings.Default.ConEmuPath != toolPath )
				{
					Properties.Settings.Default.ConEmuPath = toolPath;
				}

				// Make arguments
				var filePath	= GetFilePath();
				var arguments	= "-Reuse /Dir \"{0}\"".FormatWith( filePath );

				// Fire up tool
				FireToolAndQuit( toolPath, arguments );
			}
		}




		/// <summary>
		/// Gets path from configurations' listbox or, if nothing selected, the active's file path
		/// </summary>
		/// <exception cref="PathTooLongException">Ignore.</exception>
		private string GetFilePath()
		{
			string filePath;

			if( lbConfigurations.SelectedItem != null
				&& chActiveDocument.IsChecked != true )
			{
				filePath = lbConfigurations.SelectedItem.ToString();
			}
			else
			{
				filePath = Path.GetDirectoryName( _activePath );
			}

			return filePath;
		}




		private void FireToolAndQuit( string toolPath, string arguments )
		{
			try
			{
				var process = System.Diagnostics.Process.Start( new ProcessStartInfo
				{
					FileName = toolPath,
					Arguments = arguments,
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
				Box.Error( "Unable to fire TC/ConEmu, exception:", ex.Message );
			}

			Close();
		}




		private string GetExePath( string tool, string path )
		{
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
						Title				= "Enter path to " + tool,
					};

					if( (bool)dlg.ShowDialog( this ) == true )
					{
						path = dlg.FileName;
					}
				}
				catch( PathTooLongException ex )
				{
					Box.Error( "Path too long, exception:", ex.Message );
				}
			}

			return path;
		}
	}
}
