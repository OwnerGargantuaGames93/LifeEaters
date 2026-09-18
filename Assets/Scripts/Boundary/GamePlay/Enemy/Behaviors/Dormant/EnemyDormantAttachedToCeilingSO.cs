using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.Dormant
{
    [CreateAssetMenu(fileName = "Attached To Ceiling", menuName = "Enemies/Behaviors/Dormant/Enemy Attached To Ceiling")]
    public class EnemyDormantAttachedToCeilingSO: EnemyDormantSOBase
    {
        private readonly float _originalGravityScale = 4f;
        
        public override void DoEnterLogic()
        {
            Enemy.Rb.gravityScale = 0.0f;
            Enemy.Rb.linearVelocity = Vector2.zero;
            Enemy.Rb.bodyType = RigidbodyType2D.Kinematic;
            
            base.DoEnterLogic();
        }

        public override void DoExitLogic()
        {
            Enemy.Rb.gravityScale = _originalGravityScale;
            Enemy.Rb.bodyType = RigidbodyType2D.Dynamic;
            
            base.DoExitLogic();
        }

        public override void DoFrameUpdateLogic()
        {
            base.DoFrameUpdateLogic();
        }

        public override void DoPhysicsLogic()
        {
            base.DoPhysicsLogic();
        }
    }
}