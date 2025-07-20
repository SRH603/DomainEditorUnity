using Battlehub.RTCommon;
namespace Battlehub.RTEditor
{
	public class EditorsMapCreator : IEditorsMapCreator
	{
		#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		#endif
		[UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
		static void Register()
		{
			IOC.UnregisterFallback<IEditorsMapCreator>();
			IOC.RegisterFallback<IEditorsMapCreator>(() => new EditorsMapCreator());
		}
		
		void IEditorsMapCreator.Create(IEditorsMap map)
		{
			map.AddMapping(typeof(UnityEngine.GameObject), 0, true, false);
			map.AddMapping(typeof(Models.Asset), 1, true, false);
			map.AddMapping(typeof(System.Object), 2, true, true);
			map.AddMapping(typeof(UnityEngine.Object), 3, true, true);
			map.AddMapping(typeof(System.Boolean), 4, true, true);
			map.AddMapping(typeof(System.Enum), 5, true, true);
			map.AddMapping(typeof(System.Collections.Generic.List<>), 6, true, true);
			map.AddMapping(typeof(System.Array), 7, true, true);
			map.AddMapping(typeof(System.String), 8, true, true);
			map.AddMapping(typeof(System.Int32), 9, true, true);
			map.AddMapping(typeof(System.Single), 10, true, true);
			map.AddMapping(typeof(Range), 11, true, true);
			map.AddMapping(typeof(UnityEngine.Vector2), 12, true, true);
			map.AddMapping(typeof(UnityEngine.Vector3), 13, true, true);
			map.AddMapping(typeof(UnityEngine.Vector4), 14, true, true);
			map.AddMapping(typeof(UnityEngine.Quaternion), 15, true, true);
			map.AddMapping(typeof(UnityEngine.Color), 16, true, true);
			map.AddMapping(typeof(UnityEngine.Bounds), 17, true, true);
			map.AddMapping(typeof(RangeInt), 18, true, true);
			map.AddMapping(typeof(RangeOptions), 19, true, true);
			map.AddMapping(typeof(HeaderText), 20, true, true);
			map.AddMapping(typeof(System.Reflection.MethodInfo), 21, true, true);
			map.AddMapping(typeof(RangeFlags), 22, true, true);
			map.AddMapping(typeof(UnityEngine.LayerMask), 23, true, true);
			map.AddMapping(typeof(Utils.PersistentCall), 24, true, true);
			map.AddMapping(typeof(UnityEngine.Events.UnityEventBase), 25, true, true);
			map.AddMapping(typeof(BoolFloat), 26, true, true);
			map.AddMapping(typeof(UnityEngine.Component), 27, true, false);
			map.AddMapping(typeof(UnityEngine.AudioListener), 27, true, false);
			map.AddMapping(typeof(UnityEngine.AudioSource), 27, true, false);
			map.AddMapping(typeof(UnityEngine.BoxCollider), 28, true, false);
			map.AddMapping(typeof(UnityEngine.UI.Button), 27, true, false);
			map.AddMapping(typeof(UnityEngine.Camera), 27, true, false);
			map.AddMapping(typeof(UnityEngine.Canvas), 27, true, false);
			map.AddMapping(typeof(UnityEngine.CapsuleCollider), 28, true, false);
			map.AddMapping(typeof(UnityEngine.FixedJoint), 27, true, false);
			map.AddMapping(typeof(UnityEngine.UI.GridLayoutGroup), 27, true, false);
			map.AddMapping(typeof(UnityEngine.HingeJoint), 27, true, false);
			map.AddMapping(typeof(UnityEngine.UI.HorizontalLayoutGroup), 27, true, false);
			map.AddMapping(typeof(UnityEngine.UI.Image), 27, true, false);
			map.AddMapping(typeof(UnityEngine.UI.LayoutElement), 27, true, false);
			map.AddMapping(typeof(UnityEngine.Light), 27, true, false);
			map.AddMapping(typeof(UnityEngine.MeshCollider), 27, true, false);
			map.AddMapping(typeof(UnityEngine.MeshFilter), 27, true, false);
			map.AddMapping(typeof(UnityEngine.MeshRenderer), 27, true, false);
			map.AddMapping(typeof(UnityEngine.MonoBehaviour), 27, false, false);
			map.AddMapping(typeof(UnityEngine.RectTransform), 29, true, false);
			map.AddMapping(typeof(UnityEngine.Rigidbody), 27, true, false);
			map.AddMapping(typeof(UnityEngine.SkinnedMeshRenderer), 27, true, false);
			map.AddMapping(typeof(UnityEngine.Skybox), 27, true, false);
			map.AddMapping(typeof(UnityEngine.SphereCollider), 28, true, false);
			map.AddMapping(typeof(UnityEngine.SpringJoint), 27, true, false);
			map.AddMapping(typeof(UnityEngine.UI.Text), 27, true, false);
			map.AddMapping(typeof(UnityEngine.UI.Toggle), 27, true, false);
			map.AddMapping(typeof(UnityEngine.UI.ToggleGroup), 27, true, false);
			map.AddMapping(typeof(UnityEngine.Transform), 30, true, false);
			map.AddMapping(typeof(UnityEngine.UI.VerticalLayoutGroup), 27, true, false);
			map.AddMapping(typeof(LayersInfo), 31, true, false);
			map.AddMapping(typeof(RuntimeAnimation), 27, true, false);
			map.AddMapping(typeof(ChartInfoSerializedField), 27, true, false);
			map.AddMapping(typeof(TMPro.TextMeshProUGUI), 27, true, false);
		}
	}
}
