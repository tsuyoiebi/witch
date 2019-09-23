using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Witch.Sample
{

    public class SceneButton : MonoBehaviour
    {

        Button button;

        [SerializeField]
        string sceneName;

        // Start is called before the first frame update
        void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        void OnClick()
        {
            Book.Open(new CommonPage(sceneName));
        }
    }

    public class CommonPage : Book.Page
    {
        const string TRANSITION_SCENENAME = "Sample_Transition";
        string sceneName;
        public override string TransitionSceneName => TRANSITION_SCENENAME;
        public override string SceneName { get { return sceneName; } }

        public CommonPage(string sceneName)
        {
            this.sceneName = sceneName;
        }

        public override void AfterInitialize()
        {

        }

        public override void BeforeInitialize()
        {

        }
    }

}