using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DevTool
{
	public partial class DevCONAgent : MonoBehaviour
	{
		[SerializeField] private GameObject m_root = null;
		[SerializeField] private DevCONView_UICtrl m_view = null;
		[SerializeField] private DevCONInput_UICtrl m_input = null;
		private CDevCONLogCollector m_collector = new CDevCONLogCollector();
		private bool m_isAutoScroll = true;
		private int m_lineIdx = 0;

		private bool hasPage => m_view.GetLineCount < m_collector.GetLogCount;
		private bool isTopPage => m_view.GetLineCount - 1 == m_lineIdx;
		private bool isBottomPage => m_collector.GetLogCount - 1 == m_lineIdx;
		private void Awake()
		{
			m_root.gameObject.SetActive( false );

			DontDestroyOnLoad( gameObject );

			m_input.BindEnter( OnInputEnter );
			m_view.BindDragUp( OnDragUp );
			m_view.BindDragDown( OnDragDown );
			m_view.BindPageTop( OnPageTop );
			m_view.BindPageBottom( OnPageBottom );
			m_view.BindJumpPrev( OnJumpPrev );
			m_view.BindJumpNext( OnJumpNext );
			m_collector.BeginCollection();
			m_collector.BindLogReceived( OnLogReceived );

			// Register Commands
			CDevCONCommand.RegisterCommand( new CDevCommandTimeScale() );
		}
		private void Start()
		{
			if ( 0 < m_collector.GetLogCount )
			{
				setLine( m_collector.GetLogCount - 1 );
				updatePage();
			}
		}
		private void Update()
		{
			checkEnable();
		}
		
		private void setLine( int lineIdx )
		{
			m_lineIdx = lineIdx;

			m_view.SetActivePageTopBtn( hasPage && !isTopPage );
			m_view.SetActivePageBottomBtn( hasPage && !isBottomPage );
			m_view.SetActiveJumpPrevBtn( !isTopPage && m_collector.IsPrevIssue( m_lineIdx ) );
			m_view.SetActiveJumpNextBtn( !isBottomPage && m_collector.IsNextIssue( m_lineIdx ) );
		}
		private void updatePage()
		{
			var lineCount = m_view.GetLineCount;
			m_collector.GetLog( m_lineIdx, lineCount, ( idx, info ) =>
			{
				m_view.SetLine( idx, info._index, info._msg, info._type );
			} );
		}
		private void OnLogReceived()
		{
			if ( !m_isAutoScroll )
				return;

			setLine( m_collector.GetLogCount - 1 );

			if ( !hasPage )
			{
				m_collector.GetLog( m_lineIdx, ( info ) =>
				{
					m_view.SetLine( m_lineIdx, info._index, info._msg, info._type );
				} );
			}
			else
			{
				m_collector.GetLog( m_lineIdx, ( info ) =>
				{
					m_view.PushBackLine( info._index, info._msg, info._type );
				} );
			}
		}
		private void OnDragUp()
		{
			if ( !hasPage || isBottomPage )
				return;

			setLine( Math.Min( m_lineIdx + 1, m_collector.GetLogCount - 1 ) );

			m_collector.GetLog( m_lineIdx, ( info ) =>
			{
				m_view.PushBackLine( info._index, info._msg, info._type );
			} );

			// 맨 아래라인일 경우 실시간 로그 업데이트 되도록
			m_isAutoScroll = ( m_lineIdx == m_collector.GetLogCount - 1 ) ? true : false;
		}
		private void OnDragDown()
		{
			if ( !hasPage || isTopPage )
				return;

			m_isAutoScroll = false;

			setLine( Math.Max( m_lineIdx - 1, m_view.GetLineCount - 1 ) );

			var topIdx = m_lineIdx - ( m_view.GetLineCount - 1 );
			m_collector.GetLog( Math.Max( topIdx, 0 ), ( info ) =>
			{
				m_view.PushFrontLine( info._index, info._msg, info._type );
			} );
		}
		private void OnPageTop()
		{
			if ( !hasPage )
				return;

			m_isAutoScroll = false;

			setLine( m_view.GetLineCount - 1 );
			updatePage();
		}
		private void OnPageBottom()
		{
			if ( !hasPage || m_isAutoScroll )
				return;

			m_isAutoScroll = true;

			setLine( m_collector.GetLogCount - 1 );
			updatePage();
		}
		private void OnJumpPrev()
		{
			if ( !hasPage )
				return;

			var issueLineIdx = m_collector.GetPrevIssueLine( m_lineIdx );
			if ( issueLineIdx < 0 )
				return;

			setLine( Math.Max( issueLineIdx, m_view.GetLineCount - 1 ) );
			updatePage();
		}
		private void OnJumpNext()
		{
			if ( !hasPage )
				return;

			var issueLineIdx = m_collector.GetNextIssueLine( m_lineIdx );
			if ( issueLineIdx < 0 )
				return;

			setLine( issueLineIdx );
			updatePage();
		}
		private void OnInputEnter( string msg )
		{
			if ( !CDevCONCommand.RunCommand( msg ) )
				return;

			if ( !hasPage || m_isAutoScroll )
				return;

			m_isAutoScroll = true;

			setLine( m_collector.GetLogCount - 1 );
			updatePage();
		}
	}
#if UNITY_EDITOR
	public partial class DevCONAgent
	{
		private void checkEnable()
		{
			if ( Input.GetKeyDown( KeyCode.BackQuote ) )
			{
				m_root.SetActive( !m_root.activeSelf );
			}
		}
	}
#else
	public partial class DevCONAgent
	{
		private float m_sensorElapsed = 0;
		private void checkEnable()
		{
			if ( 0 < m_sensorElapsed )
			{
				m_sensorElapsed -= Time.deltaTime;
			}

			if ( 4 == Input.touchCount )
			{
				if ( 0 < m_sensorElapsed )
				{
					m_root.SetActive( !m_root.activeSelf );
					m_sensorElapsed = 0;
				}
				else
				{
					m_sensorElapsed = 1f;
				}
			}
		}
	}
#endif
}

