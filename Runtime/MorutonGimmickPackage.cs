using UdonSharp;
using UnityEngine;

namespace MorutonLaboratry.Script
{

    public abstract class MorutonGimmickPackage : UdonSharpBehaviour
    {
        //もるらぼのギミックに共通のEditor処理を書き込むための継承用Script
        [SerializeField] private Texture2D dummyImage;
    }
}
