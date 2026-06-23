namespace Boundary.Enemy.Interfaces
{
    public interface ITriggerCheckable
    {
        bool IsAggroed { get; set; }
        bool IsAwake { get; set; }

        void SetAggroStatus(bool isAggroed);
        void SetIsAwake(bool isAwake);
    }
}
