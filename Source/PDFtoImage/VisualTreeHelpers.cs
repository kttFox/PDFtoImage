using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Libraries {
	public class VisualTreeHelpers {
		/// <summary>
		/// 子要素を取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="current"></param>
		/// <returns></returns>
		public static T FindVisualChild<T>( DependencyObject current ) where T : DependencyObject {
			if( current != null ) {
				int numVisuals = VisualTreeHelper.GetChildrenCount( current );
				for( int i = 0; i < numVisuals; i++ ) {
					var child = VisualTreeHelper.GetChild( current, i );
					var result = ( child as T ) ?? FindVisualChild<T>( (Visual)child );

					if( result != null ) {
						return result;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// 子要素を取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="current"></param>
		/// <returns></returns>
		public static T FindVisualChild<T>( DependencyObject current, Func<T, bool> predicate ) where T : DependencyObject {
			if( predicate == null ) {
				throw new ArgumentException( "条件を指定してください。" );
			}

			if( current != null ) {
				int numVisuals = VisualTreeHelper.GetChildrenCount( current );
				for( int i = 0; i < numVisuals; i++ ) {
					var child = VisualTreeHelper.GetChild( current, i );

					if( child is T result ) {
						if( predicate( result ) ) {
							return result;
						}
					}

					result = FindVisualChild<T>( (Visual)child, predicate );
					if( result != null ) {
						return result;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// 全ての子要素を取得します。
		/// </summary>
		public static IEnumerable<T> FindVisualChildren<T>( DependencyObject current ) where T : DependencyObject {
			return FindVisualChildren<T>( current, x => true );
		}

		/// <summary>
		/// 全ての子要素を取得します。
		/// </summary>
		public static IEnumerable<T> FindVisualChildren<T>( DependencyObject current, Func<T, bool> predicate ) where T : DependencyObject {
			if( predicate == null ) {
				throw new ArgumentException( "条件を指定してください。" );
			}

			if( current != null ) {
				int numVisuals = VisualTreeHelper.GetChildrenCount( current );
				for( int i = 0; i < numVisuals; i++ ) {
					var child = VisualTreeHelper.GetChild( current, i );

					{
						if( child is T result ) {
							if( predicate( result ) ) {
								yield return result;
							}
						}
					}

					{
						var children = FindVisualChildren<T>( (Visual)child, predicate );
						if( children != null ) {
							foreach( var result in children ) {
								yield return result;
							}
							continue;
						}
					}
				}
			}
		}

		/// <summary>
		/// 子要素を取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="current"></param>
		/// <param name="name">子要素の名前</param>
		/// <returns></returns>
		public static T FindVisualChildName<T>( DependencyObject current, string name ) where T : FrameworkElement {
			if( current != null ) {
				int numVisuals = VisualTreeHelper.GetChildrenCount( current );
				for( int i = 0; i < numVisuals; i++ ) {
					var child = VisualTreeHelper.GetChild( current, i );

					if( child is T result && result.Name == name ) {
						return result;
					}

					var _result = FindVisualChildName<T>( child, name );
					if( _result != null ) {
						return _result;
					}

				}
			}

			return null;
		}


		/// <summary>
		/// 親要素を取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="current"></param>
		/// <returns></returns>
		public static T FindVisualParent<T>( DependencyObject current ) where T : DependencyObject {
			if( current != null ) {
				if( current is Visual || current is Visual3D ) {
					var parent = VisualTreeHelper.GetParent( current );

					return ( parent as T ) ?? FindVisualParent<T>( parent );
				}
			}

			return null;
		}

		/// <summary>
		/// 親要素を取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="current"></param>
		/// <returns></returns>
		public static IEnumerable<T> EnumerableParents<T>( DependencyObject current ) where T : DependencyObject {
			while( current != null ) {
				if( current is Visual || current is Visual3D ) {
					var parent = VisualTreeHelper.GetParent( current );
					yield return ( parent as T );

					current = parent;
				}
			}
		}

		/// <summary>
		/// 親要素を取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="current"></param>
		/// <returns></returns>
		public static IEnumerable<DependencyObject> EnumerableParents( DependencyObject current ) {
			while( current != null ) {
				if( current is Visual || current is Visual3D ) {
					var parent = VisualTreeHelper.GetParent( current );
					yield return parent;

					current = parent;
				}
			}
		}

		/// <summary>
		/// 親要素を取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="current"></param>
		/// <returns></returns>
		public static T FindVisualMostParent<T>( DependencyObject current ) where T : DependencyObject {
			if( current != null ) {
				var parent = VisualTreeHelper.GetParent( current );

				var result2 = FindVisualParent<T>( parent );

				if( parent is T result && result2 == null ) {
					return result;
				}
				return result2;
			}

			return null;
		}

		/// <summary>
		/// 最上位の親要素を取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="current"></param>
		/// <returns></returns>
		public static DependencyObject FindVisualMostParent( DependencyObject current ) {
			if( current != null ) {
				var parent = VisualTreeHelper.GetParent( current );

				if( parent == null ) {
					return current;
				} else {
					return FindVisualMostParent( parent );
				}
			}

			return null;
		}

		/// <summary>
		/// 親要素を取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="current"></param>
		/// <param name="func">取得条件</param>
		/// <returns></returns>
		public static T FindVisualParent<T>( DependencyObject current, Func<T, bool> predicate ) where T : DependencyObject {
			if( predicate == null ) {
				throw new ArgumentException( "条件を指定してください。" );
			}

			if( current != null ) {
				var result = FindVisualParent<T>( current );

				if( result != null && predicate( result ) ) {
					return result;
				} else {
					return FindVisualParent<T>( result, predicate );
				}
			}

			return null;
		}

		/// <summary>
		/// 兄弟要素を全て取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="current"></param>
		/// <returns></returns>
		public static IEnumerable<T> FindVisualSiblings<T>( DependencyObject current ) where T : DependencyObject {
			if( current != null ) {
				var parent = VisualTreeHelper.GetParent( current );
				if( parent == null ) {
					return null;
				}

				int numVisuals = VisualTreeHelper.GetChildrenCount( parent );
				return _FindVisualSiblings();
				IEnumerable<T> _FindVisualSiblings() {
					for( int i = 0; i < numVisuals; i++ ) {
						if( VisualTreeHelper.GetChild( parent, i ) is T result ) {
							yield return result;
						}
					}
				}
			}

			return null;
		}

		/// <summary>
		/// 兄弟要素を取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="current"></param>
		/// <returns></returns>
		public static T FindVisualSibling<T>( DependencyObject current ) where T : DependencyObject {
			return FindVisualSiblings<T>( current )?.FirstOrDefault();
		}

		/// <summary>
		/// ヒットテストを使用して最初の要素を取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reference"></param>
		/// <param name="point"></param>
		/// <returns></returns>
		public static T HitTestFirstOrDefault<T>( Visual reference, Point point ) where T : DependencyObject => HitTestFirstOrDefault<T>( reference, point, null );

		/// <summary>
		/// ヒットテストを使用して最初に条件に一致した要素を取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reference"></param>
		/// <param name="point"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static T HitTestFirstOrDefault<T>( Visual reference, Point point, Func<T, bool> predicate ) where T : DependencyObject {
			return new HitTestHelper<T>().FirstOrDefault( reference, point, predicate );
		}

		/// <summary>
		/// ヒットテストを使用してすべての要素を取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reference"></param>
		/// <param name="geometry"></param>
		/// <returns></returns>
		public static ReadOnlyCollection<T> HitTestAll<T>( Visual reference, Geometry geometry ) where T : DependencyObject => HitTestAll<T>( reference, geometry, null );

		/// <summary>
		/// ヒットテストを使用して条件に一致したすべての要素を取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reference"></param>
		/// <param name="geometry"></param>
		/// <param name="predicate">条件</param>
		/// <returns></returns>
		public static ReadOnlyCollection<T> HitTestAll<T>( Visual reference, Geometry geometry, Func<T, bool> predicate ) where T : DependencyObject {
			return new HitTestHelper<T>().All( reference, geometry, predicate );
		}

		/// <summary>
		/// ヒットテストを使用してすべての要素を取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reference"></param>
		/// <param name="point"></param>
		/// <returns></returns>
		public static ReadOnlyCollection<T> HitTestAll<T>( Visual reference, Point point ) where T : DependencyObject => HitTestAll<T>( reference, point, null );

		/// <summary>
		/// ヒットテストを使用して条件に一致したすべての要素を取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reference"></param>
		/// <param name="point"></param>
		/// <param name="predicate">条件</param>
		/// <returns></returns>
		public static ReadOnlyCollection<T> HitTestAll<T>( Visual reference, Point point, Func<T, bool> predicate ) where T : DependencyObject {
			return new HitTestHelper<T>().All( reference, point, predicate );
		}

		/// <summary>
		/// ヒットテストを使用して最初の要素を取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reference"></param>
		/// <param name="geometry"></param>
		/// <returns></returns>
		public static T HitTestFirstOrDefault<T>( Visual reference, Geometry geometry ) where T : DependencyObject => HitTestFirstOrDefault<T>( reference, geometry, null );

		/// <summary>
		/// ヒットテストを使用して最初に条件に一致した要素を取得します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reference"></param>
		/// <param name="geometry"></param>
		/// <param name="predicate">条件</param>
		/// <returns></returns>
		public static T HitTestFirstOrDefault<T>( Visual reference, Geometry geometry, Func<T, bool> predicate ) where T : DependencyObject {
			return new HitTestHelper<T>().FirstOrDefault( reference, geometry, predicate );
		}



		private class HitTestHelper<T> where T : DependencyObject {

			public T FirstOrDefault( Visual reference, Point point, Func<T, bool> predicate = null ) {
				this.predicate = predicate;

				VisualTreeHelper.HitTest( reference, null, HitTestFirst, new PointHitTestParameters( point ) );

				return this.Result;
			}

			public T FirstOrDefault( Visual reference, Geometry geometry, Func<T, bool> predicate = null ) {
				this.predicate = predicate;
				VisualTreeHelper.HitTest( reference, null, HitTestFirst, new GeometryHitTestParameters( geometry ) );

				return this.Result;
			}

			public ReadOnlyCollection<T> All( Visual reference, Point point, Func<T, bool> predicate = null ) {
				this.predicate = predicate;
				this.ResultList = new List<T>();
				VisualTreeHelper.HitTest( reference, null, HitTestAll, new PointHitTestParameters( point ) );

				return this.ResultList.AsReadOnly();
			}

			public ReadOnlyCollection<T> All( Visual reference, Geometry geometry, Func<T, bool> predicate = null ) {
				this.predicate = predicate;
				this.ResultList = new List<T>();
				VisualTreeHelper.HitTest( reference, null, HitTestAll, new GeometryHitTestParameters( geometry ) );

				return this.ResultList.AsReadOnly();
			}

			private T Result;
			private List<T> ResultList;

			Func<T, bool> predicate;

			private HitTestResultBehavior HitTestFirst( HitTestResult result ) {
				this.Result = result.VisualHit as T;
				if( this.Result == null ) {
					this.Result = FindVisualParent<T>( result.VisualHit );
				}

				if( this.Result != null ) {
					if( predicate != null ) {
						if( predicate.Invoke( this.Result ) ) {
							return HitTestResultBehavior.Stop;
						} else {
							this.Result = null;
							return HitTestResultBehavior.Continue;
						}
					}

					return HitTestResultBehavior.Stop;
				}

				return HitTestResultBehavior.Continue;
			}

			private HitTestResultBehavior HitTestAll( HitTestResult result ) {
				T hitElement = null;

				if( result.VisualHit is T t ) {
					hitElement = t;
				} else {
					var parent = VisualTreeHelpers.FindVisualParent<T>( result.VisualHit );
					if( parent != null ) {
						hitElement = parent;
					}
				}

				if( hitElement != null && this.Result != hitElement ) {
					if( predicate != null ) {
						if( predicate.Invoke( this.Result ) ) {
							ResultList.Add( hitElement );
						}
					} else {
						ResultList.Add( hitElement );
					}

					this.Result = hitElement;
				}

				return HitTestResultBehavior.Continue;
			}
		}



	}
}
