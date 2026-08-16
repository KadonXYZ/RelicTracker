using MegaCrit.Sts2.Core.ValueProps;

public static class PublicPropExtensions
{
    public static bool IsPoweredAttackRelicTracker(this ValueProp props) =>
        props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered);

    public static bool IsCardOrMonsterMoveRelicTracker(this ValueProp props) =>
        props.HasFlag(ValueProp.Move);
}
