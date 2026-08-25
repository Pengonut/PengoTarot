// PengoTarot/TarKoreanLocHelper.cs
#nullable enable
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization;

namespace PengoTarot
{
    public static class TarKoreanLocHelper
    {
        public static void InjectAll()
        {
            var loc = LocManager.Instance;

            var cardLibraryTable = loc.GetTable("card_library");
            cardLibraryTable.MergeWith(new Dictionary<string, string>
            {
                { "POOL_TAROT_TIP", "타로 카드.\n\n[gold]특별 감사[/gold]: \n번역자 SeungjunKim_8224" },
                { "POOL_PLANET_TIP", "행성 카드." }
            });

            var cardsTable = loc.GetTable("cards");
            cardsTable.MergeWith(new Dictionary<string, string>
            {
                // 0 The Fool
                { "TAR_FOOL_UPRIGHT.title", "0 - 광대" },
                { "TAR_FOOL_UPRIGHT.description", "카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]광대 - 정방향[/purple]" },
                { "TAR_FOOL_REVERSED.title", "0 - 광대" },
                { "TAR_FOOL_REVERSED.description", "\n비용이 [blue]X[/blue]가 아닌 카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]광대 - 역방향[/purple]" },

                // I The Magician
                { "TAR_MAGICIAN_UPRIGHT.title", "I - 마법사" },
                { "TAR_MAGICIAN_UPRIGHT.description", "[gold]소멸[/gold]이 없는 스킬 또는 공격 카드를 [blue]5[/blue]장 선택해 [gold]인챈트[/gold]\n[purple]마법사 - 정방향[/purple]" },
                { "TAR_MAGICIAN_REVERSED.title", "I - 마법사" },
                { "TAR_MAGICIAN_REVERSED.description", "\n[gold]소멸[/gold]이 없는 스킬 또는 공격 카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]마법사 - 역방향[/purple]" },

                // II The High Priestess
                { "TAR_HIGH_PRIESTESS_UPRIGHT.title", "II - 여사제" },
                { "TAR_HIGH_PRIESTESS_UPRIGHT.description", "[gold]휘발성[/gold]이 없는 카드 [blue]3[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]여사제 - 정방향[/purple]" },
                { "TAR_HIGH_PRIESTESS_REVERSED.title", "II - 여사제" },
                { "TAR_HIGH_PRIESTESS_REVERSED.description", "\n[gold]휘발성[/gold]이 없는 카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]여사제 - 역방향[/purple]" },

                // III The Empress
                { "TAR_EMPRESS_UPRIGHT.title", "III - 여황제" },
                { "TAR_EMPRESS_UPRIGHT.description", "카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]여황제 - 정방향[/purple]" },
                { "TAR_EMPRESS_REVERSED.title", "III - 여황제" },
                { "TAR_EMPRESS_REVERSED.description", "\n카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]여황제 - 역방향[/purple]" },

                // IV The Emperor
                { "TAR_EMPEROR_UPRIGHT.title", "IV - 황제" },
                { "TAR_EMPEROR_UPRIGHT.description", "카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]황제 - 정방향[/purple]" },
                { "TAR_EMPEROR_REVERSED.title", "IV - 황제" },
                { "TAR_EMPEROR_REVERSED.description", "\n카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]황제 - 역방향[/purple]" },

                // V The Hierophant
                { "TAR_HIEROPHANT_UPRIGHT.title", "V - 교황" },
                { "TAR_HIEROPHANT_UPRIGHT.description", "카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]교황 - 정방향[/purple]" },
                { "TAR_HIEROPHANT_REVERSED.title", "V - 교황" },
                { "TAR_HIEROPHANT_REVERSED.description", "\n카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]교황 - 역방향[/purple]" },

                // VI The Lovers
                { "TAR_LOVERS_UPRIGHT.title", "VI - 연인들" },
                { "TAR_LOVERS_UPRIGHT.description", "카드 [blue]2[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]연인들 - 정방향[/purple]" },
                { "TAR_LOVERS_REVERSED.title", "VI - 연인들" },
                { "TAR_LOVERS_REVERSED.description", "\n비용이 [blue]X[/blue]가 아닌 카드 [blue]2[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]연인들 - 역방향[/purple]" },

                // VII The Chariot
                { "TAR_CHARIOT_UPRIGHT.title", "VII - 전차" },
                { "TAR_CHARIOT_UPRIGHT.description", "비다단히트 공격 카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]전차 - 정방향[/purple]" },
                { "TAR_CHARIOT_REVERSED.title", "VII - 전차" },
                { "TAR_CHARIOT_REVERSED.description", "\n다단히트 공격 카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]전차 - 역방향[/purple]" },

                // VIII Strength
                { "TAR_STRENGTH_UPRIGHT.title", "VIII - 힘" },
                { "TAR_STRENGTH_UPRIGHT.description", "비다단히트 공격 카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]힘 - 정방향[/purple]" },
                { "TAR_STRENGTH_REVERSED.title", "VIII - 힘" },
                { "TAR_STRENGTH_REVERSED.description", "\n다단히트 공격 카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]힘 - 역방향[/purple]" },

                // IX The Hermit
                { "TAR_HERMIT_UPRIGHT.title", "IX - 은둔자" },
                { "TAR_HERMIT_UPRIGHT.description", "스킬 또는 공격 카드 [blue]2[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]은둔자 - 정방향[/purple]" },
                { "TAR_HERMIT_REVERSED.title", "IX - 은둔자" },
                { "TAR_HERMIT_REVERSED.description", "\n스킬 또는 공격 카드 [blue]2[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]은둔자 - 역방향[/purple]" },

                // X Wheel of Fortune
                { "TAR_WHEEL_OF_FORTUNE_UPRIGHT.title", "X - 운명의 수레바퀴" },
                { "TAR_WHEEL_OF_FORTUNE_UPRIGHT.description", "[red]체력[/red]을 [blue]11[/blue] 잃습니다. 고대가 아닌 무작위 유물 [blue]1[/blue]개를 [gold]복제[/gold]합니다." },
                { "TAR_WHEEL_OF_FORTUNE_REVERSED.title", "X - 운명의 수레바퀴" },
                { "TAR_WHEEL_OF_FORTUNE_REVERSED.description", "\n고대가 아닌 무작위 유물 [blue]2[/blue]개를 [red]파괴[/red]한 뒤, 고대가 아닌 무작위 유물 [blue]1[/blue]개를 [blue]3[/blue]번 [gold]복제[/gold]합니다." },

                // XI Justice
                { "TAR_JUSTICE_UPRIGHT.title", "XI - 정의" },
                { "TAR_JUSTICE_UPRIGHT.description", "광역 공격이 아닌 [gold]소멸[/gold]이 없는 공격 카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]정의 - 정방향[/purple]" },
                { "TAR_JUSTICE_REVERSED.title", "XI - 정의" },
                { "TAR_JUSTICE_REVERSED.description", "\n광역 공격이 아닌 [gold]소멸[/gold]이 없는 공격 카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]정의 - 역방향[/purple]" },

                // XII The Hanged Man
                { "TAR_HANGED_MAN_UPRIGHT.title", "XII - 매달린 남자" },
                { "TAR_HANGED_MAN_UPRIGHT.description", "[gold]소멸[/gold]이 있는 카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]매달린 남자 - 정방향[/purple]" },
                { "TAR_HANGED_MAN_REVERSED.title", "XII - 매달린 남자" },
                { "TAR_HANGED_MAN_REVERSED.description", "\n[gold]소멸[/gold]이 있는 카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]매달린 남자 - 역방향[/purple]" },

                // XIII Death
                { "TAR_DEATH_UPRIGHT.title", "XIII - 죽음" },
                { "TAR_DEATH_UPRIGHT.description", "카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]죽음 - 정방향[/purple]" },
                { "TAR_DEATH_REVERSED.title", "XIII - 죽음" },
                { "TAR_DEATH_REVERSED.description", "\n카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]죽음 - 역방향[/purple]" },

                // XIV Temperance
                { "TAR_TEMPERANCE_UPRIGHT.title", "XIV - 절제" },
                { "TAR_TEMPERANCE_UPRIGHT.description", "카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]절제 - 정방향[/purple]" },
                { "TAR_TEMPERANCE_REVERSED.title", "XIV - 절제" },
                { "TAR_TEMPERANCE_REVERSED.description", "\n카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]절제 - 역방향[/purple]" },

                // XV The Devil
                { "TAR_DEVIL_UPRIGHT.title", "XV - 악마" },
                { "TAR_DEVIL_UPRIGHT.description", "카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]악마 - 정방향[/purple]" },
                { "TAR_DEVIL_REVERSED.title", "XV - 악마" },
                { "TAR_DEVIL_REVERSED.description", "\n카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]악마 - 역방향[/purple]" },

                // XVI The Tower
                { "TAR_TOWER_UPRIGHT.title", "XVI - 탑" },
                { "TAR_TOWER_UPRIGHT.description", "덱에서 모든 기본, 일반, 고급 카드를 [red]제거[/red]합니다. 제거한 카드 [blue]1[/blue]장당 [blue]45[/blue] [gold]골드[/gold]를 얻습니다." },
                { "TAR_TOWER_REVERSED.title", "XVI - 탑" },
                { "TAR_TOWER_REVERSED.description", "\n덱에서 모든 기본 및 일반 카드를 [red]제거[/red]합니다. 제거한 카드 [blue]1[/blue]장당 [blue]15[/blue] [gold]골드[/gold]를 얻습니다." },

                // XVII The Star
                { "TAR_STAR_UPRIGHT.title", "XVII - 별" },
                { "TAR_STAR_UPRIGHT.description", "카드 [blue]3[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]별 - 정방향[/purple]" },
                { "TAR_STAR_REVERSED.title", "XVII - 별" },
                { "TAR_STAR_REVERSED.description", "\n카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]별 - 역방향[/purple]" },

                // XVIII The Moon
                { "TAR_MOON_UPRIGHT.title", "XVIII - 달" },
                { "TAR_MOON_UPRIGHT.description", "스킬 또는 공격 카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]달 - 정방향[/purple]" },
                { "TAR_MOON_REVERSED.title", "XVIII - 달" },
                { "TAR_MOON_REVERSED.description", "\n스킬 또는 공격 카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]달 - 역방향[/purple]" },

                // XIX The Sun
                { "TAR_SUN_UPRIGHT.title", "XIX - 태양" },
                { "TAR_SUN_UPRIGHT.description", "카드 [blue]3[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]태양 - 정방향[/purple]" },
                { "TAR_SUN_REVERSED.title", "XIX - 태양" },
                { "TAR_SUN_REVERSED.description", "\n카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]태양 - 역방향[/purple]" },

                // XX Judgement
                { "TAR_JUDGEMENT_UPRIGHT.title", "XX - 심판" },
                { "TAR_JUDGEMENT_UPRIGHT.description", "카드 [blue]2[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]심판 - 정방향[/purple]" },
                { "TAR_JUDGEMENT_REVERSED.title", "XX - 심판" },
                { "TAR_JUDGEMENT_REVERSED.description", "\n최대 [blue]21[/blue]장의 카드를 선택해 [gold]인챈트[/gold]\n[purple]심판 - 역방향[/purple]" },

                // XXI The World
                { "TAR_WORLD_UPRIGHT.title", "XXI - 세계" },
                { "TAR_WORLD_UPRIGHT.description", "카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]세계 - 정방향[/purple]" },
                { "TAR_WORLD_REVERSED.title", "XXI - 세계" },
                { "TAR_WORLD_REVERSED.description", "\n카드 [blue]1[/blue]장을 선택해 [gold]인챈트[/gold]\n[purple]세계 - 역방향[/purple]" },

                // SUB Negative - The Devil (Ironclad)
                { "TAR_DEVIL_UPRIGHT_SUB.title", "네거티브 - 악마" },
                { "TAR_DEVIL_UPRIGHT_SUB.description", "카드 [blue]3[/blue]장을 선택해 같은 희귀도의 아이언클래드 카드로 [gold]변형[/gold]하고, [gold]인챈트[/gold]\n[purple]네거티브 - 악마 - 정방향[/purple]" },
                { "TAR_DEVIL_REVERSED_SUB.title", "네거티브 - 악마" },
                { "TAR_DEVIL_REVERSED_SUB.description", "\n카드 [blue]3[/blue]장을 선택해 같은 희귀도의 아이언클래드 카드로 [gold]변형[/gold]하고, [gold]인챈트[/gold]\n[purple]네거티브 - 악마 - 역방향[/purple]" },

                // SUB Negative - The Moon (Silent)
                { "TAR_MOON_UPRIGHT_SUB.title", "네거티브 - 달" },
                { "TAR_MOON_UPRIGHT_SUB.description", "카드 [blue]3[/blue]장을 선택해 같은 희귀도의 사일런트 카드로 [gold]변형[/gold]하고, [gold]인챈트[/gold]\n[purple]네거티브 - 달 - 정방향[/purple]" },
                { "TAR_MOON_REVERSED_SUB.title", "네거티브 - 달" },
                { "TAR_MOON_REVERSED_SUB.description", "\n카드 [blue]3[/blue]장을 선택해 같은 희귀도의 사일런트 카드로 [gold]변형[/gold]하고, [gold]인챈트[/gold]\n[purple]네거티브 - 달 - 역방향[/purple]" },

                // SUB Negative - The Star (Regent)
                { "TAR_STAR_UPRIGHT_SUB.title", "네거티브 - 별" },
                { "TAR_STAR_UPRIGHT_SUB.description", "카드 [blue]3[/blue]장을 선택해 같은 희귀도의 리전트 카드로 [gold]변형[/gold]하고, [gold]인챈트[/gold]\n[purple]네거티브 - 별 - 정방향[/purple]" },
                { "TAR_STAR_REVERSED_SUB.title", "네거티브 - 별" },
                { "TAR_STAR_REVERSED_SUB.description", "\n카드 [blue]3[/blue]장을 선택해 같은 희귀도의 리전트 카드로 [gold]변형[/gold]하고, [gold]인챈트[/gold]\n[purple]네거티브 - 별 - 역방향[/purple]" },

                // SUB Negative - The Sun (Necrobinder)
                { "TAR_SUN_UPRIGHT_SUB.title", "네거티브 - 태양" },
                { "TAR_SUN_UPRIGHT_SUB.description", "카드 [blue]3[/blue]장을 선택해 같은 희귀도의 네크로바인더 카드로 [gold]변형[/gold]하고, [gold]인챈트[/gold]\n[purple]네거티브 - 태양 - 정방향[/purple]" },
                { "TAR_SUN_REVERSED_SUB.title", "네거티브 - 태양" },
                { "TAR_SUN_REVERSED_SUB.description", "\n카드 [blue]3[/blue]장을 선택해 같은 희귀도의 네크로바인더 카드로 [gold]변형[/gold]하고, [gold]인챈트[/gold]\n[purple]네거티브 - 태양 - 역방향[/purple]" },

                // SUB Negative - The World (Defect)
                { "TAR_WORLD_UPRIGHT_SUB.title", "네거티브 - 세계" },
                { "TAR_WORLD_UPRIGHT_SUB.description", "카드 [blue]3[/blue]장을 선택해 같은 희귀도의 디펙트 카드로 [gold]변형[/gold]하고, [gold]인챈트[/gold]\n[purple]네거티브 - 세계 - 정방향[/purple]" },
                { "TAR_WORLD_REVERSED_SUB.title", "네거티브 - 세계" },
                { "TAR_WORLD_REVERSED_SUB.description", "\n카드 [blue]3[/blue]장을 선택해 같은 희귀도의 디펙트 카드로 [gold]변형[/gold]하고, [gold]인챈트[/gold]\n[purple]네거티브 - 세계 - 역방향[/purple]" },

                // Planets - Power
                { "PLANET_MERCURY.title", "수성" },
                { "PLANET_MERCURY.description", "파워 카드 [blue]1[/blue]장을 선택해\n[gold]인챈트[/gold]\n[purple]수성[/purple]" },
                { "PLANET_VENUS.title", "금성" },
                { "PLANET_VENUS.description", "파워 카드 [blue]1[/blue]장을 선택해\n[gold]인챈트[/gold]\n[purple]금성[/purple]" },
                { "PLANET_EARTH.title", "지구" },
                { "PLANET_EARTH.description", "파워 카드 [blue]1[/blue]장을 선택해\n[gold]인챈트[/gold]\n[purple]지구[/purple]" },
                { "PLANET_MARS.title", "화성" },
                { "PLANET_MARS.description", "파워 카드 [blue]1[/blue]장을 선택해\n[gold]인챈트[/gold]\n[purple]화성[/purple]" },

                // Planets - Attack
                { "PLANET_JUPITER.title", "목성" },
                { "PLANET_JUPITER.description", "공격 카드 [blue]1[/blue]장을 선택해\n[gold]인챈트[/gold]\n[purple]목성[/purple]" },
                { "PLANET_SATURN.title", "토성" },
                { "PLANET_SATURN.description", "공격 카드 [blue]1[/blue]장을 선택해\n[gold]인챈트[/gold]\n[purple]토성[/purple]" },
                { "PLANET_URANUS.title", "천왕성" },
                { "PLANET_URANUS.description", "공격 카드 [blue]1[/blue]장을 선택해\n[gold]인챈트[/gold]\n[purple]천왕성[/purple]" },
                { "PLANET_NEPTUNE.title", "해왕성" },
                { "PLANET_NEPTUNE.description", "공격 카드 [blue]1[/blue]장을 선택해\n[gold]인챈트[/gold]\n[purple]해왕성[/purple]" },

                // Planets - Skill
                { "PLANET_PLUTO.title", "명왕성" },
                { "PLANET_PLUTO.description", "스킬 카드 [blue]1[/blue]장을 선택해\n[gold]인챈트[/gold]\n[purple]명왕성[/purple]" },
                { "PLANET_X.title", "행성 X" },
                { "PLANET_X.description", "스킬 카드 [blue]1[/blue]장을 선택해\n[gold]인챈트[/gold]\n[purple]행성 X[/purple]" },
                { "PLANET_CERES.title", "세레스" },
                { "PLANET_CERES.description", "스킬 카드 [blue]1[/blue]장을 선택해\n[gold]인챈트[/gold]\n[purple]세레스[/purple]" },
                { "PLANET_ERIS.title", "에리스" },
                { "PLANET_ERIS.description", "스킬 카드 [blue]1[/blue]장을 선택해\n[gold]인챈트[/gold]\n[purple]에리스[/purple]" },
            });

            var enchantmentsTable = loc.GetTable("enchantments");
            enchantmentsTable.MergeWith(new Dictionary<string, string>
            {
                // 0 The Fool
                { "TAR_FOOL_UPRIGHT_ENCHANTMENT.description", "전투마다 이 카드를 처음 사용하면, 이 카드를 [gold]손패[/gold]로 되돌립니다." },
                { "TAR_FOOL_UPRIGHT_ENCHANTMENT.title", "광대" },
                { "TAR_FOOL_UPRIGHT_ENCHANTMENT.extraCardText", "처음 사용하면, 이 카드를 [gold]손패[/gold]로 되돌립니다." },
                { "TAR_FOOL_REVERSED_ENCHANTMENT.description", "전투마다 이 카드를 처음 사용하면, 비용이 {energyPrefix:energyIcons(1)} 증가한 복사본 [blue]1[/blue]장을 [gold]손패[/gold]에 추가합니다." },
                { "TAR_FOOL_REVERSED_ENCHANTMENT.title", "광대" },
                { "TAR_FOOL_REVERSED_ENCHANTMENT.extraCardText", "처음 사용하면, 비용이 {energyPrefix:energyIcons(1)} 증가한 복사본 [blue]1[/blue]장을 손패에 추가합니다." },

                // I The Magician
                { "TAR_MAGICIAN_UPRIGHT_ENCHANTMENT.description", "이 카드는 [gold]소멸[/gold]을 얻습니다." },
                { "TAR_MAGICIAN_UPRIGHT_ENCHANTMENT.title", "마법사" },
                { "TAR_MAGICIAN_UPRIGHT_ENCHANTMENT.extraCardText", "마법사 - 정방향." },
                { "TAR_MAGICIAN_REVERSED_ENCHANTMENT.description", "이 카드는 사용된 후, [gold]뽑을 카드 더미[/gold]의 무작위 위치에 놓입니다." },
                { "TAR_MAGICIAN_REVERSED_ENCHANTMENT.title", "마법사" },
                { "TAR_MAGICIAN_REVERSED_ENCHANTMENT.extraCardText", "[gold]뽑을 카드 더미[/gold]의 무작위 위치에 놓입니다." },

                // II The High Priestess
                { "TAR_HIGH_PRIESTESS_UPRIGHT_ENCHANTMENT.description", "이 카드는 [gold]휘발성[/gold]을 얻습니다." },
                { "TAR_HIGH_PRIESTESS_UPRIGHT_ENCHANTMENT.title", "여사제" },
                { "TAR_HIGH_PRIESTESS_UPRIGHT_ENCHANTMENT.extraCardText", "여사제 - 정방향." },
                { "TAR_HIGH_PRIESTESS_REVERSED_ENCHANTMENT.description", "이 카드는 [gold]보존[/gold]을 얻습니다. 턴 종료 시, 이 카드가 손패에 있으면 손패에서 이 카드 왼쪽에 있는 모든 카드에 [gold]휘발성[/gold]을 부여합니다." },
                { "TAR_HIGH_PRIESTESS_REVERSED_ENCHANTMENT.title", "여사제" },
                { "TAR_HIGH_PRIESTESS_REVERSED_ENCHANTMENT.extraCardText", "턴 종료 시, 이 카드가 손패에 있으면 왼쪽 카드들에 [gold]휘발성[/gold]을 부여합니다." },

                // III The Empress
                { "TAR_EMPRESS_UPRIGHT_ENCHANTMENT.description", "이번 전투에서 이 카드를 [blue]3[/blue]번째 이상 사용할 때, 이 카드는 [gold]재사용[/gold] [blue]1[/blue]을 얻습니다." },
                { "TAR_EMPRESS_UPRIGHT_ENCHANTMENT.title", "여황제" },
                { "TAR_EMPRESS_UPRIGHT_ENCHANTMENT.extraCardText", "{PlayCount}번 사용함." },
                { "TAR_EMPRESS_REVERSED_ENCHANTMENT.description", "비용이 {energyPrefix:energyIcons(1)} 감소합니다. 이 카드는 [gold]버린 카드 더미[/gold]에 한 번 이상 들어가기 전까지 사용할 수 없습니다." },
                { "TAR_EMPRESS_REVERSED_ENCHANTMENT.title", "여황제" },
                { "TAR_EMPRESS_REVERSED_ENCHANTMENT.extraCardText", "버린 카드 더미에 한 번 들어가기 전까지 사용할 수 없습니다." },

                // IV The Emperor
                { "TAR_EMPEROR_UPRIGHT_ENCHANTMENT.description", "이 카드는 [gold]보존[/gold]을 얻습니다." },
                { "TAR_EMPEROR_UPRIGHT_ENCHANTMENT.title", "황제" },
                { "TAR_EMPEROR_UPRIGHT_ENCHANTMENT.extraCardText", "황제 - 정방향." },
                { "TAR_EMPEROR_REVERSED_ENCHANTMENT.description", "턴 종료 시, 이 카드가 손패에 있으면 손패의 무작위 카드 [blue]2[/blue]장에 [gold]보존[/gold]을 부여합니다." },
                { "TAR_EMPEROR_REVERSED_ENCHANTMENT.title", "황제" },
                { "TAR_EMPEROR_REVERSED_ENCHANTMENT.extraCardText", "턴 종료 시, 손패의 무작위 카드 [blue]2[/blue]장에 [gold]보존[/gold]을 부여합니다." },

                // V The Hierophant
                { "TAR_HIEROPHANT_UPRIGHT_ENCHANTMENT.description", "이 카드가 사용된 후, [gold]손패[/gold]의 카드 [blue]1[/blue]장을 [gold]강화[/gold]합니다." },
                { "TAR_HIEROPHANT_UPRIGHT_ENCHANTMENT.title", "교황" },
                { "TAR_HIEROPHANT_UPRIGHT_ENCHANTMENT.extraCardText", "[gold]손패[/gold]의 카드 [blue]1[/blue]장을 [gold]강화[/gold]합니다." },
                { "TAR_HIEROPHANT_REVERSED_ENCHANTMENT.description", "이 카드가 사용된 후, [gold]손패[/gold]의 카드를 원하는 만큼 [red]열화[/red]시킵니다. [red]열화[/red]시킨 카드 [blue]1[/blue]장마다, [gold]뽑을 카드 더미[/gold] 또는 [gold]버린 카드 더미[/gold]의 무작위 카드 [blue]3[/blue]장을 [gold]강화[/gold]합니다." },
                { "TAR_HIEROPHANT_REVERSED_ENCHANTMENT.title", "교황" },
                { "TAR_HIEROPHANT_REVERSED_ENCHANTMENT.extraCardText", "이 카드는 어떤 대가도 아끼지 않습니다." },

                // VI The Lovers
                { "TAR_LOVERS_UPRIGHT_ENCHANTMENT.description", "이 카드가 [gold]손패[/gold]에 들어오면, [purple]연인들[/purple]로 인챈트된 다른 카드를 [gold]뽑을 카드 더미[/gold], [gold]버린 카드 더미[/gold] 또는 [gold]소멸 더미[/gold]에서 찾아 [gold]손패[/gold]에 넣습니다." },
                { "TAR_LOVERS_UPRIGHT_ENCHANTMENT.title", "연인들" },
                { "TAR_LOVERS_UPRIGHT_ENCHANTMENT.extraCardText", "이 카드는 연인들을 찾습니다." },
                { "TAR_LOVERS_REVERSED_ENCHANTMENT.description", "이 카드를 사용한 후, [purple]연인들[/purple]로 인챈트된 다른 카드를 [gold]손패[/gold]에 넣습니다. 그 카드는 이번 턴 동안 비용이 {energyPrefix:energyIcons(1)} 증가합니다." },
                { "TAR_LOVERS_REVERSED_ENCHANTMENT.title", "연인들" },
                { "TAR_LOVERS_REVERSED_ENCHANTMENT.extraCardText", "이 카드는 연인들을 찾습니다...?" },

                // VII The Chariot
                { "TAR_CHARIOT_UPRIGHT_ENCHANTMENT.description", "추가로 [blue]1[/blue] [gold]취약[/gold]을 부여합니다." },
                { "TAR_CHARIOT_UPRIGHT_ENCHANTMENT.title", "전차" },
                { "TAR_CHARIOT_UPRIGHT_ENCHANTMENT.extraCardText", "추가로 [blue]1[/blue] [gold]취약[/gold]을 부여합니다." },
                { "TAR_CHARIOT_REVERSED_ENCHANTMENT.description", "이 카드가 적에게 피해를 줄 때마다, [blue]1[/blue] [gold]취약[/gold]을 추가로 부여합니다.\n자신에게 [blue]1[/blue] [gold]취약[/gold]을 부여합니다." },
                { "TAR_CHARIOT_REVERSED_ENCHANTMENT.title", "전차" },
                { "TAR_CHARIOT_REVERSED_ENCHANTMENT.extraCardText", "타격마다 [blue]1[/blue] [gold]취약[/gold]을 부여합니다.\n자신에게 [blue]1[/blue] [gold]취약[/gold]을 부여합니다." },

                // VIII Strength
                { "TAR_STRENGTH_UPRIGHT_ENCHANTMENT.description", "추가로 [blue]1[/blue] [gold]약화[/gold]를 부여합니다." },
                { "TAR_STRENGTH_UPRIGHT_ENCHANTMENT.title", "힘" },
                { "TAR_STRENGTH_UPRIGHT_ENCHANTMENT.extraCardText", "추가로 [blue]1[/blue] [gold]약화[/gold]를 부여합니다." },
                { "TAR_STRENGTH_REVERSED_ENCHANTMENT.description", "이 카드가 적에게 피해를 줄 때마다, [blue]1[/blue] [gold]약화[/gold]를 추가로 부여합니다.\n자신에게 [blue]1[/blue] [gold]약화[/gold]를 부여합니다." },
                { "TAR_STRENGTH_REVERSED_ENCHANTMENT.title", "힘" },
                { "TAR_STRENGTH_REVERSED_ENCHANTMENT.extraCardText", "타격마다 [blue]1[/blue] [gold]약화[/gold]를 부여합니다.\n자신에게 [blue]1[/blue] [gold]약화[/gold]를 부여합니다." },

                // IX The Hermit
                { "TAR_HERMIT_UPRIGHT_ENCHANTMENT.description", "전투 시작 시, 이 카드를 [gold]소멸[/gold]시킵니다. [blue]2[/blue]번째 턴 시작 시, 이 카드를 [gold]손패[/gold]에 넣습니다." },
                { "TAR_HERMIT_UPRIGHT_ENCHANTMENT.title", "은둔자" },
                { "TAR_HERMIT_UPRIGHT_ENCHANTMENT.extraCardText", "은둔자 - 정방향." },
                { "TAR_HERMIT_REVERSED_ENCHANTMENT.description", "전투 시작 시, 이 카드를 [gold]소멸[/gold]시킵니다. [blue]7[/blue]번째 턴 시작 시, 이 카드를 [gold]손패[/gold]에 넣습니다. 이번 턴 동안 비용이 [blue]0[/blue]이 됩니다." },
                { "TAR_HERMIT_REVERSED_ENCHANTMENT.title", "은둔자" },
                { "TAR_HERMIT_REVERSED_ENCHANTMENT.extraCardText", "은둔자 - 역방향." },

                // XI Justice
                { "TAR_JUSTICE_UPRIGHT_ENCHANTMENT.description", "[gold]소멸[/gold]을 얻습니다. 이 카드가 주는 피해가 [blue]2[/blue]배가 됩니다." },
                { "TAR_JUSTICE_UPRIGHT_ENCHANTMENT.title", "정의" },
                { "TAR_JUSTICE_UPRIGHT_ENCHANTMENT.extraCardText", "정의 - 정방향." },
                { "TAR_JUSTICE_REVERSED_ENCHANTMENT.description", "[gold]소멸[/gold]을 얻습니다. 이 카드로 피해를 줄 때, 준 피해만큼 [gold]방어도[/gold]를 얻습니다." },
                { "TAR_JUSTICE_REVERSED_ENCHANTMENT.title", "정의" },
                { "TAR_JUSTICE_REVERSED_ENCHANTMENT.extraCardText", "준 피해만큼 [gold]방어도[/gold]를 얻습니다." },

                // XII The Hanged Man
                { "TAR_HANGED_MAN_UPRIGHT_ENCHANTMENT.description", "다른 카드가 [gold]소멸[/gold]되려 할 때, 이 카드가 [gold]뽑을 카드 더미[/gold]에 있으면 이 카드를 사용해, 다른 카드가 소멸되지 않게 합니다." },
                { "TAR_HANGED_MAN_UPRIGHT_ENCHANTMENT.title", "매달린 남자" },
                { "TAR_HANGED_MAN_UPRIGHT_ENCHANTMENT.extraCardText", "이 카드는 희생을 갈망합니다." },
                { "TAR_HANGED_MAN_REVERSED_ENCHANTMENT.description", "사용 시, [gold]손패[/gold]의 무작위 카드 [blue]1[/blue]장을 [gold]소멸[/gold]시킵니다. 이 카드는 [gold]버린 카드 더미[/gold]로 갑니다." },
                { "TAR_HANGED_MAN_REVERSED_ENCHANTMENT.title", "매달린 남자" },
                { "TAR_HANGED_MAN_REVERSED_ENCHANTMENT.extraCardText", "이 카드는 희생을 갈망합니다...?" },

                // XIII Death
                { "TAR_DEATH_UPRIGHT_ENCHANTMENT.description", "이 카드는 비용 [blue]0[/blue]으로 사용할 수 있습니다. 이 카드를 사용한 후, 턴을 종료합니다." },
                { "TAR_DEATH_UPRIGHT_ENCHANTMENT.title", "죽음" },
                { "TAR_DEATH_UPRIGHT_ENCHANTMENT.extraCardText", "사용 후 턴을 종료합니다." },
                { "TAR_DEATH_REVERSED_ENCHANTMENT.description", "이 카드는 비용 [blue]0[/blue]으로 사용할 수 있습니다. 이 카드가 [gold]손패[/gold]에 있는 동안 카드를 뽑을 수 없습니다.\n이 카드는 사용 시 카드를 뽑을 수 있습니다." },
                { "TAR_DEATH_REVERSED_ENCHANTMENT.title", "죽음" },
                { "TAR_DEATH_REVERSED_ENCHANTMENT.extraCardText", "죽음이 당신을 지켜봅니다." },

                // XIV Temperance
                { "TAR_TEMPERANCE_UPRIGHT_ENCHANTMENT.description", "전투마다 이 카드를 처음 사용하면, [blue]10[/blue] [gold]골드[/gold]를 얻습니다." },
                { "TAR_TEMPERANCE_UPRIGHT_ENCHANTMENT.title", "절제" },
                { "TAR_TEMPERANCE_UPRIGHT_ENCHANTMENT.extraCardText", "전투마다 처음 사용 시 [blue]10[/blue] [gold]골드[/gold]를 얻습니다." },
                { "TAR_TEMPERANCE_REVERSED_ENCHANTMENT.description", "이 카드가 사용된 후, 이번 턴에 잃은 체력 [blue]1[/blue]마다 전투 종료 시 [blue]5[/blue] [gold]골드[/gold]를 얻습니다." },
                { "TAR_TEMPERANCE_REVERSED_ENCHANTMENT.title", "절제" },
                { "TAR_TEMPERANCE_REVERSED_ENCHANTMENT.extraCardText", "이번 턴에 잃은 체력 [blue]1[/blue]마다 전투 종료 시 [blue]5[/blue] [gold]골드[/gold]를 얻습니다." },

                // XV The Devil
                { "TAR_DEVIL_UPRIGHT_ENCHANTMENT.description", "비용이 {energyPrefix:energyIcons(1)} 감소합니다. 손패에 있을 때, 이 카드를 다른 카드보다 먼저 사용해야 합니다." },
                { "TAR_DEVIL_UPRIGHT_ENCHANTMENT.title", "악마" },
                { "TAR_DEVIL_UPRIGHT_ENCHANTMENT.extraCardText", "*공정한* 거래." },
                { "TAR_DEVIL_REVERSED_ENCHANTMENT.description", "이번 전투에서 체력을 [blue]3[/blue] 잃을 때마다 비용이 {energyPrefix:energyIcons(1)} 감소합니다. 사용하면 초기화됩니다." },
                { "TAR_DEVIL_REVERSED_ENCHANTMENT.title", "악마" },
                { "TAR_DEVIL_REVERSED_ENCHANTMENT.extraCardText", "피로 움직입니다." },

                // XVII The Star
                { "TAR_STAR_UPRIGHT_ENCHANTMENT.description", "사용 시, 현재 [img]res://images/packed/sprite_fonts/star_icon.png[/img] 수만큼 [gold]방어도[/gold]를 얻습니다." },
                { "TAR_STAR_UPRIGHT_ENCHANTMENT.title", "별" },
                { "TAR_STAR_UPRIGHT_ENCHANTMENT.extraCardText", "별빛은 나의 방패가 되고, 우주는 나를 감싼다." },
                { "TAR_STAR_REVERSED_ENCHANTMENT.description", "이 카드의 {energyPrefix:energyIcons(1)} 비용과 [img]res://images/packed/sprite_fonts/star_icon.png[/img] 비용을 서로 바꿉니다." },
                { "TAR_STAR_REVERSED_ENCHANTMENT.title", "별" },
                { "TAR_STAR_REVERSED_ENCHANTMENT.extraCardText", "별이 꺼져도, 나는 운명을 거슬러 걷는다." },

                // XVIII The Moon
                { "TAR_MOON_UPRIGHT_ENCHANTMENT.description", "첫 번째 턴 종료 시 이 카드를 자동으로 사용한 뒤, 이 카드를 다시 [gold]뽑을 카드 더미[/gold]에 놓습니다." },
                { "TAR_MOON_UPRIGHT_ENCHANTMENT.title", "달" },
                { "TAR_MOON_UPRIGHT_ENCHANTMENT.extraCardText", "......" },
                { "TAR_MOON_REVERSED_ENCHANTMENT.description", "이 카드가 버려질 때마다, 이 카드를 [gold]손패[/gold]로 되돌립니다." },
                { "TAR_MOON_REVERSED_ENCHANTMENT.title", "달" },
                { "TAR_MOON_REVERSED_ENCHANTMENT.extraCardText", "......" },

                // XIX The Sun
                { "TAR_SUN_UPRIGHT_ENCHANTMENT.description", "사용 시, 자신에게 걸린 무작위 [gold]디버프[/gold] [blue]1[/blue]개의 중첩을 절반으로 줄입니다." },
                { "TAR_SUN_UPRIGHT_ENCHANTMENT.title", "태양" },
                { "TAR_SUN_UPRIGHT_ENCHANTMENT.extraCardText", "부패가 멀리 머물기를." },
                { "TAR_SUN_REVERSED_ENCHANTMENT.description", "이 카드는 비용 [blue]0[/blue]으로 사용할 수 있습니다. 대신, 이 카드의 원래 에너지 비용의 [blue]6[/blue]배만큼 [gold]종말[/gold]을 얻습니다." },
                { "TAR_SUN_REVERSED_ENCHANTMENT.title", "태양" },
                { "TAR_SUN_REVERSED_ENCHANTMENT.extraCardText", "마침내 안식하기를." },

                // XX Judgement
                { "TAR_JUDGEMENT_UPRIGHT_ENCHANTMENT.description", "전투 시작 시, 이 카드는 무작위 카드로 [gold]변형[/gold]됩니다." },
                { "TAR_JUDGEMENT_UPRIGHT_ENCHANTMENT.title", "심판" },
                { "TAR_JUDGEMENT_UPRIGHT_ENCHANTMENT.extraCardText", "전투 시작 시 무작위 카드로 변형됩니다." },
                { "TAR_JUDGEMENT_REVERSED_ENCHANTMENT.description", "전투 시작 시, 이 인챈트가 붙은 모든 카드를 덱에 추가된 순서대로 [gold]뽑을 카드 더미[/gold] 맨 아래에 놓습니다." },
                { "TAR_JUDGEMENT_REVERSED_ENCHANTMENT.title", "심판" },
                { "TAR_JUDGEMENT_REVERSED_ENCHANTMENT.extraCardText", "심판 - 역방향." },

                // XXI The World
                { "TAR_WORLD_UPRIGHT_ENCHANTMENT.description", "[gold]구체[/gold]를 발현할 때마다, 이 카드가 [gold]손패[/gold]가 아닌 곳에 있으면 [gold]손패[/gold]로 가져오고, 이번 턴 동안 비용이 {energyPrefix:energyIcons(1)} 증가합니다." },
                { "TAR_WORLD_UPRIGHT_ENCHANTMENT.title", "세계" },
                { "TAR_WORLD_UPRIGHT_ENCHANTMENT.extraCardText", "안녕, 세계." },
                { "TAR_WORLD_REVERSED_ENCHANTMENT.description", "전투마다 이 카드를 처음 사용하면, [blue]1[/blue] [gold]인공물[/gold]을 얻습니다." },
                { "TAR_WORLD_REVERSED_ENCHANTMENT.title", "세계" },
                { "TAR_WORLD_REVERSED_ENCHANTMENT.extraCardText", "디펙트." },

                // SUB Negative Enchantments
                { "TAR_DEVIL_UPRIGHT_SUB_ENCHANTMENT.title", "네거티브 - 악마" },
                { "TAR_DEVIL_UPRIGHT_SUB_ENCHANTMENT.description", "이 카드가 [gold]손패[/gold]에 들어올 때마다, 카드를 [blue]2[/blue]장 뽑습니다." },
                { "TAR_DEVIL_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "손패에 들어올 때 카드를 [blue]2[/blue]장 뽑습니다." },
                { "TAR_DEVIL_REVERSED_SUB_ENCHANTMENT.title", "네거티브 - 악마" },
                { "TAR_DEVIL_REVERSED_SUB_ENCHANTMENT.description", "이 카드가 [gold]손패[/gold]에 들어올 때마다, {energyPrefix:energyIcons(1)}을 얻습니다." },
                { "TAR_DEVIL_REVERSED_SUB_ENCHANTMENT.extraCardText", "손패에 들어올 때 {energyPrefix:energyIcons(1)}을 얻습니다." },

                { "TAR_MOON_UPRIGHT_SUB_ENCHANTMENT.title", "네거티브 - 달" },
                { "TAR_MOON_UPRIGHT_SUB_ENCHANTMENT.description", "이 카드가 [gold]손패[/gold]에 들어올 때마다, 카드를 [blue]2[/blue]장 뽑습니다." },
                { "TAR_MOON_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "손패에 들어올 때 카드를 [blue]2[/blue]장 뽑습니다." },
                { "TAR_MOON_REVERSED_SUB_ENCHANTMENT.title", "네거티브 - 달" },
                { "TAR_MOON_REVERSED_SUB_ENCHANTMENT.description", "이 카드가 [gold]손패[/gold]에 들어올 때마다, {energyPrefix:energyIcons(1)}을 얻습니다." },
                { "TAR_MOON_REVERSED_SUB_ENCHANTMENT.extraCardText", "손패에 들어올 때 {energyPrefix:energyIcons(1)}을 얻습니다." },

                { "TAR_STAR_UPRIGHT_SUB_ENCHANTMENT.title", "네거티브 - 별" },
                { "TAR_STAR_UPRIGHT_SUB_ENCHANTMENT.description", "이 카드가 [gold]손패[/gold]에 들어올 때마다, 카드를 [blue]2[/blue]장 뽑습니다." },
                { "TAR_STAR_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "손패에 들어올 때 카드를 [blue]2[/blue]장 뽑습니다." },
                { "TAR_STAR_REVERSED_SUB_ENCHANTMENT.title", "네거티브 - 별" },
                { "TAR_STAR_REVERSED_SUB_ENCHANTMENT.description", "이 카드가 [gold]손패[/gold]에 들어올 때마다, {energyPrefix:energyIcons(1)}을 얻습니다." },
                { "TAR_STAR_REVERSED_SUB_ENCHANTMENT.extraCardText", "손패에 들어올 때 {energyPrefix:energyIcons(1)}을 얻습니다." },

                { "TAR_SUN_UPRIGHT_SUB_ENCHANTMENT.title", "네거티브 - 태양" },
                { "TAR_SUN_UPRIGHT_SUB_ENCHANTMENT.description", "이 카드가 [gold]손패[/gold]에 들어올 때마다, 카드를 [blue]2[/blue]장 뽑습니다." },
                { "TAR_SUN_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "손패에 들어올 때 카드를 [blue]2[/blue]장 뽑습니다." },
                { "TAR_SUN_REVERSED_SUB_ENCHANTMENT.title", "네거티브 - 태양" },
                { "TAR_SUN_REVERSED_SUB_ENCHANTMENT.description", "이 카드가 [gold]손패[/gold]에 들어올 때마다, {energyPrefix:energyIcons(1)}을 얻습니다." },
                { "TAR_SUN_REVERSED_SUB_ENCHANTMENT.extraCardText", "손패에 들어올 때 {energyPrefix:energyIcons(1)}을 얻습니다." },

                { "TAR_WORLD_UPRIGHT_SUB_ENCHANTMENT.title", "네거티브 - 세계" },
                { "TAR_WORLD_UPRIGHT_SUB_ENCHANTMENT.description", "이 카드가 [gold]손패[/gold]에 들어올 때마다, 카드를 [blue]2[/blue]장 뽑습니다." },
                { "TAR_WORLD_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "손패에 들어올 때 카드를 [blue]2[/blue]장 뽑습니다." },
                { "TAR_WORLD_REVERSED_SUB_ENCHANTMENT.title", "네거티브 - 세계" },
                { "TAR_WORLD_REVERSED_SUB_ENCHANTMENT.description", "이 카드가 [gold]손패[/gold]에 들어올 때마다, {energyPrefix:energyIcons(1)}을 얻습니다." },
                { "TAR_WORLD_REVERSED_SUB_ENCHANTMENT.extraCardText", "손패에 들어올 때 {energyPrefix:energyIcons(1)}을 얻습니다." },

                // Planets - Power
                { "PLANET_MERCURY_ENCHANTMENT.title", "수성" },
                { "PLANET_MERCURY_ENCHANTMENT.description", "아군 [blue]1[/blue]명을 지정합니다. 이번 전투 동안, 매 턴 종료 시 상대의 [gold]뽑을 카드 더미[/gold] 위 [blue]5[/blue]장을 보고 원하는 만큼 버립니다." },
                { "PLANET_MERCURY_ENCHANTMENT.extraCardText", "그대 위해 안개를 헤치리." },

                { "PLANET_VENUS_ENCHANTMENT.title", "금성" },
                { "PLANET_VENUS_ENCHANTMENT.description", "아군 [blue]1[/blue]명을 지정합니다. 이번 전투 동안, 매 턴 종료 시 상대의 [gold]버린 카드 더미[/gold] 위 [blue]5[/blue]장을 보고 원하는 만큼 골라 상대의 [gold]손패[/gold]에 넣습니다." },
                { "PLANET_VENUS_ENCHANTMENT.extraCardText", "그대 위해 잃은 것을 찾으리." },

                { "PLANET_EARTH_ENCHANTMENT.title", "지구" },
                { "PLANET_EARTH_ENCHANTMENT.description", "아군 [blue]1[/blue]명을 지정합니다. 이번 전투 동안, 두 사람의 [gold]에너지[/gold]를 동기화합니다." },
                { "PLANET_EARTH_ENCHANTMENT.extraCardText", "그대와 함께 땅에 서리." },

                { "PLANET_MARS_ENCHANTMENT.title", "화성" },
                { "PLANET_MARS_ENCHANTMENT.description", "아군 [blue]1[/blue]명을 지정합니다. 이번 전투 동안, 두 사람의 생성물을 동기화합니다." },
                { "PLANET_MARS_ENCHANTMENT.extraCardText", "그대와 함께 새로이 창조하리." },

                // Planets - Attack
                { "PLANET_JUPITER_ENCHANTMENT.title", "목성" },
                { "PLANET_JUPITER_ENCHANTMENT.description", "이 카드에 적중당한 적은, 이번 턴에 받는 공격 피해만큼 모든 플레이어에게 [gold]골드[/gold]를 줍니다.\n사용 후 [blue]10[/blue]초 뒤, [red]턴을 종료합니다[/red]." },
                { "PLANET_JUPITER_ENCHANTMENT.extraCardText", "내가 황금을 부르리." },

                { "PLANET_SATURN_ENCHANTMENT.title", "토성" },
                { "PLANET_SATURN_ENCHANTMENT.description", "이 카드에 적중당한 적은, 이번 턴에 받는 공격 피해가 이 카드의 피해량보다 낮을 수 없습니다.\n사용 후 [blue]10[/blue]초 뒤, [red]턴을 종료합니다[/red]." },
                { "PLANET_SATURN_ENCHANTMENT.extraCardText", "내가 한계를 그으리." },

                { "PLANET_URANUS_ENCHANTMENT.title", "천왕성" },
                { "PLANET_URANUS_ENCHANTMENT.description", "사용 후, 비용이 {energyPrefix:energyIcons(1)} 증가하고 [blue]2[/blue] [gold]재사용[/gold]을 얻으며, 무작위 아군의 [gold]뽑을 카드 더미[/gold]에 들어갑니다." },
                { "PLANET_URANUS_ENCHANTMENT.extraCardText", "나는 다시 싸우리." },

                { "PLANET_NEPTUNE_ENCHANTMENT.title", "해왕성" },
                { "PLANET_NEPTUNE_ENCHANTMENT.description", "사용 후, 이 카드의 복사본을 모든 플레이어의 [gold]버린 카드 더미[/gold]에 넣습니다." },
                { "PLANET_NEPTUNE_ENCHANTMENT.extraCardText", "나는 얻음을 나누리." },

                // Planets - Skill
                { "PLANET_PLUTO_ENCHANTMENT.title", "명왕성" },
                { "PLANET_PLUTO_ENCHANTMENT.description", "전투마다 처음 사용하면, 아군의 [purple]명왕성[/purple] 인챈트 카드를 이번 턴 동안 비용 [blue]0[/blue]으로 만들고 각자의 [gold]손패[/gold]에 넣습니다." },
                { "PLANET_PLUTO_ENCHANTMENT.extraCardText", "서로를 이해하기에." },

                { "PLANET_X_ENCHANTMENT.title", "행성 X" },
                { "PLANET_X_ENCHANTMENT.description", "전투마다 처음 사용하면, 아군의 [purple]행성 X[/purple] 인챈트 카드에 [blue]4[/blue] [gold]재사용[/gold]을 부여합니다." },
                { "PLANET_X_ENCHANTMENT.extraCardText", "서로를 지탱하기에." },

                { "PLANET_CERES_ENCHANTMENT.title", "세레스" },
                { "PLANET_CERES_ENCHANTMENT.description", "전투마다 처음 사용하면, 아군의 [purple]세레스[/purple] 인챈트 카드를 [gold]복제[/gold]하여 각자의 더미에 넣습니다." },
                { "PLANET_CERES_ENCHANTMENT.extraCardText", "그러니 결코 혼자가 아니다." },

                { "PLANET_ERIS_ENCHANTMENT.title", "에리스" },
                { "PLANET_ERIS_ENCHANTMENT.description", "전투마다 처음 사용하면, 아군의 [purple]에리스[/purple] 인챈트 카드를 [gold]복제[/gold]하여 모든 플레이어의 [gold]손패[/gold]에 넣습니다. 복사본에는 인챈트가 없습니다." },
                { "PLANET_ERIS_ENCHANTMENT.extraCardText", "그러니 모두가 하나다." },
            });

            var powersTable = loc.GetTable("powers");
            powersTable.MergeWith(new Dictionary<string, string>
            {
                { "TAR_TEMPERANCE_REVERSED_POWER.title", "절제 - 역방향" },
                { "TAR_TEMPERANCE_REVERSED_POWER.description", "이번 턴에 잃은 체력 [blue]1[/blue]마다 전투 종료 시 같은 양의 [gold]골드[/gold]를 얻습니다." },
                { "TAR_TEMPERANCE_REVERSED_POWER.smartDescription", "이번 턴에 잃은 체력 [blue]1[/blue]마다 전투 종료 시 [blue]{Amount}[/blue] [gold]골드[/gold]를 얻습니다." },

                { "TAR_CHARIOT_REVERSED_POWER.title", "전차 - 역방향" },
                { "TAR_CHARIOT_REVERSED_POWER.description", "이 적이 처음으로 방어도로 막지 못한 피해를 주면, 당신은 [gold]취약[/gold]을 [blue]1[/blue] 얻습니다." },
                { "TAR_CHARIOT_REVERSED_POWER.smartDescription", "이 적이 처음으로 방어도로 막지 못한 피해를 주면, 당신은 [gold]취약[/gold]을 [blue]{Amount}[/blue] 얻습니다." },

                { "TAR_STRENGTH_REVERSED_POWER.title", "힘 - 역방향" },
                { "TAR_STRENGTH_REVERSED_POWER.description", "이 적이 처음으로 방어도로 막지 못한 피해를 주면, 당신은 [gold]약화[/gold]를 [blue]1[/blue] 얻습니다." },
                { "TAR_STRENGTH_REVERSED_POWER.smartDescription", "이 적이 처음으로 방어도로 막지 못한 피해를 주면, 당신은 [gold]약화[/gold]를 [blue]{Amount}[/blue] 얻습니다." },

                { "TAR_HERMIT_REVERSED_POWER.title", "은둔자 - 역방향" },
                { "TAR_HERMIT_REVERSED_POWER.description", "턴 종료 시 이 수치에 해당하는 [gold]방어도[/gold]를 얻습니다. 방어도로 막지 못한 피해를 받을 때마다 [blue]1[/blue] 감소합니다." },
                { "TAR_HERMIT_REVERSED_POWER.smartDescription", "턴 종료 시 [blue]{Amount}[/blue]만큼 [gold]방어도[/gold]를 얻습니다. 방어도로 막지 못한 피해를 받을 때마다 [blue]1[/blue] 감소합니다." },

                { "TAR_JUSTICE_REVERSED_POWER.title", "정의 - 역방향" },
                { "TAR_JUSTICE_REVERSED_POWER.description", "매 턴 처음 사용하는 공격 카드가 [gold]소멸[/gold]됩니다." },
                { "TAR_JUSTICE_REVERSED_POWER.smartDescription", "매 턴 처음 사용하는 공격 카드가 [gold]소멸[/gold]됩니다." },

                { "TAR_HANGED_MAN_REVERSED_POWER.title", "매달린 남자 - 역방향" },
                { "TAR_HANGED_MAN_REVERSED_POWER.description", "매 턴 처음 사용하는 스킬 카드가 [gold]소멸[/gold]됩니다." },
                { "TAR_HANGED_MAN_REVERSED_POWER.smartDescription", "매 턴 처음 사용하는 스킬 카드가 [gold]소멸[/gold]됩니다." },

                { "TAR_DEATH_REVERSED_POWER.title", "죽음 - 역방향" },
                { "TAR_DEATH_REVERSED_POWER.description", "이 전투에서 [gold]파워 카드[/gold]를 사용할 때마다 즉시 [red]턴을 종료[/red]합니다." },
                { "TAR_DEATH_REVERSED_POWER.smartDescription", "이 전투에서 [gold]파워 카드[/gold]를 사용할 때마다 즉시 [red]턴을 종료[/red]합니다." },

                { "PLANET_MERCURY_POWER.title", "수성" },
                { "PLANET_MERCURY_POWER.description", "아군의 버리기 단계 종료 후, 그 뽑을 카드 더미 위 [blue]5[/blue]장을 보고 원하는 만큼 버립니다." },
                { "PLANET_MERCURY_POWER.smartDescription", "{PairedName}의 버리기 단계 종료 후, 그 뽑을 카드 더미 위 [blue]5[/blue]장을 보고 원하는 만큼 버립니다." },

                { "PLANET_VENUS_POWER.title", "금성" },
                { "PLANET_VENUS_POWER.description", "아군의 버리기 단계 종료 후, 그 [gold]버린 카드 더미[/gold] 위 [blue]5[/blue]장을 보고 원하는 만큼 골라 그 [gold]손패[/gold]에 넣습니다." },
                { "PLANET_VENUS_POWER.smartDescription", "{PairedName}의 버리기 단계 종료 후, 그 [gold]버린 카드 더미[/gold] 위 [blue]5[/blue]장을 보고 원하는 만큼 골라 그 [gold]손패[/gold]에 넣습니다." },

                { "PLANET_EARTH_POWER.title", "지구" },
                { "PLANET_EARTH_POWER.description", "아군과 에너지{energyPrefix:energyIcons(1)}를 공유합니다." },
                { "PLANET_EARTH_POWER.smartDescription", "{PairedName}과 에너지{energyPrefix:energyIcons(1)}를 공유합니다." },

                { "PLANET_MARS_POWER.title", "화성" },
                { "PLANET_MARS_POWER.description", "아군과 생성물을 공유합니다." },
                { "PLANET_MARS_POWER.smartDescription", "{PairedName}과 생성물을 공유합니다." },

                { "PLANET_JUPITER_POWER.title", "목성" },
                { "PLANET_JUPITER_POWER.description", "이번 턴 동안, 이 적이 공격 피해를 [blue]1[/blue] 받을 때마다 전투 종료 후 모든 플레이어가 같은 양의 [gold]골드[/gold]를 얻습니다." },
                { "PLANET_JUPITER_POWER.smartDescription", "이번 턴 동안, 이 적이 공격 피해를 [blue]1[/blue] 받을 때마다 전투 종료 후 모든 플레이어가 [blue]{Amount}[/blue] [gold]골드[/gold]를 얻습니다." },

                { "PLANET_SATURN_POWER.title", "토성" },
                { "PLANET_SATURN_POWER.description", "이번 턴 동안, 이 적에게 가하는 공격 피해는 [blue]{Amount}[/blue]보다 낮을 수 없습니다." },
                { "PLANET_SATURN_POWER.smartDescription", "이번 턴 동안, 이 적에게 가하는 공격 피해는 [blue]{Amount}[/blue]보다 낮을 수 없습니다." },

                { "PLANET_GOLD_POWER.title", "축적" },
                { "PLANET_GOLD_POWER.description", "전투 종료 시, [blue]{Amount}[/blue] [gold]골드[/gold]를 얻습니다." },
                { "PLANET_GOLD_POWER.smartDescription", "전투 종료 시, [blue]{Amount}[/blue] [gold]골드[/gold]를 얻습니다." },

                { "TICK_TACK_POWER.title", "카운트다운" },
                { "TICK_TACK_POWER.description", "[blue]0[/blue]이 되면, 턴이 강제 종료됩니다." },
                { "TICK_TACK_POWER.smartDescription", "[blue]{Amount}[/blue]초 후, [red]턴이 강제 종료됩니다[/red]." },
            });

            var gameplayUiTable = loc.GetTable("gameplay_ui");
            gameplayUiTable.MergeWith(new Dictionary<string, string>
            {
                { "CHOOSE_CARD_DOWNGRADE_HEADER", "열화시킬 카드를 원하는 만큼 선택하세요" },
                { "PLANET_MERCURY_SELECTION_PROMPT", "아군의 뽑을 카드 더미 위에서 버릴 카드를 원하는 만큼 선택하세요" },
                { "PLANET_VENUS_SELECTION_PROMPT", "아군의 버린 카드 더미 위에서 손패로 되돌릴 카드를 원하는 만큼 선택하세요" },
                { "VANILLA_STYLE_TAROT", "타로 카드 클래식" },
                { "VANILLA_STYLE_PLANET", "행성 카드 클래식" },
            });

            var roomTable = loc.GetTable("merchant_room");
            roomTable.MergeWith(new Dictionary<string, string>
            {
                { "TAROT_PILE_ENTRY.title", "타로 팩" },
                { "TAROT_PILE_ENTRY.description", "타로 카드 [blue]3[/blue]장을 뽑고 [blue]1[/blue]장을 선택해 덱의 카드 [blue]1[/blue]장에 [gold]인챈트[/gold]합니다.\n가끔 이상한 효과가 일어납니다..." }
            });


            var relicsTable = loc.GetTable("relics");
            relicsTable.MergeWith(new Dictionary<string, string>
            {
                { "STARGAZER_KIT.title", "천체관측 키트" },
                { "STARGAZER_KIT.description", "[gold]휴식처[/gold]에서 [gold]천체관측[/gold]을 합니다.\n[gold]고대의 존재[/gold] 노드에 진입하면 [blue]2[/blue]회 추가 사용 횟수를 얻습니다." },
                { "STARGAZER_KIT.flavor", "그 좋은 밤으로 순순히 들어가지 마라." }
            });

            var restSiteTable = loc.GetTable("rest_site_ui");
            restSiteTable.MergeWith(new Dictionary<string, string>
            {
                { "OPTION_STARGAZE.description", "무작위 행성 카드 [blue]3[/blue]장 중 [blue]1[/blue]장을 골라 [gold]덱[/gold]의 비멀티플레이 카드 한 장에 [gold]인챈트[/gold]합니다." },
                { "OPTION_STARGAZE.name", "천체관측" },
            });

            var mainMenuUiTable = loc.GetTable("main_menu_ui");
            mainMenuUiTable.MergeWith(new Dictionary<string, string>
            {
                { "HEXTECH_WARNING_TITLE", "PengoTarot × Hextech 호환성 안내" },
                { "HEXTECH_WARNING_PAGE1", "PengoTarot과 Hextech 모드를 동시에 설치했습니다. PengoTarot 제작자의 안내:\n\nHextech는 구식이고 과격한 다중 인챈트 구현을 사용하며, 원본의 인챈트 판정을 하드코딩으로 수정해서 대부분의 인챈트 콘텐츠 모드와 호환되지 않습니다." },
                { "HEXTECH_WARNING_PAGE2", "더 안정적인 다중 인챈트 경험을 원하시면 MultiEnchantment 사용을 권장합니다.\n\n문의 사항이 있으시면 먼저 Hextech 모드 제작자에게 연락해 구식 구현을 수정해 주시기 바랍니다. 본 모드는 효과적인 호환을 제공할 수 없습니다." },
                { "HEXTECH_WARNING_NEXT", "다음" },
                { "HEXTECH_WARNING_ACK", "알겠습니다" },
            });
        }
    }
}
