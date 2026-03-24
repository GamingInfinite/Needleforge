using Needleforge.Attacks;
using System.Collections;
using UnityEngine;

namespace Needleforge.Components;

/// <summary>
/// A component which, when enabled, causes the linked <see cref="ChargedAttack"/>
/// to trigger its first step when it's GameObject is activated;
/// or if requested, triggers all of its steps in sequence.
/// </summary>
public class ChargedAttackAutoTrigger : MonoBehaviour
{
    /// <summary>
    /// The attack this component is attached to and controls.
    /// </summary>
    public ChargedAttack? attack;

    private void OnEnable()
    {
        if (attack != null && attack.PlayOnActivation && !attack.Steps.IsNullOrEmpty())
            StartCoroutine(SlashInSequence());
    }

    private void OnDisable()=> StopAllCoroutines();

    private IEnumerator SlashInSequence() {
        yield return null;
        foreach(var step in attack!.Steps)
        {
            step.StartAttack();
            yield return null;

            var anim = step.GameObject!.GetComponent<tk2dSpriteAnimator>();
            while (anim.Playing && anim.CurrentClip != null)
                yield return null;

            if (!attack.PlayStepsInSequence)
                break;
        }
        if (attack.DisableAfterLastStep)
            attack.GameObject!.SetActive(false);
    }
}
