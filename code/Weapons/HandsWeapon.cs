public class HandsWeapon : BaseWeapon
{
	public override void OnControl( Player player )
	{
        if (Input.Down("attack1"))
        {
            Input.SetAction("use", true);
        }
    }
}
