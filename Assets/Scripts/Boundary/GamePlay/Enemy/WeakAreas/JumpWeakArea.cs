using Boundary.GamePlay.Enemy.Base;
using Boundary.Player;
using UnityEngine;
using Utils;

namespace Boundary.Enemy.WeakAreas
{
    public class JumpWeakArea: MonoBehaviour
    {
        private BaseEnemy _enemy;
        private PlayerController _playerController;
        
        private void Awake()
        {
            _enemy = GetComponentInParent<BaseEnemy>();
        }
        
        private void Start()
        {
            _playerController = GameObject.FindGameObjectWithTag(Constants.PlayerTag).GetComponent<PlayerController>();
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerFeetTag) && _playerController.IsFalling())
            {
                _enemy.OnStomped();
            }
        }
    }
}