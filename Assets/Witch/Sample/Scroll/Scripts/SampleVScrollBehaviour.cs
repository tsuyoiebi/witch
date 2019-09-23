using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Witch.Sample
{

    public class SampleVScrollBehaviour : MonoBehaviour
    {
        [SerializeField]
        ScrollRect scrollRect;

        [SerializeField]
        SampleVScrollContentView prefab1;

        [SerializeField]
        SampleVScrollContentView prefab2;

        [SerializeField]
        VScroll vScroll;

        IEnumerator Start()
        {
            vScroll = VScroll.Create(scrollRect);
            vScroll.SetViewMargin(10f);
            vScroll.SetContentMargin(5, 0, 5, 0);
            for (var i = 0; i < 10; i++)
            {
                vScroll.AddContent(prefab1, new SampleVScrollContentInfo());
            }
            for (var i = 0; i < 10; i++)
            {
                vScroll.AddContent(prefab2, new SampleVScrollContentInfo());
            }
            for (var i = 0; i < 10; i++)
            {
                vScroll.AddContent(prefab1, new SampleVScrollContentInfo());
            }
            yield return null;
            // 1frameまつ必要がある。
            // UIの値反映が恐らくUpdate後になるのが理由？
            // TODO: またなくてもいけるようにする
            vScroll.Apply();
        }

    }

}