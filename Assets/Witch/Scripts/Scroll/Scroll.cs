using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Witch
{
    public class VScroll
    {
        ScrollRect scrollRect;

        List<ScrollContent> scrollContents;
        List<ScrollInstanceCache> instancies;

        Margin viewMargin = new Margin();
        Margin contentMargin = new Margin();

        public float ViewAreaMargin { get; set; } = 100f;

        public float PositionX { get; set; } = 0f;

        public void SetViewMargin(float value)
        {
            SetViewMargin(value, value, value, value);
        }

        public void SetViewMargin(float top, float right, float bottom, float left)
        {
            viewMargin.top = top;
            viewMargin.right = right;
            viewMargin.bottom = bottom;
            viewMargin.left = left;
            Sync();
        }

        public void SetContentMargin(float value)
        {
            SetContentMargin(value, value, value, value);
        }

        public void SetContentMargin(float top, float right, float bottom, float left)
        {
            contentMargin.top = top;
            contentMargin.right = right;
            contentMargin.bottom = bottom;
            contentMargin.left = left;
            Sync();
        }

        public void AddContent(ScrollContentView prefab, ScrollContentInfo contentInfo)
        {
            var scrollContent = new ScrollContent(prefab, contentInfo);
            scrollContents.Add(scrollContent);
            Sync();
        }

        void Initialize()
        {
            scrollContents = new List<ScrollContent>();
            instancies = new List<ScrollInstanceCache>();
            scrollRect.onValueChanged.AddListener(Apply);
        }

        public void Apply()
        {
            var height = viewMargin.top;
            foreach (var content in scrollContents)
            {
                var size = content.GetSize();
                height += size.y;
                height += contentMargin.top + contentMargin.bottom;
            }
            height += viewMargin.bottom;
            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, height);

            var startY = scrollRect.content.localPosition.y;
            var currentViewport = new Rect(0, startY - ViewAreaMargin / 2f, scrollRect.viewport.rect.width, scrollRect.viewport.rect.height + ViewAreaMargin);

            foreach (var content in scrollContents)
            {
                if (content.IsActivate())
                {
                    if (!content.IsInner(currentViewport))
                    {
                        content.Deactivate();
                    }
                }
            }

            foreach (var content in scrollContents)
            {
                if (!content.IsActivate())
                {
                    if (content.IsInner(currentViewport))
                    {
                        var cache = GetInstanceFromCache(content);
                        content.Activate(cache.instance);
                    }
                }
            }
        }

        ScrollInstanceCache GetInstanceFromCache(ScrollContent content)
        {
            var instance = instancies.Find(i => !i.instance.gameObject.activeSelf && i.prefabId == content.GetPrefabId());
            if (instance == null)
            {
                var clone = Object.Instantiate(content.GetPrefab(), scrollRect.content, false);
                instance = new ScrollInstanceCache();
                instance.prefabId = content.GetPrefabId();
                instance.instance = clone;
                instancies.Add(instance);
            }
            else if (instance.instance == null)
            {
                instancies.Remove(instance);
                instance = GetInstanceFromCache(content);
            }
            return instance;
        }

        void Apply(Vector2 value)
        {
            Apply();
        }

        public void Sync()
        {
            var y = 0f;
            foreach (var content in scrollContents)
            {
                var size = content.GetSize();
                content.SerPosition(new Vector2(PositionX, size.y / 2f + y + contentMargin.top + viewMargin.top));
                y += size.y + contentMargin.top + contentMargin.bottom;
                content.Sync();
            }
        }

        VScroll(ScrollRect scrollRect)
        {
            this.scrollRect = scrollRect ?? throw new UnityException("変数scrillRectがNullは許容されていません。");

            if (this.scrollRect.horizontal)
            {
                throw new UnityException("横方向へのスクロールは許容されていません。");
            }
            else if (!this.scrollRect.vertical)
            {
                throw new UnityException("縦方向へのスクロールを許容してください。");
            }
            // TODO: 比較の仕方がよくないのでどこかで正常化を図る。
            //else if (this.scrollRect.content.anchorMax.y != 1f || this.scrollRect.content.anchorMin.y != 1f ||
            //    this.scrollRect.content.anchorMax.x != 0.5f || this.scrollRect.content.anchorMax.x != 0.5f)
            //{
            //    throw new UnityException("Anchorはtop・centerを原点として設定してください。");
            //}

            scrollRect.content.pivot = new Vector2(0.5f, 1f);
            Initialize();
        }

        class ScrollInstanceCache
        {
            public int prefabId;
            public ScrollContentView instance;
        }

        class ScrollContent
        {
            ScrollContentView prefab;
            ScrollContentInfo contentInfo;

            ScrollContentView referenceInstance;
            RectTransform prefabRectTransform;

            Vector2 position;
            Rect rect;

            float overrideWidth = -1f;

            public ScrollContentView GetPrefab()
            {
                return prefab;
            }

            public int GetPrefabId()
            {
                return prefab.GetInstanceID();
            }

            public bool IsInner(Rect rect)
            {
                return rect.Overlaps(this.rect, true);
            }

            public void Sync()
            {
                var size = GetSize();
                rect = new Rect(position.x, position.y, size.x, size.y);
            }

            public void SerPosition(Vector2 position)
            {
                this.position = position;
            }

            public Vector2 GetPosition()
            {
                return position;
            }

            public Vector2 GetSize()
            {
                if (prefab is IInstanceContentViewSize)
                {
                    var viewSize = referenceInstance as IInstanceContentViewSize;
                    return viewSize.GetCustomSize();
                }
                else
                {
                    return new Vector2(
                            overrideWidth > 0f ? overrideWidth : prefabRectTransform.rect.width, 
                            prefabRectTransform.rect.height
                        );
                }
            }

            public bool IsActivate()
            {
                return referenceInstance != null;
            }

            public void Activate(ScrollContentView instance)
            {
                referenceInstance = instance;
                referenceInstance.transform.localPosition = new Vector3(rect.x, -rect.y);
                referenceInstance.Activate(contentInfo);
                referenceInstance.gameObject.SetActive(true);
            }

            public void Deactivate()
            {
                referenceInstance.Deactivate(contentInfo);
                referenceInstance.gameObject.SetActive(false);
                referenceInstance = null;
            }

            public ScrollContent(ScrollContentView prefab, ScrollContentInfo contentInfo)
            {
                this.prefab = prefab;
                this.contentInfo = contentInfo;
                prefabRectTransform = (RectTransform)prefab.transform;
            }
        }

        class Margin
        {
            public float top;
            public float bottom;
            public float right;
            public float left;

        }

        public static VScroll Create(ScrollRect scrollRect)
        {
            return new VScroll(scrollRect);
        }
        

    }

    public abstract class ScrollContentInfo
    {

    }

    public interface IInstanceContentViewSize
    {
        Vector2 GetCustomSize();
    }



}