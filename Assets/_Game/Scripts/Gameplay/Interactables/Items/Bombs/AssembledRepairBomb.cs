using UnityEngine;

public class AssembledRepairBomb : ItemAssembled
{
    [Header("Bomb Settings")]
    [SerializeField] private EBombType _bombType;
    [SerializeField] private RepairComponent _repairComponent;

    private bool _hasExploded;

    private void Start()
    {
        _repairComponent.OnExploded += RepairComponent_OnExploded; ;
    }

    public override void Pickup(Transform carryPoint)
    {
        base.Pickup(carryPoint);

        if (_repairComponent != null)
        {
            _repairComponent.Disarm();
        }
    }

    public override void Drop()
    {
        base.Drop();

        if (_repairComponent != null)
        {
            _repairComponent.Arm();
        }
    }

    private void RepairComponent_OnExploded(object sender, System.EventArgs e)
    {
        if (_hasExploded)
            return;

        _hasExploded = true;

        // Bomb-specific explosion effects
        switch (_bombType)
        {
            case EBombType.RepairBomb:
                // Healing particles
                break;
            case EBombType.StunBomb:
                // Stun waves
                break;
            case EBombType.IceBomb:
                // Freeze effect
                break;
            case EBombType.FlameBomb:
                // Fire spread
                break;
            case EBombType.MagnetBomb:
                // Pull force
                break;
        }

        // Destroy the bomb item (blast component will handle explosion visuals)
        //Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_repairComponent != null)
        {
            _repairComponent.OnExploded -= RepairComponent_OnExploded;
        }
    }
}