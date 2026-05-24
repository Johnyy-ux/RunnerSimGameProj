using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThemeTestButton : MonoBehaviour
{
    public ThemeData themeToApply;

    public void ApplyThisTheme()
    {
        if (themeToApply == null) return;

        ThemeManager.Instance.SelectTheme(themeToApply);
        Debug.Log($"Тема применена: {themeToApply.themeName}");
    }
}
