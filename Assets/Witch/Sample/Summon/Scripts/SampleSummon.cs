using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Witch.Sample
{

    public class SampleSummon : MonoBehaviour
    {

        List<AssetPath> assetPaths = new List<AssetPath>
        {
            new AssetPath{ path = "Summon/sample1" , type = SummonType.Resources },
            new AssetPath{ path = "Summon/sample2" , type = SummonType.Resources },
            new AssetPath{ path = "Summon/sample3" , type = SummonType.ResourcesAsync },
            new AssetPath{ path = "Summon/sample4" , type = SummonType.Resources },
            new AssetPath{ path = "Summon/sample5" , type = SummonType.Resources },
            new AssetPath{ path = "Summon/sample1" , type = SummonType.Resources },
            new AssetPath{ path = "Summon/sample2" , type = SummonType.ResourcesAsync },
            new AssetPath{ path = "Summon/sample3" , type = SummonType.Resources },
            new AssetPath{ path = "Summon/sample4" , type = SummonType.ResourcesAsync },
            new AssetPath{ path = "Summon/sample5" , type = SummonType.ResourcesAsync },
            new AssetPath{ path = "Summon/sample1" , type = SummonType.ResourcesAsync },
            new AssetPath{ path = "Summon/sample2" , type = SummonType.ResourcesAsync },
            new AssetPath{ path = "Summon/sample3" , type = SummonType.ResourcesAsync },
            new AssetPath{ path = "Summon/sample4" , type = SummonType.ResourcesAsync },
            new AssetPath{ path = "Summon/sample5" , type = SummonType.ResourcesAsync },
        };

        [SerializeField]
        Image prefab;
        [SerializeField]
        ScrollRect scrollRect;

        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0; i < assetPaths.Count; i++)
            {
                assetPaths[i].id = i + 1;
                Summon.Contract(assetPaths[i].path, assetPaths[i].type);
            }

            foreach (var assetPath in assetPaths)
            {
                var instantiate = Instantiate(prefab,scrollRect.content.transform,false);
                Summon.Call<Sprite>(assetPath.id, (sprite) => instantiate.sprite = sprite);
            }
        }
    }


    public class AssetPath
    {
        public int id;
        public string path;
        public SummonType type;
    }
}