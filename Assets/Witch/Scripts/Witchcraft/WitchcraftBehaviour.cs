using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Witch
{

    public enum CallbackType
    {
        Update,
        LateUpdate,
        FixedUpdate
    }

    public class Magic
    {

    }

    public class Witchcraft
    {

        public bool IsCompleted { get; private set; }
        public bool IsCanceled { get; private set; }
        public bool IsError { get; private set; }
        public System.Exception Exception { get; private set; }
        public Magic Result { get; private set; }
        public bool Executed { get; private set; }

        public System.Action<Magic> OnCompleted { get; private set; }
        public System.Action OnCanceled { get; private set; }
        public System.Action<System.Exception> OnError { get; private set; }

        System.Func<Magic> exec;
        bool async;
        bool finished;
        CallbackType callbackType;

        public static Witchcraft Execute(System.Func<Magic> func, CallbackType callbackType)
        {
            var witchcraft = new Witchcraft();
            witchcraft.exec = func;
            witchcraft.async = false;
            witchcraft.callbackType = callbackType;
            WitchcraftBehaviour.GetInstance().witchcrafts.Add(witchcraft);
            return witchcraft;
        }

        public static Witchcraft ExecuteAsync(System.Func<Magic> func, CallbackType callbackType)
        {
            var witchcraft = new Witchcraft();
            witchcraft.exec = func;
            witchcraft.async = true;
            witchcraft.callbackType = callbackType;
            WitchcraftBehaviour.GetInstance().witchcrafts.Add(witchcraft);
            return witchcraft;
        }

        public static void ExecuteCoroutine(IEnumerator routine)
        {
            WitchcraftBehaviour.GetInstance().StartCoroutine(routine);
        }

        public static void StopCoroutine(IEnumerator routine)
        {
            WitchcraftBehaviour.GetInstance().StopCoroutine(routine);
        }

        public void SetCompleted(System.Action<Magic> action)
        {
            OnCompleted = action;
        }

        public void SetCandeled(System.Action action)
        {
            OnCanceled = action;
        }

        public void SetError(System.Action<System.Exception> action)
        {
            OnError = action;
        }

        public void Cancel()
        {
            if (!IsCanceled)
            {
                IsCanceled = true;
            }
        }

        Witchcraft() { }

        class WitchcraftBehaviour : MonoBehaviour
        {
            static WitchcraftBehaviour instance;

            Task task;
            bool loop = true;

            public static WitchcraftBehaviour GetInstance()
            {
                if (instance == null)
                {
                    // TODO: 関数化する。
                    ThreadPool.GetMinThreads(out int workMin, out int ioMin);
                    ThreadPool.SetMinThreads(workMin * 4, ioMin);
                    var g = new GameObject("WitchcraftBehaviour");
                    instance = g.AddComponent<WitchcraftBehaviour>();
                    instance.task = Task.Run(instance.Loop);
                    DontDestroyOnLoad(g);
                }
                return instance;
            }

            public List<Witchcraft> witchcrafts = new List<Witchcraft>();

            void Start()
            {
                DontDestroyOnLoad(this);
            }

            void Update()
            {
                Exec(witchcrafts.FindAll(w => !w.finished && w.callbackType == CallbackType.Update));
            }

            void FixedUpdate()
            {
                Exec(witchcrafts.FindAll(w => !w.finished && w.callbackType == CallbackType.FixedUpdate));
            }

            void LateUpdate()
            {
                Exec(witchcrafts.FindAll(w => !w.finished && w.callbackType == CallbackType.LateUpdate));
            }

            void Exec(List<Witchcraft> witchcrafts)
            {
                foreach (var witchcraft in witchcrafts)
                {
                    if (witchcraft.IsCompleted)
                    {
                        witchcraft.OnCompleted?.Invoke(witchcraft.Result);
                        witchcraft.finished = true;
                    }
                    else if (witchcraft.IsCanceled)
                    {
                        witchcraft.OnCanceled?.Invoke();
                        witchcraft.finished = true;
                    }
                    else if (witchcraft.IsError)
                    {
                        witchcraft.OnError?.Invoke(witchcraft.Exception);
                        witchcraft.finished = true;
                    }
                }
            }

            void Loop()
            {
                var tasks = new List<WitchTask>();
                while (loop)
                {
                    foreach (var witchcraft in witchcrafts)
                    {
                        if (!witchcraft.Executed)
                        {
                            if (witchcraft.async)
                            {
                                witchcraft.Executed = true;
                                var task = Task.Run(witchcraft.exec);
                                var witchTask = new WitchTask();
                                witchTask.witchcraft = witchcraft;
                                witchTask.task = task;
                                tasks.Add(witchTask);
                            }
                            else
                            {
                                witchcraft.Executed = true;
                                try
                                {
                                    witchcraft.Result = witchcraft.exec();
                                    witchcraft.IsCompleted = true;
                                }
                                catch (System.Exception e)
                                {
                                    witchcraft.Exception = e;
                                    witchcraft.IsError = true;
                                }
                            }
                        }
                    }

                    witchcrafts.RemoveAll(w => w.finished);

                    foreach (var task in tasks)
                    {
                        if (task.task.IsCompleted)
                        {
                            task.witchcraft.Result = task.task.Result;
                            task.witchcraft.IsCompleted = true;
                        }
                        else if (task.task.IsCanceled)
                        {
                            task.witchcraft.IsCanceled = true;
                        }
                        else if (task.task.IsFaulted)
                        {
                            task.witchcraft.Exception = task.task.Exception;
                            task.witchcraft.IsError = true;
                        }
                    }

                    tasks.RemoveAll(task => task.witchcraft.IsCompleted || task.witchcraft.IsCanceled || task.witchcraft.IsError);
                }
            }

            void OnDestroy()
            {
                task = null;
                loop = false;
            }

            class WitchTask
            {
                public Witchcraft witchcraft;
                public Task<Magic> task;
            }
        }
    }

}