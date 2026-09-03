using AirlockClient.Managers;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using System.Collections;

namespace AirlockClient.Handlers
{
    public static class CoroutineHandler
    {
        public static void Start(IEnumerator routine)
        {
            AirlockClientManager.Instance.StartCoroutine(CollectionExtensions.WrapToIl2Cpp(routine));
        }

        public static void Stop(IEnumerator routine)
        {
            AirlockClientManager.Instance.StopCoroutine(CollectionExtensions.WrapToIl2Cpp(routine));
        }
    }
}
