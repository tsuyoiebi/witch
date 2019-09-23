using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Witch
{

    public class Clock
    {
        System.DateTime dateTime;
        int frame;

        public Clock()
        {
            // 初回生成時は実行時の前フレームの時間分減らして補正する。
            // フレームに入ってからの誤差の補正はこのフレームではできないのでしない。
            dateTime = System.DateTime.Now.AddSeconds(-Time.deltaTime);
            frame = Time.frameCount;
            ClockBehaviour.GetInstance().clocks.Add(this);
        }

        public System.DateTime Now()
        {
            Update();
            return new System.DateTime(dateTime.Ticks);
        }

        void Update()
        {
            if (frame < Time.frameCount)
            {
                frame = Time.frameCount;
                dateTime = dateTime.AddSeconds(Time.deltaTime);
            }
        }


        class ClockBehaviour : MonoBehaviour
        {

            static ClockBehaviour instance;

            public static ClockBehaviour GetInstance()
            {
                if (instance == null)
                {
                    var g = new GameObject("ClockBehaviour");
                    instance = g.AddComponent<ClockBehaviour>();
                    DontDestroyOnLoad(g);
                }
                return instance;
            }

            public List<Clock> clocks = new List<Clock>();

            void Start()
            {
                DontDestroyOnLoad(this);
            }

            void Update()
            {
                foreach (var clock in clocks)
                {
                    clock.Update();
                }
            }
        }

    }

}