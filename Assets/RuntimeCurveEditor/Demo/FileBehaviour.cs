using UnityEngine;

namespace DemoApplication
{
	public class FileBehaviour : MonoBehaviour
	{

		public FileSelectionControlBehaviour FileSelectionControlBehaviourProp { set; private get; }

		// Use this for initialization
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		public void FileSelect()
		{
			FileSelectionControlBehaviourProp.FileSelect(GetComponent<RectTransform>());
		}
	}
}