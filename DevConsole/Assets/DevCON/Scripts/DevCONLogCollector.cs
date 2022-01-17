using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DevTool
{
	public class CDevCONLogCollector
	{
		private DevCONLogContainer<DevLogInfo> m_logTbl = new DevCONLogContainer<DevLogInfo>();
		private List<int/*log Index*/> m_issueTbl = new List<int>();
		private UnityAction m_onLogReceived = null;
		public struct DevLogInfo
		{
			public int _index;
			public string _msg;
			public LogType _type;
		}

		public int GetLogCount => m_logTbl.Count;
		public void BeginCollection()
		{
			Application.logMessageReceivedThreaded += onHandleLogThreaded;
		}
		public void EndCollection()
		{
			Application.logMessageReceivedThreaded -= onHandleLogThreaded;
		}
		public void BindLogReceived( UnityAction fun )
		{
			m_onLogReceived = fun;
		}
		private void UnbindLogReceived()
		{
			m_onLogReceived = null;
		}
		public void GetLog( int startIndex, int length, UnityAction<int, DevLogInfo> fun )
		{
			var lastIdx = m_logTbl.Count - 1;
			var startIdx = startIndex - ( length -1 );
			if ( lastIdx < startIdx )
				return;

			var endIdx = startIdx + ( length - 1 );
			if ( lastIdx < endIdx )
				return;

			var repeat = 0;
			for ( int i = startIdx; i <= endIdx; ++i )
			{
				var info = m_logTbl[i];
				fun.Invoke( repeat++, info );
			}
		}
		public void GetLog( int index, UnityAction<DevLogInfo> fun )
		{
			if ( index < 0 || m_logTbl.Count <= index )
				return;

			fun.Invoke( m_logTbl[index] );
		}
		public int GetPrevIssueLine( int index )
		{
			if ( index < 0 )
				return -1;

			for ( int i = m_issueTbl.Count - 1; 0 <= i; --i )
			{
				if ( m_issueTbl[i] < index )
				{
					return m_issueTbl[i];
				}
			}

			return -1;
		}
		public int GetNextIssueLine( int index )
		{
			if ( index < 0 )
				return -1;

			var len = m_issueTbl.Count;
			for ( int i = 0; i < len; ++i )
			{
				if ( index < m_issueTbl[i] )
				{
					return m_issueTbl[i];
				}
			}

			return -1;
		}
		public bool IsPrevIssue( int index )
		{
			if ( index < 0 )
				return false;

			for ( int i = m_issueTbl.Count - 1; 0 <= i; --i )
			{
				if ( m_issueTbl[i] < index )
				{
					return true;
				}
			}
			return false;
		}
		public bool IsNextIssue( int index )
		{
			if ( index < 0 )
				return false;

			var len = m_issueTbl.Count;
			for ( int i = 0; i < len; ++i )
			{
				if ( index < m_issueTbl[i] )
				{
					return true;
				}
			}
			return false;
		}
		void onHandleLogThreaded( string message, string stackTrace, LogType type )
		{
			var index = m_logTbl.Count;

			// add issue table
			if ( !( LogType.Log == type ) )
			{
				m_issueTbl.Add( index );
			}

			// add log table
			var info = new DevLogInfo
			{
				_index = index,
				_type = type,
				_msg = $"[{DateTime.Now.ToString( "HH:mm:ss" )}]\u00A0{message.Replace( ' ', '\u00A0' )}" 
						+ $"\n{stackTrace.Replace( ' ', '\u00A0' )}",
			};

			m_logTbl.Add( info );

			m_onLogReceived?.Invoke();
		}
	}
	// container 
	public class DevCONLogContainer<T>
	{
		private T[] m_table;
		private int m_idx;
		private int m_max = 131072;

		public int Count { get; private set; }
		public T this[int index]
		{
			get { return m_table[( m_idx + index ) % m_table.Length]; }
			set { m_table[( m_idx + index ) % m_table.Length] = value; }
		}
		public DevCONLogContainer()
		{
			m_table = new T[1024];
			m_idx = 0;
		}
		public void Add( T value )
		{
			if ( m_table.Length <= Count )
			{
				var capa = Count * 2;
				if ( m_max < capa )
				{
					m_table[m_idx++] = value;
					if ( m_table.Length <= m_idx )
					{
						m_idx = 0;
					}
					return;
				}

				System.Array.Resize( ref m_table, capa );
			}

			m_table[Count++] = value;
		}
	}
}