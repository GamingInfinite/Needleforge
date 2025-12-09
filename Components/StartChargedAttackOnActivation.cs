using System.Collections;
using UnityEngine;

namespace Needleforge.Components;

public class StartChargedAttackOnActivation : MonoBehaviour
{
    private void OnEnable()
    {
        var firstAttack = transform.GetComponentInChildren<NailSlashWithEndEvent>();

        if (firstAttack)
            firstAttack.StartCoroutine(SlashOneFrameLater(firstAttack));
    }

    private static IEnumerator SlashOneFrameLater(NailSlashWithEndEvent slash)
    {
        yield return null;
        slash.StartSlash();
    }
}
