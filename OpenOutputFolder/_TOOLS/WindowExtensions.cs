﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LaraSPQ.Tools
{
	internal static class WindowExtensions
	{
		// https://stackoverflow.com/questions/339620/how-do-i-remove-minimize-and-maximize-from-a-resizable-window-in-wpf
		private const int	GWL_STYLE		= -16;
		private const int	WS_MAXIMIZEBOX	= 0x10000;
		private const int	WS_MINIMIZEBOX	= 0x20000;

		[ DllImport( "user32.dll" ) ]
		extern private static int GetWindowLong( IntPtr hwnd, int index );
		[ DllImport( "user32.dll" ) ]
		extern private static int SetWindowLong( IntPtr hwnd, int index, int value );

		internal static void HideMinimizeAndMaximizeButtons( this Window window,
															 bool hideMaximize = true,
															 bool hideMinimize = true )
		{
			window.SourceInitialized += ( s, e ) =>
			{
				var hwnd			= new System.Windows.Interop.WindowInteropHelper( window ).Handle;
				var currentStyle	= GetWindowLong( hwnd, GWL_STYLE );
				var currentStyleM	= currentStyle & ( ( hideMaximize == true )? ~WS_MAXIMIZEBOX : ~0 );
				var currentStyleMm	= currentStyleM & ( ( hideMinimize == true )? ~WS_MINIMIZEBOX : ~0 );


				SetWindowLong( hwnd, GWL_STYLE, ( currentStyleMm ) );
			};
		}




		// https://stackoverflow.com/questions/26260654/wpf-converting-bitmap-to-imagesource
		[ DllImport( "gdi32.dll", EntryPoint = "DeleteObject" ) ]
		[ return : MarshalAs( UnmanagedType.Bool ) ]
		extern private static bool DeleteObject([ In ] IntPtr hObject );

		internal static ImageSource ImageSourceFromIcon( Icon icon )
		{
			var imageSource = null as ImageSource;

			try
			{
				var bmp		= icon.ToBitmap();
				var handle	= bmp.GetHbitmap();

				imageSource = Imaging.CreateBitmapSourceFromHBitmap( handle,
																	 IntPtr.Zero,
																	 Int32Rect.Empty,
																	 BitmapSizeOptions.FromEmptyOptions() );
				DeleteObject( handle );
				bmp.Dispose();
			}
			catch( Exception )
			{
				// Ignore exception quietly
				imageSource = null;
			}

			return imageSource;
		}




		// https://github.com/GrzegorzKozub/VisualStudioExtensions

		[ DllImport( "user32.dll" ) ]
		internal static extern bool SetForegroundWindow( IntPtr hWnd );
	}
}
