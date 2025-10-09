using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace CCE.Coroutines
{
    public static class CoroutineUtils
    {
        public static IEnumerator WaitForSecondsAndThen(float seconds, [NotNull] Action action)
        {
            yield return new WaitForSeconds(seconds);
            
            action.Invoke();
        }
    }
}
