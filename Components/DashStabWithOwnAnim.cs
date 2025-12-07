using System;
using System.Collections.Generic;
using System.Text;

namespace Needleforge.Components;

public class DashStabWithOwnAnim : DashStabNailAttack {

	public string animName = "";

	public override void OnSlashStarting() {
		base.OnSlashStarting();
		animator.PlayFromFrame(animName, 0);
	}

}
