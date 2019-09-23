using UnityEngine;
using System.Collections;

namespace Witch
{

    public abstract class ScrollContentView : MonoBehaviour
    {
        public abstract void Activate(ScrollContentInfo contentInfo);
        public abstract void Deactivate(ScrollContentInfo contentInfo);
    }

}