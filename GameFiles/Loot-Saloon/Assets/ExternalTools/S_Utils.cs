using UnityEngine;
using System;
using System.Linq;
using random = System.Random;
using System.Collections.Generic;

public static class S_Utils
{
    public static float RandomFloat(float p_min, float p_max, random p_rand)
    {
        return p_min + ((p_max - p_min) * (float) p_rand.NextDouble());
    }

    public static float RandomFloat(float p_min, float p_max)
    {
        return RandomFloat(p_min, p_max, new random());
    }

    // https://stackoverflow.com/questions/273313/randomize-a-listt
    public static void Shuffle<T>(List<T> p_list)  
    {  
        random rand = new random();
        int n = p_list.Count;  
        
        while (n > 1) {  
            n--;  
            int k = rand.Next(n + 1);
            (p_list[n], p_list[k]) = (p_list[k], p_list[n]);
        }
    }

    public static T Choice<T>(List<T> p_list)
    {
        return p_list[new random().Next(p_list.Count)];
    }

    public static T WeightedChoice<T>((float, T)[] p_values)
    {
        float totalWeight = p_values.Select(tuple => tuple.Item1).Sum();
        float upto = 0f;
        float r = RandomFloat(0, totalWeight);

        foreach (var (weight, choice) in p_values)
        {
            upto += weight;
            if (upto >= r)
                return choice;
        }

        throw new Exception("we shouldn't have gotten here");
    }

    // https://stackoverflow.com/a/47176199
    public static float ToNearestMultiple(float p_value, float p_multipleOf) 
    {
        return (float) Math.Round((decimal) p_value / (decimal) p_multipleOf, MidpointRounding.AwayFromZero) * p_multipleOf;
    }

    public static void ScrollIndex(ref int p_current, int p_listCount, int p_amount)
    {
        int maxIndex = p_listCount - 1;
        p_current += p_amount;

        if (p_current > maxIndex)
            p_current -= p_listCount;
        else if (p_current < 0)
            p_current += p_listCount;
    }

    public static Camera CameraOrMain(Camera p_cam)
    {
        return p_cam != null ? p_cam : Camera.main;
    }

    public static void CopyToClipboard(string p_text)
    {
        GUIUtility.systemCopyBuffer = p_text;
    }
}
