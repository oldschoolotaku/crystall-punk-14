using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.EntityEffects;
using Content.Shared.Sprite;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Content.Shared._CP14.EntityEffects;

public sealed partial class CP14ChangeSizeEntityEffect : EntityEffect
{
    /// <summary>
    /// Defines, what will be the maximum scale of the entity
    /// </summary>
    [DataField(required: true)]
    [JsonPropertyName("size")]
    public float Size = default!;


    /// <summary>
    /// Do we want for the effect to stay after our solution has been consumed?
    /// </summary>
    [DataField]
    [JsonPropertyName("permanent")]
    public bool Permanent = false;

    private bool _hasChangedFixture = false;

    public override void Effect(EntityEffectBaseArgs args)
    {
        var entman = args.EntityManager;
        var solution = entman.GetComponent<SolutionContainerManagerComponent>(args.TargetEntity);
        var size = entman.System<SharedScaleVisualsSystem>().GetSpriteScale(args.TargetEntity);

        //determining how much we should change the size in smaller sidesteps (cool looking)
        var sidestep = Size / 10;

        if (Permanent == false && _hasChangedFixture == true)
        {
            entman.System<SharedScaleVisualsSystem>().SetSpriteScale(args.TargetEntity, Vector2.One);
            entman.System<SharedPhysicsSystem>().ScaleFixtures(args.TargetEntity, Size * (1 / Size));
            //entman.System<SharedPhysicsSystem>().SetVertices(args.TargetEntity, )
        }

        switch (Size > 1)
        {
            case true:
                size.X += sidestep;
                size.Y += sidestep;

                if (size.X > Size)
                {
                    size.X = Size;
                    size.Y = Size;
                }

                entman.System<SharedScaleVisualsSystem>().SetSpriteScale(args.TargetEntity, size);
                break;

            case false:
                size.X -= sidestep;
                size.Y -= sidestep;

                if (size.X > Size)
                {
                    size.X = Size;
                    size.Y = Size;
                }

                entman.System<SharedScaleVisualsSystem>().SetSpriteScale(args.TargetEntity, size);
                break;
        }

        if (!_hasChangedFixture)
        {
            entman.System<SharedPhysicsSystem>().ScaleFixtures(args.TargetEntity, Size);
            _hasChangedFixture = true;
        }
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
        Loc.GetString("cp14-reagent-effect-guidebook-change-size-reaction", ("size", Size));
    //cp14-reagent-effect-guidebook-change-size-reaction = Changes size to x.xx
}
