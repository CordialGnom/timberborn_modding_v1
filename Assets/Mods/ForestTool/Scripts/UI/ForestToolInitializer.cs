using Timberborn.SingletonSystem;
using Timberborn.UILayoutSystem;
using UnityEngine;
using Cordial.Mods.ForestTool.Scripts.UI.Events;
using UnityEngine.UIElements;

namespace Cordial.Mods.ForestTool.Scripts.UI
{
    public class ForestToolInitializer : ILoadableSingleton
    {
        private readonly UILayout _uiLayout;
        private readonly EventBus _eventBus;
        private readonly ForestToolPanel _forestToolPanel;

        public ForestToolInitializer(
            UILayout uiLayout,
            EventBus eventBus,
            ForestToolPanel forestToolPanel)
        {
            _uiLayout = uiLayout;
            _eventBus = eventBus;
            _forestToolPanel = forestToolPanel;
        }

        public void Load()
        {
            var panelRoot = _forestToolPanel.Build();

            var container = new VisualElement();
            container.style.position = Position.Absolute;  // keep manual for now
            container.style.top = 100;
            container.style.right = 10;
            container.style.width = 250;
            container.Add(panelRoot);

            _uiLayout.AddTopRight(container, 5);  // was AddAbsoluteItem
            _eventBus.Register(this);
        }

        [OnEvent]
        public void OnForestToolSelectedEvent(ForestToolSelectedEvent evt)
        {

            Debug.Log("FT: Select Event");

            if (evt == null) return;
            _forestToolPanel.SetVisible(true);
        }

        [OnEvent]
        public void OnForestToolUnselectedEvent(ForestToolUnselectedEvent evt)
        {
            Debug.Log("FT: Unselect Event");
            if (evt == null) return;
            _forestToolPanel.SetVisible(false);
        }
    }
}