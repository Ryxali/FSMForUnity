using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{
    internal class NodeVisualElement : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<NodeVisualElement, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }

        public string Title { get => title; set { title = lightContent.Q<Label>(UIMap_GraphView.Title).text = fullContent.Q<Label>(UIMap_GraphView.Title).text = value; } }

        public string Subheading { get => subheading; set { subheading = lightContent.Q<Label>(UIMap_GraphView.Subheading).text = fullContent.Q<Label>(UIMap_GraphView.Subheading).text = value; } }


        private readonly VisualElement lightContent;
        private readonly VisualElement fullContent;
        private VisualElement boxContainer;
        
        private string title;
        private string subheading;



        public NodeVisualElement()
        {
            var lightContentAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_GraphView.GraphNodeLightContentPath);
            var fullContentAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_GraphView.GraphNodeFullContentPath);
            lightContent = lightContentAsset.Instantiate().Children().First();
            fullContent = fullContentAsset.Instantiate().Children().First();
            RegisterCallback<CustomStyleResolvedEvent>(OnStylesResolved);
            RegisterCallback<GeometryChangedEvent>(OnGeometryUpdated);
        }


        private void OnGeometryUpdated(GeometryChangedEvent evt)
        {
            boxContainer = this.Q(UIMap_GraphView.BoxContainer);
            if (InLightContentTreshold(evt.newRect.size))
            {
                Debug.Log($"LIGHT '{evt.newRect.size}");
                //lightContent.RemoveFromHierarchy();
                fullContent.RemoveFromHierarchy();
                boxContainer.Add(lightContent);
            }
            else
            {
                Debug.Log($"FULL '{evt.newRect.size}");
                lightContent.RemoveFromHierarchy();
                //fullContent.RemoveFromHierarchy();
                boxContainer.Add(fullContent);
            }
        }

        private void OnStylesResolved(CustomStyleResolvedEvent evt)
        {
        }

        private bool InLightContentTreshold(Vector2 size) {
            return size.x < 80 || size.y < 80;
        }
    }
}
