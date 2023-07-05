using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnTranslate.utility;

namespace EnTranslate_Chinese.QuickInfo
{
    internal class EnQuickInfoController : IIntellisenseController
    {
        private ITextView m_textView;
        private IList<ITextBuffer> m_subjectBuffers;
        private EnQuickInfoControllerProvider m_provider;
        private IQuickInfoSession m_session;

        private static readonly object _lockObject = new object();
        static Action<Action> _debounce = null;
        /// <summary>
        /// 防抖函数
        /// </summary>
        public static Action<Action> debounce
        {
            get
            {
                if (_debounce == null)
                {
                    lock (_lockObject)
                    {
                        if (_debounce == null)
                        {
                            _debounce = utlis.Debounce(50);
                        }
                    }
                }
                return _debounce;
            }
        }

        internal EnQuickInfoController(ITextView textView, IList<ITextBuffer> subjectBuffers, EnQuickInfoControllerProvider provider)
        {
            m_textView = textView;
            m_subjectBuffers = subjectBuffers;
            m_provider = provider;

            m_textView.MouseHover += this.OnTextViewMouseHover;
        }

        private void OnTextViewMouseHover(object sender, MouseHoverEventArgs e)
        {
            debounce(() =>
            {
                //find the mouse position by mapping down to the subject buffer
                SnapshotPoint? point = m_textView.BufferGraph.MapDownToFirstMatch
                     (new SnapshotPoint(m_textView.TextSnapshot, e.Position),
                    PointTrackingMode.Positive,
                    snapshot => m_subjectBuffers.Contains(snapshot.TextBuffer),
                    PositionAffinity.Predecessor);

                if (point != null)
                {
                    ITrackingPoint triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position,
                    PointTrackingMode.Positive);

                    if (!m_provider.QuickInfoBroker.IsQuickInfoActive(m_textView))
                    {
                        m_session = m_provider.QuickInfoBroker.TriggerQuickInfo(m_textView, triggerPoint, true);
                    }
                }
            });
        }

        public void Detach(ITextView textView)
        {
            if (m_textView == textView)
            {
                m_textView.MouseHover -= this.OnTextViewMouseHover;
                m_textView = null;
            }
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }
    }
}
