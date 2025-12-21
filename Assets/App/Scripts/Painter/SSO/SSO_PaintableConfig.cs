using UnityEngine;

[CreateAssetMenu(fileName = "SSO_PaintableConfig", menuName = "SSO/Painter/SSO_PaintableConfig")]
public class SSO_PaintableConfig : ScriptableObject
{
    public int TextureSize = 512;
    public FilterMode FilterMode = FilterMode.Bilinear;
    public TextureWrapMode WrapMode = TextureWrapMode.Clamp;
}