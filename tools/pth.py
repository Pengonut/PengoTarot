import os

output_dir = r"D:\[Tool] Godot\STS2Mod\PengoTarot\src\Core\Models\Enchantments"
os.makedirs(output_dir, exist_ok=True)

# 行星名 -> 允许附魔的卡牌类型
planets = {
    "Mercury": "Power",
    "Venus":   "Power",
    "Earth":   "Power",
    "Mars":    "Power",
    "Jupiter": "Attack",
    "Saturn":  "Attack",
    "Uranus":  "Attack",
    "Neptune": "Attack",
    "Pluto":   "Skill",
    "X":       "Skill",
    "Ceres":   "Skill",
    "Eris":    "Skill",
}

template = """\
#nullable enable
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class Planet{className}Enchantment : EnchantmentModel
{{
    public override bool HasExtraCardText => true;

    public override bool CanEnchantCardType(CardType cardType) => cardType == CardType.{cardType};
}}
"""

for planet_name, card_type in planets.items():
    class_name = planet_name
    file_path = os.path.join(output_dir, f"Planet{class_name}Enchantment.cs")
    with open(file_path, "w", encoding="utf-8") as f:
        f.write(template.format(
            className=class_name,
            cardType=card_type
        ))
    print(f"Generated {file_path}")

print("Done. All planet enchantments created.")