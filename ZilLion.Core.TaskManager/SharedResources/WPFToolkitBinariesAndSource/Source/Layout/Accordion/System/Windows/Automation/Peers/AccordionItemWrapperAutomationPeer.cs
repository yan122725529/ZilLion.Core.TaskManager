// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System.Windows.Automation.Peers;
using System.Windows.Controls.Accordion.System.Windows.Controls;

namespace System.Windows.Controls.Accordion.System.Windows.Automation.Peers
{
    /// <summary>
    /// Wraps an <see cref="T:System.Windows.Controls.Accordion.System.Windows.Controls.AccordionItem" />.
    /// </summary>
    public class AccordionItemWrapperAutomationPeer : FrameworkElementAutomationPeer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="item">The <see cref="T:System.Windows.Controls.Accordion.System.Windows.Controls.AccordionItem" /> to wrap.</param>
        public AccordionItemWrapperAutomationPeer(AccordionItem item)
            : base(item)
        {
        }
    }
}
