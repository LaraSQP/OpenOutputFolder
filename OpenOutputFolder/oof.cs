using System;
using System.ComponentModel.Design;
using System.Windows.Interop;
using EnvDTE80;
using LaraSPQ.Tools;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace OpenOutputFolder
{
	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class Oof
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x0100;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = new Guid( "1ce59136-e2c0-4bef-9bbd-d2e134213c6e" );

		/// <summary>
		/// VS Package that provides this command, not null.
		/// </summary>
		private AsyncPackage Package
		{
			get; set;
		}
		private static DTE2 Dte
		{
			get; set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Oof"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		/// <param name="commandService">Command service to add command to, not null.</param>
		private Oof( AsyncPackage package, OleMenuCommandService commandService )
		{
			Package			= package ?? throw new ArgumentNullException( nameof( package ) );
			commandService	= commandService ?? throw new ArgumentNullException( nameof( commandService ) );

			var menuCommandID	= new CommandID( CommandSet, CommandId );
			var menuItem		= new MenuCommand( Execute, menuCommandID );

			commandService.AddCommand( menuItem );
		}




		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static Oof Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the service provider from the owner package.
		/// </summary>
		private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
		{
			get
			{
				return Package;
			}
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		/// <exception cref="OperationCanceledException"></exception>
		public static async Task InitializeAsync( AsyncPackage package, DTE2 dte )
		{
			// Switch to the main thread - the call to AddCommand in Command1's constructor requires
			// the UI thread.
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync( package.DisposalToken );

			Dte = dte;

			OleMenuCommandService commandService = await package.GetServiceAsync( typeof( IMenuCommandService ) ) as OleMenuCommandService;

			Instance = new Oof( package, commandService );
		}




		/// <summary>
		/// This function is the callback used to execute the command when the menu item is clicked.
		/// See the constructor to see how the menu item is associated with this function using
		/// OleMenuCommandService service and MenuCommand class.
		/// </summary>
		private void Execute( object sender, EventArgs e )
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dlg		= new SelectTool( Dte, Package );
			var hwnd	= new IntPtr( Dte.MainWindow.HWnd );
			var window	= ( System.Windows.Window )HwndSource.FromHwnd( hwnd ).RootVisual;

			dlg.Owner = window;

			try
			{
				dlg.ShowDialog();
			}
			catch( InvalidOperationException ex )
			{
				Box.Error( "Unable to display OpenOutputFolder window, exception:", ex.Message );
			}
		}
	}
}
