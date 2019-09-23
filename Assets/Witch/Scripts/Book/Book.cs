using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Witch
{

    public static class Book
    {
        static bool initialized;
        static List<Page> pages;
        static bool loading;

        public static void Initialize()
        {
            if (initialized)
            {
                return;
            }
            pages = new List<Page>();
            initialized = true;
        }

        public static bool IsLoading()
        {
            return loading;
        }

        public static void Open(Page page, bool historyReset = false)
        {
            if (!initialized)
            {
                Initialize();
            }
            if (loading)
            {
                return;
            }
            if (historyReset)
            {
                pages.Clear();
            }
            loading = true;
            pages.Add(page);
            page.BeforeInitialize();
            var load = SceneManager.LoadSceneAsync(page.TransitionSceneName, LoadSceneMode.Additive);
            load.completed += TransitionLoaded;
        }

        public static void Back()
        {
            if (pages.Count < 2)
            {
                return;
            }
            var last = GetLastPage();
            pages.RemoveAt(pages.Count - 1);
            pages.RemoveAt(pages.Count - 2);
            Open(last);
        }

        static void TransitionLoaded(AsyncOperation async)
        {
            var lastScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 2);
            var unload = SceneManager.UnloadSceneAsync(lastScene);
            unload.completed += Unloaded;
        }

        static void Unloaded(AsyncOperation async)
        {
            var load = Resources.UnloadUnusedAssets();
            load.completed += UnloadedMemory;
        }

        static void UnloadedMemory(AsyncOperation async)
        {
            var page = GetLoadPage();
            var load = SceneManager.LoadSceneAsync(page.SceneName, LoadSceneMode.Additive);
            load.completed += Loaded;
        }

        static void Loaded(AsyncOperation async)
        {
            loading = false;
            var page = GetCurrentPage();
            page.AfterInitialize();
            if (page.IsTransitionWait)
            {
                Witchcraft.ExecuteCoroutine(TransitionWait(page));
            }
            else
            {
                var lastScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 2);
                SceneManager.UnloadSceneAsync(lastScene);
            }
        }

        static IEnumerator TransitionWait(Page page)
        {
            while (page.IsTransitionWait)
            {
                yield return null;
            }
            var lastScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 2);
            SceneManager.UnloadSceneAsync(lastScene);
        }

        static Page GetLoadPage()
        {
            return pages[pages.Count - 1];
        }

        public static Page GetLastPage()
        {
            if ((loading ? 3 : 2) > pages.Count)
            {
                return null;
            }
            return loading ? pages[pages.Count - 3] : pages[pages.Count - 2];
        }

        public static Page GetCurrentPage()
        {
            if ((loading ? 2 : 1) > pages.Count)
            {
                return null;
            }
            return loading ? pages[pages.Count - 2] : pages[pages.Count - 1];
        }

        public abstract class Page
        {
            public virtual string TransitionSceneName { get; }
            public virtual string SceneName { get; }
            public bool IsTransitionWait { get; set; } = false;
            public abstract void BeforeInitialize();
            public abstract void AfterInitialize();
        }
    }



}