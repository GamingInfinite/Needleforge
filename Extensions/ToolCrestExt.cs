using System.Linq;
using UnityEngine;

namespace Needleforge.Extensions
{
    public static class ToolCrestExt
    {
        public static void AddToolSlot(this ToolCrest crest, ToolItemType color, AttackToolBinding binding, Vector2 position, bool isLocked)
        {
            var list = crest.slots.ToList();
            ToolCrest.SlotInfo newSlot = new()
            {
                AttackBinding = binding,
                Type = color,
                Position = position,
                IsLocked = isLocked,
                NavUpIndex = -1,
                NavUpFallbackIndex = -1,
                NavRightIndex = -1,
                NavRightFallbackIndex = -1,
                NavLeftIndex = -1,
                NavLeftFallbackIndex = -1,
                NavDownIndex = -1,
                NavDownFallbackIndex = -1,
            };
            list.Add(newSlot);
        }

        public static void AddSkillSlot(this ToolCrest crest, AttackToolBinding binding, Vector2 position, bool isLocked)
        {
            crest.AddToolSlot(ToolItemType.Skill, binding, position, isLocked);
        }

        public static void AddRedSlot(this ToolCrest crest, AttackToolBinding binding, Vector2 position, bool isLocked)
        {
            crest.AddToolSlot(ToolItemType.Red, binding, position, isLocked);
        }

        public static void AddYellowSlot(this ToolCrest crest, Vector2 position, bool isLocked)
        {
            crest.AddToolSlot(ToolItemType.Yellow, AttackToolBinding.Neutral, position, isLocked);
        }

        public static void AddBlueSlot(this ToolCrest crest, Vector2 position, bool isLocked)
        {
            crest.AddToolSlot(ToolItemType.Blue, AttackToolBinding.Neutral, position, isLocked);
        }
    }
}
