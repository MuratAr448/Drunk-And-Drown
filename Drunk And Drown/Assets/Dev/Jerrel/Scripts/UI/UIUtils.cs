using UnityEngine;

public static class UIUtils
{
    public static string FormatNumber(float number)
    {
        float absValue = Mathf.Abs(number);
        string sign = number < 0 ? "-" : "";

        if (absValue >= 1000000f)
        {
            float val = absValue / 1000000f;
            return $"{sign}{val.ToString("0.##")}m";
        }
        if (absValue >= 1000f)
        {
            float val = absValue / 1000f;
            return $"{sign}{val.ToString("0.##")}k";
        }

        return $"{sign}{absValue.ToString("0.##")}";
    }

    public static string FormatNumber(int number)
    {
        return FormatNumber((float)number);
    }
}
