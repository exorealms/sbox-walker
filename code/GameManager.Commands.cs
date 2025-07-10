public sealed partial class GameManager
{
	[ConCmd( "spawn" )]
	public static void Spawn( string modelname )
	{
		var player = Player.FindLocalPlayer();
		if ( !player.IsValid() )
			return;

		var tr = Game.ActiveScene.Trace.Ray( player.EyeTransform.Position, player.EyeTransform.Position + player.EyeTransform.Rotation.Forward * 500 )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( player.GameObject )
			.Run();

		var modelRotation = Rotation.From( new Angles( 0, player.EyeTransform.Rotation.Angles().yaw, 0 ) ) * Rotation.FromAxis( Vector3.Up, 180 );

		SpawnModel( modelname, tr.EndPosition, modelRotation, player.GameObject );
		Sandbox.Services.Stats.Increment( "spawn.model", 1, modelname );
	}

	[Rpc.Broadcast]
	static void SpawnModel( string modelname, Vector3 endPos, Rotation modelRotation, GameObject playerObject )
	{
		if ( Networking.IsClient )
			return;

		//
		// Does this look like a package?
		//
		if ( modelname.Count( x => x == '.' ) == 1 && !modelname.EndsWith( ".vmdl", StringComparison.OrdinalIgnoreCase ) && !modelname.EndsWith( ".vmdl_c", StringComparison.OrdinalIgnoreCase ) )
		{
			modelname = SpawnPackageModel( modelname, playerObject ).GetAwaiter().GetResult();
			if ( modelname == null )
				return;
		}

		var model = Model.Load( modelname );
		if ( model == null || model.IsError )
			return;

		var go = new GameObject
		{
			WorldPosition = endPos + Vector3.Down * model.PhysicsBounds.Mins.z,
			WorldRotation = modelRotation,
			Tags = { "solid" }
		};

		var prop = go.AddComponent<Prop>();
		prop.Model = model;

		if ( prop.Components.TryGet<SkinnedModelRenderer>( out var renderer ) )
		{
			renderer.CreateBoneObjects = true;
		}

		//var propHelper = go.AddComponent<PropHelper>();

		////var rb = propHelper.Rigidbody;
		//if ( rb.IsValid() )
		//{
		//	// If there's no physics model, create a simple OBB
		//	foreach ( var shape in rb.PhysicsBody.Shapes )
		//	{
		//		if ( !shape.IsMeshShape )
		//			continue;

		//		var newCollider = go.AddComponent<BoxCollider>();
		//		newCollider.Scale = model.PhysicsBounds.Size;
		//	}
		//}

		go.NetworkSpawn( playerObject.Network.Owner );
		go.Network.SetOrphanedMode( NetworkOrphaned.Host );
	}

	static async Task<string> SpawnPackageModel( string packageName, GameObject source )
	{
		var package = await Package.Fetch( packageName, false );
		if ( package == null || package.TypeName != "model" || package.Revision == null )
		{
			// spawn error particles
			return null;
		}

		if ( !source.IsValid() ) return null; // source gameobject died or disconnected or something

		var model = package.GetMeta( "PrimaryAsset", "models/dev/error.vmdl" );

		// downloads if not downloads, mounts if not mounted
		await package.MountAsync();

		return model;
	}
}
