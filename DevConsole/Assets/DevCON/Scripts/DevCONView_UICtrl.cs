using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace DevTool
{
	/*======================================================================================================
			class View

			- UGUI labelMax = 16382
			- m_maxDetailMsgLen : detailText 세팅하는 string Max 16249 ( Mesh can not have more than 65000 vertices )
	======================================================================================================*/
	public class DevCONView_UICtrl : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
	{
		[Header( "Setting" )]
		[SerializeField] private GameObject m_viewPanel = null;
		[SerializeField] private GameObject m_detailViewPanel = null;
		[SerializeField] private Text m_detailText = null;
		[SerializeField] private Button m_pageTopBtn = null;
		[SerializeField] private Button m_pageBottomBtn = null;
		[SerializeField] private Button m_jumpPrevBtn = null;
		[SerializeField] private Button m_jumpNextBtn = null;
		private UnityAction m_onDragUp = null;
		private UnityAction m_onDragDown = null;
		private ScrollRect m_scrollRect = null;
		private CDevLogViewLine[] m_lines = null;
		private int m_detailLogIdx = -1;
		private int m_textCount = 0;
		private const int m_maxDetailMsgLen = 16000;
		private enum LogTypeLayout
		{
			LOGTYPE_COUNT = LogType.Exception + 1,
		}
		public static readonly Color[] COLOR_BY_LOGTYPE = new Color[( int )LogTypeLayout.LOGTYPE_COUNT]
		{
			Color.red,	// Error
			Color.red,	// Assert
			Color.yellow,	// Warning
			new Color(0.86f, 0.86f, 0.86f),	// Log
			new Color(0.86f, 0.86f, 0.86f),	// Exception
		};

		public int GetLineCount => m_textCount;
		private void Awake()
		{
			if ( null == m_viewPanel )
			{
				Debug.LogWarning( "Binding not found : [m_viewPanel]" );
				return;
			}
			if ( null == m_detailViewPanel )
			{
				Debug.LogWarning( "Binding not found : [m_detailViewPanel]" );
				return;
			}

			m_pageTopBtn.gameObject.SetActive( false );
			m_pageBottomBtn.gameObject.SetActive( false );
			m_jumpPrevBtn.gameObject.SetActive( false );
			m_jumpNextBtn.gameObject.SetActive( false );

			// auto bind view text component
			m_textCount = m_viewPanel.transform.childCount;
			m_lines = new CDevLogViewLine[m_textCount];
			for ( int i = 0; i < m_textCount; ++i )
			{
				var children = m_viewPanel.transform.GetChild( i );
				var logView = new CDevLogViewLine
				{
					_text = children.GetComponentInChildren<Text>(),
					_image = children.GetComponentInChildren<Image>(),
					_type = LogType.Log,
				};
				m_lines[i] = logView;

				var viewLine = children.GetComponent<DevCONViewLine_UICtrl>();
				viewLine.SetIndex( i );
				viewLine.BindLineClick( OnLineClick );
			}

			// bind detail view text component
			m_detailText = m_detailViewPanel.GetComponentInChildren<Text>();
			m_detailLogIdx = -1;

			// bind scroll view
			m_scrollRect = m_detailViewPanel.GetComponentInChildren<ScrollRect>();
		}
		private void clear()
		{
			m_detailLogIdx = -1;
			m_detailText.text = string.Empty;
		}
		public void SetActivePageTopBtn( bool isActive )
		{
			m_pageTopBtn.gameObject.SetActive( isActive );
		}
		public void SetActivePageBottomBtn( bool isActive )
		{
			m_pageBottomBtn.gameObject.SetActive( isActive );
		}
		public void SetActiveJumpPrevBtn( bool isActive )
		{
			m_jumpPrevBtn.gameObject.SetActive( isActive );
		}
		public void SetActiveJumpNextBtn( bool isActive )
		{
			m_jumpNextBtn.gameObject.SetActive( isActive );
		}
		public void BindDragUp( UnityAction fun )
		{
			m_onDragUp = fun;
		}
		public void BindDragDown( UnityAction fun )
		{
			m_onDragDown = fun;
		}
		public void BindPageTop( UnityAction fun )
		{
			m_pageTopBtn.onClick.AddListener( fun );
		}
		public void BindPageBottom( UnityAction fun )
		{
			m_pageBottomBtn.onClick.AddListener( fun );
		}
		public void BindJumpPrev( UnityAction fun )
		{
			m_jumpPrevBtn.onClick.AddListener( fun );
		}
		public void BindJumpNext( UnityAction fun )
		{
			m_jumpNextBtn.onClick.AddListener( fun );
		}
		/*======================================================================================================
				Line
		======================================================================================================*/
		public void SetLine( int lineidx, int logidx, string log = "", LogType type = LogType.Log )
		{
			if ( lineidx < 0 || m_textCount <= lineidx )
				return;

			var line = m_lines[lineidx];
			line.SetLogIndex( logidx );
			line.SetLog( log );
			line.SetFontColor( COLOR_BY_LOGTYPE[( int )type] );
			line.SetBackColor( logidx == m_detailLogIdx );
		}
		public void PushFrontLine( int logidx, string log, LogType type )
		{
			for ( int i = m_lines.Length - 1; 1 <= i; --i )
			{
				var idx = i - 1;
				if ( idx < 0 )
					break;

				m_lines[i].CopyData( m_lines[idx] );
			}

			var line = m_lines[0];
			line.SetLogIndex( logidx );
			line.SetLog( log );
			line.SetFontColor( COLOR_BY_LOGTYPE[( int )type] );
			line.SetBackColor( logidx == m_detailLogIdx );
		}
		public void PushBackLine( int logidx, string log, LogType type )
		{
			var len = m_lines.Length;
			for ( int i = 0; i < len; ++i )
			{
				var idx = i + 1;
				if ( len <= idx )
					break;

				m_lines[i].CopyData( m_lines[idx] );
			}

			var line = m_lines[len - 1];
			line.SetLogIndex( logidx );
			line.SetLog( log );
			line.SetFontColor( COLOR_BY_LOGTYPE[( int )type] );
			line.SetBackColor( logidx == m_detailLogIdx );
		}
		public void OnLineClick( int lineidx )
		{
			if ( lineidx < 0 || m_textCount <= lineidx )
				return;

			var selectLine = m_lines[lineidx];
			if ( selectLine.GetLogIndex == m_detailLogIdx )
				return;

			// color
			if ( 0 <= m_detailLogIdx )
			{
				var beginLogIdx = m_lines[0]._logIndex;
				var simulateIdx = m_detailLogIdx - beginLogIdx;
				if ( 0 <= simulateIdx && simulateIdx < m_textCount )
				{
					var line = m_lines[simulateIdx];
					line.SetBackColor( false );
				}
			}
			selectLine.SetBackColor( true );

			// scroll
			var contentRT = m_scrollRect.content.GetComponent<RectTransform>();
			contentRT.anchoredPosition = new Vector2( 0, 0 );
			m_scrollRect.StopMovement();

			// text
			var log = selectLine.GetLog;
			var safeText = log.Length <= m_maxDetailMsgLen ? log : log.Substring( 0, m_maxDetailMsgLen ) + " <message truncated...>";
			m_detailText.text = safeText;

			m_detailLogIdx = selectLine.GetLogIndex;
		}
		/*======================================================================================================
				EventSystems
		======================================================================================================*/
		private bool m_isDragging = true;
		private float m_beginPosY = 0f;
		public void OnBeginDrag( PointerEventData eventData )
		{
			m_isDragging = true;
			m_beginPosY = eventData.position.y;
		}
		public void OnDrag( PointerEventData eventData )
		{
			if ( !m_isDragging )
				return;

			var ignore = 10f;
			if ( ( m_beginPosY + ignore ) < eventData.position.y )
			{
				m_beginPosY = eventData.position.y;
				m_onDragUp?.Invoke();
			}
			else if ( eventData.position.y < ( m_beginPosY - ignore ) )
			{
				m_beginPosY = eventData.position.y;
				m_onDragDown?.Invoke();
			}
		}
		public void OnEndDrag( PointerEventData eventData )
		{
			m_isDragging = false;
		}
	}
	/*======================================================================================================
			class ViewLine
	======================================================================================================*/
	public class CDevLogViewLine
	{
		public int _logIndex = 0;
		public LogType _type = LogType.Log;
		public Text _text = null;
		public Image _image = null;

		private enum BCType
		{
			LINE_1,
			LINE_2,
			SELECT,
		}
		private enum BCTypeLayout
		{
			COUNT = BCType.SELECT + 1,
		}
		public static readonly Color[] COLOR_BY_TYPE = new Color[( int )BCTypeLayout.COUNT]
		{
			new Color( 0.21f, 0.21f, 0.21f, 0.94f ), // unselect type1
			new Color( 0.24f, 0.24f, 0.24f, 0.94f ), // unselect type1
			new Color( 0.13f, 0.29f, 0.42f, 0.94f ), // select type
		};

		public string GetLog => _text.text;
		public int GetLogIndex => _logIndex;
		public bool IsValid => 0 <= _logIndex;
		public void Clear()
		{
			_logIndex = -1;
		}
		public void CopyData( CDevLogViewLine line )
		{
			_logIndex = line.GetLogIndex;
			_text.text = line.GetLog;
			_type = line._type;
			_text.color = line._text.color;
			_image.color = line._image.color;
		}
		public void SetLog( string log )
		{
			_text.text = log;
		}
		public void SetFontColor( Color color )
		{
			_text.color = color;
		}
		public void SetLogIndex( int logIdx )
		{
			_logIndex = logIdx;
		}
		public void SetBackColor( bool isSelect )
		{
			if ( isSelect )
			{
				_image.color = COLOR_BY_TYPE[( int )BCType.SELECT];
			}
			else
			{
				_image.color = ( _logIndex % 2 == 0 ) ?
					COLOR_BY_TYPE[( int )BCType.LINE_1] : COLOR_BY_TYPE[( int )BCType.LINE_2];
			}
		}
	}

	/*======================================================================================================
				Drag 방향 체크 불편하면 이걸로
	======================================================================================================*/
	//private float m_sqrDetectLength = 0;
	//private const float m__halfOffset = 90 / 2;
	//private const float m__up = m__halfOffset;
	//private const float m__left = 90 - m__halfOffset;
	//private const float m__right = 90 + m__halfOffset;
	//private const float m__down = 180 - m__halfOffset;
	//public void OnDrag( PointerEventData eventData )
	//{
	//	if ( !m_isDragging )
	//		return;
	//	float angle = Vector3.Angle( Vector3.up, eventData.delta.normalized );
	//	if ( angle <= m__up )
	//	{
	//		Debug.Log( $"###### up" );
	//	}
	//	else if ( m__left <= angle && angle <= m__right )
	//	{
	//	}
	//	else if ( m__down <= angle )
	//	{
	//		Debug.Log( $"###### down" );
	//	}
	//}
}