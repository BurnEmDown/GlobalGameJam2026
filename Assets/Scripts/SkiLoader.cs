using UnityCoreKit.Runtime.Bootstrap;
using UnityEngine.SceneManagement;

public class SkiLoader : Loader
{
    public override void OnInitComplete()
    {
        SceneManager.LoadScene("GameScene");
    }
}