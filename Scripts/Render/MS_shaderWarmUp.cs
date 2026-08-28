using UnityEngine;

/// <summary>
/// スタート時に指定ShaderCollections内のシェーダーをすべてコンパイルする
/// </summary>
/// 

public class MS_shaderWarmUp : MonoBehaviour
{

    [SerializeField] ShaderVariantCollection ShaderVariantCollection;
    void Start()
    {
        ShaderVariantCollection.WarmUp();
    }
}
