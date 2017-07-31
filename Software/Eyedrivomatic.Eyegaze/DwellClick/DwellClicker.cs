// Copyright (c) 2016 Eyedrivomatic Authors
//
// This file is part of the 'Eyedrivomatic' PC application.
//
//    This program is intended for use as part of the 'Eyedrivomatic System' for 
//    controlling an electric wheelchair using soley the user's eyes. 
//
//    Eyedrivomatic is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Eyedrivomatic is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace Eyedrivomatic.Eyegaze.DwellClick
{
    public static class DwellClicker
    {
        public static bool Click(UIElement element)
        {
            var peer = UIElementAutomationPeer.FromElement(element) ?? UIElementAutomationPeer.CreatePeerForElement(element);

            return InvokeElement(peer)
                   || ToggleElement(peer)
                   || SelectElement(peer)
                   || SelectTabElement(element)
                   || SelectListBoxElement(element);
        }

        private static bool InvokeElement(AutomationPeer peer)
        {
            var invokePattern = peer?.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            if (invokePattern == null) return false;
            invokePattern.Invoke();
            return true;
        }

        private static bool ToggleElement(AutomationPeer peer)
        {
            var togglePattern = peer?.GetPattern(PatternInterface.Toggle) as IToggleProvider;
            if (togglePattern == null) return false;
            togglePattern.Toggle();
            return true;
        }

        private static bool SelectElement(AutomationPeer peer)
        {
            var selectionPattern = peer?.GetPattern(PatternInterface.SelectionItem) as ISelectionItemProvider;
            if (selectionPattern == null) return false;
            selectionPattern.Select();
            return true;
        }

        private static bool SelectTabElement(UIElement element)
        {
            //The AutomationPeer returned by UIElementAutomationPeer.CreatePeerForElement to a TabItem is stupid and cannot select the tab.
            //The "correct" approach is apparently to create a custom tab item adn a custom automation provider. This however works just as well.
            // It's just not as elegant as it requires down-casting... yuck!
            var tabItem = element as TabItem;
            if (tabItem == null) return false;
            return (tabItem.IsSelected = true);
        }

        private static bool SelectListBoxElement(UIElement element)
        {
            //The AutomationPeer returned by UIElementAutomationPeer.CreatePeerForElement to a TabItem is stupid and cannot select the tab.
            //The "correct" approach is apparently to create a custom tab item adn a custom automation provider. This however works just as well.
            // It's just not as elegant as it requires down-casting... yuck!
            var listBoxItem = element as ListBoxItem;
            if (listBoxItem == null) return false;
            return (listBoxItem.IsSelected = true);
        }
    }
}
