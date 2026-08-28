using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using System.Linq;
/// <summary>
/// animatorを探してPlayableDirectorのトラックにバインドする
/// playableDirector：対象PlayableDirector
/// sourceObjectName：探すGameObjectの名前
/// targetTrackName：バインドするTrack名
/// </summary>
public class MS_BindPlayableDirectorTrack : MonoBehaviour
{
    [SerializeField] PlayableDirector playableDirector;
    [SerializeField] string sourceObjectName;
    [SerializeField] string targetTrackName;

    GameObject foundGameObject;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        foundGameObject = GameObject.Find(sourceObjectName);
        if (foundGameObject)
        {
            animator = foundGameObject.GetComponent<Animator>();
            var binding = playableDirector.playableAsset.outputs.First(c => c.streamName == targetTrackName);
            playableDirector.SetGenericBinding(binding.sourceObject, animator);
        }
    }
}
