using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage.Streams;
using Libraries;

namespace PDFtoImage {
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();

			progressBar.Visibility = Visibility.Hidden;
			comboBoxOldValue = comboBoxDPI.Text;
		}

		private async void Window_ContentRendered( object sender, EventArgs e ) {

			var target = VisualTreeHelpers.FindVisualChild<TextBox>( comboBoxDPI );
			target.LostFocus += this.ComboBoxDPI_LostFocus;


			var args = Environment.GetCommandLineArgs();

			if( args.Length <= 1 ) {
				return;
			}

			await ドロップ( args );
		}

		private async void Button_Click( object sender, RoutedEventArgs e ) {

			var initialDirectory = "";
			if( File.Exists( textBoxTargetFile.Text ) ) {
				initialDirectory = Path.GetDirectoryName( textBoxTargetFile.Text );
			} else if( Directory.Exists( textBoxTargetFile.Text ) ) {
				initialDirectory = textBoxTargetFile.Text;
			}

			var f = new OpenFileDialog() {
				Title = "変換するPDFファイルを選択してください。",
				InitialDirectory = initialDirectory,
				Filter = "PDFファイル|*.pdf|すべてのファイル|*.*",
			};
			if( f.ShowDialog() != true ) {
				return;
			}

			textBoxTargetFile.Text = f.FileName;

			await PDF変換実行();
		}

		async Task PDF変換実行() {
			if( !File.Exists( textBoxTargetFile?.Text ) ) {
				return;
			}

			button保存.IsEnabled = false;

			viewer.Children.Clear();

			var file = await Windows.Storage.StorageFile.GetFileFromPathAsync( textBoxTargetFile.Text );

			if( IsDPIエラー( comboBoxDPI.Text, out var dpi ) ) {
				return;
			}

			var pdfDocument = await PdfDocument.LoadFromFileAsync( file );

			try {
				progressBar.Visibility = Visibility.Visible;
				progressBar.Maximum = pdfDocument.PageCount;
				progressBar.Value = 0;

				for( uint i = 0; i < pdfDocument.PageCount; i++ ) {
					progressBar.Value++;
					await Task.Delay( 1 );

					using( var page = pdfDocument.GetPage( i ) ) {
						using( var stream = new InMemoryRandomAccessStream() ) {
							var renderOptions = new PdfPageRenderOptions() {
								DestinationWidth = (uint)Math.Round( page.Dimensions.ArtBox.Width / 96.0 * dpi ),
								BitmapEncoderId = Windows.Graphics.Imaging.BitmapEncoder.PngEncoderId,
							};
							if( RadioButtonJpeg.IsChecked == true ) {
								renderOptions.BitmapEncoderId = Windows.Graphics.Imaging.BitmapEncoder.JpegEncoderId;
							}

							await page.RenderToStreamAsync( stream, renderOptions );

							var bitmapFrame = BitmapDecoder.Create( stream.AsStream(), BitmapCreateOptions.None, BitmapCacheOption.OnLoad ).Frames[0];

							viewer.Children.Add(
								new Border() {
									Margin = new Thickness( 3 ),
									BorderThickness = new Thickness( 1 ),
									BorderBrush = Brushes.Black,
									Child = new Image() { Source = bitmapFrame }
								}
							);

							//using( var fileStream1 = new FileStream( $"./file2{i}.png", FileMode.Create ) ) {
							//	await RandomAccessStream.CopyAndCloseAsync( stream.GetInputStreamAt( 0 ), fileStream1.AsOutputStream() );
							//}

						}
					}
				}

			} finally {
				progressBar.Visibility = Visibility.Hidden;
			}

			button保存.IsEnabled = true;
		}

		/// <summary>
		/// 保存
		/// </summary>
		private async void Button_Click_1( object sender, RoutedEventArgs e ) {

			var extension = ( RadioButtonPng.IsChecked == true ) ? ".png" : ".jpg";
			var baseFilename = Path.GetFileNameWithoutExtension( textBoxTargetFile.Text );

			var f = new SaveFileDialog() {
				Title = "保存フォルダを選択してください。",
				FileName = baseFilename + extension,

				Filter = ( RadioButtonPng.IsChecked == true ) ? "PNGファイル|*.png" : "JPEGファイル|*.jpg",
			};
			if( f.ShowDialog() != true ) {
				return;
			}

			baseFilename = Path.GetFileNameWithoutExtension( f.FileName );
			var saveDir = Path.GetDirectoryName( f.FileName );

			var filenames = Enumerable.Range( 1, viewer.Children.Count ).Select( x => baseFilename + $" ({x})" + extension ).ToArray();

			if( Directory.EnumerateFiles( saveDir ).Any( x => filenames.Contains( Path.GetFileName( x ) ) ) ) {
				var r = MessageBox.Show( this, "すでにファイルが存在しています。\n上書きしてもよろしいですか？", "上書き確認", MessageBoxButton.OKCancel, MessageBoxImage.Question );
				if( r != MessageBoxResult.OK ) {
					return;
				}
			}

			try {
				progressBar.Visibility = Visibility.Visible;
				progressBar.Maximum = filenames.Length;
				progressBar.Value = 0;

				var images = VisualTreeHelpers.FindVisualChildren<Image>( viewer ).Select( x => x.Source );
				foreach( var (image, filename) in images.Zip( filenames, ( a, b ) => (a, b) ) ) {
					progressBar.Value++;
					await Task.Delay( 1 );

					BitmapEncoder encoder = null;
					if( RadioButtonPng.IsChecked == true ) {
						encoder = new PngBitmapEncoder() {
							Frames = { BitmapFrame.Create( (BitmapSource)image ) },
						};
					} else {
						encoder = new JpegBitmapEncoder() {
							Frames = { BitmapFrame.Create( (BitmapSource)image ) },
						};
					}

					using( var file = File.Create( Path.Combine( saveDir, filename ) ) ) {
						encoder.Save( file );
					}
				}

				progressBar.Maximum = progressBar.Value;
				await Task.Delay( 1 );

			} catch( Exception ex ) {
				MessageBox.Show( this, ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error );

			} finally {
				progressBar.Visibility = Visibility.Hidden;
			}
		}

		private async void RadioButton_Checked( object sender, RoutedEventArgs e ) {
			await PDF変換実行();
		}



		void AssociatedObject_DragOver( object sender, DragEventArgs e ) {
			var files = e.Data.GetData( DataFormats.FileDrop ) as string[];

			if( files.Any( x => x.ToLower().EndsWith( ".pdf" ) ) ) {
				e.Effects = DragDropEffects.All;
			} else {
				e.Effects = DragDropEffects.None;
			}

			e.Handled = true;
		}

		async void AssociatedObject_Drop( object sender, DragEventArgs e ) {
			e.Handled = true;

			string[] files = e.Data.GetData( DataFormats.FileDrop ) as string[];

			await ドロップ( files );
		}

		async Task ドロップ( string[] files ) {
			var file = files.FirstOrDefault( x => x.ToLower().EndsWith( ".pdf" ) );
			if( file == null ) {
				return;
			}

			textBoxTargetFile.Text = file;

			await PDF変換実行();
		}


		string comboBoxOldValue;

		private async void ComboBoxDPI_SelectionChanged( object sender, SelectionChangedEventArgs e ) {

			if( e.AddedItems.Count < 1 ) {
				return;
			}
			var newValue = (string)e.AddedItems[0];

			if( comboBoxOldValue == newValue ) {
				return;
			}

			if( IsDPIエラー( newValue, out var dpi ) ) {
				return;
			}

			comboBoxOldValue = newValue;

			await PDF変換実行();
		}

		private async void ComboBoxDPI_LostFocus( object sender, RoutedEventArgs e ) {

			if( comboBoxOldValue == comboBoxDPI.Text ) {
				return;
			}

			if( IsDPIエラー( comboBoxDPI.Text, out var dpi ) ) {
				return;
			}

			comboBoxOldValue = comboBoxDPI.Text;

			await PDF変換実行();
		}

		bool IsDPIエラー( string value, out double dpi ) {
			if( !double.TryParse( value, out dpi ) ) {
				MessageBox.Show( this, "解像度には数字を入力してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error );

				return true;
			}
			if( double.IsNaN( dpi ) || dpi < 10 || 2000 < dpi ) {
				MessageBox.Show( this, "解像度は10から2000までの値を入力してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error );

				return true;
			}

			return false;
		}

	}
}
