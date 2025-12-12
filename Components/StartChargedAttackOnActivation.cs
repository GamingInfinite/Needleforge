using System.Collections;
using UnityEngine;

namespace Needleforge.Components;

/// <summary>
/// A component which causes a GameObject to tell the first <see cref="NailSlashWithEndEvent"/>
/// among its child objects to attack immediately when the object is activated.
/// Made for the use of <see cref="Attacks.ChargedAttack"/>.
/// </summary>
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
