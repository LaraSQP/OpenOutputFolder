using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace LaraSPQ.Tools
{
	internal static class Paths
	{
		/// <summary>
		/// Populates a listbox with all configurations for all projects in the current solution (if any)
		/// Then, selects in the listbox the Solution Explorer's selected item in (or the active document if none is selected)
		/// </summary>
		/// https://github.com/GrzegorzKozub/VisualStudioExtensions
		/// https://github.com/Therena/ConEmuIntegration
		/// <exception cref="PathTooLongException">Ignore.</exception>
		/// <exception cref="InvalidOperationException">Ignore.</exception>
		public static bool ListSolutionConfigurations( DTE2 dte,
													   ListBox lbConfigurations,
													   ref string activePath )
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			// Get solution and list all its projects, each with all its configurations
			var projects = GetProjects( dte.Solution );

			if( projects.Count == 0 )
			{
				// Quit quietly
				return false;
			}

			foreach( Project project in projects )
			{
				if( project.ConfigurationManager == null )
				{
					continue;
				}

				var configurations = project.ConfigurationManager.ConfigurationRowNames as Array;

				foreach( string configurationName in configurations )
				{
					var configurationRow = project.ConfigurationManager.ConfigurationRow( configurationName ).Item( 1 );

					if( HasProperty( configurationRow.Properties, "OutputPath" ) == true )
					{
						var outpath		= configurationRow.Properties.Item( "OutputPath" ).Value.ToString();
						var projectPath = Path.GetDirectoryName( project.FullName );
						var fullPath	= Path.Combine( projectPath, outpath );

						lbConfigurations.Items.Add( fullPath );
					}
				}
			}

			// Select the appropriate configuration and record the path to the active document
			var activeProject = null as Project;

			if( dte.SelectedItems.Count > 0 )
			{
				var item = dte.SelectedItems.Item( 1 );

				if( item.ProjectItem != null
					&& ( Guid.Parse( item.ProjectItem.Kind ) == VSConstants.GUID_ItemType_PhysicalFile
						 || Guid.Parse( item.ProjectItem.Kind ) == VSConstants.GUID_ItemType_PhysicalFolder )
					&& item.ProjectItem.Properties != null
					&& item.ProjectItem.Properties.Item( "FullPath" ) != null )
				{
					// It is a project's file or folder
					activePath = item.ProjectItem.Properties.Item( "FullPath" ).Value.ToString();

					activeProject = item.ProjectItem.Collection.ContainingProject;
				}
				else
				{
					// It is a solution or a solution folder (including its contents and projects)
					activePath = dte.Solution.FullName;

					if( item.Project != null )
					{
						activeProject = item.Project;
					}
					else if( dte.Solution.Projects.Count > 0 )
					{
						if( dte.Solution.Projects.Count > 0 )
						{
							// Pick the first available
							var em = dte.Solution.Projects.GetEnumerator();

							while( em.MoveNext() == true )
							{
								activeProject = em.Current as Project;

								if( activeProject != null )
								{
									break;
								}
							}
						}
					}
				}
			}
			else if( dte.ActiveDocument != null )
			{
				// There is nothing selected in the Solution Explorer (is it possible?)
				// but there is a document is open in the IDE, try using it
				activePath = dte.ActiveDocument.FullName;

				activeProject = dte.ActiveDocument.ProjectItem.Collection.ContainingProject;
			}
			else
			{
				// Quit quietly
				return false;
			}

			// Highlight active configuration (if any)
			if( activeProject != null )
			{
				HighlightActiveConfiguration( lbConfigurations, activeProject );
			}

			return true;
		}




		/// <summary>
		/// Selects the active configuration in a listbox of configurations
		/// </summary>
		/// <exception cref="PathTooLongException">Ignore.</exception>
		/// <exception cref="InvalidOperationException">Ignore.</exception>
		private static void HighlightActiveConfiguration( ListBox lbConfigurations, Project project )
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var configurationManager	= project.ConfigurationManager;
			var activeConfigurationName = configurationManager.ActiveConfiguration.ConfigurationName;
			var configurationRow		= configurationManager.ConfigurationRow( activeConfigurationName ).Item( 1 );

			if( HasProperty( configurationRow.Properties, "OutputPath" ) == true )
			{
				var outpath		= configurationRow.Properties.Item( "OutputPath" ).Value.ToString();
				var projectPath = Path.GetDirectoryName( project.FullName );
				var fullPath	= Path.Combine( projectPath, outpath );
				var item		= lbConfigurations.Items.Cast<string>().First( x => x == fullPath );

				lbConfigurations.SelectedItem = item;
			}
		}




		/// <summary>
		/// Checks if a property exists
		/// https://github.com/Therena/ConEmuIntegration
		/// </summary>
		private static bool HasProperty( Properties properties, string propertyName )
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if( properties != null )
			{
				foreach( Property item in properties )
				{
					if( item != null
						&& item.Name == propertyName )
					{
						return true;
					}
				}
			}

			return false;
		}




		/// <summary>
		/// Returns all properties (kept for development purposes)
		/// https://stackoverflow.com/a/46659735/1908746
		/// </summary>
#pragma warning disable VSTHRD010
		private static List<string> GetPropertiesString( Properties properties )
		{
			var ls = new List<string>();

			foreach( Property property in properties )
			{
				try
				{
					ls.Add( property.Name + ":=" + property.Value.ToString() );
				}
				catch( Exception ex )
				{
					var x = ex.Message;
				}
			}

			return ls;
		}




#pragma warning restore VSTHRD010



		/// <summary>
		/// Queries for all projects in solution, recursively (without recursion)
		/// https://stackoverflow.com/a/54387504/1908746
		/// </summary>
#pragma warning disable VSTHRD010
		private static List<Project> GetProjects( Solution sln )
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var ls = new List<Project>();

			ls.AddRange( sln.Projects.Cast<Project>() );

			for( int i = 0; i < ls.Count; i++ )
			{
				// OfType will ignore null's (i.e., unloaded project)
				ls.AddRange( ls[ i ].ProjectItems.Cast<ProjectItem>().Select( x => x.SubProject ).OfType<Project>() );
			}

			return ls;
		}




#pragma warning restore VSTHRD010
	}
}
