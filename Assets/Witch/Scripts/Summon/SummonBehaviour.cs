using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Witch
{

    public static class Summon
    {

        static bool initialized;

        static List<Entry> entries;
        static List<Cache> caches;
        static List<Work> works;

        public static int MaxLoadingCount { get; set; } = 10;

        public static void Initialize()
        {
            if (initialized)
            {
                return;
            }
            entries = new List<Entry>();
            caches = new List<Cache>();
            works = new List<Work>();
            initialized = true;
            SummonBehaviour.GetInstance();
        }

        public static void Contract(string assetPath, SummonType summonType)
        {
            if (!initialized)
            {
                Initialize();
            }
            var entry = new Entry { id = entries.Count + 1, assetPath = assetPath, summonType = summonType };
            entries.Add(entry);
        }

        public static int GetEntryId(string assetPath)
        {
            var entry = entries.Find(e => e.assetPath == assetPath);
            return entry != null ? entry.id : -1;
        }

        public static void Call<T>(int id, System.Action<T> result) where T : UnityEngine.Object
        {
            var entry = entries.Find(e => e.id == id);
            Call<T>(entry, result);
        }

        public static void Call<T>(string assetPath, System.Action<T> result) where T : UnityEngine.Object
        {
            var entry = entries.Find(e => e.assetPath == assetPath);
            Call<T>(entry, result);
        }

        static void Call<T>(Entry entry, System.Action<T> result) where T : UnityEngine.Object
        {
            if (entry != null)
            {
                var cache = caches.Find(c => c.id == entry.id);
                if (cache != null)
                {
                    result((T)cache.unityObject);
                }
                else if (entry.summonType == SummonType.Resources)
                {
                    var asset = Resources.Load<T>(entry.assetPath);
                    result(asset);
                    cache = new Cache { id = entry.id, unityObject = asset };
                    caches.Add(cache);
                }
                else if (entry.summonType == SummonType.ResourcesAsync)
                {
                    var work = works.Find(w => w.entry.id == entry.id);
                    if (work == null)
                    {
                        work = new Work { entry = entry, results = new List<System.Action<Object>>() };
                        work.startLoad = () => { work.resourceRequest = Resources.LoadAsync<T>(work.entry.assetPath); };
                        works.Add(work);
                    }
                    work.results.Add((unityObject) => { result((T)unityObject); });
                }
            }
            else
            {
                throw new UnityException("登録されていないアセットを呼び出そうとしています");
            }
        }

        public static void CacheClear(int id)
        {
            var entry = entries.Find(e => e.id == id);
            CacheClear(entry);
        }

        public static void CacheClear(string assetPath)
        {
            var entry = entries.Find(e => e.assetPath == assetPath);
            CacheClear(entry);
        }

        static void CacheClear(Entry entry)
        {
            caches.RemoveAll(c => c.id == entry.id);
        }

        public static void CacheClean()
        {
            caches.Clear();
        }

        public static void Cancel(int id)
        {
            var entry = entries.Find(e => e.id == id);
            Cancel(entry);
        }

        public static void Cancel(string assetPath)
        {
            var entry = entries.Find(e => e.assetPath == assetPath);
            Cancel(entry);
        }

        static void Cancel(Entry entry)
        {
            var works = Summon.works.FindAll(c => c.entry.id == entry.id && !c.isCancel);
            foreach (var work in works)
            {
                work.isCancel = true;
            }
        }

        static void Cancel()
        {
            var works = Summon.works.FindAll(c => !c.isCancel);
            foreach (var work in works)
            {
                work.isCancel = true;
            }
        }

        class Cache
        {
            public int id;
            public UnityEngine.Object unityObject;
        }

        class Entry
        {
            public int id;
            public string assetPath;
            public SummonType summonType;
        }

        class Work
        {
            public Entry entry;
            public bool isCancel;
            public bool isLoading;
            public List<System.Action<Object>> results;
            public ResourceRequest resourceRequest;
            public System.Action startLoad;
        }

        class SummonBehaviour : MonoBehaviour
        {
            static SummonBehaviour instance;

            public static SummonBehaviour GetInstance()
            {
                if (instance == null)
                {
                    var g = new GameObject("SummonBehaviour");
                    instance = g.AddComponent<SummonBehaviour>();
                    DontDestroyOnLoad(g);
                }
                return instance;
            }

            void LateUpdate()
            {
                var loadingCount = 0;
                for (int i = works.Count - 1; i >= 0; i--)
                {
                    Work work = Summon.works[i];
                    if (work.isLoading)
                    {
                        loadingCount++;
                    }
                    if (MaxLoadingCount <= loadingCount)
                    {
                        break;
                    }
                }
                for (int i = works.Count - 1; i >= 0; i--)
                {
                    Work work = Summon.works[i];
                    if (work.isLoading)
                    {
                        if (work.isCancel)
                        {
                            works.RemoveAt(i);
                        }
                        else if (work.resourceRequest.isDone && work.resourceRequest.asset != null)
                        {
                            var cache = new Cache { id = work.entry.id, unityObject = work.resourceRequest.asset };
                            caches.Add(cache);
                            foreach (var result in work.results)
                            {
                                result(cache.unityObject);
                            }
                            loadingCount--;
                            works.RemoveAt(i);
                        }
                    }

                }
                for (int i = works.Count - 1; i >= 0; i--)
                {
                    if (MaxLoadingCount <= loadingCount)
                    {
                        break;
                    }
                    Work work = Summon.works[i];
                    if (!work.isLoading)
                    {
                        work.isLoading = true;
                        work.startLoad();
                        loadingCount++;
                    }
                }
            }
        }

    }

    // TODO: AssetBundle対応
    // TODO: AssetBundle.Dependencies対応
    // TODO: バイナリ対応
    // TODO: StreamingAsset対応
    public enum SummonType
    {
        Resources,
        ResourcesAsync,
    }

}