using UnityEngine;
using System;
using System.Collections.Generic;

public static class S_VariablesChecker
{
    /// <summary>
    /// A list of checks that will check if the given variable triggers verifications
    /// </summary>
    static List<Func<string, (object variable, string variableName), bool>> _defaultVariableChecks = new()
    {
        IsVariableNullCheck,
    };

    #region -= Checks =-

    public static bool IsVariableNullCheck(string p_gameObjectName, (object variable, string variableName) p_variableToCheck)
    {
        if (p_variableToCheck.variable == null || (p_variableToCheck.variable is UnityEngine.Object unityObject && unityObject == null))
        {
            Debug.LogError(
                $"ERROR ! The variable '{p_variableToCheck.variableName}' in '{p_gameObjectName}' GameObject is null," +
                $" please set it through the Unity inspector, or directly at the variable initialization."
            );
            return true;
        }

        return false;
    }

    public static bool IsNumberVariableUnderZeroCheck(string p_gameObjectName, (object variable, string variableName) p_variableToCheck)
    {
        if ((float)p_variableToCheck.variable < 0)
        {
            Debug.LogError(
                $"ERROR ! The variable '{p_variableToCheck.variableName}' in '{p_gameObjectName}' GameObject is under zero."
            );
            return true;
        }

        return false;
    }

    public static bool IsNumberVariableEqualZeroCheck(string p_gameObjectName, (object variable, string variableName) p_variableToCheck)
    {
        if ((float)p_variableToCheck.variable == 0)
        {
            Debug.LogError(
                $"ERROR ! The variable '{p_variableToCheck.variableName}' in '{p_gameObjectName}' GameObject equals zero."
            );
            return true;
        }

        return false;
    }

    // You can add more checks here.
    #endregion

    /// <summary>
    /// Returns if the given variables are correcly setted (for now it's only checking if it's null).
    ///  
    /// <para> ------------------- </para>
    /// 
    /// <para> <b> WARNING : </b> The variable passed are copied, that means that if you past a very big variable this method call will cause lag due to memory management. </para>
    /// 
    /// <para> Sadly we can't pass your variables as references because the keyword 'params' before it is incompatible. </para>
    /// 
    /// <para> However you can use instead the <see cref = "IsVariableCorrectlySetted"/> method. </para>
    /// 
    /// <para> ------------------- </para>
    /// 
    /// <para> <b> Code utilization example n°1 : </b> <code>
    /// 
    /// void Start()
    /// {
    ///     if (!S_VariablesChecker.AreVariablesCorrectlySetted(gameObject.name, null,
    ///         (_exampleVariable1, nameof(_exampleVariable1)),
    ///         (_exampleVariable2, nameof(_exampleVariable2)),
    ///         (_exampleVariable3, nameof(_exampleVariable3))
    ///     )) return;
    /// }
    /// </code> </para> 
    /// 
    /// <para> <b> Code utilization example n°2 : </b> </para>
    /// <para> <b> NOTE : </b>  Due to a conflict with the XML system and the example code below, the " IN angle brackets, OUT angle brackets " will be replaced by " / \ ". <code>
    /// 
    /// List/Func/string, (object variable, string variableName), bool\\ variableChecks = new()
    /// {
    ///     S_VariablesChecker.IsVariableNullCheck,
    ///     IsVariableTestCheck
    ///     // You can add more if wanted
    /// }
    /// 
    /// void Start()
    /// {
    ///     S_VariablesChecker.AreVariablesCorrectlySetted(gameObject.name, variableChecks,
    ///         (_exampleVariable1, nameof(_exampleVariable1)),
    ///         (_exampleVariable2, nameof(_exampleVariable2)),
    ///         (_exampleVariable3, nameof(_exampleVariable3))
    ///     );
    /// }
    /// 
    /// static bool IsVariableTestCheck(string p_gameObjectName, (object variable, string variableName) p_variableToCheck)
    /// {
    ///     Debug.Log("I'm always returning 'false' because why not.");
    /// 
    ///     return false;
    /// }
    /// </code> </para> </summary>
    /// <param name = "p_variableOwnerName"> The variable owner name, can be for example the GameObject name, or the Class name. </param>
    /// <param name = "p_customVariableChecks"> A list of functions (methods that returns something), 
    /// that list of functions correspond to all the checks that the given variable will pass throw, to use default ones just pass 'null'.
    /// You can see the default ones in <see cref = "_defaultVariableChecks"/> </param>
    /// <param name = "p_variablesToCheck"> A params of tuple (an object and a string) </param>
    public static bool AreVariablesCorrectlySetted(string p_variableOwnerName,
        List<Func<string, (object variable, string variableName), bool>> p_customVariableChecks = null,
        params (object variable, string variableName)[] p_variablesToCheck)
    {
        bool areVariablesCorreclySetted = true;

        List<Func<string, (object variable, string variableName), bool>> variableChecks = _defaultVariableChecks;

        // Checking if the user gave customs variable checks
        if (p_customVariableChecks != null)
        {
            variableChecks = p_customVariableChecks;
        }

        // Looping throw each given variables
        for (int i = 0; i < p_variablesToCheck.Length; i++)
        {
            // Doing every checks
            for (int j = 0; j < variableChecks.Count; j++)
            {
                if (variableChecks[j](p_variableOwnerName, p_variablesToCheck[i]))
                {
                    areVariablesCorreclySetted = false;
                }
            }
        }

        return areVariablesCorreclySetted;
    }

    /// <summary>
    /// <b> WARNINIG ! </b> Not fully usable for now.
    /// </summary>
    /// <param name = "p_gameObjectName">  </param>
    /// <param name = "p_variable">  </param>
    /// <param name = "p_variableName">  </param>
    /// <param name = "p_customVariableChecks">  </param>
    [Obsolete]
    public static bool IsVariableCorrectlySetted(string p_gameObjectName, ref object p_variable, string p_variableName,
        List<Func<string, (object variable, string variableName), bool>> p_customVariableChecks)
    {
        bool areVariablesCorreclySetted = true;

        List<Func<string, (object variable, string variableName), bool>> variableChecks = _defaultVariableChecks;

        // Checking if the user gave customs variable checks
        if (p_customVariableChecks != null)
        {
            variableChecks = p_customVariableChecks;
        }

        for (int i = 0; i < variableChecks.Count; i++)
        {
            if (variableChecks[i](p_gameObjectName, (p_variable, p_variableName)))
            {
                areVariablesCorreclySetted = false;
            }
        }

        return areVariablesCorreclySetted;
    }
}