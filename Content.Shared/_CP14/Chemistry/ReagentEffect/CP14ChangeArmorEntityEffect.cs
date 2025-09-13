using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Content.Shared.Localizations;
using Robust.Shared.Prototypes;
using System.Text.Json.Serialization;

namespace Content.Shared._CP14.EntityEffects;
public sealed partial class CP14ChangeArmorEntityEffect : EntityEffect
{
    [DataField(required: true)]
    [JsonPropertyName("damage")]
    public DamageSpecifier Armor = default!;

    public override void Effect(EntityEffectBaseArgs args)
    {
        throw new NotImplementedException();
    }

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        var damages = new List<string>();
        var heals = false;
        var deals = false;

        var damageSpec = new DamageSpecifier(Armor);

        var universalReagentDamageModifier = entSys.GetEntitySystem<DamageableSystem>().UniversalReagentDamageModifier;
        var universalReagentHealModifier = entSys.GetEntitySystem<DamageableSystem>().UniversalReagentHealModifier;

        if (universalReagentDamageModifier != 1 || universalReagentHealModifier != 1)
        {
            foreach (var (type, val) in damageSpec.DamageDict)
            {
                if (val < 0f)
                {
                    damageSpec.DamageDict[type] = val * universalReagentHealModifier;
                }
                if (val > 0f)
                {
                    damageSpec.DamageDict[type] = val * universalReagentDamageModifier;
                }
            }
        }

        damageSpec = entSys.GetEntitySystem<DamageableSystem>().ApplyUniversalAllModifiers(damageSpec);

        foreach (var (kind, amount) in damageSpec.DamageDict)
        {
            var sign = FixedPoint2.Sign(amount);

            if (sign < 0)
                heals = true;
            if (sign > 0)
                deals = true;

            damages.Add(
                Loc.GetString("health-change-display",
                    ("kind", prototype.Index<DamageTypePrototype>(kind).LocalizedName),
                    ("amount", MathF.Abs(amount.Float())),
                    ("deltasign", sign)
                ));
        }

        var healsordeals = heals ? (deals ? "both" : "heals") : (deals ? "deals" : "none");

        return Loc.GetString("reagent-effect-guidebook-health-change",
            ("chance", Probability),
            ("changes", ContentLocalizationManager.FormatList(damages)),
            ("healsordeals", healsordeals));
    }
}
